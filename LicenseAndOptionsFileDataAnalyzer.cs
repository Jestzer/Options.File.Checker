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
            var analysisResult = LicenseAndOptionsFileDataGatherer.GatherData(licenseFilePath, optionsFilePath);
            
            foreach (var includeEntry in includeDictionary)
            {
                
            }
            foreach (var excludeEntry in excludeDictionary)
            {

            }
            return (serverLineHasPort, daemonLineHasPort, caseSensitivity, licenseFileDictionary, includeDictionary, includeBorrowDictionary, includeAllDictionary, excludeDictionary, excludeBorrowDictionary, excludeAllDictionary, reserveDictionary, maxDictionary, groupDictionary, hostGroupDictionary, null);
        }
    }
}
