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
            if (err != null )
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
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeEntry.Key, includeEntry.Value), licenseFileDictionary);
                    }
                    foreach (var includeAllEntry in includeAllDictionary)
                    {
                        optionSelected = "INCLUDEALL";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeAllEntry.Key, includeAllEntry.Value), licenseFileDictionary);
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

        private static void SeatSubtractor(string optionSelected, KeyValuePair<int, dynamic> optionsEntry, Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary)
        {
            int optionLineIndex = optionsEntry.Key;

            // Determine the option we're using.
            if (optionSelected == "INCLUDE")
            {
                Tuple<string, string, string, string, string> optionsData = optionsEntry.Value;
                string productName = optionsData.Item1;
                string licenseNumber = optionsData.Item2;
                string productKey = optionsData.Item3;
                string clientType = optionsData.Item4;
                string clientSpecified = optionsData.Item5;
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
                int reserveSeatCount = optionsData.Item1;
                string productName = optionsData.Item2;
                string licenseNumber = optionsData.Item3;
                string productKey = optionsData.Item4;
                string clientType = optionsData.Item5;
                string clientSpecified = optionsData.Item6;
            }            

            // Go through each line and subtract seats accordingly.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileLicenseOffering = licenseFileData.Item4;
                string licenseFileLicenseNumber = licenseFileData.Item5;

                // CNU licenses have unlimited seats.
                if (licenseFileLicenseOffering.Contains("CNU") && (licenseFileSeatCount == 9999999))
                {
                    continue;
                }
            }
        }
    }
}
