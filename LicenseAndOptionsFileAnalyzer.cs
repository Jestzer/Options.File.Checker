using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Options.File.Checker.WPF
{
    public partial class LicenseAndOptionsFileAnalyzer
    {
        // Do Regex stuff now to be efficient and stuff blah blah blah.
        [GeneratedRegex("port=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countPortEqualsRegex();
        [GeneratedRegex("options=", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countOptionsEquals();
        [GeneratedRegex("# BEGIN--------------", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex countCommentedBeginLines();

        private static bool serverLineHasPort = true;
        private static bool daemonLineHasPort = false;
        private static bool cnuIsUsed = false;
        private static Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary = [];
        private static Dictionary<int, Tuple<string, string, string, string, string>> includeDictionary = [];
        private static Dictionary<int, Tuple<string, string, string, string, string>> includeBorrowDictionary = [];
        private static Dictionary<int, Tuple<string, string>> includeAllDictionary = [];
        private static Dictionary<int, Tuple<string, string, string, string, string>> excludeDictionary = [];
        private static Dictionary<int, Tuple<string, string, string, string, string>> excludeBorrowDictionary = [];
        private static Dictionary<int, Tuple<string, string>> excludeAllDictionary = [];
        private static Dictionary<int, Tuple<int, string, string, string, string, string>> reserveDictionary = [];
        private static Dictionary<string, Tuple<int, string, string>> maxDictionary = [];
        private static Dictionary<int, Tuple<string, string, int>> groupDictionary = [];
        private static Dictionary<int, Tuple<string, string>> hostGroupDictionary = [];
        private static string? err = string.Empty;

        public static (
            bool serverLineHasPort,
            bool daemonLineHasPort,
            bool cnuIsUsed,
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

            AnalyzeFiles(string optionsFilePath, string licenseFilePath)
        {
            // Reset everything!
            serverLineHasPort = true;
            daemonLineHasPort = false;
            cnuIsUsed = false;
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

            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(licenseFilePath);
            string[] optionsFileContentsLines = System.IO.File.ReadAllLines(optionsFilePath);

            // Remove the line breaks to make life easier.
            licenseFileContentsLines = licenseFileContentsLines
                .Select(line => line.Replace("\r", "").Replace("\n", "").Replace("\t", ""))
                .ToArray();

            // License file information gathering.
            for (int licenseLineIndex = 0; licenseLineIndex < licenseFileContentsLines.Length; licenseLineIndex++)
            {
                string line = licenseFileContentsLines[licenseLineIndex];
                string productName = string.Empty;
                int seatCount = 0;
                string plpLicenseNumber = string.Empty;

                if (line.TrimStart().StartsWith("SERVER"))
                {
                    // SERVER lines should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        err = "There is an issue with the selected license file: your SERVER line(s) are listed after a product.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }
                    serverLineCount++;

                    string[] lineParts = line.Split(' ');
                    string serverWord = lineParts[0];
                    string serverHostID = lineParts[2];

                    if (lineParts.Length < 3)
                    {
                        err = "There is an issue with the selected license file: you are missing information from your SERVER line.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                            err = "There is an issue with the selected license file: you have stray information on your SERVER line.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (serverWord != "SERVER")
                        {
                            err = "There is an issue with the selected license file: it does not start with the word SERVER.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (!serverHostID.Contains("INTERNET=") && serverHostID.Length != 12)
                        {
                            err = "There is an issue with the selected license file: you have not specified your Host ID correctly.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Congrats, you /may/ have not made any mistakes on your SERVER line.
                    }
                    else
                    {
                        err = "There is an issue with the selected license file: you have stray information on your SERVER line.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // There is no situation where you should have more than 3 SERVER lines.
                    if (serverLineCount > 3 || serverLineCount == 2)
                    {
                        err = "There is an issue with the selected license file: it has too many SERVER lines. Only 1 or 3 are accepted.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                }
                else if (line.TrimStart().StartsWith("DAEMON"))
                {
                    // DAEMON line should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        err = "There is an issue with the selected license file: your DAEMON line is listed after a product.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // There should only be one DAEMON line.
                    daemonLineCount++;
                    if (daemonLineCount > 1)
                    {
                        err = "There is an issue with the selected license file: you have more than 1 DAEMON line.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // port= and options= should only appear once.
                    int countPortEquals = countPortEqualsRegex().Matches(line).Count;
                    int countOptionsEquals = LicenseAndOptionsFileAnalyzer.countOptionsEquals().Matches(line).Count;
                    int countCommentedBeginLines = LicenseAndOptionsFileAnalyzer.countCommentedBeginLines().Matches(line).Count;

                    if (countCommentedBeginLines > 0)
                    {
                        err = "There is an issue with the selected license file: it has content that is intended to be commented out in your DAEMON line.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (countPortEquals > 1)
                    {
                        err = "There is an issue with the selected license file: you have specified more than 1 port number for MLM.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (countOptionsEquals > 1)
                    {
                        err = "There is an issue with the selected license file: you have specified the path to more than 1 options file.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (countOptionsEquals == 0)
                    {
                        err = "There is an issue with the selected license file: you did not specify the path to the options file. " +
                            "If you included the path, but did not use options= to specify it, MathWorks licenses ask that you do so, even if they technically work without options=.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // daemonProperty1 and 2 could either be a port number or path to an options file.
                    string[] lineParts = line.Split(' ');

                    // Just having the word "DAEMON" isn't enough.
                    if (lineParts.Length == 1)
                    {
                        err = "There is an issue with the selected license file: you have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // Checking for the vendor daemon MLM.
                    string daemonVendor = lineParts[1];

                    if (string.IsNullOrWhiteSpace(daemonVendor))
                    {
                        err = "There is an issue with the selected license file: there are too many spaces between \"DAEMON\" and \"MLM\".";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // The vendor daemon needs to MLM. Not mlm or anything else.
                    if (daemonVendor != "MLM")
                    {
                        err = "There is an issue with the selected license file: you have incorrectly specified the vendor daemon MLM.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // Just specifying "DAEMON MLM" isn't enough.
                    if (lineParts.Length == 2)
                    {
                        err = "There is an issue with the selected license file: you did not specify the path to the vendor daemon MLM.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // You're missing your options file path.
                    if (lineParts.Length == 3)
                    {
                        err = "There is an issue with the selected license file: you did not specify the path to the options file.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (countPortEquals != 1)
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
                        Regex regex = new Regex(pattern);
                        Match match = regex.Match(line);

                        if (match.Success)
                        {
                            licenseNumber = match.Groups[1].Value;
                        }
                    }
                    else if (line.Contains("SN="))
                    {
                        pattern = @"SN=([^\s]+)";
                        Regex regex = new Regex(pattern);
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
                        err = $"There is an issue with the selected license file: the license number was not found for the product {productName}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                                err = "There is an issue with the selected license file: it is formatted incorrectly. " +
                                    $"{productName}'s license offering is being read as Total Headcount, but also Network Named User, which doesn't exist.";
                                return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                        else
                        {
                            err = $"There is an issue with the selected license file: the product {productName} has an invalid license offering.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                            err = $"There is an issue with the selected license file: the product {productName} comes from an Individual " +
                                "or Designated Computer license, which cannot use an options file.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    else
                    {
                        if (line.Contains("PLATFORMS=x"))
                        {
                            err = $"There is an issue with the selected license file: the product {productName} comes from an Individual " +
                                 $"or Designated Computer license generated from a PLP on Windows, which cannot use an options file.";
                        }
                        else
                        {
                            err = $"There is an issue with the selected license file: the product {productName} has an valid license offering.";
                        }
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                        err = $"There is an issue with the selected license file: The product {productName} on license number " +
                            $"{licenseNumber} expired on {productExpirationDate}. Please update your license file appropriately before proceeding.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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

                        if (licenseOffering.Contains("CNU"))
                        {
                            cnuIsUsed = true;
                        }
                    }

                    if (licenseOffering == "lo=CN" && (seatCount == 0) && licenseNumber == "220668")
                    {
                        if ((productVersion <= 18) || (productName.Contains("Polyspace") && productVersion <= 22))
                        {
                            err = $"There is an issue with the selected license file: it contains a Designated Computer or Individual license, {licenseNumber}.";
                        }
                        else
                        {
                            err = $"There is an issue with the selected license file: it contains a Designated Computer license, {licenseNumber}, " +
                                "that is incorrectly labeled as a Concurrent license.";
                        }
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (!licenseOffering.Contains("CNU") && rawSeatCount == "uncounted")
                    {
                        err = "There is an issue with the selected license file: it contains an Individual or Designated Computer license, " +
                            $"which cannot use an options file. The license number is question is {licenseNumber}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (seatCount < 1 && line.Contains("asset_info="))
                    {
                        err = $"There is an issue with the selected license file: {productName} on license {licenseNumber} is reading with a seat count of zero or less.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // Before proceeding, make sure the values we've collected are valid.
                    if (string.IsNullOrWhiteSpace(productName))
                    {
                        err = $"There is an issue with the selected license file: a product name is being detected as blank on {licenseNumber}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (licenseNumber.Contains("broken") || string.IsNullOrWhiteSpace(licenseNumber) || Regex.IsMatch(licenseNumber, @"^[^Rab_\d]+$"))
                    {
                        err = $"There is an issue with the selected license file: an invalid license number, {licenseNumber}, is detected for {productName}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (licenseOffering.Contains("broken") || string.IsNullOrWhiteSpace(licenseOffering))
                    {
                        err = $"There is an issue with the selected license file: a license offering could not be detected for {productName} " +
                            $"on license number {licenseNumber}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    if (string.IsNullOrWhiteSpace(productKey))
                    {
                        err = $"There is an issue with the selected license file: a product key could not be detected for {productName} on license number {licenseNumber}.";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    licenseFileDictionary[licenseLineIndex] = Tuple.Create(productName, seatCount, productKey, licenseOffering, licenseNumber);
                }
            }

            // Options file information gathering.
            for (int optionsLineIndex = 0; optionsLineIndex < optionsFileContentsLines.Length; optionsLineIndex++)
            {
                string line = optionsFileContentsLines[optionsLineIndex];

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
                        err = $"There is an issue with the selected options file: you have an incorrectly formatted {optionType} line. It is missing necessary information. " +
                            $"The line in question is \"{line}\".";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                            err = $"There is an issue with the selected options file: one of your {optionType} lines has a stray quotation mark. " +
                                $"The line in question reads as this: {line}";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        productName = productName.Replace("\"", "");
                        licenseNumber = lineParts[2];
                        if (!productName.Contains(':'))
                        {
                            if (licenseNumber.Contains("key="))
                            {
                                productKey = lineParts[2];
                                string unfixedProductKey = productKey;
                                string quotedProductKey = unfixedProductKey.Replace("key=", "");
                                productKey = quotedProductKey.Replace("\"", "");
                                licenseNumber = string.Empty;
                            }
                            // asset_info=
                            else
                            {
                                string unfixedLicenseNumber = licenseNumber;
                                string quoteLicenseNumber = Regex.Replace(unfixedLicenseNumber, "asset_info=", "", RegexOptions.IgnoreCase);
                                licenseNumber = quoteLicenseNumber.Replace("\"", "");
                                productKey = string.Empty;
                            }

                            clientType = lineParts[3];
                            clientSpecified = string.Join(" ", lineParts.Skip(4)).TrimEnd();
                        }
                        // If you have " and :
                        else
                        {
                            string[] colonParts = productName.Split(":");
                            if (colonParts.Length != 2)
                            {
                                err = $"There is an issue with the selected options file: one of your {optionType} lines has a stray colon for {productName}.";
                                return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                    // In case you decided to use a : instead of ""...
                    else if (productName.Contains(':'))
                    {
                        string[] colonParts = productName.Split(":");
                        if (colonParts.Length != 2)
                        {
                            err = $"There is an issue with the selected options file: one of your {optionType} lines has a stray colon for {productName}.";
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
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
                        err = "There is an issue with the selected options file: you have an incorrectly formatted MAX line. It is missing necessary information. " +
                            $"The line in question is \"{line}\".";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    int maxSeats = int.Parse(lineParts[1]);
                    string maxProductName = lineParts[2];
                    string maxClientType = lineParts[3];
                    string maxClientSpecified = string.Join(" ", lineParts.Skip(4));

                    maxDictionary[maxProductName] = Tuple.Create(maxSeats, maxClientType, maxClientSpecified);
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
                        err = "There is an issue with the selected options file: you have an incorrectly formatted HOST_GROUP line. It is missing necessary information. " +
                            $"The line in question is \"{line}\".";
                        return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    string hostGroupName = lineParts[1];
                    string hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                    hostGroupDictionary[optionsLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);
                }
            }
            return (serverLineHasPort, daemonLineHasPort, cnuIsUsed, licenseFileDictionary, includeDictionary, includeBorrowDictionary, includeAllDictionary, excludeDictionary, excludeBorrowDictionary, excludeAllDictionary, reserveDictionary, maxDictionary, groupDictionary, hostGroupDictionary, null);
        }
    }
}
