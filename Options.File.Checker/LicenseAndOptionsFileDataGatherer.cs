using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Options.File.Checker
{
    public partial class LicenseAndOptionsFileDataGatherer
    {
        // Do Regex stuff now to be efficient and stuff blah blah blah.
        [GeneratedRegex("port=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countPortEqualsRegex();
        [GeneratedRegex("options=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countOptionsEquals();
        [GeneratedRegex("# BEGIN--------------", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countCommentedBeginLines();

        [GeneratedRegex("key=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex KeyEquals();

        [GeneratedRegex("asset_info=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex AssetInfo();

        private static bool serverLineHasPort = true;
        private static bool daemonLineHasPort = false;
        private static bool daemonPortIsCNUFriendly = false;
        private static bool caseSensitivity = true;
        private static readonly Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> includeDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> includeBorrowDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string>> includeAllDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> excludeDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> excludeBorrowDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string>> excludeAllDictionary = [];
        private static readonly Dictionary<int, Tuple<int, string, string, string, string, string>> reserveDictionary = [];
        private static readonly Dictionary<string, Tuple<int, string, string>> maxDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, int>> groupDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string>> hostGroupDictionary = [];
        private static string? err = string.Empty;

        public static (
            bool serverLineHasPort,
            bool daemonLineHasPort,
            bool daemonPortIsCNUFriendly,
            bool caseSensitivity,
            Dictionary<int, Tuple<string, int, string, string, string>>? licenseFileDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? includeDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? includeBorrowDictionary,
            Dictionary<int, Tuple<string, string>>? includeAllDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? excludeDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? excludeBorrowDictionary,
            Dictionary<int, Tuple<string, string>>? excludeAllDictionary,
            Dictionary<int, Tuple<int, string, string, string, string, string>>? reserveDictionary,
            Dictionary<string, Tuple<int, string, string>>? maxDictionary,
            Dictionary<int, Tuple<string, string, int>>? groupDictionary,
            Dictionary<int, Tuple<string, string>>? hostGroupDictionary,
            string? err)

            GatherData(string licenseFilePath, string optionsFilePath)
        {
            // I'm putting this here so that we can print its contents if we hit a generic error message.
            string line = string.Empty;

            try
            {
                // Load the file's contents.
                string[] licenseFileContentsLines = System.IO.File.ReadAllLines(licenseFilePath);
                string[] optionsFileContentsLines = System.IO.File.ReadAllLines(optionsFilePath);

                string optionsFileContents = string.Join(Environment.NewLine, optionsFileContentsLines);
                string licenseFileContents = string.Join(Environment.NewLine, licenseFileContentsLines);

                string lineBreaksToRemove = "\\\r\n\t";
                licenseFileContents = licenseFileContents.Replace(lineBreaksToRemove, string.Empty);

                // Put it back together!
                licenseFileContentsLines = licenseFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Next, let's check for some obvious errors.
                // Make sure, you know, the files exist.
                if (!System.IO.File.Exists(licenseFilePath) || !System.IO.File.Exists(optionsFilePath))
                {
                    err = "The license and/or the options file you selected either no longer exists or you don't have permissions to read one of them.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                // Error time again, in case you decided to be sneaky and close the program or manually enter the filepath.
                if (string.IsNullOrWhiteSpace(optionsFileContents))
                {
                    err = "There is an issue with the license file: it is either empty or only contains white space.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                // Make sure you actually picked an options file.
                if (!optionsFileContents.Contains("INCLUDE") && !optionsFileContents.Contains("EXCLUDE") && !optionsFileContents.Contains("RESERVE")
                    && !optionsFileContents.Contains("MAX") && !optionsFileContents.Contains("LINGER") && !optionsFileContents.Contains("LOG") &&
                    !optionsFileContents.Contains("TIMEOUT"))
                {
                    err = "There is an issue with the options file: it is likely not an options file or contains no usable content.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                if (!System.IO.File.ReadAllText(licenseFilePath).Contains("INCREMENT"))
                {
                    err = "There is an issue with the license file: it is either not a license file or is corrupted.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                if (licenseFileContents.Contains("lo=IN") || licenseFileContents.Contains("lo=DC") || licenseFileContents.Contains("lo=CIN"))
                {
                    err = "There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                        "which cannot use an options file.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                if (System.IO.File.ReadAllText(licenseFilePath).Contains("CONTRACT_ID="))
                {
                    err = "There is an issue with the license file: it is not a MathWorks license file.";
                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                // Reset everything!
                serverLineHasPort = true;
                daemonLineHasPort = false;
                licenseFileDictionary.Clear();
                includeDictionary.Clear();
                includeBorrowDictionary.Clear();
                includeAllDictionary.Clear();
                excludeDictionary.Clear();
                excludeBorrowDictionary.Clear();
                excludeAllDictionary.Clear();
                reserveDictionary.Clear();
                maxDictionary.Clear();
                groupDictionary.Clear();
                hostGroupDictionary.Clear();
                err = string.Empty;

                // Stuff that the class won't output.
                bool containsPLP = false;
                int serverLineCount = 0;
                int daemonLineCount = 0;
                bool productLinesHaveBeenReached = false;

                // License file information gathering.
                for (int licenseLineIndex = 0; licenseLineIndex < licenseFileContentsLines.Length; licenseLineIndex++)
                {
                    line = licenseFileContentsLines[licenseLineIndex];
                    string productName = string.Empty;
                    int seatCount = 0;
                    string plpLicenseNumber = string.Empty;

                    if (line.TrimStart().StartsWith("SERVER"))
                    {
                        // SERVER lines should come before the product(s).
                        if (productLinesHaveBeenReached)
                        {
                            err = "There is an issue with the license file: your SERVER line(s) are listed after a product.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                        serverLineCount++;

                        string[] lineParts = line.Split(' ');
                        string serverWord = lineParts[0];
                        string serverHostID = lineParts[2];

                        if (serverHostID == "27000" || serverHostID == "27001" || serverHostID == "27002" || serverHostID == "27003" || serverHostID == "27004" || serverHostID == "27005" )
                        {
                            err = "There is an issue with the license file: you have likely omitted your Host ID and attempted to specify a SERVER port number. " +
                                "Because you have omitted the Host ID, the port you've attempted to specify will not be used.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (lineParts.Length < 3)
                        {
                            err = "There is an issue with the license file: you are missing information from your SERVER line.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                        else if (lineParts.Length == 3)
                        {
                            serverLineHasPort = false;
                        }
                        else if (lineParts.Length == 4)
                        {
                            // Check to make sure you're using a port number.
                            if (!int.TryParse(lineParts[3], out int serverPort))
                            {
                                err = "There is an issue with the license file: you have stray information on your SERVER line.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }

                            if (serverWord != "SERVER")
                            {
                                err = "There is an issue with the license file: it does not start with the word SERVER.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }

                            if (!serverHostID.Contains("INTERNET=") && serverHostID.Length != 12)
                            {
                                err = "There is an issue with the license file: you have not specified your Host ID correctly.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }

                            // Congrats, you /may/ have not made any mistakes on your SERVER line.
                        }
                        else if (lineParts.Length == 5)
                        {
                            if (lineParts[4] == "")
                            {
                                continue; // Your stray space shall be ignored... this time.
                            }
                            else
                            {
                                err = "There is an issue with the license file: you have stray information on your SERVER line.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                        else
                        {
                            err = "There is an issue with the license file: you have stray information on your SERVER line.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // There is no situation where you should have more than 3 SERVER lines.
                        if (serverLineCount > 3 || serverLineCount == 2)
                        {
                            err = "There is an issue with the license file: it has too many SERVER lines. Only 1 or 3 are accepted.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                    }
                    else if (line.TrimStart().StartsWith("DAEMON"))
                    {
                        // DAEMON line should come before the product(s).
                        if (productLinesHaveBeenReached)
                        {
                            err = "There is an issue with the license file: your DAEMON line is listed after a product.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // There should only be one DAEMON line.
                        daemonLineCount++;
                        if (daemonLineCount > 1)
                        {
                            err = "There is an issue with the license file: you have more than 1 DAEMON line.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // port= and options= should only appear once.
                        int countPortEquals = countPortEqualsRegex().Matches(line).Count;
                        int countOptionsEquals = LicenseAndOptionsFileDataGatherer.countOptionsEquals().Matches(line).Count;
                        int countCommentedBeginLines = LicenseAndOptionsFileDataGatherer.countCommentedBeginLines().Matches(line).Count;

                        // For the CNU kids.
                        if (line.Contains("PORT="))
                        {
                            daemonPortIsCNUFriendly = true;
                        }

                        if (countCommentedBeginLines > 0)
                        {
                            err = "There is an issue with the license file: it has content that is intended to be commented out in your DAEMON line.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (countPortEquals > 1)
                        {
                            err = "There is an issue with the license file: you have specified more than 1 port number for MLM.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (countOptionsEquals > 1)
                        {
                            err = "There is an issue with the license file: you have specified the path to more than 1 options file.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (countOptionsEquals == 0)
                        {
                            err = "There is an issue with the license file: you did not specify the path to the options file. " +
                                "If you included the path, but did not use options= to specify it, MathWorks licenses ask that you do so, even if they technically work without options=.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // daemonProperty1 and 2 could either be a port number or path to an options file.
                        string[] lineParts = line.Split(' ');

                        // Just having the word "DAEMON" isn't enough.
                        if (lineParts.Length == 1)
                        {
                            err = "There is an issue with the license file: you have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Checking for the vendor daemon MLM.
                        string daemonVendor = lineParts[1];

                        if (string.IsNullOrWhiteSpace(daemonVendor))
                        {
                            err = "There is an issue with the license file: there are too many spaces between \"DAEMON\" and \"MLM\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // The vendor daemon needs to MLM. Not mlm or anything else.
                        if (daemonVendor != "MLM")
                        {
                            err = "There is an issue with the license file: you have incorrectly specified the vendor daemon MLM.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Just specifying "DAEMON MLM" isn't enough.
                        if (lineParts.Length == 2)
                        {
                            err = "There is an issue with the license file: you did not specify the path to the vendor daemon MLM.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // You're missing your options file path.
                        if (lineParts.Length == 3)
                        {
                            err = "There is an issue with the license file: you did not specify the path to the options file.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (countPortEquals == 1)
                        {
                            daemonLineHasPort = true;
                        }
                    }
                    // Where the product information is found.
                    else if (line.TrimStart().StartsWith("INCREMENT"))
                    {
                        productLinesHaveBeenReached = true;
                        string[] lineParts = line.Split(' ');
                        productName = lineParts[1];
                        int productVersion = int.Parse(lineParts[3]);
                        string productExpirationDate = lineParts[4];
                        string productKey = lineParts[6];
                        string licenseOffering = string.Empty;
                        string licenseNumber = string.Empty;
                        _ = int.TryParse(lineParts[5], out seatCount);
                        string rawSeatCount = lineParts[5];

                        // License number.
                        string pattern = @"asset_info=([^\s]+)";

                        if (line.Contains("asset_info="))
                        {
                            Regex regex = new(pattern);
                            Match match = regex.Match(line);

                            if (match.Success)
                            {
                                licenseNumber = match.Groups[1].Value;
                            }
                        }
                        else if (line.Contains("SN="))
                        {
                            pattern = @"SN=([^\s]+)";
                            Regex regex = new(pattern);
                            Match match = regex.Match(line);

                            if (match.Success)
                            {
                                licenseNumber = match.Groups[1].Value;
                            }
                            if (productName == "TMW_Archive") // Welcome to the land of PLPs!
                            {
                                containsPLP = true;
                                plpLicenseNumber = licenseNumber;
                                continue;
                            }
                        }
                        else if (containsPLP && productName.Contains("PolySpace")) // This is the best guess we can make if you're using a PLP-era product.
                        {
                            licenseNumber = plpLicenseNumber;
                        }
                        else
                        {
                            err = $"There is an issue with the license file: the license number {licenseNumber} was not found for the product {productName}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // License offering.
                        if (line.Contains("lo="))
                        {
                            if (line.Contains("lo=CN:"))
                            {
                                licenseOffering = "lo=CN";
                            }
                            else if (line.Contains("lo=CNU"))
                            {
                                licenseOffering = "CNU";
                            }
                            else if (line.Contains("lo=NNU"))
                            {
                                licenseOffering = "NNU";
                            }
                            else if (line.Contains("lo=TH"))
                            {
                                if (!line.Contains("USER_BASED="))
                                {
                                    licenseOffering = "lo=CN";
                                }
                                else
                                {
                                    err = "There is an issue with the license file: it is formatted incorrectly. " +
                                        $"{productName}'s license offering is being read as Total Headcount, but also Network Named User, which doesn't exist.";
                                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                                }
                            }
                            else
                            {
                                err = $"There is an issue with the license file: the product {productName} has an invalid license offering.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                        else if (line.Contains("lr=") || containsPLP && !line.Contains("asset_info=")) // Figure out your trial or PLP's license offering.
                        {
                            if (seatCount > 0)
                            {
                                if (line.Contains("USER_BASED"))
                                {
                                    licenseOffering = "NNU";
                                }
                                else
                                {
                                    if (containsPLP && !line.Contains("asset_info="))
                                    {
                                        licenseOffering = "lo=DC"; // See PLP-era explaination below.
                                    }
                                    else
                                    {
                                        licenseOffering = "lo=CN";
                                    }
                                }
                            }
                            // This means you're likely using a macOS or Linux PLP-era license, which CAN use an options file... I think it has to.
                            else if (containsPLP && !line.Contains("asset_info="))
                            {
                                licenseOffering = "lo=IN";
                                seatCount = 1;
                            }
                            else
                            {
                                err = $"There is an issue with the license file: the product {productName} comes from an Individual " +
                                    "or Designated Computer license, which cannot use an options file.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                        else
                        {
                            if (line.Contains("PLATFORMS=x"))
                            {
                                err = $"There is an issue with the license file: the product {productName} comes from an Individual " +
                                     $"or Designated Computer license generated from a PLP on Windows, which cannot use an options file.";
                            }
                            else
                            {
                                err = $"There is an issue with the license file: the product {productName} has an valid license offering.";
                            }
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Check the product's expiration date. Year 0000 means perpetual.
                        if (productExpirationDate == "01-jan-0000")
                        {
                            productExpirationDate = "01-jan-2999";
                        }

                        // Convert/parse the productExpirationDate string to a DateTime object.
                        DateTime expirationDate = DateTime.ParseExact(productExpirationDate, "dd-MMM-yyyy", CultureInfo.InvariantCulture);

                        // Get the current system date.
                        DateTime currentDate = DateTime.Now.Date;

                        if (expirationDate < currentDate)
                        {
                            err = $"There is an issue with the license file: The product {productName} on license number " +
                                $"{licenseNumber} expired on {productExpirationDate}. Please update your license file appropriately before proceeding.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (licenseOffering.Contains("NNU"))
                        {
                            if (seatCount != 1 && !containsPLP)
                            {
                                seatCount /= 2;
                            }
                        }

                        // Technically infinite. This should avoid at least 1 unnecessary error report.
                        if (licenseOffering.Contains("CNU") && (seatCount == 0))
                        {
                            seatCount = 9999999;
                        }

                        if (licenseOffering == "lo=CN" && (seatCount == 0) && licenseNumber == "220668")
                        {
                            if ((productVersion <= 18) || (productName.Contains("Polyspace") && productVersion <= 22))
                            {
                                err = $"There is an issue with the license file: it contains a Designated Computer or Individual license, {licenseNumber}.";
                            }
                            else
                            {
                                err = $"There is an issue with the license file: it contains a Designated Computer license, {licenseNumber}, " +
                                    "that is incorrectly labeled as a Concurrent license.";
                            }
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (!licenseOffering.Contains("CNU") && rawSeatCount == "uncounted")
                        {
                            err = "There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                                $"which cannot use an options file. The license number is question is {licenseNumber}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (seatCount < 1 && line.Contains("asset_info="))
                        {
                            err = $"There is an issue with the license file: {productName} on license {licenseNumber} is reading with a seat count of zero or less.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Before proceeding, make sure the values we've collected are valid.
                        if (string.IsNullOrWhiteSpace(productName))
                        {
                            err = $"There is an issue with the license file: a product name is being detected as blank on {licenseNumber}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (licenseNumber.Contains("broken") || string.IsNullOrWhiteSpace(licenseNumber) || Regex.IsMatch(licenseNumber, @"^[^Rab_\d]+$"))
                        {
                            err = $"There is an issue with the license file: an invalid license number, {licenseNumber}, is detected for {productName}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (licenseOffering.Contains("broken") || string.IsNullOrWhiteSpace(licenseOffering))
                        {
                            err = $"There is an issue with the license file: a license offering could not be detected for {productName} " +
                                $"on license number {licenseNumber}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (string.IsNullOrWhiteSpace(productKey))
                        {
                            err = $"There is an issue with the license file: a product key could not be detected for {productName} on license number {licenseNumber}.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        licenseFileDictionary[licenseLineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber);
                    }
                }

                // Options file information gathering.
                for (int optionsLineIndex = 0; optionsLineIndex < optionsFileContentsLines.Length; optionsLineIndex++)
                {
                    line = optionsFileContentsLines[optionsLineIndex];

                    if (line.TrimStart().StartsWith("INCLUDE ") || line.TrimStart().StartsWith("INCLUDE_BORROW ") || line.TrimStart().StartsWith("EXCLUDE ") || line.TrimStart().StartsWith("EXCLUDE_BORROW "))
                    {
                        string optionType = string.Empty;

                        if (line.TrimStart().StartsWith("INCLUDE ")) { optionType = "INCLUDE"; }
                        else if (line.TrimStart().StartsWith("INCLUDE_BORROW ")) { optionType = "INCLUDE_BORROW"; }
                        else if (line.TrimStart().StartsWith("EXCLUDE ")) { optionType = "EXCLUDE"; }
                        else if (line.TrimStart().StartsWith("EXCLUDE_BORROW ")) { optionType = "EXCLUDE_BORROW"; }

                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 4)
                        {
                            err = $"There is an issue with the options file: you have an incorrectly formatted {optionType} line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        string productName = lineParts[1];
                        string licenseNumber;
                        string productKey;
                        string clientType;
                        string clientSpecified;

                        if (productName.Contains('"'))
                        {
                            // Check for stray quotation marks.
                            int quoteCount = line.Count(c => c == '"');
                            if (quoteCount % 2 != 0)
                            {
                                err = $"There is an issue with the options file: one of your {optionType} lines has a stray quotation mark. " +
                                    $"The line in question reads as this: {line}";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }

                            productName = productName.Replace("\"", "");
                            licenseNumber = lineParts[2];
                            if (!productName.Contains(':'))
                            {
                                if (licenseNumber.Contains("key=", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    productKey = lineParts[2];
                                    string unfixedProductKey = productKey;
                                    string quotedProductKey = KeyEquals().Replace(unfixedProductKey, "");
                                    productKey = quotedProductKey.Replace("\"", "");
                                    licenseNumber = string.Empty;
                                }
                                else // asset_info=
                                {
                                    string unfixedLicenseNumber = licenseNumber;
                                    string quoteLicenseNumber = AssetInfo().Replace(unfixedLicenseNumber, "");
                                    licenseNumber = quoteLicenseNumber.Replace("\"", "");
                                    productKey = string.Empty;
                                }

                                clientType = lineParts[3];

                                if (clientType != "USER" && clientType != "GROUP" && clientType != "HOST" && clientType != "HOST_GROUP" && clientType != "DISPLAY" &&
                                clientType != "PROJECT" && clientType != "INTERNET")
                                {
                                    err = $"There is an issue with the options file: you have incorrectly specified the client type on a line using {optionType}." +
                                        $"You attempted to use \"{clientType}\". Please reformat this {optionType} line.";
                                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                                }

                                clientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                            else // If you have " and :
                            {
                                string[] colonParts = productName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    err = $"There is an issue with the options file: one of your {optionType} lines has a stray colon for {productName}.";
                                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                                }
                                productName = colonParts[0];
                                if (colonParts[1].Contains("key="))
                                {
                                    string unfixedProductKey = colonParts[1];
                                    productKey = unfixedProductKey.Replace("key=", "");
                                    licenseNumber = string.Empty;
                                }
                                else
                                {
                                    string unfixedLicenseNumber = colonParts[1];
                                    licenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    productKey = string.Empty;
                                }
                                clientType = lineParts[2];
                                clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            }
                        }                        
                        else if (productName.Contains(':')) // In case you decided to use a : instead of ""...
                        {
                            string[] colonParts = productName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                err = $"There is an issue with the options file: one of your {optionType} lines has a stray colon for {productName}.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                            productName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedProductKey = colonParts[1];
                                productKey = unfixedProductKey.Replace("key=", "");
                                licenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedLicenseNumber = colonParts[1];
                                licenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                productKey = string.Empty;
                            }
                            clientType = lineParts[2];
                            clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                        }
                        else
                        {
                            clientType = lineParts[2];
                            clientSpecified = string.Join(" ", lineParts.Skip(3)).TrimEnd();
                            licenseNumber = string.Empty;
                            productKey = string.Empty;
                        }

                        if (line.TrimStart().StartsWith("INCLUDE ")) { includeDictionary[optionsLineIndex] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified); }
                        else if (line.TrimStart().StartsWith("INCLUDE_BORROW ")) { includeBorrowDictionary[optionsLineIndex] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified); }
                        else if (line.TrimStart().StartsWith("EXCLUDE ")) { excludeDictionary[optionsLineIndex] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified); }
                        else if (line.TrimStart().StartsWith("EXCLUDE_BORROW ")) { excludeBorrowDictionary[optionsLineIndex] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified); }
                    }
                    else if (line.TrimStart().StartsWith("INCLUDEALL ") || line.TrimStart().StartsWith("EXCLUDEALL "))
                    {
                        string optionSpecified = string.Empty; // Could either be INCLUDEALL or EXCLUDEALL.
                        string clientType; // Examples include GROUP or USER.                    
                        string clientSpecified = string.Empty; // Examples include "matlab_group" or "root".
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (line.TrimStart().StartsWith("INCLUDEALL ")) { optionSpecified = "INCLUDEALL"; }
                        else if (line.TrimStart().StartsWith("EXCLUDEALL ")) { optionSpecified = "EXCLUDEALL"; }

                        if (lineParts.Length < 3)
                        {
                            err = $"There is an issue with the options file: you have an incorrectly formatted {optionSpecified} line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        clientType = lineParts[1];
                        clientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (clientType != "USER" && clientType != "GROUP" && clientType != "HOST" && clientType != "HOST_GROUP" && clientType != "DISPLAY" &&
                            clientType != "PROJECT" && clientType != "INTERNET")
                        {
                            err = $"There is an issue with the options file: you have incorrectly specified the client type on an {optionSpecified} " +
                                $"line as \"{clientType}\". Please reformat this {optionSpecified} line's client type to something such as \"USER\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (line.TrimStart().StartsWith("INCLUDEALL ")) { includeAllDictionary[optionsLineIndex] = Tuple.Create(clientType, clientSpecified); }
                        else if (line.TrimStart().StartsWith("EXCLUDEALL ")) { excludeAllDictionary[optionsLineIndex] = Tuple.Create(clientType, clientSpecified); }
                    }
                    else if (line.TrimStart().StartsWith("MAX "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 5)
                        {
                            err = "There is an issue with the options file: you have an incorrectly formatted MAX line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        int maxSeats = int.Parse(lineParts[1]);
                        string maxProductName = lineParts[2];
                        string maxClientType = lineParts[3];
                        string maxClientSpecified = string.Join(" ", lineParts.Skip(4));

                        maxDictionary[maxProductName] = Tuple.Create(maxSeats, maxClientType, maxClientSpecified);
                    }
                    else if (line.TrimStart().StartsWith("RESERVE "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 5)
                        {
                            err = "There is an issue with the options file: you have an incorrectly formatted RESERVE line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Check for stray quotation marks.
                        int quoteCount = line.Count(c => c == '"');
                        if (quoteCount % 2 != 0)
                        {
                            err = "There is an issue with the options file: one of your RESERVE lines has a stray quotation mark. " +
                                $"The line in question reads as this: {line}";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        string reserveSeatsString = lineParts[1];
                        string reserveProductName = lineParts[2];
                        string reserveLicenseNumber;
                        string reserveProductKey;
                        string reserveClientType;
                        string reserveClientSpecified;

                        // Convert the seat count from a string to a integer.
                        if (int.TryParse(reserveSeatsString, out int reserveSeatCount))
                        {
                            // Parsing was successful.
                        }
                        else
                        {
                            err = "There is an issue with the options file: you have incorrectly specified the seat count for one of your RESERVE lines. " +
                                "You either chose an invalid number or specified something other than a number.";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (reserveSeatCount <= 0)
                        {
                            err = "There is an issue with the options file: you specified a RESERVE line with a seat count of 0 or less... why?";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (reserveProductName.Contains('"'))
                        {
                            reserveProductName = reserveProductName.Replace("\"", "");
                            reserveLicenseNumber = lineParts[3];
                            if (!reserveProductName.Contains(':'))
                            {
                                if (reserveLicenseNumber.Contains("key="))
                                {
                                    reserveProductKey = lineParts[3];
                                    string unfixedReserveProductKey = reserveProductKey;
                                    string quotedReserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                    reserveProductKey = quotedReserveProductKey.Replace("\"", "");
                                    reserveLicenseNumber = string.Empty;
                                }
                                // asset_info=
                                else
                                {
                                    string unfixedReserveLicenseNumber = reserveLicenseNumber;
                                    string quoteReserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                    reserveLicenseNumber = quoteReserveLicenseNumber.Replace("\"", "");
                                    reserveProductKey = string.Empty;
                                }

                                reserveClientType = lineParts[4];
                                reserveClientSpecified = string.Join(" ", lineParts.Skip(5)).TrimEnd();
                            }
                            // If you have " and :
                            else
                            {
                                string[] colonParts = reserveProductName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    err = $"There is an issue with the options file: one of your RESERVE lines has a stray colon for {reserveProductName}.";
                                    return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                                }
                                reserveProductName = colonParts[0];
                                if (colonParts[1].Contains("key="))
                                {
                                    string unfixedReserveProductKey = colonParts[1];
                                    reserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                    reserveLicenseNumber = string.Empty;
                                }
                                else
                                {
                                    string unfixedReserveLicenseNumber = colonParts[1];
                                    reserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase); reserveProductKey = string.Empty;
                                }
                                reserveClientType = lineParts[3];
                                reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                        }
                        // In case you decided to use a : instead of ""...
                        else if (reserveProductName.Contains(':'))
                        {
                            string[] colonParts = reserveProductName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                err = $"There is an issue with the options file: one of your RESERVE lines has a stray colon for {reserveProductName}.";
                                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                            reserveProductName = colonParts[0];
                            if (colonParts[1].Contains("key="))
                            {
                                string unfixedReserveProductKey = colonParts[1];
                                reserveProductKey = unfixedReserveProductKey.Replace("key=", "");
                                reserveLicenseNumber = string.Empty;
                            }
                            else
                            {
                                string unfixedReserveLicenseNumber = colonParts[1];
                                reserveLicenseNumber = Regex.Replace(unfixedReserveLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase); reserveProductKey = string.Empty;
                            }
                            reserveClientType = lineParts[3];
                            reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        else
                        {
                            reserveClientType = lineParts[3];
                            reserveClientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            reserveLicenseNumber = string.Empty;
                            reserveProductKey = string.Empty;
                        }
                        reserveDictionary[optionsLineIndex] = Tuple.Create(reserveSeatCount, reserveProductName, reserveLicenseNumber, reserveProductKey, reserveClientType, reserveClientSpecified);
                    }
                    else if (line.TrimStart().StartsWith("GROUP "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 3)
                        {
                            err = "There is an issue with the options file: you have an incorrectly formatted GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        string groupName = lineParts[1];
                        string groupUsers = string.Join(" ", lineParts.Skip(2)).TrimEnd();
                        int groupUserCount = groupUsers.Split(' ').Length;

                        groupDictionary[optionsLineIndex] = Tuple.Create(groupName, groupUsers, groupUserCount);
                    }
                    else if (line.TrimStart().StartsWith("HOST_GROUP "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        if (lineParts.Length < 3)
                        {
                            err = "There is an issue with the options file: you have an incorrectly formatted HOST_GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        string hostGroupName = lineParts[1];
                        string hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                        hostGroupDictionary[optionsLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);
                    }
                    else if (line.TrimStart().StartsWith("GROUPCASEINSENSITIVE ON"))
                    {
                        caseSensitivity = false;
                    }                    
                    else if (line.TrimStart().StartsWith("TIMEOUTALL ") || line.TrimStart().StartsWith("DEBUGLOG ") || line.TrimStart().StartsWith("LINGER ") || line.TrimStart().StartsWith("MAX_OVERDRAFT ") 
                        || line.TrimStart().StartsWith("REPORTLOG ") || line.TrimStart().StartsWith("TIMEOUT ") || line.TrimStart().StartsWith("BORROW ") || line.TrimStart().StartsWith("NOLOG ") 
                        || line.TrimStart().StartsWith("DEFAULT ") || line.TrimStart().StartsWith("HIDDEN ") || line.TrimStart().StartsWith("#") || line == "")
                    {
                        // Other valid line beginnings that I currently do nothing with.
                    }
                    else // This should help spot my stupid typos.
                    {
                        err = "There is an issue with the options file: you have started a line with an unrecognized option. Please make sure you didn't make any typos. " +
                            $"The line in question's contents are: \"{line}\".";
                        return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }
                }

                return (serverLineHasPort, daemonLineHasPort, daemonPortIsCNUFriendly, caseSensitivity, licenseFileDictionary, includeDictionary, includeBorrowDictionary, includeAllDictionary, excludeDictionary, excludeBorrowDictionary, excludeAllDictionary, reserveDictionary, maxDictionary, groupDictionary, hostGroupDictionary, null);
            }
            catch (Exception ex)
            {
                if (ex.Message == "The value cannot be an empty string. (Parameter 'path')")
                {
                    err = "You left the license or options file text field blank.";
                }
                else if (ex.Message == "Index was outside the bounds of the array.")
                {
                    err = $"There is a formatting issue in your license/options file. This is the line in question's contents: \"{line}\"";
                }
                else
                {
                    err = $"You managed to break something. How? Here's the automatic message: {ex.Message}";
                }

                return (false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }
        }
    }
}
