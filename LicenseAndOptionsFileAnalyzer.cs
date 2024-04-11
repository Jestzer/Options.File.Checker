using System;
using System.Collections.Generic;
using System.Linq;

namespace Options.File.Checker.WPF
{
    public class LicenseAndOptionsFileAnalyzer
    {
        public static (
            Dictionary<string, Tuple<int, string, string>>? MaxDictionary,
            Dictionary<int, Tuple<string, string>>? HostGroupDictionary, 
            string? ErrorMessage) 
            AnalyzeFiles(string optionsFilePath, string licenseFilePath)
        {
            var maxDictionary = new Dictionary<string, Tuple<int, string, string>>();
            var hostGroupDictionary = new Dictionary<int, Tuple<string, string>>();

            string[] optionsFileContentsLines = System.IO.File.ReadAllLines(licenseFilePath);
            string[] licenseFileContentsLines = System.IO.File.ReadAllLines(optionsFilePath);

            // Options file information gathering.
            for (int optionsLineIndex = 0; optionsLineIndex < optionsFileContentsLines.Length; optionsLineIndex++)
            {
                string line = optionsFileContentsLines[optionsLineIndex];                
                string err;

                if (line.TrimStart().StartsWith("MAX "))
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
                        return (null, null, err);
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
                        return (null, null, err);
                    }

                    string hostGroupName = lineParts[1];
                    string hostGroupClientSpecified = string.Join(" ", lineParts.Skip(2));

                    hostGroupDictionary[optionsLineIndex] = Tuple.Create(hostGroupName, hostGroupClientSpecified);

                }                
            }
            return (maxDictionary, hostGroupDictionary, null);
        }
    }
}
