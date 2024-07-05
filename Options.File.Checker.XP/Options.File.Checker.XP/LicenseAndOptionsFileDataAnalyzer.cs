using System;
using System.Collections.Generic;

namespace Options.File.Checker
{
    public class AnalyzeDataResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public bool ServerLineHasPort { get; set; }
        public bool DaemonLineHasPort { get; set; }
        public bool DaemonPortIsCNUFriendly { get; set; }
        public bool CaseSensitivity { get; set; }
        public Dictionary<int, Tuple<string, int, string, string, string>> LicenseFileDictionary { get; set; }
        public Dictionary<int, Tuple<string, string, string, string, string>> IncludeDictionary { get; set; }
        public Dictionary<int, Tuple<string, string, string, string, string>> IncludeBorrowDictionary { get; set; }
        public Dictionary<int, Tuple<string, string>> IncludeAllDictionary { get; set; }
        public Dictionary<int, Tuple<string, string, string, string, string>> ExcludeDictionary { get; set; }
        public Dictionary<int, Tuple<string, string, string, string, string>> ExcludeBorrowDictionary { get; set; }
        public Dictionary<int, Tuple<string, string>> ExcludeAllDictionary { get; set; }
        public Dictionary<int, Tuple<int, string, string, string, string, string>> ReserveDictionary { get; set; }
        public Dictionary<string, Tuple<int, string, string>> MaxDictionary { get; set; }
        public Dictionary<int, Tuple<string, string, int>> GroupDictionary { get; set; }
        public Dictionary<int, Tuple<string, string>> HostGroupDictionary { get; set; }
    }

    public partial class LicenseAndOptionsFileDataAnalyzer
    {
        public static bool serverLineHasPort;
        public static bool daemonLineHasPort;
        public static bool daemonPortIsCNUFriendly;
        public static bool caseSensitivity;
        public static bool unspecifiedLicenseOrProductKey;
        public static string err;

        public static AnalyzeDataResult AnalyzeData(string licenseFilePath, string optionsFilePath)
        {
            AnalyzeDataResult result = new AnalyzeDataResult();
            result.Success = true;

            // I'm putting this here so that we can print its contents if we hit a generic error message.
            string line = string.Empty;

            // Gather the data from the license and options files first.
            GatherDataResult gatherResult = LicenseAndOptionsFileDataGatherer.GatherData(licenseFilePath, optionsFilePath);

            bool unspecifiedLicenseOrProductKey = false;

            // Don't proceed if you've got an error.
            if (!gatherResult.Success)
            {
                result.Success = false;
                result.ErrorMessage = gatherResult.ErrorMessage;
                return result;
            }

            // Use the gathered data.
            result.ServerLineHasPort = gatherResult.serverLineHasPort;
            result.DaemonLineHasPort = gatherResult.daemonLineHasPort;
            result.DaemonPortIsCNUFriendly = gatherResult.daemonPortIsCNUFriendly;
            result.CaseSensitivity = gatherResult.caseSensitivity;
            result.LicenseFileDictionary = gatherResult.LicenseFileDictionary;
            result.IncludeDictionary = gatherResult.IncludeDictionary;
            result.IncludeBorrowDictionary = gatherResult.IncludeBorrowDictionary;
            result.IncludeAllDictionary = gatherResult.IncludeAllDictionary;
            result.ExcludeDictionary = gatherResult.ExcludeDictionary;
            result.ExcludeBorrowDictionary = gatherResult.ExcludeBorrowDictionary;
            result.ExcludeAllDictionary = gatherResult.ExcludeAllDictionary;
            result.ReserveDictionary = gatherResult.ReserveDictionary;
            result.MaxDictionary = gatherResult.MaxDictionary;
            result.GroupDictionary = gatherResult.GroupDictionary;
            result.HostGroupDictionary = gatherResult.HostGroupDictionary;

            string optionSelected = string.Empty;

            // Try to make meaningful conclusions from said data.
            try
            {
                // Make sure that any GROUPs or HOST_GROUPs specified actually exist.
                // It'll be more efficient if we do this all at once, rather than repeatedly in the foreach loop below.
                // In words of all shitty programmers, the situation where any of these are null "sHoUlD nEvEr hApPeN" but I am trying to be the better man and give some kind of error if they don't.
                // Ensure that dictionaries are non-nullable.
                var nonNullIncludeDictionary = result.IncludeDictionary ?? new Dictionary<int, Tuple<string, string, string, string, string>>();
                var nonNullExcludeDictionary = result.ExcludeDictionary ?? new Dictionary<int, Tuple<string, string, string, string, string>>();
                var nonNullReserveDictionary = result.ReserveDictionary ?? new Dictionary<int, Tuple<int, string, string, string, string, string>>();
                var nonNullIncludeAllDictionary = result.IncludeAllDictionary ?? new Dictionary<int, Tuple<string, string>>();
                var nonNullExcludeAllDictionary = result.ExcludeAllDictionary ?? new Dictionary<int, Tuple<string, string>>();
                var nonNullGroupDictionary = result.GroupDictionary ?? new Dictionary<int, Tuple<string, string, int>>();
                var nonNullHostGroupDictionary = result.HostGroupDictionary ?? new Dictionary<int, Tuple<string, string>>();
                err = PerformGroupCheck(nonNullIncludeDictionary, nonNullGroupDictionary, nonNullHostGroupDictionary, tuple => Tuple.Create(tuple.Item4, tuple.Item5), "INCLUDE");
                if (!string.IsNullOrEmpty(err))
                {
                    result.Success = false;
                    result.ErrorMessage = err;
                    return result;
                }

                err = PerformGroupCheck(nonNullExcludeDictionary, nonNullGroupDictionary, nonNullHostGroupDictionary, tuple => Tuple.Create(tuple.Item4, tuple.Item5), "EXCLUDE");
                if (!string.IsNullOrEmpty(err))
                {
                    result.Success = false;
                    result.ErrorMessage = err;
                    return result;
                }

                err = PerformGroupCheck(nonNullReserveDictionary, nonNullGroupDictionary, nonNullHostGroupDictionary, tuple => Tuple.Create(tuple.Item5, tuple.Item6), "RESERVE");
                if (!string.IsNullOrEmpty(err))
                {
                    result.Success = false;
                    result.ErrorMessage = err;
                    return result;
                }

                err = PerformGroupCheck(nonNullIncludeAllDictionary, nonNullGroupDictionary, nonNullHostGroupDictionary, tuple => Tuple.Create(tuple.Item1, tuple.Item2), "INCLUDEALL");
                if (!string.IsNullOrEmpty(err))
                {
                    result.Success = false;
                    result.ErrorMessage = err;
                    return result;
                }

                err = PerformGroupCheck(nonNullExcludeAllDictionary, nonNullGroupDictionary, nonNullHostGroupDictionary, tuple => Tuple.Create(tuple.Item1, tuple.Item2), "EXCLUDEALL");
                if (!string.IsNullOrEmpty(err))
                {
                    result.Success = false;
                    result.ErrorMessage = err;
                    return result;
                }

                // Subtract seats now.
                foreach (var includeEntry in nonNullIncludeDictionary)
                {
                    optionSelected = "INCLUDE";
                    SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeEntry.Key, includeEntry.Value), result.LicenseFileDictionary, result.GroupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                    if (err != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = err;
                        return result;
                    }
                }
                foreach (var includeAllEntry in nonNullIncludeAllDictionary)
                {
                    optionSelected = "INCLUDEALL";
                    SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeAllEntry.Key, includeAllEntry.Value), result.LicenseFileDictionary, result.GroupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                    if (err != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = err;
                        return result;
                    }
                }
                foreach (var reserveEntry in nonNullReserveDictionary)
                {
                    optionSelected = "RESERVE";
                    SeatSubtractor(optionSelected, new KeyValuePair<int, object>(reserveEntry.Key, reserveEntry.Value), result.LicenseFileDictionary, result.GroupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                    if (err != null)
                    {
                        result.Success = false;
                        result.ErrorMessage = err;
                        return result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex.Message == "The value cannot be an empty string. (Parameter 'path')")
                {
                    err = "You left the license or options file text field blank.";
                }
                else if (ex.Message == "Index was outside the bounds of the array.")
                {
                    err = "There is a formatting issue in your license/options file. This is the line in question's contents: " + line;
                }
                else
                {
                    err = "You managed to break something. How? Here's the automatic message: " + ex.Message;
                }

                result.Success = false;
                result.ErrorMessage = err;
                return result;
            }
        }

        private static void SeatSubtractor(string optionSelected, KeyValuePair<int, dynamic> optionsEntry, Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary, Dictionary<int, Tuple<string, string, int>> groupDictionary, ref bool unspecifiedLicenseOrProductKey, ref string err)
        {
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
                clientType = optionsData.Item1;
                clientSpecified = optionsData.Item2;
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
            bool usableLicenseNumberOrProductKeyFoundInLicenseFile = false;

            // Go through each line and subtract seats accordingly.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileProductKey = licenseFileData.Item3;
                string licenseFileLicenseOffering = licenseFileData.Item4;
                string licenseFileLicenseNumber = licenseFileData.Item5;

                // CNU licenses have unlimited seats.
                if (licenseFileLicenseOffering.Contains("CNU") && (licenseFileSeatCount == 9999999))
                {
                    continue;
                }

                // We start seat subtraction by checking to see if the product you're specifying exists in the license file.
                if (string.Equals(productName, licenseFileProductName, StringComparison.OrdinalIgnoreCase) || optionSelected == "INCLUDEALL")
                {
                    matchingProductFoundInLicenseFile = true;

                    if (licenseNumber == licenseFileLicenseNumber || productKey == licenseFileProductKey)
                    {
                        usableLicenseNumberOrProductKeyFoundInLicenseFile = true; // Continue on, adventurer.
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(licenseNumber) || !string.IsNullOrEmpty(productKey))
                        {
                            continue; // If the option entry in question has specified a license number or product key (ew), then we actually need to find a matching license number/product key.
                        }
                        else // If you're here, your option entry does not use a license number/product key, so we'll check if the current license file license number/product key has any remaining seats we can use/subtract from.
                        {
                            if (licenseFileSeatCount == 0)
                            {
                                continue; // See if we can find another entry with the same product that does not have a seat count of 0.
                            }
                            else
                            {
                                if (optionSelected != "INCLUDEALL") unspecifiedLicenseOrProductKey = true;

                                usableLicenseNumberOrProductKeyFoundInLicenseFile = true;
                            }
                        }
                    }

                    // RESERVE lines don't care about your clientType.
                    if (optionSelected == "RESERVE")
                    {
                        licenseFileSeatCount -= reserveSeatCount;

                        // Error out if the seat count is negative.
                        if (licenseFileSeatCount < 0)
                        {
                            if (licenseFileLicenseOffering != "lo=CN")
                            {
                                // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                int remainingSeatCount = licenseFileSeatCount;
                                licenseFileSeatCount = 0;
                                licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                                licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                {
                                    if (!string.IsNullOrWhiteSpace(licenseNumber))
                                    {
                                        err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName +
                                        " for license " + licenseNumber + ".";
                                    }
                                    else
                                    {
                                        err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName + ".";
                                    }
                                }
                                return;
                            }
                        }
                    }
                    else if (optionSelected == "INCLUDEALL")
                    {
                        // Without this, the recorded product name will be blank.
                        productName = licenseFileProductName;

                        if (licenseFileLicenseOffering != "NNU") // You can't use INCLUDEALL with NNU.
                        {
                            if (clientType == "USER")
                            {
                                // Subtract 1 from seatCount, since you only specified a single user.
                                licenseFileSeatCount--;

                                // Error out if the seat count is negative and not CN.
                                if (licenseFileSeatCount < 0)
                                {
                                    if (licenseFileLicenseOffering != "lo=CN")
                                    {
                                        err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName + ". " +
                                            "Don't forget that you are using at least 1 INCLUDEALL line.";
                                        return;
                                    }
                                }
                            }
                            else if (clientType == "GROUP")
                            {
                                if (string.IsNullOrWhiteSpace(clientSpecified))
                                {
                                    err = "There is an issue with the options file: you have specified to use a GROUP on an INCLUDEALL line, but you did not specify which GROUP.";
                                    return;
                                }

                                // Subtract from seat count based on the number of users in the GROUP.
                                foreach (var optionsGroupEntry in groupDictionary)
                                {
                                    // Load GROUP specifications.
                                    int optionsGroupLineIndex = optionsGroupEntry.Key;
                                    Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                    string groupName = optionsGroupData.Item1;
                                    string groupUsers = optionsGroupData.Item2;
                                    int groupUserCount = optionsGroupData.Item3;

                                    if (groupName == clientSpecified)
                                    {
                                        // Subtract the appropriate number of seats.
                                        licenseFileSeatCount -= groupUserCount;

                                        // Error out if the seat count is negative.
                                        if (licenseFileSeatCount < 0)
                                        {
                                            if (licenseFileLicenseOffering != "lo=CN")
                                            {
                                                err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName + ". " +
                                                    "Don't forget that you are using at least 1 INCLUDEALL line.";
                                                return;
                                            }
                                        }
                                    }
                                }

                            }
                            else if (clientType == "HOST_GROUP" || clientType == "HOST" || clientType == "DISPLAY" || clientType == "PROJECT" || clientType == "INTERNET")
                            {
                                // There is no subtraction that can be done because you've specified a client type that can be shared between any number of users.
                            }
                            else
                            {
                                err = "There is an issue with the options file: you specified an invalid client type for an INCLUDEALL line.";
                                return;
                            }
                        }
                    }
                    else // optionSelected == INCLUDE
                    {
                        if (clientType == "USER")
                        {
                            // Check that a user has actually been specified.
                            if (string.IsNullOrWhiteSpace(clientSpecified))
                            {
                                err = "There is an issue with the options file: you have specified a USER to be able to use " + licenseFileProductName + ", " +
                                "but you did not define the USER.";
                                return;
                            }

                            licenseFileSeatCount--;

                            // Error out if the seat count is negative.
                            if (licenseFileSeatCount < 0)
                            {
                                if (licenseFileLicenseOffering != "lo=CN")
                                {
                                    // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                    int remainingSeatCount = licenseFileSeatCount;
                                    licenseFileSeatCount = 0;
                                    licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                                    licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                    if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                    {
                                        if (!string.IsNullOrWhiteSpace(licenseNumber))
                                        {
                                            err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName +
                                            " for license " + licenseNumber + ".";
                                        }
                                        else
                                        {
                                            err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName + ".";
                                        }
                                    }
                                    return;
                                }
                            }
                        }
                        else if (clientType == "GROUP")
                        {
                            // Check that a group has actually been specified.
                            if (string.IsNullOrWhiteSpace(clientSpecified))
                            {
                                if (!string.IsNullOrWhiteSpace(licenseNumber))
                                {
                                    err = "There is an issue with the options file: You have specified a GROUP to be able to use " + licenseFileProductName +
                                    " for license {licenseNumber}, but you did not specify which GROUP.";
                                }
                                else
                                {
                                    err = "There is an issue with the options file: You have specified a GROUP to be able to use " + licenseFileProductName + ", but you did not specify which GROUP.";
                                }

                                return;
                            }

                            foreach (var optionsGroupEntry in groupDictionary)
                            {
                                // Load GROUP specifications.
                                int optionsGroupLineIndex = optionsGroupEntry.Key;
                                Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                string groupName = optionsGroupData.Item1;
                                string groupUsers = optionsGroupData.Item2;
                                int groupUserCount = optionsGroupData.Item3;

                                if (groupName == clientSpecified)
                                {
                                    // Subtract the appropriate number of seats.
                                    licenseFileSeatCount -= groupUserCount;

                                    // Error out if the seat count is negative.
                                    if (licenseFileSeatCount < 0)
                                    {
                                        if (licenseFileLicenseOffering != "lo=CN")
                                        {
                                            // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                            int remainingSeatCount = licenseFileSeatCount;
                                            licenseFileSeatCount = 0;
                                            licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                                            licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                            if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                            {
                                                if (!string.IsNullOrWhiteSpace(licenseNumber))
                                                {
                                                    err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName +
                                                    " for license " + licenseNumber + ".";
                                                }
                                                else
                                                {
                                                    err = "There is an issue with the options file: you have specified too many users to be able to use " + licenseFileProductName + ".";
                                                }
                                            }
                                            return;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                    licenseFileDictionary[licenseLineIndex] = licenseFileData;

                    if (optionSelected != "INCLUDEALL")
                    {
                        break; // We don't need to go through any other products since we've already done seat subtraction.
                    }
                }
            }

            // You can't give away what you don't have.
            if (!matchingProductFoundInLicenseFile && optionSelected != "INCLUDEALL")
            {
                err = "There is an issue with the options file: you specified a product, " + productName + ", but this product is not in your license file. " +
                        "Product names must match the ones found in the license file after the word INCREMENT. Typos will result in this error message appearing.";
                return;
            }

            // You're likely here because you didn't specify a license number in your specified option and there is no combination of products with seats remaining from any licenses in the license file to subtract from.
            if (!usableLicenseNumberOrProductKeyFoundInLicenseFile && optionSelected != "INCLUDEALL")
            {
                if (!string.IsNullOrEmpty(licenseNumber))
                {
                    err = "There is an issue with the options file: you have specified a license number, " + licenseNumber + ", which does not exist in the license file.";
                }
                else if (!string.IsNullOrEmpty(productKey))
                {
                    err = "There is an issue with the options file: you have specified a product key, " + productKey + ", which does not exist in the license file.";
                }
                else
                {
                    err = "There is an issue with the options file: you have specified too many users to be able to use " + productName + ".";
                }

                return;
            }
        }

        // Define a delegate that takes a tuple and returns the groupType and specified strings.
        public delegate Tuple<string, string> TupleExtractor<TTuple>(TTuple tuple);

        // Method to check if the groups you're specifying in your options have also been defined.
        private static string PerformGroupCheck<TKey, TTuple>(
    Dictionary<TKey, TTuple> optionsIndex,
    Dictionary<TKey, Tuple<string, string, int>> groupIndex,
    Dictionary<TKey, Tuple<string, string>> hostGroupIndex,
    TupleExtractor<TTuple> extractor,
    string lineType)
        {
            foreach (var entry in optionsIndex)
            {
                TTuple tuple = entry.Value;

                // Use the extractor to get the groupType and specified strings.
                Tuple<string, string> extracted = extractor(tuple);
                string groupType = extracted.Item1;
                string specified = extracted.Item2;

                if (groupType == "GROUP")
                {
                    bool groupFound = false;

                    foreach (var optionsGroupEntry in groupIndex)
                    {
                        Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;
                        string groupName = optionsGroupData.Item1;

                        if (groupName == specified)
                        {
                            groupFound = true;
                            break; // Matching GROUP found, exit the loop.
                        }
                    }

                    if (!groupFound)
                    {
                        // No matching group found.
                        string aOrAn = lineType == "RESERVE" ? "a" : "an"; // Grammar is important, kids!
                        string err = "There is an issue with the options file: you specified a " + groupType + " on " + aOrAn + " " + lineType + " line named " + specified + ", " +
                                     "but this " + groupType + " does not exist in your options file. Please check your " + groupType + "s for any typos. HOST_GROUP and GROUP are separate.";
                        return err;
                    }
                }
                else if (groupType == "HOST_GROUP")
                {
                    bool hostGroupFound = false;

                    foreach (var optionsHostGroupEntry in hostGroupIndex)
                    {
                        Tuple<string, string> optionsHostGroupData = optionsHostGroupEntry.Value;
                        string hostGroupName = optionsHostGroupData.Item1;

                        if (hostGroupName == specified)
                        {
                            hostGroupFound = true;
                            break; // Matching HOST_GROUP found, exit the loop.
                        }
                    }

                    if (!hostGroupFound)
                    {
                        // No matching host group found.
                        string aOrAn = lineType == "RESERVE" ? "a" : "an";
                        string err = "There is an issue with the options file: you specified a " + groupType + " on " + aOrAn + " " + lineType + " line named " + specified + ", " +
                                     "but this " + groupType + " does not exist in your options file. Please check your " + groupType + "s for any typos. HOST_GROUP and GROUP are separate.";
                        return err;
                    }
                }
                else
                {
                    continue; // No need to do this for other client types.
                }
            }
            return null;
        }

        private static bool SubtractFromDuplicateProducts(string optionsLicenseNumber, string optionsProductName, int seatsToSubtract, Dictionary<int, Tuple<string, int, string, string, string>> licenseFileDictionary)
        {
            bool successfullySubtracted = false;

            // Search the license for a matching product and license number, if specified.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileLicenseNumber = licenseFileData.Item5;

                if (licenseFileProductName == optionsProductName)
                {
                    // Disqualify the match if a license number is being used and doesn't match.
                    if (string.IsNullOrEmpty(optionsLicenseNumber) || optionsLicenseNumber == licenseFileLicenseNumber)
                    {
                        // Correct the seatsToSubtract to be positive integer.
                        int positiveSeatCount = Math.Abs(seatsToSubtract);

                        licenseFileSeatCount -= positiveSeatCount;

                        if (licenseFileSeatCount < 0)
                        {
                            // Look for another match to subtract remaining seats.
                            seatsToSubtract = licenseFileSeatCount;

                            licenseFileSeatCount = 0;
                        }
                        else
                        {
                            successfullySubtracted = true;
                        }

                        licenseFileData = Tuple.Create(optionsProductName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5);
                        licenseFileDictionary[licenseLineIndex] = licenseFileData;

                        if (successfullySubtracted) break;
                    }
                }
            }
            return successfullySubtracted;
        }
    }
}
public class GatherDataResult
{
    public bool ServerLineHasPort { get; set; }
    public bool DaemonLineHasPort { get; set; }
    // Add other properties...
    public Dictionary<int, Tuple<string, int, string, string, string>> LicenseFileDictionary { get; set; }
    // Initialize other dictionary properties...
    public string Err { get; set; }

    public GatherDataResult()
    {
        LicenseFileDictionary = new Dictionary<int, Tuple<string, int, string, string, string>>();
        // Initialize other dictionaries...
        Err = string.Empty;
    }
}
