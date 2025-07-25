using System; // Don't let your IDE deceive you, you need these!
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace Options.File.Checker
{
    public abstract class LicenseAndOptionsFileDataAnalyzer
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

            // This is what happens when you have a bug that only exists in your published build...
            int debugPoint = 0;

            // Gather the data from the license and options files first.
            var (serverLineHasPort,
                daemonLineHasPort,
                daemonPortIsCnuFriendly,
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
            debugPoint = 1;
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
                debugPoint = 2;
                if (includeDictionary != null && groupDictionary != null && hostGroupDictionary != null && excludeDictionary != null && reserveDictionary != null && includeAllDictionary != null && excludeAllDictionary != null)
                {
                    debugPoint = 3;
                    if (includeDictionary.Count != 0)
                    {
                        err = PerformGroupCheck(includeDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item4, tuple.Item5), "INCLUDE", ref debugPoint);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                        debugPoint = 20;
                    }
                    if (excludeDictionary.Count != 0)
                    {
                        err = PerformGroupCheck(excludeDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item4, tuple.Item5), "EXCLUDE", ref debugPoint);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    if (reserveDictionary.Count != 0)
                    {
                        err = PerformGroupCheck(reserveDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item5, tuple.Item6), "RESERVE", ref debugPoint);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    if (includeAllDictionary.Count != 0)
                    {
                        err = PerformGroupCheck(includeAllDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item1, tuple.Item2), "INCLUDEALL", ref debugPoint);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }

                    if (excludeAllDictionary.Count != 0)
                    {
                        err = PerformGroupCheck(excludeAllDictionary, groupDictionary, hostGroupDictionary, tuple => (tuple.Item1, tuple.Item2), "EXCLUDEALL", ref debugPoint);
                        if (!string.IsNullOrEmpty(err))
                        {
                            return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                        }
                    }
                    debugPoint = 21;
                    err = string.Empty;
                }
                else
                {
                    err = "Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub.";
                    return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                }

                debugPoint = 22;
                // Subtract seats now.
                if (licenseFileDictionary != null)
                {
                    debugPoint = 23;
                    if (includeDictionary.Count != 0)
                    {
                        debugPoint = 24;
                        foreach (var includeEntry in includeDictionary)
                        {
                            debugPoint = 25;
                            optionSelected = "INCLUDE";
                            SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeEntry.Key, includeEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err, ref debugPoint);
                            debugPoint = 26;
                            if (!string.IsNullOrEmpty(err))
                            {
                                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                    }

                    if (includeAllDictionary.Count != 0)
                    {
                        foreach (var includeAllEntry in includeAllDictionary)
                        {
                            optionSelected = "INCLUDEALL";
                            SeatSubtractor(optionSelected, new KeyValuePair<int, object>(includeAllEntry.Key, includeAllEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err, ref debugPoint);

                            if (!string.IsNullOrEmpty(err))
                            {
                                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
                        }
                    }

                    if (reserveDictionary.Count != 0)
                    {
                        foreach (var reserveEntry in reserveDictionary)
                        {
                            optionSelected = "RESERVE";
                            SeatSubtractor(optionSelected, new KeyValuePair<int, object>(reserveEntry.Key, reserveEntry.Value), licenseFileDictionary, groupDictionary, ref unspecifiedLicenseOrProductKey, ref err, ref debugPoint);

                            if (!string.IsNullOrEmpty(err))
                            {
                                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
                            }
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
                    daemonPortIsCnuFriendly,
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
                err = ex.Message switch
                {
                    "The value cannot be an empty string. (Parameter 'path')" =>
                        "Error: you left the license or options file text field blank.",
                    "Index was outside the bounds of the array." =>
                        $"Error: there is a formatting issue in your license/options file. This is the line in question's contents: \"{line}\"",
                    _ =>
                        $"Error: you managed to break something. How? Here's the automatic message from the Analyzer: {ex.Message} Debug point: {debugPoint}."
                };

                return (false, false, false, false, false, false, false, false, null, null, null, null, null, null, null, null, null, null, null, err);
            }
        }

        private static void SeatSubtractor(string optionSelected, KeyValuePair<int, dynamic> optionsEntry, Dictionary<int, Tuple<string, int, string, string, string, List<string>, int>> licenseFileDictionary, Dictionary<int, Tuple<string, string, int>> groupDictionary, ref bool unspecifiedLicenseOrProductKey, ref string? err, ref int debugPoint)
        {
            bool forceSubtraction = false;
            debugPoint = 27;
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
                // Cast optionsEntry.Value to the expected Tuple type.
                if (optionsEntry.Value is not Tuple<string, string, string, string, string, string> optionsData)
                {
                    err = "Error: optionsEntry.Value is not of the expected Tuple type in the Analyzer. Please report this on GitHub.";
                    return;
                }

                // Don't listen to Rider's lies. These can be null when the application is published, as well as the other 2 sets below.
                productName = optionsData.Item1 ?? "DefaultProductName";
                licenseNumber = optionsData.Item2 ?? "DefaultLicenseNumber";
                productKey = optionsData.Item3 ?? "DefaultProductKey";
                clientType = optionsData.Item4 ?? "DefaultClientType";
                clientSpecified = optionsData.Item5 ?? "DefaultClientSpecified";
                rawOptionLine = optionsData.Item6 ?? "DefaultRawOptionLine";
            }
            else if (optionSelected == "INCLUDEALL")
            {
                if (optionsEntry.Value is not Tuple<string, string, string> optionsData)
                {
                    err = "optionsEntry.Value is not of the expected Tuple type for INCLUDEALL.";
                    return;
                }

                clientType = optionsData.Item1 ?? "DefaultClientType";
                clientSpecified = optionsData.Item2 ?? "DefaultClientSpecified";
                rawOptionLine = optionsData.Item3 ?? "DefaultRawOptionLine";
            }
            else if (optionSelected == "RESERVE")
            {
                if (optionsEntry.Value is not Tuple<int, string, string, string, string, string, string> optionsData)
                {
                    err = "optionsEntry.Value is not of the expected Tuple type for RESERVE.";
                    return;
                }

                reserveSeatCount = optionsData.Item1;
                productName = optionsData.Item2 ?? "DefaultProductName";
                licenseNumber = optionsData.Item3 ?? "DefaultLicenseNumber";
                productKey = optionsData.Item4 ?? "DefaultProductKey";
                clientType = optionsData.Item5 ?? "DefaultClientType";
                clientSpecified = optionsData.Item6 ?? "DefaultClientSpecified";
                rawOptionLine = optionsData.Item7 ?? "DefaultRawOptionLine";
            }
            else
            {
                err = $"optionSelected is an invalid selection in the Analyzer. Please report this issue on GitHub. Debug point: {debugPoint}.";
            }

            debugPoint = 34;
            
            bool matchingProductFoundInLicenseFile = false;
            bool usableLicenseNumberOrProductKeyFoundInLicenseFile = false;
            int remainingSeatsToSubtract = 0; // The 0 has no significance and will be set accordingly later on, if used.
            bool doneSubtractingSeats = false;
            bool firstAttemptToSubtractSeats = true;
            List<Tuple<string, string>> linesThatHaveBeenRecorded = []; // string = the line itself, string = licenseFileProductKey...
            //... We're using the licenseFileProductKey so we can uniquely identify each INCREMENT.
            // You might be worried that this isn't enough information to separate distinct INCLUDE/whatever lines from each other, but since this List/Tuple is defined in this SeatSubtractor method, it shouldn't matter.

            List<int> licenseLinesReached = [];
            
            while (!doneSubtractingSeats)
            {
                debugPoint = 35;
                // Go through each line and subtract seats accordingly.
                foreach (var licenseFileEntry in licenseFileDictionary)
                {
                    bool needToGoToNextEntry = false;
                    int licenseLineIndex = licenseFileEntry.Key;
                    Tuple<string, int, string, string, string, List<string>, int> licenseFileData = licenseFileEntry.Value;

                    string licenseFileProductName = licenseFileData.Item1;
                    int licenseFileSeatCount = licenseFileData.Item2;
                    string licenseFileProductKey = licenseFileData.Item3;
                    string licenseFileLicenseOffering = licenseFileData.Item4;
                    string licenseFileLicenseNumber = licenseFileData.Item5;
                    List<string> linesThatSubtractSeats = licenseFileData.Item6;
                    int originalLicenseFileSeatCount = licenseFileData.Item7;
                    
                    // We use the license file's line index to detect if we've already tried subtracting seats. If we have, we should indicate so.
                    // Failing to do so will result in an NNU-only license file with an INCLUDEALL line being stuck here forever.
                    licenseLinesReached.Add(licenseLineIndex);
                    if (licenseLinesReached.Contains(licenseLineIndex) && optionSelected == "INCLUDEALL")
                    { firstAttemptToSubtractSeats = false;}

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
                                if (forceSubtraction) // Don't make sure search all day for something that doesn't exist, unless we must.
                                {
                                    if (!usableLicenseNumberOrProductKeyFoundInLicenseFile)
                                    {
                                        if (!string.IsNullOrEmpty(licenseNumber))
                                        {
                                            err = $"There is an issue with the options file: you have specified a license number, {licenseNumber}, which does not exist in the license file for the product {productName}.";
                                            return;
                                        }
                                        else if (!string.IsNullOrEmpty(productKey))
                                        {
                                            err = $"There is an issue with the options file: you have specified a product key, {productKey}, which does not exist in the license file.";
                                            return;
                                        }
                                        // ... this'll hopefully cover anything I missed.
                                        err = $"There is an issue with the options file: you have specified either a license number or product key, ({licenseNumber}, {productKey}), which does not exist in the license file.";
                                        return;
                                    }
                                }
                                continue; // If the option entry in question has specified a license number or product key (ew), then we actually need to find a matching license number/product key.
                            }
                            else // If you're here, your option entry does not use a license number/product key, so we'll check if the current license file license number/product key has any remaining seats we can use/subtract from.
                            {
                                if (licenseFileSeatCount == 0 && !forceSubtraction)
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

                        switch (optionSelected)
                        {
                            // RESERVE lines don't care about your clientType.
                            case "RESERVE":
                                {
                                    if (firstAttemptToSubtractSeats)
                                    {
                                        if (reserveSeatCount > licenseFileSeatCount && !forceSubtraction)
                                        {
                                            // Subtract as much as possible from licenseFileSeatCount.
                                            int seatsToSubtract = Math.Min(reserveSeatCount, licenseFileSeatCount);
                                            licenseFileSeatCount -= seatsToSubtract;

                                            // Calculate the remaining seats that couldn't be subtracted.
                                            remainingSeatsToSubtract = reserveSeatCount - seatsToSubtract;
                                            firstAttemptToSubtractSeats = false;

                                            // Record the line used to subtract this seat.
                                            linesThatSubtractSeats.Add(rawOptionLine);
                                            linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));

                                            needToGoToNextEntry = true;
                                        }
                                        else
                                        {
                                            // Subtract the appropriate number of seats.
                                            licenseFileSeatCount -= reserveSeatCount;

                                            // Record the line used to subtract this seat.
                                            linesThatSubtractSeats.Add(rawOptionLine);
                                            linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));

                                            doneSubtractingSeats = true;
                                            forceSubtraction = false;
                                        }
                                    }
                                    else
                                    {
                                        if (remainingSeatsToSubtract > licenseFileSeatCount)
                                        {
                                            if (licenseFileSeatCount != 0 || forceSubtraction)
                                            {
                                                int seatsToSubtract = Math.Min(remainingSeatsToSubtract, licenseFileSeatCount);
                                                licenseFileSeatCount -= remainingSeatsToSubtract;

                                                // Calculate the remaining seats that couldn't be subtracted.
                                                remainingSeatsToSubtract = reserveSeatCount - seatsToSubtract;

                                                bool needToSkipRawOptionLineRecording = false;
                                                foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                                {
                                                    if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                    {
                                                        needToSkipRawOptionLineRecording = true;
                                                        break;
                                                    }
                                                }

                                                if (!needToSkipRawOptionLineRecording)
                                                {
                                                    linesThatSubtractSeats.Add(rawOptionLine);
                                                    linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                                }

                                                if (forceSubtraction) { doneSubtractingSeats = true; forceSubtraction = false; }
                                            }
                                        }
                                        else
                                        {
                                            licenseFileSeatCount -= remainingSeatsToSubtract;

                                            bool needToSkipRawOptionLineRecording = false;
                                            foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                            {
                                                if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                {
                                                    needToSkipRawOptionLineRecording = true;
                                                    break;
                                                }
                                            }

                                            if (!needToSkipRawOptionLineRecording)
                                            {
                                                linesThatSubtractSeats.Add(rawOptionLine);
                                                linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                            }

                                            doneSubtractingSeats = true;
                                            forceSubtraction = false;
                                        }
                                    }
                                    break;
                                }
                            // INCLUDEALL should be simpler because we are subtracting a set seat count from every product listed, no matter what.
                            // Spreading out seats between multiple instances of the same product will not be done because of this.
                            case "INCLUDEALL":
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

                                            doneSubtractingSeats = true; // I see no reason this won't always be true. We have to subtract on CN and CNU licenses and setting this shouldn't pose any issues... right?
                                            forceSubtraction = false;
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

                                                groupName = groupName.Trim('"');
                                                groupName = groupName.Trim([' ', '\t']);
                                                clientSpecified = clientSpecified.Trim([' ', '\t']);

                                                if (groupName == clientSpecified)
                                                {
                                                    // Subtract the appropriate number of seats.
                                                    licenseFileSeatCount -= groupUserCount;

                                                    // Record the line used to subtract this seat.
                                                    linesThatSubtractSeats.Add(rawOptionLine);

                                                    doneSubtractingSeats = true;
                                                    forceSubtraction = false;
                                                    break;
                                                }
                                            }
                                        }
                                        // There is no subtraction that can be done because you've specified a client type that can be shared between any number of users.
                                        else if (clientType is "HOST_GROUP" or "HOST" or "DISPLAY" or "PROJECT" or "INTERNET") { }
                                        else
                                        {
                                            err = "There is an issue with the options file: you specified an invalid client type for an INCLUDEALL line.";
                                            return;
                                        }
                                    }
                                    // NNU licenses.
                                    else
                                    { // If we've already attempted to subtract seats and there are only NNU licenses remaining, then we have nothing left we can subtract from.
                                        if (!firstAttemptToSubtractSeats) { doneSubtractingSeats = true; }

                                    }
                                    break;
                                }
                            // optionSelected == INCLUDE
                            default:
                                switch (clientType)
                                {
                                    // Check that a user has actually been specified.
                                    case "USER" when string.IsNullOrWhiteSpace(clientSpecified):
                                        err = $"There is an issue with the options file: you have specified a USER to be able to use {licenseFileProductName}, " +
                                              "but you did not define the USER.";
                                        return;
                                    // Record the line used to subtract this seat.
                                    case "USER":
                                        {
                                            if (licenseFileSeatCount <= 0)
                                            {
                                                if (forceSubtraction)
                                                {
                                                    licenseFileSeatCount--;

                                                    bool needToSkipRawOptionLineRecording = false;
                                                    foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                                    {
                                                        if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                        {
                                                            needToSkipRawOptionLineRecording = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!needToSkipRawOptionLineRecording)
                                                    {
                                                        linesThatSubtractSeats.Add(rawOptionLine);
                                                        linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                                    }

                                                    doneSubtractingSeats = true;
                                                    forceSubtraction = false;
                                                }
                                                else { continue; }
                                            }
                                            else
                                            {
                                                licenseFileSeatCount--;

                                                bool needToSkipRawOptionLineRecording = false;
                                                foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                                {
                                                    if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                    {
                                                        needToSkipRawOptionLineRecording = true;
                                                        break;
                                                    }
                                                }

                                                if (!needToSkipRawOptionLineRecording)
                                                {
                                                    linesThatSubtractSeats.Add(rawOptionLine);
                                                    linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                                }

                                                doneSubtractingSeats = true;
                                                forceSubtraction = false;
                                            }
                                            break;
                                        }

                                    // Check that a group has actually been specified.
                                    case "GROUP" when string.IsNullOrWhiteSpace(clientSpecified):
                                        {
                                            if (!string.IsNullOrWhiteSpace(licenseNumber))
                                            {
                                                err = $"There is an issue with the options file: you have specified a GROUP to be able to use {licenseFileProductName} " +
                                                      $"for license {licenseNumber}, but you did not specify which GROUP.";
                                            }
                                            else
                                            {
                                                err = $"There is an issue with the options file: you have specified a GROUP to be able to use {licenseFileProductName}, but you did not specify which GROUP.";
                                            }
                                            return;
                                        }
                                    case "GROUP":
                                        {
                                            foreach (var optionsGroupEntry in groupDictionary)
                                            {
                                                // Load GROUP specifications.
                                                int optionsGroupLineIndex = optionsGroupEntry.Key;
                                                Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;

                                                string groupName = optionsGroupData.Item1;
                                                //string groupUsers = optionsGroupData.Item2;
                                                int groupUserCount = optionsGroupData.Item3;

                                                groupName = groupName.Trim('"');
                                                groupName = groupName.Trim([' ', '\t']);
                                                clientSpecified = clientSpecified.Trim([' ', '\t']);

                                                if (groupName == clientSpecified)
                                                {
                                                    // If we're going to get a negative seat count, we need to subtract as many seats as we can from as many matching products as we can...
                                                    // ... If you specified a license number, then it needs to match (we already checked for this earlier.) However, a product key...
                                                    // ... is always unique and therefore, we won't bother looking for another license file entry.
                                                    // If we've hit forceSubtraction, that means we've already gone through every license file entry and didn't find any other candidates...
                                                    // ... with a positive seat count (1 or greater), so we're just going to subtract it from whatever (likely the first instance of the product.)
                                                    if (firstAttemptToSubtractSeats)
                                                    {
                                                        if (groupUserCount > licenseFileSeatCount && !forceSubtraction)
                                                        {
                                                            // Subtract as much as possible from licenseFileSeatCount.
                                                            int seatsToSubtract = Math.Min(groupUserCount, licenseFileSeatCount);
                                                            licenseFileSeatCount -= seatsToSubtract;

                                                            // Calculate the remaining seats that couldn't be subtracted.
                                                            remainingSeatsToSubtract = groupUserCount - seatsToSubtract;
                                                            firstAttemptToSubtractSeats = false;

                                                            // Record the line used to subtract this seat.
                                                            linesThatSubtractSeats.Add(rawOptionLine);
                                                            linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));

                                                            needToGoToNextEntry = true;
                                                        }
                                                        else
                                                        {
                                                            // Subtract the appropriate number of seats.
                                                            licenseFileSeatCount -= groupUserCount;

                                                            // Record the line used to subtract this seat.
                                                            linesThatSubtractSeats.Add(rawOptionLine);
                                                            linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));

                                                            doneSubtractingSeats = true;
                                                            forceSubtraction = false;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (remainingSeatsToSubtract > licenseFileSeatCount)
                                                        {
                                                            if (licenseFileSeatCount != 0 || forceSubtraction)
                                                            {
                                                                int seatsToSubtract = Math.Min(remainingSeatsToSubtract, licenseFileSeatCount);
                                                                licenseFileSeatCount -= remainingSeatsToSubtract;

                                                                // Calculate the remaining seats that couldn't be subtracted.
                                                                remainingSeatsToSubtract = groupUserCount - seatsToSubtract;

                                                                // Record the line used to subtract this seat.
                                                                bool needToSkipRawOptionLineRecording = false;
                                                                foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                                                {
                                                                    if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                                    {
                                                                        needToSkipRawOptionLineRecording = true;
                                                                        break;
                                                                    }
                                                                }

                                                                if (!needToSkipRawOptionLineRecording)
                                                                {
                                                                    linesThatSubtractSeats.Add(rawOptionLine);
                                                                    linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                                                }

                                                                if (forceSubtraction) { doneSubtractingSeats = true; forceSubtraction = false; }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            licenseFileSeatCount -= remainingSeatsToSubtract;

                                                            bool needToSkipRawOptionLineRecording = false;
                                                            foreach (var recordedLineTuple in linesThatHaveBeenRecorded)
                                                            {
                                                                if (recordedLineTuple.Item1 == rawOptionLine && recordedLineTuple.Item2 == licenseFileProductKey)
                                                                {
                                                                    needToSkipRawOptionLineRecording = true;
                                                                    break;
                                                                }
                                                            }

                                                            if (!needToSkipRawOptionLineRecording)
                                                            {
                                                                linesThatSubtractSeats.Add(rawOptionLine);
                                                                linesThatHaveBeenRecorded.Add(Tuple.Create(rawOptionLine, licenseFileProductKey));
                                                            }

                                                            doneSubtractingSeats = true;
                                                            forceSubtraction = false;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    // You have a clientType that does not require subtraction because we cannot subtract a definitive number of seats from it.
                                    default:
                                        {
                                            doneSubtractingSeats = true;
                                        }
                                        break;
                                }
                                break;
                        }

                        licenseFileData = Tuple.Create(productName, licenseFileSeatCount, licenseFileData.Item3, licenseFileData.Item4, licenseFileData.Item5, linesThatSubtractSeats, originalLicenseFileSeatCount);
                        licenseFileDictionary[licenseLineIndex] = licenseFileData;
                        if (needToGoToNextEntry) { continue; }
                        if (optionSelected != "INCLUDEALL") { break; } // We don't need to go through any other products since we've already done seat subtraction.
                    }
                    else
                    {
                        if (forceSubtraction)
                        {
                            // You can't give away what you don't have.
                            if (!matchingProductFoundInLicenseFile && optionSelected != "INCLUDEALL")
                            {
                                err = $"There is an issue with the options file: you specified a product, {productName}, but this product is not in your license file. " +
                                        "Product names must match the ones found in the license file after the word INCREMENT. Any typos will result in this error being shown.";
                                return;
                            }
                        }
                    }
                }
                if (!doneSubtractingSeats)
                {
                    forceSubtraction = true;
                }
            }
        }

        // Method to check if the groups you're specifying in your options have also been defined.
        // Define a delegate that takes a tuple and returns the groupType and specified strings.
        private delegate (string groupType, string specified) TupleExtractor<in TTuple>(TTuple tuple);

        private static string? PerformGroupCheck<TKey, TTuple>(
            Dictionary<TKey, TTuple> optionsIndex,
            Dictionary<TKey, Tuple<string, string, int>> groupIndex,
            Dictionary<TKey, Tuple<string, string>> hostGroupIndex,
            TupleExtractor<TTuple> extractor,
            string lineType,
            ref int debugPoint) where TKey : notnull
        {
            debugPoint = 4;

            var firstEntry = optionsIndex.FirstOrDefault();
            if (firstEntry.Key == null)
            {
                return "The key of the first entry in optionsIndex is null.";
            }

            if (firstEntry.Value == null)
            {
                //return null;
                return $"The value of the first entry in optionsIndex is null. {lineType}";
            }

            debugPoint = 5;
            //return $"optionsIndex contains {optionsIndex.Count} entries.";
            foreach (var entry in optionsIndex)
            {
                debugPoint = 6;
                TTuple tuple = entry.Value;
                debugPoint = 7;
                string err = string.Empty;
                debugPoint = 8;

                // Use the extractor to get the groupType and specified strings.
                (string groupType, string specified) = extractor(tuple);

                specified = specified.Trim([' ', '\t']);

                switch (groupType)
                {
                    case "GROUP":
                        {
                            bool groupFound = false;
                            debugPoint = 9;
                            foreach (var optionsGroupEntry in groupIndex)
                            {
                                Tuple<string, string, int> optionsGroupData = optionsGroupEntry.Value;
                                string groupName = optionsGroupData.Item1;
                                string groupUsers = optionsGroupData.Item2;

                                debugPoint = 10;
                                if (string.IsNullOrEmpty(groupUsers))
                                {
                                    err = $"There is an issue with the options file: you attempted to use an empty GROUP. The GROUP name is {groupName}.";
                                    return err;
                                }
                                debugPoint = 11;
                                if (groupName == specified)
                                {
                                    debugPoint = 12;
                                    groupFound = true;
                                    break; // Matching GROUP found, exit the loop.
                                }
                            }

                            debugPoint = 13;
                            if (!groupFound)
                            {
                                debugPoint = 14;
                                // No matching group found.
                                string aOrAn = lineType == "RESERVE" ? "a" : "an"; // Grammar is important, kids!
                                debugPoint = 15;
                                if (specified.Contains("USER"))
                                {
                                    debugPoint = 16;
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
                            debugPoint = 17;
                            break;
                        }
                    case "HOST_GROUP":
                        {
                            bool hostGroupFound = false;

                            foreach (var optionsHostGroupEntry in hostGroupIndex)
                            {
                                Tuple<string, string> optionsHostGroupData = optionsHostGroupEntry.Value;
                                string hostGroupName = optionsHostGroupData.Item1;

                                hostGroupName = hostGroupName.Trim('"');
                                hostGroupName = hostGroupName.Trim([' ', '\t']);

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
                                err = $"There is an issue with the options file: you specified a {groupType} on {aOrAn} {lineType} line named \"{specified}\", " +
                                      $"but this {groupType} does not exist in your options file. Please check your {groupType}s for any typos. HOST_GROUP and GROUP are separate.";
                                return err;
                            }

                            break;
                        }
                    default:
                        continue; // No need to do this for other client types.
                }
                debugPoint = 18;
            }
            debugPoint = 19;
            return null;
        }
    }
}
