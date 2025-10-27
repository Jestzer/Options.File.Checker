function analyzeData() {
    let line;
    let unspecifiedLicenseOrProductKey = false;
    let optionSelected;

    try {
        if (includeDictionary !== null && groupDictionary !== null && hostGroupDictionary !== null && excludeDictionary !== null && reserveDictionary !== null && includeAllDictionary !== null && excludeAllDictionary !== null) {
            let dictionaryToUse;
            if (includeDictionary.count !== 0) {
                dictionaryToUse = includeDictionary;
                let dictionaryToUseString = "includeDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (includeBorrowDictionary.count !== 0) {
                dictionaryToUse = includeBorrowDictionary;
                let dictionaryToUseString = "includeBorrowDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (includeAllDictionary.count !== 0) {
                dictionaryToUse = includeAllDictionary;
                let dictionaryToUseString = "includeAllDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (excludeDictionary.count !== 0) {
                dictionaryToUse = excludeDictionary;
                let dictionaryToUseString = "excludeDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (excludeBorrowDictionary.count !== 0) {
                dictionaryToUse = excludeBorrowDictionary;
                let dictionaryToUseString = "excludeBorrowDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (excludeAllDictionary.count !== 0) {
                dictionaryToUse = excludeAllDictionary;
                let dictionaryToUseString = "excludeAllDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }
            if (reserveDictionary.count !== 0) {
                dictionaryToUse = reserveDictionary;
                let dictionaryToUseString = "reserveDictionary";
                performGroupCheck(dictionaryToUse, dictionaryToUseString)
            }

        } else {
            errorMessageFunction("Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub." +
                `The line in question reads as this: \"${currentLine}\".`);
            return;
        }
    } catch (rawErrorMessage) {
        errorMessageFunction(`\`Something broke really badly in the Analyzer. What a bummer. Here's the automatically generated error: ${rawErrorMessage}`);
    }
}

function performGroupCheck(dictionaryToUse, dictionaryToUseString) {

    let entries = Object.entries(dictionaryToUse);
    for (let [dictionaryKey, dictionaryEntry] of entries) {

        let clientType;
        let clientSpecified;
        switch (dictionaryToUseString) {
            case "includeDictionary":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "includeBorrowDictionary":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "includeAllDictionary":
                [clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "excludeDictionary":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "excludeBorrowDictionary":
                [clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "excludeAllDictionary":
                [productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
            case "reserveDictionary":
                [reserveSeatsNumber, productName, licenseNumber, productKey, clientType, clientSpecified, currentLine] = dictionaryEntry;
                break;
        }

        switch (clientType) {
            case "GROUP":
                break;
            case "HOST_GROUP":
                break;
            default:
                continue;
        }
    }
}