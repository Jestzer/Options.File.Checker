function analyzeData() {
    let line;
    let unspecifiedLicenseOrProductKey = false;
    let optionSelected;

    try {
        // Let's start with ensuring that the arrays exist and that any groups you've specified exist.
        if (includeDictionary !== null && groupDictionary !== null && hostGroupDictionary !== null && excludeDictionary !== null && reserveDictionary !== null && includeAllDictionary !== null && excludeAllDictionary !== null) {
            let dictionaryToUse;
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


        console.log("shut up, something happens, okay?")
    } catch (rawErrorMessage) {
        errorMessageFunction(`Something broke really badly in the Analyzer. What a bummer. Here's the automatically generated error: ${rawErrorMessage}`);
    }
}

function performGroupCheck(dictionaryToUse, dictionaryToUseString) {

    let entries = Object.entries(dictionaryToUse);
    for (let [dictionaryKey, dictionaryEntry] of entries) {

        let clientType;
        let clientSpecified;

        switch (dictionaryToUseString) {
            case "INCLUDE":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "INCLUDE_BORROW":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "INCLUDEALL":
                [clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "EXCLUDE":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "EXCLUDE_BORROW":
                [clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "EXCLUDEALL":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "RESERVE":
                [reserveSeatsNumber, productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "MAX":
                [maxSeats, productName, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
        }

        switch (clientType) {
            case "GROUP":
                let groupName;
                let groupUsers;
                let matchingGroupFound = false;
                let groupEntries = Object.entries(groupDictionary);
                clientSpecified = clientSpecified.replace('"', '')
                clientSpecified = clientSpecified.replace('\t', '')

                for (let [groupDictionaryKey, groupEntry] of groupEntries) {
                    [groupName, groupUsers, groupUserCount] = groupEntry

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
                let hostGroupName;
                let hostGroupUsers;
                let matchingHostGroupFound = false;
                let hostGroupEntries = Object.entries(hostGroupDictionary);

                for (let [hostGroupDictionaryKey, hostGroupEntry] of hostGroupEntries) {
                    [hostGroupName, hostGroupUsers, hostGroupUserCount] = hostGroupEntry

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