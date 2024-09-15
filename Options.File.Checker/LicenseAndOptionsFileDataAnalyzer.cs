using System; // Don't let VSC deceive you, you need these!
using System.Collections.Generic;

namespace Options.File.Checker
{
    public partial class LicenseAndOptionsFileDataAnalyzer
    {
        public static (
            bool serverLineHasPort,
            bool daemonLineHasPort,
            bool daemonPortIsCNUFriendly,
            bool caseSensitivity,
            bool unspecifiedLicenseOrProductKey,
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

            AnalyzeData(string licenseFilePath, string optionsFilePath)
        {
            // I'm putting this here so that we can print its contents if we hit a generic error message.
            string line = string.Empty;

            // Gather the data from the license and options files first.
            var (serverLineHasPort,
                daemonLineHasPort,
                daemonPortIsCNUFriendly,
                caseSensitivity,
                optionsFileUsesMatlabParallelServer,
                wildcardsAreUsed,
                ipAddressesAreUsed,
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

            // Only thing that needs to be cleared here since it's not in the Gatherer.
            bool unspecifiedLicenseOrProductKey = false;

            // Don't proceed if you've got an error.
            if (err != null)
            {
                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }

            string optionSelected = string.Empty;

            // Try to make meaningful conclusions from said data.
            try
            {
                // Make sure that any GROUPs or HOST_GROUPs specified actually exist.
                // It'll be more efficient if we do this all at once, rather than repeatedly in the foreach loop below.
                // In words of all shitty programmers, the situation where any of these are null "sHoUlD nEvEr hApPeN" but I am trying to be the better man and give some kind of error if they don't.
                if (includeDictionary != null && groupDictionary != null && hostGroupDictionary != null && excludeDictionary != null && reserveDictionary != null && includeAllDictionary != null && excludeAllDictionary != null)
                {

                    err = PerformGroupCheck(includeDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item4, tuple.Item5), "INCLUDE");
                    if (!string.IsNullOrEmpty(err))
                    {
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    err = PerformGroupCheck(excludeDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item4, tuple.Item5), "EXCLUDE");
                    if (!string.IsNullOrEmpty(err))
                    {
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    err = PerformGroupCheck(reserveDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item5, tuple.Item6), "RESERVE");
                    if (!string.IsNullOrEmpty(err))
                    {
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    err = PerformGroupCheck(includeAllDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item1, tuple.Item2), "INCLUDEALL");
                    if (!string.IsNullOrEmpty(err))
                    {
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    err = PerformGroupCheck(excludeAllDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item1, tuple.Item2), "EXCLUDEALL");
                    if (!string.IsNullOrEmpty(err))
                    {
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }
                }
                else
                {
                    err = "Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub.";
                    return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                // Subtract seats now.
                if (licenseFileDictionary != null)
                {
                    foreach (var includeEntry in includeDictionary)
                    {
                        optionSelected = "INCLUDE";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeEntry.Key, includeEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                        if (err != null)
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    foreach (var includeAllEntry in includeAllDictionary)
                    {
                        optionSelected = "INCLUDEALL";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeAllEntry.Key, includeAllEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                        if (err != null)
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }

                    foreach (var reserveEntry in reserveDictionary)
                    {
                        optionSelected = "RESERVE";
                        SeatSubtractor(optionSelected, new KeyValuePair<int, object>(reserveEntry.Key, reserveEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err);

                        if (err != null)
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                }
                else
                {
                    err = "Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub.";
                    return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                // If you're only using NNU license(s), we need to make sure you've included at LEAST one INCLUDE line.
                // We'll first check if you're using an NNU only license. If you are, then we'll see if you have any INCLUDE lines.
                // If you do, then we'll make sure at least one of those INCLUDE lines uses a GROUP or USER.
                bool nnuExclusiveLicense = true;
                foreach (var licenseFileEntry in licenseFileDictionary)
                {
                    int licenseLineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string, List<string>, int> licenseFileData = licenseFileEntry.Value;

                    string licenseFileProductName = licenseFileData.Item1;
                    int licenseFileSeatCount = licenseFileData.Item2;
                    string licenseFileProductKey = licenseFileData.Item3;
                    string licenseFileLicenseOffering = licenseFileData.Item4;
                    string licenseFileLicenseNumber = licenseFileData.Item5;

                    if (licenseFileLicenseOffering != "NNU")
                    {
                        nnuExclusiveLicense = false;
                        break;
                    }
                }
                if (nnuExclusiveLicense)
                {
                    if (includeDictionary.Count == 0)
                    {
                        err = "There is an issue with the options file: you have no INCLUDE lines with an all-NNU license. You need these to use an NNU license.";
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }

                    bool foundValidIncludeLine = false;
                    foreach (var includeEntry in includeDictionary)
                    {
                        Tuple<string, string, string, string, string, string> includeData = includeEntry.Value;
                        string productName = includeData.Item1;
                        string licenseNumber = includeData.Item2;
                        string productKey = includeData.Item3;
                        string clientType = includeData.Item4;
                        string clientSpecified = includeData.Item5;
                        string rawOptionLine = includeData.Item6;

                        if (clientType == "USER" || clientType == "GROUP")
                        {
                            foundValidIncludeLine = true;
                            break;
                        }
                    }
                    if (!foundValidIncludeLine)
                    {
                        err = "There is an issue with the options file: you have no INCLUDE lines with a USER or GROUP. You need these to use an NNU license.";
                        return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                    }
                }

                return (serverLineHasPort,
                    daemonLineHasPort,
                    daemonPortIsCNUFriendly,
                    caseSensitivity,
                    unspecifiedLicenseOrProductKey,
                    optionsFileUsesMatlabParallelServer,
                    wildcardsAreUsed,
                    ipAddressesAreUsed,
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

                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }
        }

        private static void SeatSubtractor(string optionSelected, KeyValuePair<int, dynamic> optionsEntry, Dictionary<int, Tuple<string, int, string, string, string, List<string>, int>> licenseFileDictionary, Dictionary<int, Tuple<string, string, int>> groupDictionary, ref bool unspecifiedLicenseOrProductKey, ref string? err)
        {
            // These need to be defined outside the if statements below so they can be used across them.
            int reserveSeatCount = 0;
            string productName = string.Empty;
            string licenseNumber = string.Empty;
            string productKey = string.Empty;
            string clientType = string.Empty;
            string clientSpecified = string.Empty;
            string rawOptionLine = string.Empty;

            // Determine the option we're using. Set variables appropriately.
            if (optionSelected == "INCLUDE")
            {
                Tuple<string, string, string, string, string, string> optionsData = optionsEntry.Value;
                productName = optionsData.Item1;
                licenseNumber = optionsData.Item2;
                productKey = optionsData.Item3;
                clientType = optionsData.Item4;
                clientSpecified = optionsData.Item5;
                rawOptionLine = optionsData.Item6;
            }
            else if (optionSelected == "INCLUDEALL")
            {
                Tuple<string, string, string> optionsData = optionsEntry.Value;
                clientType = optionsData.Item1;
                clientSpecified = optionsData.Item2;
                rawOptionLine = optionsData.Item3;
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
                // # Add some code to include this. rawOptionLine = optionsData.Item7;
            }

            bool matchingProductFoundInLicenseFile = false;
            bool usableLicenseNumberOrProductKeyFoundInLicenseFile = false;

            // Go through each line and subtract seats accordingly.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string, List<string>, int> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileProductKey = licenseFileData.Item3;
                string licenseFileLicenseOffering = licenseFileData.Item4;
                string licenseFileLicenseNumber = licenseFileData.Item5;
                List<string> linesThatSubtractSeats = licenseFileData.Item6;
                int originalLicenseFileSeatCount = licenseFileData.Item7;

                // We start seat subtraction by checking to see if the product you're specifying exists in the license file.
                // Case-senstivity does not matter, apparently.
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

                        // Record the line used to subtract this seat.
                        linesThatSubtractSeats.Add(rawOptionLine);

                        // Error out if the seat count is negative.
                        if (licenseFileSeatCount < 0)
                        {
                            if (licenseFileLicenseOffering != "lo=CN")
                            {
                                // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                int remainingSeatCount = licenseFileSeatCount;
                                licenseFileSeatCount = 0;
                                licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, linesThatSubtractSeats, originalLicenseFileSeatCount);
                                licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                {
                                    if (!string.IsNullOrWhiteSpace(licenseNumber))
                                    {
                                        err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName} " +
                                        $"for license {licenseNumber}.";
                                    }
                                    else
                                    {
                                        err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName}.";
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

                                // Record the line used to subtract this seat.
                                linesThatSubtractSeats.Add(rawOptionLine);

                                // Error out if the seat count is negative and not CN.
                                if (licenseFileSeatCount < 0)
                                {
                                    if (licenseFileLicenseOffering != "lo=CN")
                                    {
                                        err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName}. " +
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

                                        // Record the line used to subtract this seat.
                                        linesThatSubtractSeats.Add(rawOptionLine);

                                        // Error out if the seat count is negative.
                                        if (licenseFileSeatCount < 0)
                                        {
                                            if (licenseFileLicenseOffering != "lo=CN")
                                            {
                                                err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName}. " +
                                                    "Don't forget that you are using at least 1 INCLUDEALL line.";
                                                return;
                                            }
                                        }
                                    }
                                }

                            }
                            // There is no subtraction that can be done because you've specified a client type that can be shared between any number of users.
                            else if (clientType == "HOST_GROUP" || clientType == "HOST" || clientType == "DISPLAY" || clientType == "PROJECT" || clientType == "INTERNET") { }
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
                                err = $"There is an issue with the options file: you have specified a USER to be able to use {licenseFileProductName}, " +
                                "but you did not define the USER.";
                                return;
                            }

                            // Record the line used to subtract this seat.
                            linesThatSubtractSeats.Add(rawOptionLine);

                            licenseFileSeatCount--;

                            // Error out if the seat count is negative.
                            if (licenseFileSeatCount < 0)
                            {
                                if (licenseFileLicenseOffering != "lo=CN")
                                {
                                    // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                    int remainingSeatCount = licenseFileSeatCount;
                                    licenseFileSeatCount = 0;
                                    licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, linesThatSubtractSeats, originalLicenseFileSeatCount);
                                    licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                    if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                    {
                                        if (!string.IsNullOrWhiteSpace(licenseNumber))
                                        {
                                            err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName} " +
                                            $"for license {licenseNumber}.";
                                        }
                                        else
                                        {
                                            err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName}.";
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
                                    err = $"There is an issue with the options file: You have specified a GROUP to be able to use {licenseFileProductName} " +
                                    $"for license {licenseNumber}, but you did not specify which GROUP.";
                                }
                                else
                                {
                                    err = $"There is an issue with the options file: You have specified a GROUP to be able to use {licenseFileProductName}, but you did not specify which GROUP.";
                                }

                                return;
                            }

                            foreach (var optionsGroupEntry in groupDictionary)
                            {
                                // Load GROUP specifications.
                                int optionsGroupLineIndex = optionsGroupEntry.Key;
                                Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                string groupName = optionsGroupData.Item1;
                                //string groupUsers = optionsGroupData.Item2;
                                int groupUserCount = optionsGroupData.Item3;

                                if (groupName == clientSpecified)
                                {
                                    // Subtract the appropriate number of seats.
                                    licenseFileSeatCount -= groupUserCount;

                                    // Record the line used to subtract this seat.
                                    linesThatSubtractSeats.Add(rawOptionLine);

                                    // Error out if the seat count is negative.
                                    if (licenseFileSeatCount < 0)
                                    {
                                        if (licenseFileLicenseOffering != "lo=CN")
                                        {
                                            // Let's see if we can find a duplicate product on the license to subtract from, unless you specified a productKey.
                                            int remainingSeatCount = licenseFileSeatCount;
                                            licenseFileSeatCount = 0;
                                            licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, licenseFileData.Item6, licenseFileData.Item7);
                                            licenseFileDictionary[licenseLineIndex] = licenseFileData;

                                            if (!string.IsNullOrEmpty(productKey) || !SubtractFromDuplicateProducts(licenseNumber, productName, remainingSeatCount, licenseFileDictionary))
                                            {
                                                if (!string.IsNullOrWhiteSpace(licenseNumber))
                                                {
                                                    err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName} " +
                                                    $"for license {licenseNumber}.";
                                                }
                                                else
                                                {
                                                    err = $"There is an issue with the options file: you have specified too many users to be able to use {licenseFileProductName}.";
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

                    licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, linesThatSubtractSeats, originalLicenseFileSeatCount);
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
                err = $"There is an issue with the options file: you specified a product, {productName}, but this product is not in your license file. " +
                        "Product names must match the ones found in the license file after the word INCREMENT. Any typos will result in this error being shown.";
                return;
            }

            // You're likely here because you didn't specify a license number in your specified option and there is no combination of products with seats remaining from any licenses in the license file to subtract from.
            if (!usableLicenseNumberOrProductKeyFoundInLicenseFile && optionSelected != "INCLUDEALL")
            {
                if (!string.IsNullOrEmpty(licenseNumber))
                {
                    err = $"There is an issue with the options file: you have specified a license number, {licenseNumber}, which does not exist in the license file for the product {productName}.";
                }
                else if (!string.IsNullOrEmpty(productKey))
                {
                    err = $"There is an issue with the options file: you have specified a product key, {productKey}, which does not exist in the license file.";
                }
                else
                {
                    err = $"There is an issue with the options file: you have specified too many users to be able to use {productName}.";
                }

                return;
            }
        }

        // Method to check if the groups you're specifying in your options have also been defined.
        // Define a delegate that takes a tuple and returns the groupType and specified strings.
        public delegate (string groupType, string specified) TupleExtractor<in TTuple>(TTuple tuple);

        private static string? PerformGroupCheck<TKey, TTuple>(
            Dictionary<TKey, TTuple> optionsIndex,
            Dictionary<TKey, Tuple<string, string, int>> groupIndex,
            Dictionary<TKey, Tuple<string, string>> hostGroupIndex,
            TupleExtractor<TTuple> extractor,
            string lineType) where TKey : notnull
        {
            foreach (var entry in optionsIndex)
            {
                TTuple tuple = entry.Value;

                // Use the extractor to get the groupType and specified strings.
                (string groupType, string specified) = extractor(tuple);

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
                        string err = string.Empty;
                        if (specified.Contains("USER"))
                        {
                            err = $"There is an issue with the options file: you specified a {groupType} on {aOrAn} {lineType} line named \"{specified}\", " +
                                  $"but this {groupType} does not exist in your options file. Please check your {groupType}s for any typos. HOST_GROUP and GROUP are separate. " +
                                  "You cannot specify a GROUP and a USER on a single INCLUDE line.";
                        }
                        else
                        {
                            err = $"There is an issue with the options file: you specified a {groupType} on {aOrAn} {lineType} line named \"{specified}\", " +
                                  $"but this {groupType} does not exist in your options file. Please check your {groupType}s for any typos. HOST_GROUP and GROUP are separate.";
                        }

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
                        string err = $"There is an issue with the options file: you specified a {groupType} on {aOrAn} {lineType} line named \"{specified}\", " +
                                     $"but this {groupType} does not exist in your options file. Please check your {groupType}s for any typos. HOST_GROUP and GROUP are separate.";
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

        private static bool SubtractFromDuplicateProducts(string optionsLicenseNumber, string optionsProductName, int seatsToSubtract, Dictionary<int, Tuple<string, int, string, string, string, List<string>, int>> licenseFileDictionary)
        {
            bool successfullySubtracted = false;

            // Search the license for a matching product and license number, if specified.
            foreach (var licenseFileEntry in licenseFileDictionary)
            {
                int licenseLineIndex = licenseFileEntry.Key;
                Tuple<string, int, string, string, string, List<string>, int> licenseFileData = licenseFileEntry.Value;

                string licenseFileProductName = licenseFileData.Item1;
                int licenseFileSeatCount = licenseFileData.Item2;
                string licenseFileLicenseNumber = licenseFileData.Item5;
                List<string> linesThatSubtractSeats = licenseFileData.Item6;
                int originalLicenseFileSeatCount = licenseFileData.Item7;

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

                        licenseFileData = Tuple.Create(optionsProductName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, linesThatSubtractSeats, originalLicenseFileSeatCount);
                        licenseFileDictionary[licenseLineIndex] = licenseFileData;

                        if (successfullySubtracted) break;
                    }
                }
            }
            return successfullySubtracted;
        }
    }
}
