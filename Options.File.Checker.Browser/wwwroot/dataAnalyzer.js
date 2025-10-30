function analyzeData() {
    let dictionaryToUse;
    window.unspecifiedLicenseOrProductKey = false;

    try {
        // Let's start with ensuring that the arrays exist and that any groups you've specified exist.
        if (includeDictionary !== null && groupDictionary !== null && hostGroupDictionary !== null && excludeDictionary !== null && reserveDictionary !== null && includeAllDictionary !== null && excludeAllDictionary !== null) {
            if (Object.keys(includeDictionary).length > 0) {
                dictionaryToUse = includeDictionary;
                let dictionaryToUseString = "INCLUDE";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(includeBorrowDictionary).length > 0) {
                dictionaryToUse = includeBorrowDictionary;
                let dictionaryToUseString = "INCLUDE_BORROW";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(includeAllDictionary).length > 0) {
                dictionaryToUse = includeAllDictionary;
                let dictionaryToUseString = "INCLUDEALL";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(excludeDictionary).length > 0) {
                dictionaryToUse = excludeDictionary;
                let dictionaryToUseString = "EXCLUDE";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(excludeBorrowDictionary).length > 0) {
                dictionaryToUse = excludeBorrowDictionary;
                let dictionaryToUseString = "EXCLUDE_BORROW";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(excludeAllDictionary).length > 0) {
                dictionaryToUse = excludeAllDictionary;
                let dictionaryToUseString = "EXCLUDEALL";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(reserveDictionary).length > 0) {
                dictionaryToUse = reserveDictionary;
                let dictionaryToUseString = "RESERVE";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (Object.keys(maxDictionary).length > 0) {
                dictionaryToUse = maxDictionary;
                let dictionaryToUseString = "MAX";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }

        } else {
            errorMessageFunction("Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub." +
                `The line in question reads as this: \"${currentLine}\".`);
            return;
        }

        // Now we may subtract seats.
        if (Object.keys(includeDictionary).length > 0) {
            dictionaryToUse = includeDictionary;
            let dictionaryToUseString = "INCLUDE";
            let dictionaryEntries = Object.entries(dictionaryToUse);
            for (let [dictionaryKey, dictionaryEntry] of dictionaryEntries) {
                seatSubtractor(dictionaryToUse, dictionaryToUseString, dictionaryEntry, dictionaryEntries);
            }
        }

        if (Object.keys(includeAllDictionary).length > 0) {
            dictionaryToUse = includeAllDictionary;
            let dictionaryToUseString = "INCLUDEALL";
            let dictionaryEntries = Object.entries(dictionaryToUse);
            for (let [dictionaryKey, dictionaryEntry] of dictionaryEntries) {
                seatSubtractor(dictionaryToUse, dictionaryToUseString, dictionaryEntry, dictionaryEntries);
            }
        }

        if (Object.keys(reserveDictionary).length > 0) {
            dictionaryToUse = reserveDictionary;
            let dictionaryToUseString = "RESERVE";
            let dictionaryEntries = Object.entries(dictionaryToUse);
            for (let [dictionaryKey, dictionaryEntry] of dictionaryEntries) {
                seatSubtractor(dictionaryToUse, dictionaryToUseString, dictionaryEntry, dictionaryEntries);
            }
        }

        // If you're only using NNU license(s), we need to make sure you've included at LEAST one INCLUDE line...
        // ...We'll first check if you're using an NNU only license. If you are, then we'll see if you have any INCLUDE lines...
        // ...If you do, then we'll make sure at least one of those INCLUDE lines uses a GROUP or USER.
        let nnuExclusiveLicense = true;
        let licenseFileDictionaryEntries = Object.entries(licenseFileDictionary);

        for (let [licenseFileDictionaryKey, licenseFileDictionaryEntry] of licenseFileDictionaryEntries) {

            let licenseFileLicenseOffering = licenseFileDictionaryEntry.licenseOffering;

            if (licenseFileLicenseOffering !== "NNU") {
                nnuExclusiveLicense = false;
                break;
            }
        }

        if (nnuExclusiveLicense === true) {

            if (Object.keys(includeDictionary).length > 0) {
                errorMessageFunction("There is an issue with the options file: you have no INCLUDE lines with an all-NNU license. You need these to use an NNU license.");
                return;
            }

            let foundValidIncludeLine = false;
            let includeDictionaryEntries = Object.entries(includeDictionary);

            for (let [includeDictionaryKey, includeDictionaryEntry] of includeDictionaryEntries) {

                let includeClientSpecified = includeDictionaryEntry.clientSpecified;

                if (includeClientSpecified === "USER" || includeClientSpecified === "GROUP") {
                    foundValidIncludeLine = true;
                    break;
                }
            }
            if (foundValidIncludeLine === false) {
                errorMessageFunction("There is an issue with the options file: you have no INCLUDE lines with a USER or GROUP. You need these to use an NNU license.");
            }
        }
    } catch (rawErrorMessage) {
        errorMessageFunction(`Something broke really badly in the Analyzer. What a bummer. Here's the automatically generated error: ${rawErrorMessage}.`);
    }
}

function performGroupCheck(dictionaryToUse, dictionaryToUseString) {

    let dictionaryEntries = Object.entries(dictionaryToUse);
    for (let [dictionaryKey, dictionaryEntry] of dictionaryEntries) {

        let clientType = dictionaryEntry.clientType;
        let clientSpecified = dictionaryEntry.clientSpecified;

        switch (clientType) {
            case "GROUP":
                let matchingGroupFound = false;
                let groupEntries = Object.entries(groupDictionary);
                clientSpecified = clientSpecified.replace('"', '')
                clientSpecified = clientSpecified.replace('\t', '')

                for (let [groupDictionaryKey, groupEntry] of groupEntries) {

                    let groupName = groupEntry.groupName;
                    let groupUsers = groupEntry.combinedUsers ?? groupEntry.groupUsers;

                    if (!groupUsers || !groupUsers.trim()) {
                        errorMessageFunction(`There is an issue with the options file: you attempted to use an empty GROUP. The GROUP name is ${groupName}.`);
                        return;
                    }

                    if (groupName === clientSpecified) {
                        matchingGroupFound = true;
                        break;
                    }
                }

                if (matchingGroupFound === false) {
                    let aOrAn = (dictionaryToUseString === "RESERVE" || dictionaryToUseString === "MAX") ? "a" : "an"; // Grammar is important, kids!
                    errorMessageFunction(`There is an issue with the options file: you specified a ${clientType} on ${aOrAn} ${dictionaryToUseString} named \"${clientSpecified}\", ` +
                        `but this ${clientType} does not exist in your options file. Please check your ${clientType}s for any typos. HOST_GROUP and GROUP are separate.`);
                    return;
                }
                break;

            case "HOST_GROUP":
                let matchingHostGroupFound = false;
                let hostGroupEntries = Object.entries(hostGroupDictionary);

                for (let [hostGroupDictionaryKey, hostGroupEntry] of hostGroupEntries) {

                    let hostGroupName = hostGroupEntry.hostGroupName;
                    let hostGroupUsers = hostGroupEntry.combinedUsers ?? hostGroupEntry.hostGroupClientSpecified;

                    hostGroupName = hostGroupName.replace('"', '');
                    hostGroupName = hostGroupName.replace('\t', '');

                    if (!hostGroupUsers || !hostGroupUsers.trim()) {
                        errorMessageFunction(`There is an issue with the options file: you attempted to use an empty GROUP. The GROUP name is ${hostGroupName}.`);
                        return;
                    }

                    if (hostGroupName === clientSpecified) {
                        matchingHostGroupFound = true;
                        break;
                    }
                }

                if (matchingHostGroupFound === false) {
                    let aOrAn = (dictionaryToUseString === "RESERVE" || dictionaryToUseString === "MAX") ? "a" : "an"; // Grammar is important, kids!
                    errorMessageFunction(`There is an issue with the options file: you specified a ${clientType} on ${aOrAn} ${dictionaryToUseString} named \"${clientSpecified}\", ` +
                        `but this ${clientType} does not exist in your options file. Please check your ${clientType}s for any typos. HOST_GROUP and GROUP are separate.`);
                    return;
                }
                break;
            default:
            // continue;
        }
    }
}

function seatSubtractor(dictionaryToUse, dictionaryToUseString, dictionaryEntry, dictionaryEntries) {

    let reserveSeatCount = dictionaryEntry?.reserveSeatsNumber ?? 0;
    let productName = dictionaryEntry?.productName ?? dictionaryEntry?.reserveProductName ?? "No productName found. :(";
    let licenseNumber = dictionaryEntry?.licenseNumber ?? dictionaryEntry?.reserveLicenseNumber ?? "No licenseNumber found. :(";
    let productKey = dictionaryEntry?.productKey ?? dictionaryEntry?.reserveProductKey ?? "No productKey found. :(";
    let clientType = dictionaryEntry?.clientType ?? dictionaryEntry?.reserveClientType ?? "No clientType found. :(";
    let clientSpecified = dictionaryEntry?.clientSpecified ?? dictionaryEntry?.reserveClientSpecified ?? "No clientSpecified found. :(";
    let rawOptionLine = dictionaryEntry?.savedLine ?? "No rawOptionLine found. :(";

    let forceSeatSubtraction = false;
    let matchingProductFoundInLicenses = false;
    let remainingSeatsToSubtract = 0;
    let doneSubtractingSeats = false;
    let firstAttemptToSubtractSeats = true;
    let linesThatHaveBeenRecorded = [];

    while (doneSubtractingSeats === false) {

    }
}