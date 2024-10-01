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

        [GeneratedRegex(@"^[^Rab_\d]+$")]
        private static partial Regex LicenseNumber();

        private static bool _serverLineHasPort = true;
        private static bool _daemonLineHasPort = false;
        private static bool _daemonPortIsCnuFriendly = false;
        private static bool _caseSensitivity = true;
        private static bool _optionsFileUsesMatlabParallelServer = false;
        private static bool _wildcardsAreUsed = false;
        private static bool _ipAddressesAreUsed = false;
        private static readonly Dictionary<int, Tuple<string, int, string, string, string, List<string>, int>> licenseFileDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string, string>> includeDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> includeBorrowDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string>> includeAllDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> excludeDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, string, string, string>> excludeBorrowDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string>> excludeAllDictionary = [];
        private static readonly Dictionary<int, Tuple<int, string, string, string, string, string, string>> reserveDictionary = [];
        private static readonly Dictionary<string, Tuple<int, string, string>> maxDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string, int>> groupDictionary = [];
        private static readonly Dictionary<int, Tuple<string, string>> hostGroupDictionary = [];
        private static string? _err = string.Empty;
        
        // Putting this here so that they can be printed in try-catch error messages.
        private static string? productExpirationDate;

        public static (
            bool serverLineHasPort,
            bool daemonLineHasPort,
            bool daemonPortIsCNUFriendly,
            bool caseSensitivity,
            bool optionsFileUsesMatlabParallelServer,
            bool wildcardsAreUsed,
            bool ipAddressesAreUsed,
            Dictionary<int, Tuple<string, int, string, string, string, List<string>, int>>? licenseFileDictionary,
            Dictionary<int, Tuple<string, string, string, string, string, string>>? includeDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? includeBorrowDictionary,
            Dictionary<int, Tuple<string, string, string>>? includeAllDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? excludeDictionary,
            Dictionary<int, Tuple<string, string, string, string, string>>? excludeBorrowDictionary,
            Dictionary<int, Tuple<string, string>>? excludeAllDictionary,
            Dictionary<int, Tuple<int, string, string, string, string, string, string>>? reserveDictionary,
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

                // Remove Windows line breaks.
                string lineBreaksToRemove = "\\\r\n\t";
                licenseFileContents = licenseFileContents.Replace(lineBreaksToRemove, string.Empty);
                optionsFileContents = optionsFileContents.Replace(lineBreaksToRemove, string.Empty);

                // Remove Unix line breaks.
                lineBreaksToRemove = "\\\r\n";
                licenseFileContents = licenseFileContents.Replace(lineBreaksToRemove, string.Empty);
                optionsFileContents = optionsFileContents.Replace(lineBreaksToRemove, string.Empty);

                // Remove more Unix line breaks...
                lineBreaksToRemove = "\\\n\t";
                licenseFileContents = licenseFileContents.Replace(lineBreaksToRemove, string.Empty);
                optionsFileContents = optionsFileContents.Replace(lineBreaksToRemove, string.Empty);

                // Remove empty space that will likely appear on Unix systems.
                string emptySpaceToRemove = "        ";
                licenseFileContents = licenseFileContents.Replace(emptySpaceToRemove, string.Empty);

                // ...just in case...
                lineBreaksToRemove = "\\\n";
                licenseFileContents = licenseFileContents.Replace(lineBreaksToRemove, string.Empty);

                // Put it back together!
                licenseFileContentsLines = licenseFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                optionsFileContentsLines = optionsFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Next, let's check for some obvious errors.
                // Make sure, you know, the files exist.
                if (!System.IO.File.Exists(licenseFilePath) || !System.IO.File.Exists(optionsFilePath))
                {
                    _err = "The license and/or the options file you selected either no longer exists or you don't have permissions to read one of them.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                // Error time again, in case you decided to be sneaky and close the program or manually enter the filepath.
                if (string.IsNullOrWhiteSpace(optionsFileContents))
                {
                    _err = "There is an issue with the license file: it is either empty or only contains white space.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                // Make sure you actually picked an options file.
                if (!optionsFileContents.Contains("INCLUDE") && !optionsFileContents.Contains("EXCLUDE") && !optionsFileContents.Contains("RESERVE")
                    && !optionsFileContents.Contains("MAX") && !optionsFileContents.Contains("LINGER") && !optionsFileContents.Contains("LOG") &&
                    !optionsFileContents.Contains("TIMEOUT"))
                {
                    _err = "There is an issue with the options file: it is likely not an options file or contains no usable content.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                if (!System.IO.File.ReadAllText(licenseFilePath).Contains("INCREMENT"))
                {
                    _err = "There is an issue with the license file: it is either not a license file or is corrupted.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                if (licenseFileContents.Contains("lo=IN") || licenseFileContents.Contains("lo=DC") || licenseFileContents.Contains("lo=CIN"))
                {
                    _err = "There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                        "which cannot use an options file.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                if (System.IO.File.ReadAllText(licenseFilePath).Contains("CONTRACT_ID="))
                {
                    _err = "There is an issue with the license file: it contains at least 1 non-MathWorks product.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                }

                // Reset everything!
                _serverLineHasPort = true;
                _daemonLineHasPort = false;
                _daemonPortIsCnuFriendly = false;
                _caseSensitivity = false;
                _optionsFileUsesMatlabParallelServer = false;
                _wildcardsAreUsed = false;
                _ipAddressesAreUsed = false;
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
                _err = string.Empty;

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
                            _err = "There is an issue with the license file: your SERVER line(s) are listed after a product.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }
                        serverLineCount++;

                        string[] lineParts = line.Split(' ');
                        string serverWord = lineParts[0];
                        string serverHostID = lineParts[2];

                        if (serverHostID == "27000" || serverHostID == "27001" || serverHostID == "27002" || serverHostID == "27003" || serverHostID == "27004" || serverHostID == "27005")
                        {
                            _err = "There is an issue with the license file: you have likely omitted your Host ID and attempted to specify a SERVER port number. " +
                                "Because you have omitted the Host ID, the port you've attempted to specify will not be used.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (lineParts.Length < 3)
                        {
                            _err = "There is an issue with the license file: you are missing information from your SERVER line.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }
                        else if (lineParts.Length == 3)
                        {
                            _serverLineHasPort = false;
                        }
                        else if (lineParts.Length == 4)
                        {
                            // Check to make sure you're using a port number.
                            if (!int.TryParse(lineParts[3], out int serverPort))
                            {
                                _err = "There is an issue with the license file: you have stray information on your SERVER line.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }

                            if (serverWord != "SERVER")
                            {
                                _err = "There is an issue with the license file: it does not start with the word SERVER.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }

                            if (!serverHostID.Contains("INTERNET=") && serverHostID.Length != 12)
                            {
                                _err = "There is an issue with the license file: you have not specified your Host ID correctly.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                _err = "There is an issue with the license file: you have stray information on your SERVER line.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }
                        }
                        else
                        {
                            _err = "There is an issue with the license file: you have stray information on your SERVER line.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }
                    }
                    else if (line.TrimStart().StartsWith("DAEMON") || line.TrimStart().StartsWith("VENDOR"))
                    {
                        // DAEMON line should come before the product(s).
                        if (productLinesHaveBeenReached)
                        {
                            _err = "There is an issue with the license file: your DAEMON line is listed after a product.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // There should only be one DAEMON line.
                        daemonLineCount++;
                        if (daemonLineCount > 1)
                        {
                            _err = "There is an issue with the license file: you have more than 1 DAEMON line.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // port= and options= should only appear once.
                        int countPortEquals = countPortEqualsRegex().Matches(line).Count;
                        int countOptionsEquals = LicenseAndOptionsFileDataGatherer.countOptionsEquals().Matches(line).Count;
                        int countCommentedBeginLines = LicenseAndOptionsFileDataGatherer.countCommentedBeginLines().Matches(line).Count;

                        // For the CNU kids.
                        if (line.Contains("PORT="))
                        {
                            _daemonPortIsCnuFriendly = true;
                        }

                        if (countCommentedBeginLines > 0)
                        {
                            _err = "There is an issue with the license file: it has content that is intended to be commented out in your DAEMON line.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (countPortEquals > 1)
                        {
                            _err = "There is an issue with the license file: you have specified more than 1 port number for MLM.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (countOptionsEquals > 1)
                        {
                            _err = "There is an issue with the license file: you have specified the path to more than 1 options file.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (countOptionsEquals == 0)
                        {
                            _err = "There is an issue with the license file: you did not specify the path to the options file. " +
                                "If you included the path, but did not use options= to specify it, MathWorks licenses ask that you do so, even if they technically work without options=.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // daemonProperty1 and 2 could either be a port number or path to an options file.
                        string[] lineParts = line.Split(' ');

                        // Just having the word "DAEMON" isn't enough.
                        if (lineParts.Length == 1)
                        {
                            _err = "There is an issue with the license file: you have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Checking for the vendor daemon MLM.
                        string daemonVendor = lineParts[1];

                        if (string.IsNullOrWhiteSpace(daemonVendor))
                        {
                            _err = "There is an issue with the license file: there are too many spaces between \"DAEMON\" and \"MLM\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // The vendor daemon needs to MLM. Not mlm or anything else.
                        if (daemonVendor != "MLM")
                        {
                            _err = "There is an issue with the license file: you have incorrectly specified the vendor daemon MLM.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Just specifying "DAEMON MLM" isn't enough.
                        if (lineParts.Length == 2)
                        {
                            _err = "There is an issue with the license file: you did not specify the path to the vendor daemon MLM.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // You're missing your options file path.
                        if (lineParts.Length == 3)
                        {
                            _err = "There is an issue with the license file: you did not specify the path to the options file.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (countPortEquals == 1)
                        {
                            _daemonLineHasPort = true;
                        }
                    }
                    // Where the product information is found.
                    else if (line.TrimStart().StartsWith("INCREMENT"))
                    {
                        productLinesHaveBeenReached = true;
                        string[] lineParts = line.Split(' ');
                        productName = lineParts[1];
                        int productVersion = int.Parse(lineParts[3]);
                        productExpirationDate = lineParts[4];
                        string productKey = lineParts[6];
                        string licenseOffering = string.Empty;
                        string licenseNumber = string.Empty;
                        _ = int.TryParse(lineParts[5], out seatCount);
                        string rawSeatCount = lineParts[5];

                        // Product Key validation.
                        if (productKey.Length > 20)
                        {
                            _err = "There is an issue with the license file: one of your product keys is greater that 20 characters long. This means it's likely been " +
                            $"tampered with. This is what the product key is being read as: {productKey}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

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
                            if (string.IsNullOrWhiteSpace(licenseNumber))
                            {
                                _err = $"There is an issue with the license file: the license number was not found for the product {productName} with the product key {productKey}. " +
                                "Your license file has likely been tampered with. Please regenate it for this product before proceeding.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }
                            _err = $"There is an issue with the license file: the license number {licenseNumber} was not found for the product {productName}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                    _err = "There is an issue with the license file: it is formatted incorrectly. " +
                                        $"{productName}'s license offering is being read as Total Headcount, but also Network Named User, which doesn't exist.";
                                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                                }
                            }
                            else
                            {
                                _err = $"There is an issue with the license file: the product {productName} has an invalid license offering.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                _err = $"There is an issue with the license file: the product {productName} comes from an Individual " +
                                    "or Designated Computer license, which cannot use an options file.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }
                        }
                        else
                        {
                            if (line.Contains("PLATFORMS=x"))
                            {
                                _err = $"There is an issue with the license file: the product {productName} comes from an Individual " +
                                     "or Designated Computer license generated from a PLP on Windows, which cannot use an options file.";
                            }
                            else
                            {
                                _err = $"There is an issue with the license file: the product {productName} has an valid license offering.";
                            }
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                            _err = $"There is an issue with the license file: The product {productName} on license number " +
                                $"{licenseNumber} expired on {productExpirationDate}. Please update your license file appropriately before proceeding.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (licenseOffering.Contains("NNU"))
                        {
                            if (seatCount != 1 && !containsPLP)
                            {
                                seatCount /= 2;
                            }
                        }

                        if (licenseOffering == "lo=CN" && (seatCount == 0) && licenseNumber == "220668")
                        {
                            if ((productVersion <= 18) || (productName.Contains("Polyspace") && productVersion <= 22))
                            {
                                _err = $"There is an issue with the license file: it contains a Designated Computer or Individual license, {licenseNumber}.";
                            }
                            else
                            {
                                _err = $"There is an issue with the license file: it contains a Designated Computer license, {licenseNumber}, " +
                                    "that is incorrectly labeled as a Concurrent license.";
                            }
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (rawSeatCount == "uncounted")
                        {
                            _err = "There is an issue with the license file: it contains an Individual or Designated Computer license, " +
                                $"which cannot use an options file. The license number is question is {licenseNumber}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (seatCount < 1 && line.Contains("asset_info="))
                        {
                            _err = $"There is an issue with the license file: {productName} on license {licenseNumber} is reading with a seat count of zero or less.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Before proceeding, make sure the values we've collected are valid.
                        if (string.IsNullOrWhiteSpace(productName))
                        {
                            _err = $"There is an issue with the license file: a product name is being detected as blank on {licenseNumber}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (string.IsNullOrWhiteSpace(licenseNumber) || LicenseNumber().IsMatch(licenseNumber))
                        {
                            if (licenseNumber == "DEMO")
                            {
                                _err = $"There is an issue with the license file: an invalid license number is detected for your trial license of {productName}. " +
                                "Your trial license file has likely been tampered with. Please regenerate it before proceeding.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                            }
                            _err = $"There is an issue with the license file: an invalid license number, {licenseNumber}, is detected for {productName}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (string.IsNullOrWhiteSpace(licenseOffering))
                        {
                            _err = $"There is an issue with the license file: a license offering could not be detected for {productName} " +
                                $"on license number {licenseNumber}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (string.IsNullOrWhiteSpace(productKey))
                        {
                            _err = $"There is an issue with the license file: a product key could not be detected for {productName} on license number {licenseNumber}.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (productName == "MATLAB_Distrib_Comp_Engine" && licenseOffering == "NNU")
                        {
                            _err = "There is an issue with the license file: you have a license for MATLAB Parallel Server, but it is being registered as part of an NNU license, which is not possible. " +
                            "Please regenerate your MATLAB Parallel Server license and then replace it with the existing contents in your license file.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        List<string> linesThatSubtractSeats = []; // This needs to be set here even if it's just going to be empty until the analyzer class potentially fills it.
                        int originalLicenseFileSeatCount = seatCount;
                        licenseFileDictionary[licenseLineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber, linesThatSubtractSeats, originalLicenseFileSeatCount);
                    }
                    else if (line.TrimStart().StartsWith('#') || string.IsNullOrWhiteSpace(line)) { } // Ignore empty and commented out lines.
                    else if (line.TrimStart().StartsWith("USE_SERVER"))
                    {
                        // # Add some code to complain about this.
                    }
                    else
                    {
                        _err = "There is an issue with the license file: it has an unrecognized line. You likely manually edited the license file and likely need to regenerate it. " +
                        $"The lines contents are the following: {line}";
                        return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                    }
                }

                // There is no situation where you should have more than 3 SERVER lines.
                if (serverLineCount is > 3 or 2)
                {
                    _err = "There is an issue with the license file: it has too many SERVER lines. Only 1 or 3 are accepted.";
                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                            _err = $"There is an issue with the options file: you have an incorrectly formatted {optionType} line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                _err = $"There is an issue with the options file: one of your {optionType} lines has a stray quotation mark. " +
                                    $"The line in question reads as this: {line}";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                    _err = $"There is an issue with the options file: you have incorrectly specified the client type on a line using {optionType}." +
                                        $"You attempted to use \"{clientType}\". Please reformat this {optionType} line.";
                                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                                }

                                clientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                            }
                            else // If you have " and :
                            {
                                string[] colonParts = productName.Split(":");
                                if (colonParts.Length != 2)
                                {
                                    _err = $"There is an issue with the options file: one of your {optionType} lines has a stray colon for {productName}.";
                                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                    licenseNumber = AssetInfo().Replace(unfixedLicenseNumber, "");
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
                                _err = $"There is an issue with the options file: one of your {optionType} lines has a stray colon for {productName}.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                licenseNumber = AssetInfo().Replace(unfixedLicenseNumber, "");
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

                        // Validate your clientType.
                        if (clientType != "USER" && clientType != "HOST" && clientType != "DISPLAY" && clientType != "GROUP" && clientType != "HOST_GROUP" && clientType != "INTERNET")
                        {
                            _err = "There is an issue with the options file: you have an incorrectly formatted client type. It would typically be something like USER or GROUP, but yours is being detected " +
                            $"as {clientType}. Please make sure you have formatted the line with client type correctly. The line in question reads as \"{line}\"";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Check for wildcards and IP addresses.
                        if (clientSpecified.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(clientSpecified, ipAddressPattern)) { _ipAddressesAreUsed = true; }

                        // Listen, we all have bad ideas.
                        if (productName == "MATLAB_Distrib_Comp_Engine") { _optionsFileUsesMatlabParallelServer = true; }

                        if (line.TrimStart().StartsWith("INCLUDE ")) { includeDictionary[optionsLineIndex] = Tuple.Create(productName, licenseNumber, productKey, clientType, clientSpecified, line); }
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
                            _err = $"There is an issue with the options file: you have an incorrectly formatted {optionSpecified} line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        clientType = lineParts[1];
                        clientSpecified = string.Join(" ", lineParts.Skip(2));

                        if (clientType != "USER" && clientType != "GROUP" && clientType != "HOST" && clientType != "HOST_GROUP" && clientType != "DISPLAY" &&
                            clientType != "PROJECT" && clientType != "INTERNET")
                        {
                            _err = $"There is an issue with the options file: you have incorrectly specified the client type on an {optionSpecified} " +
                                $"line as \"{clientType}\". Please reformat this {optionSpecified} line's client type to something such as \"USER\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Check for wildcards and IP addresses.
                        if (clientSpecified.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(clientSpecified, ipAddressPattern)) { _ipAddressesAreUsed = true; }

                        if (line.TrimStart().StartsWith("INCLUDEALL ")) { includeAllDictionary[optionsLineIndex] = Tuple.Create(clientType, clientSpecified, line); }
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
                            _err = "There is an issue with the options file: you have an incorrectly formatted MAX line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        int maxSeats = int.Parse(lineParts[1]);
                        string maxProductName = lineParts[2];
                        string maxClientType = lineParts[3];
                        string maxClientSpecified = string.Join(" ", lineParts.Skip(4));

                        // Check for wildcards and IP addresses.
                        if (maxClientSpecified.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(maxClientSpecified, ipAddressPattern)) { _ipAddressesAreUsed = true; }

                        if (maxProductName == "MATLAB_Distrib_Comp_Engine") { _optionsFileUsesMatlabParallelServer = true; }

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
                            _err = "There is an issue with the options file: you have an incorrectly formatted RESERVE line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Check for stray quotation marks.
                        int quoteCount = line.Count(c => c == '"');
                        if (quoteCount % 2 != 0)
                        {
                            _err = "There is an issue with the options file: one of your RESERVE lines has a stray quotation mark. " +
                                $"The line in question reads as this: {line}";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                            _err = "There is an issue with the options file: you have incorrectly specified the seat count for one of your RESERVE lines. " +
                                "You either chose an invalid number or specified something other than a number.";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        if (reserveSeatCount <= 0)
                        {
                            _err = "There is an issue with the options file: you specified a RESERVE line with a seat count of 0 or less... why?";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                    _err = $"There is an issue with the options file: one of your RESERVE lines has a stray colon for {reserveProductName}.";
                                    return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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
                                _err = $"There is an issue with the options file: one of your RESERVE lines has a stray colon for {reserveProductName}.";
                                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
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

                        // Check for wildcards and IP addresses.
                        if (reserveClientSpecified.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(reserveClientSpecified, ipAddressPattern)) { _ipAddressesAreUsed = true; }

                        if (reserveProductName == "MATLAB_Distrib_Comp_Engine") { _optionsFileUsesMatlabParallelServer = true; }

                        reserveDictionary[optionsLineIndex] = Tuple.Create(reserveSeatCount, reserveProductName, reserveLicenseNumber, reserveProductKey, reserveClientType, reserveClientSpecified, line);
                    }
                    else if (line.TrimStart().StartsWith("GROUP "))
                    {
                        string[] lineParts = line.Split(' ');

                        // Stop putting in random spaces.
                        while (string.IsNullOrWhiteSpace(lineParts[0]) && lineParts.Length > 1)
                        {
                            lineParts = lineParts.Skip(1).ToArray();
                        }

                        // # Add some code that compensates for empty GROUP lines that ALSO have a duplicate GROUP name with non-empty lines...
                        if (lineParts.Length < 3)
                        {
                            _err = "There is an issue with the options file: you have an incorrectly formatted GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        // Remove any elements after lineParts[2] that are blank, whitespace, or tabs
                        lineParts = lineParts.Take(3)
                                             .Concat(lineParts.Skip(3).Where(part => !string.IsNullOrWhiteSpace(part)))
                                             .ToArray();

                        string groupName = lineParts[1];
                        string groupUsers = string.Join(" ", lineParts.Skip(2)).TrimEnd();
                        int groupUserCount = groupUsers.Split(' ').Length;

                        // Check if the groupName already exists in the dictionary. If it does, we want to combine them, since this is what FlexLM does.
                        var existingEntry = groupDictionary.Values.FirstOrDefault(g => g.Item1 == groupName);
                        if (existingEntry != null)
                        {
                            string combinedUsers = existingEntry.Item2 + " " + groupUsers;
                            int combinedUserCount = existingEntry.Item3 + groupUserCount;

                            // Find the key of the existing entry.
                            var existingKey = groupDictionary.FirstOrDefault(x => x.Value.Item1 == groupName).Key;

                            // Update the existing entry in the dictionary.
                            groupDictionary[existingKey] = Tuple.Create(groupName, combinedUsers, combinedUserCount);
                        }
                        else
                        {
                            // Otherwise, just proceed as usual.
                            groupDictionary[optionsLineIndex] = Tuple.Create(groupName, groupUsers, groupUserCount);
                        }

                        // Check for wildcards and IP addresses.
                        if (groupUsers.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(groupUsers, ipAddressPattern)) { _ipAddressesAreUsed = true; }
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
                            _err = "There is an issue with the options file: you have an incorrectly formatted HOST_GROUP line. It is missing necessary information. " +
                                $"The line in question is \"{line}\".";
                            return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                        }

                        string hostGroupName = lineParts[1];
                        string hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2)).TrimEnd();

                        // Same as GROUP, combine HOST_GROUPs with duplicate names and treat them as one.
                        var existingEntry = hostGroupDictionary.Values.FirstOrDefault(g => g.Item1 == hostGroupName);
                        if (existingEntry != null)
                        {
                            string combinedUsers = existingEntry.Item2 + " " + hostGroupClientSpecified;
                            var existingKey = hostGroupDictionary.FirstOrDefault(x => x.Value.Item1 == hostGroupName).Key;
                            hostGroupDictionary[existingKey] = Tuple.Create(hostGroupName, combinedUsers);
                        }
                        else
                        {
                            hostGroupDictionary[optionsLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);
                        }

                        // Check for wildcards and IP addresses.
                        if (hostGroupClientSpecified.Contains('*')) { _wildcardsAreUsed = true; }

                        string ipAddressPattern = @"\d{2,3}\."; // I'll assume your IP addresses are something like ##. and/or ###.
                        if (Regex.IsMatch(hostGroupClientSpecified, ipAddressPattern)) { _ipAddressesAreUsed = true; }
                    }
                    else if (line.TrimStart().StartsWith("GROUPCASEINSENSITIVE ON"))
                    {
                        _caseSensitivity = false;
                    }
                    else if (line.TrimStart().StartsWith("TIMEOUTALL ") || line.TrimStart().StartsWith("DEBUGLOG ") || line.TrimStart().StartsWith("LINGER ") || line.TrimStart().StartsWith("MAX_OVERDRAFT ")
                        || line.TrimStart().StartsWith("REPORTLOG ") || line.TrimStart().StartsWith("TIMEOUT ") || line.TrimStart().StartsWith("BORROW ") || line.TrimStart().StartsWith("NOLOG ")
                        || line.TrimStart().StartsWith("DEFAULT ") || line.TrimStart().StartsWith("HIDDEN ") || line.TrimStart().StartsWith("MAX_BORROW_HOURS") || line.TrimStart().StartsWith('#') 
                        || string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("BORROW_LOWWATER "))
                    {
                        // Other valid line beginnings that I currently do nothing with.
                    }
                    else // This should help spot my stupid typos.
                    {
                        _err = "There is an issue with the options file: you have started a line with an unrecognized option. Please make sure you didn't make any typos. " +
                            $"The line in question's contents are: \"{line}\".";
                        return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
                    }
                }

                return (_serverLineHasPort, _daemonLineHasPort, _daemonPortIsCnuFriendly, _caseSensitivity, _optionsFileUsesMatlabParallelServer, _wildcardsAreUsed, _ipAddressesAreUsed, licenseFileDictionary, includeDictionary, includeBorrowDictionary, includeAllDictionary, excludeDictionary, excludeBorrowDictionary, excludeAllDictionary, reserveDictionary, maxDictionary, groupDictionary, hostGroupDictionary, null);
            }
            catch (Exception ex)
            {
                if (ex.Message == "The value cannot be an empty string. (Parameter 'path')")
                {
                    _err = "You left the license or options file text field blank.";
                }
                else if (ex.Message == "Index was outside the bounds of the array.")
                {
                    _err = $"There is a formatting issue in your license/options file. This is the line in question's contents: \"{line}\"";
                }
                else if (ex.Message.Contains("is not supported in calendar 'System.Globalization.GregorianCalendar'."))
                {
                    _err = $"There is an issue with your license file: it contains a date not recognized in the Gregorian Calendar: {productExpirationDate}. Please regenerate your license file.";
                }
                else
                {
                    _err = $"You managed to break something. How? Here's the automatic message: {ex.Message}";
                }

                return (false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, _err);
            }
        }
    }
}
