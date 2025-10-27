function analyzeData() {
    let line;
    let unspecifiedLicenseOrProductKey = false;
    let optionSelected;

    try {
        if (includeDictionary !== null && groupDictionary !== null && hostGroupDictionary !== null && excludeDictionary !== null && reserveDictionary !== null && includeAllDictionary !== null && excludeAllDictionary !== null) {

        }
        else {
            errorMessageFunction("Apparently one of the dictionaries in the Analyzer is null and therefore, the code in it cannot proceed. Please submit an issue for this on GitHub." +
                `The line in question reads as this: \"${currentLine}\".`);
            return;
        }
    } catch (rawErrorMessage) {


    }
}