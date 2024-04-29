using System;
using System.Collections.Generic;
using System.Windows.Shapes;

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

            // Gather the data from the license and options files first.
            var analysisResult = LicenseAndOptionsFileDataGatherer.GatherData(licenseFilePath, optionsFilePath);

            // Try to make meaningful conclusions from said data.
            try
            {
                if (includeDictionary != null && excludeDictionary != null)
                {
                    foreach (var includeEntry in includeDictionary)
                    {

                    }
                    foreach (var excludeEntry in excludeDictionary)
                    {

                    }
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
    }
}
