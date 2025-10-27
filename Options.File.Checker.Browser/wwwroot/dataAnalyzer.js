function analyzeData() {
    let line;
    let unspecifiedLicenseOrProductKey = false;
    let optionSelected;

    try {
        if (includeDictionary !== null && groupDictionary !== null && hostGroupDictionary !== null && excludeDictionary !== null && reserveDictionary !== null && includeAllDictionary !== null && excludeAllDictionary !== null) {
            let dictionaryToUse;
            if (Object.keys(includeDictionary).length > 0) {
                dictionaryToUse = includeDictionary;
                let dictionaryTypeToUse = "INCLUDE";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(includeBorrowDictionary).length > 0) {
                dictionaryToUse = includeBorrowDictionary;
                let dictionaryTypeToUse = "INCLUDE_BORROW";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(includeAllDictionary).length > 0) {
                dictionaryToUse = includeAllDictionary;
                let dictionaryTypeToUse = "INCLUDEALL";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(excludeDictionary).length > 0) {
                dictionaryToUse = excludeDictionary;
                let dictionaryTypeToUse = "EXCLUDE";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(excludeBorrowDictionary).length > 0) {
                dictionaryToUse = excludeBorrowDictionary;
                let dictionaryTypeToUse = "EXCLUDE_BORROW";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(excludeAllDictionary).length > 0) {
                dictionaryToUse = excludeAllDictionary;
                let dictionaryTypeToUse = "EXCLUDEALL";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(reserveDictionary).length > 0) {
                dictionaryToUse = reserveDictionary;
                let dictionaryTypeToUse = "RESERVE";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }
            if (Object.keys(maxDictionary).length > 0) {
                dictionaryToUse = maxDictionary;
                let dictionaryTypeToUse = "MAX";
                performGroupCheck(dictionaryToUse, dictionaryTypeToUse)
            }

        } else {
            errorMessageFunction("Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub." +
                `The line in question reads as this: \"${currentLine}\".`);
            return;
        }

        console.log("shut up, something happens, okay?")
    } catch (rawErrorMessage) {
        errorMessageFunction(`Something broke really badly in the Analyzer. What a bummer. Here's the automatically generated error: ${rawErrorMessage}`);
    }
}

function performGroupCheck(dictionaryToUse, dictionaryTypeToUse) {

    let entries = Object.entries(dictionaryToUse);
    for (let [dictionaryKey, dictionaryEntry] of entries) {

        let clientType;
        let clientSpecified;

        switch (dictionaryTypeToUse) {
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
                let hostGroupEntries = Object.entries(hostGroupDictionary);

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
                    let aOrAn = (dictionaryTypeToUse === "RESERVE" || dictionaryTypeToUse === "MAX") ? "a" : "an"; // Grammar is important, kids!
                    errorMessageFunction(`There is an issue with the options file: you specified a ${clientType} on ${aOrAn} ${dictionaryTypeToUse} named \"${clientSpecified}\", ` +
                        `but this ${clientType} does not exist in your options file. Please check your ${clientType}s for any typos. HOST_GROUP and GROUP are separate.`);
                    return;
                }
                break;

            case "HOST_GROUP":
                hostGroupFound = false;
                break;
            default:
                continue;
        }
    }
}