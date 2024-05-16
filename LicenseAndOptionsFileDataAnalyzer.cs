using System;
using System.Collections.Generic;

namespace Options.File.Checker.WPF
{
    public partial class LicenseAndOptionsFileDataAnalyzer
    {
        public static (
            bool serverLineHasPort,
            bool daemonLineHasPort,
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

            AnalyzeData(string licenseFilePath, string optionsFilePath)
        {
            // I'm putting this here so that we can print its contents if we hit a generic error message.
            string line = string.Empty;

            // Gather the data from the license and options files first.
            var (serverLineHasPort,
                daemonLineHasPort,
                caseSensitivity,
                licenseFileDictionary,
                includeDictionary,
                includeBorrowDictionary,
                includeAllDictionary,
                excludeDictionary,
                excludeBorrowDictionary,
                excludeAllDictionary,
                reserveDictionary,
                maxDictionary,
                groupDictionary,
                hostGroupDictionary,
                err) = LicenseAndOptionsFileDataGatherer.GatherData(licenseFilePath, optionsFilePath);

            // Don't proceed if you've got an error.
            if (err != null)
            {
                return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }

            string optionSelected = string.Empty;

            // Try to make meaningful conclusions from said data.
            try
            {
                // # Add some code to make sure that any GROUPs or HOST_GROUPs specified actually exist.
                // It'll be more efficient if we do this all at once, rather than repeatedly in the foreach loop below.

                if (includeDictionary != null && includeAllDictionary != null && licenseFileDictionary != null)
                {
                    foreach (var includeEntry in includeDictionary)
                    {
                        optionSelected = "INCLUDE";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeEntry.Key, includeEntry.Value), licenseFileDictionary, err);

                        if (err != null)
                        {
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    foreach (var includeAllEntry in includeAllDictionary)
                    {
                        optionSelected = "INCLUDEALL";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeAllEntry.Key, includeAllEntry.Value), licenseFileDictionary, err);

                        if (err != null)
                        {
                            return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                }
                else
                {
                    err = "Apparently one of the dictionaries in the Analyzer is empty and therefore, the code in it cannot proceed.";
                    return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                return (serverLineHasPort,
                    daemonLineHasPort,
                    caseSensitivity,
                    licenseFileDictionary,
                    includeDictionary,
                    includeBorrowDictionary,
                    includeAllDictionary,
                    excludeDictionary,
                    excludeBorrowDictionary,
                    excludeAllDictionary,
                    reserveDictionary,
                    maxDictionary,
                    groupDictionary,
                    hostGroupDictionary,
                    null);
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

                return (false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }
        }

        private static void SeatSubtractor(string optionSelected, KeyValuePair<int, dynamic> optionsEntry, Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary, string? err)
        {
            int optionLineIndex = optionsEntry.Key;

            // These need to be defined outside the if statements below so they can be used across them.
            int reserveSeatCount = 0;
            string productName = string.Empty;
            string licenseNumber = string.Empty;
            string productKey = string.Empty;
            string clientType = string.Empty;
            string clientSpecified = string.Empty;

            // Determine the option we're using.
            if (optionSelected == "INCLUDE")
            {
                Tuple<string, string, string, string, string> optionsData = optionsEntry.Value;
                productName = optionsData.Item1;
                licenseNumber = optionsData.Item2;
                productKey = optionsData.Item3;
                clientType = optionsData.Item4;
                clientSpecified = optionsData.Item5;
            }
            else if (optionSelected == "INCLUDEALL")
            {
                Tuple<string, string> optionsData = optionsEntry.Value;
                string includeAllClientType = optionsData.Item1;
                string includeAllClientSpecified = optionsData.Item2;
            }
            else if (optionSelected == "RESERVE")
            {
                Tuple<int, string, string, string, string, string> optionsData = optionsEntry.Value;
                reserveSeatCount = optionsData.Item1;
                productName = optionsData.Item2;
                licenseNumber = optionsData.Item3;
                productKey = optionsData.Item4;
                clientType = optionsData.Item5;
                clientSpecified = optionsData.Item6;
            }

            bool matchingProductFoundInLicenseFile = false;
            bool usableLicenseNumberFoundInLicenseFile = false;

            // Go through each line and subtract seats accordingly.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileLicenseOffering = licenseFileData.Item4;
                string licenseFileLicenseNumber = licenseFileData.Item5;

                // CNU licenses have unlimited seats, so nothing should be done with them...
                // # Add some code here. The problem with having this here is that you can specify products that don't exist and therefore should be lower in this code's section.
                // I'm excluding CN because unlike CNU, CN can never have "uncounted" seat count.
                if (licenseFileLicenseOffering.Contains("CNU") && (licenseFileSeatCount == 9999999))
                {
                    continue;
                }

                // We start seat subtraction by checking to see if the product you're specifying exists in the license file.
                if (productName == licenseFileProductName)
                {
                    matchingProductFoundInLicenseFile = true;

                    if (licenseNumber == licenseFileLicenseNumber)
                    {
                        // Continue on, adventurer.
                        usableLicenseNumberFoundInLicenseFile = true;
                    }

                    // If you didn't specify a license number, then we need to find a license number we can use, if any.
                    else
                    {
                        if (licenseFileSeatCount == 0)
                        {
                            // See if we can find another entry with the same product that does not have a seat count of 0.
                            continue;
                        }
                        else
                        {
                            usableLicenseNumberFoundInLicenseFile = true;
                        }
                    }

                    if (clientType == "USER")
                    {
                        // Check that a user has actually been specified.
                        if (string.IsNullOrWhiteSpace(clientSpecified))
                        {
                            err = $"There is an issue with the selected options file: you have specified a USER to be able to use {productName}, " +
                            "but you did not define the USER.";
                            return;
                        }

                        licenseFileSeatCount--;

                        // Error out if the seat count is negative.
                        if (licenseFileSeatCount < 0)
                        {
                            if (licenseFileLicenseOffering == "lo=CN")
                            {
                                // Continue and print out a warning later on since technically you can specify more users than you have seats for on CN.
                            }
                            else
                            {
                                err = $"There is an issue with the selected options file: you have specified too many users to be able to use {productName}.";
                                return;
                            }
                        }
                    }
                    else if (clientType == "GROUP")
                    {

                    }

                    licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                    licenseFileDictionary[licenseLineIndex] = licenseFileData;
                }
            }

            // You can't give away what you don't have.
            if (!matchingProductFoundInLicenseFile)
            {
                err = $"There is an issue with the selected options file: you specified a product, {productName}, but this product is not in your license file. " +
                        $"Product names must match the ones found in the license file after the word INCREMENT and they are case-sensitive.";
                return;
            }

            // You're likely here because you didn't specify a license number in your specified option and there is no combination of products with seats remaining from any licenses in the license file to subtract from.
            if (!usableLicenseNumberFoundInLicenseFile)
            {
                err = $"There is an issue with the selected options file: You have specified too many users to be able to use {productName}.";
                return;
            }
        }
    }
}
