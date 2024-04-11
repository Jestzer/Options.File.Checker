using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Options.File.Checker.WPF
{
    public class LicenseAndOptionsFileAnalyzer
    {
        private static bool serverLineHasPort = false;
        private static bool daemonLineHasPort = false;
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
            includeDictionary.Clear();
            includeBorrowDictionary.Clear();
            excludeDictionary.Clear();
            excludeBorrowDictionary.Clear();
            maxDictionary.Clear();
            hostGroupDictionary.Clear();
            err = string.Empty;
            int serverLineCount = 0;
            bool productLinesHaveBeenReached = false;

            string[] optionsFileContentsLines = System.IO.File.ReadAllLines(licenseFilePath);
            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(optionsFilePath);

            // License file information gathering.
            for (int licenseLineIndex = 0; licenseLineIndex < licenseFileContentsLines.Length; licenseLineIndex++)
            {
                string line = licenseFileContentsLines[licenseLineIndex];

                if (line.TrimStart().StartsWith("SERVER"))
                {
                    // SERVER lines should come before the product(s).
                    if (productLinesHaveBeenReached)
                    {
                        err = "There is an issue with the selected license file: your SERVER line(s) are listed after a product.";
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                    }
                    serverLineCount++;

                    string[] lineParts = line.Split(' ');
                    string serverWord = lineParts[0];
                    string serverHostID = lineParts[2];

                    if (lineParts.Length < 3 || lineParts.Length == 3)
                    {
                        err = "There is an issue with the selected license file: you are missing information from your SERVER line.";
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                    }
                    else if (lineParts.Length == 4)
                    {
                        // Check to make sure you're using a port number.
                        if (!int.TryParse(lineParts[3], out int serverPort))
                        {
                            err = "There is an issue with the selected license file: you have stray information on your SERVER line.";
                            return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (serverWord != "SERVER")
                        {
                            err = "There is an issue with the selected license file: it does not start with the word SERVER.";
                            return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        if (!serverHostID.Contains("INTERNET=") && serverHostID.Length != 12)
                        {
                            err = "There is an issue with the selected license file: you have not specified your Host ID correctly.";
                            return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                        }

                        // Congrats, you /may/ have not made any mistakes on your SERVER line.
                    }
                    else
                    {
                        err = "There is an issue with the selected license file: you have stray information on your SERVER line.";
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    // There is no situation where you should have more than 3 SERVER lines.
                    if (serverLineCount > 3)
                    {
                        err = "There is an issue with the selected license file: it has too many SERVER lines. Only 1 or 3 are accepted.";
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                    }

                }
                else if (line.TrimStart().StartsWith("DAEMON"))
                {

                }
                else if (line.TrimStart().StartsWith("INCREMENT"))
                {

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
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
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
                            return (false, false, null, null, null, null, null, null, null, null, null, null, err);
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
                                return (false, false, null, null, null, null, null, null, null, null, null, null, err);
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
                            return (false, false, null, null, null, null, null, null, null, null, null, null, err);
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
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
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
                        return (false, false, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    string hostGroupName = lineParts[1];
                    string hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                    hostGroupDictionary[optionsLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);
                }
            }
            return (serverLineHasPort, daemonLineHasPort, includeDictionary, includeBorrowDictionary, includeAllDictionary, excludeDictionary, excludeBorrowDictionary, excludeAllDictionary, reserveDictionary, maxDictionary, groupDictionary, hostGroupDictionary, null);
        }
    }
}
