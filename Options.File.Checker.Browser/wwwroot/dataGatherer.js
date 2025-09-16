function gatherData() {
    window.serverLineHasPort = true;
    window.daemonLineHasPort = true;
    window.daemonPortIsCnuFriendly = false;
    window.caseSensitivity = true;
    window.optionsFileUsesMatlabParallelServer = false;
    window.wildCardsAreUsed = false;
    window.ipAddressesAreUsed = false;
    window.licenseFileMap = new Map([])
    window.includeMap = new Map([])
    window.includeBorrowMap = new Map([])
    window.includeAllMap = new Map([])
    window.excludeMap = new Map([])
    window.excludeBorrowMap = new Map([])
    window.excludeAllMap = new Map([])
    window.reserveMap = new Map([])
    window.maxMap = new Map([])
    window.groupMap = new Map([])
    window.hostGroupMap = new Map([])
    // window.errorMessage = "No error message set."
    window.productExpirationDate = "No product expiration date set."
    window.currentLine = "The variable 'line' is empty."

    // Remove line breaks from license file.
    window.licenseFileText = window.licenseFileRawText
        .replace(/\\\r\n/g, '')
        .replace(/\\\n/g, '');

    // Break into lines, for later usage.
    const licenseFileContentsLines = window.licenseFileText.split(/\r\n|\r|\n/);

    // Same process for options file.
    window.optionsFileText = window.optionsFileRawText
        .replace(/\\\r\n/g, '')
        .replace(/\\\n/g, '');

    const optionsFileContentsLines = optionsFileText.split(/\r\n|\r|\n/);

    // Let's start checking for obvious issues.
    if (!window.optionsFileText.includes("INCLUDE") && !window.optionsFileText.includes("EXCLUDE") && !window.optionsFileText.includes("RESERVE")
        && !window.optionsFileText.includes("MAX") && !window.optionsFileText.includes("LINGER") && !window.optionsFileText.includes("LOG") &&
        !window.optionsFileText.includes("TIMEOUT")) {
        errorMessageFunction("There is an issue with the options file: it is likely not an options file or contains no usable content.")
        return;
    }
    console.log("Here be dragons.")
}