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
    let productName = dictionaryEntry?.productName ?? dictionaryEntry?.reserveProductName;
    let licenseNumber = dictionaryEntry?.licenseNumber ?? dictionaryEntry?.reserveLicenseNumber;
    let productKey = dictionaryEntry?.productKey ?? dictionaryEntry?.reserveProductKey;
    let clientType = dictionaryEntry?.clientType ?? dictionaryEntry?.reserveClientType;
    let clientSpecified = dictionaryEntry?.clientSpecified ?? dictionaryEntry?.reserveClientSpecified;
    let rawOptionLine = dictionaryEntry?.savedLine;

    let forceSeatSubtraction = false;
    let matchingProductFoundInLicenseFile = false;
    let usableLicenseNumberOrProductKeyFoundInLicenseFile = false;
    let remainingSeatsToSubtract = 0;
    let doneSubtractingSeats = false;
    let firstAttemptToSubtractSeats = true;
    let linesThatHaveBeenRecorded = [];
    let licenseLinesReached = [];

    while (doneSubtractingSeats === false) {
        // We have the information as to how many seats we need to subtract, so now we will start going through each line of the license file and subtract seats accordingly.
        let licenseFileDictionaryEntries = Object.entries(licenseFileDictionary);
        for (let [licenseFileDictionaryKey, licenseFileDictionaryEntry] of licenseFileDictionaryEntries) {

            let needToGoToNextEntry = false;
            let licenseLineIndex = licenseFileDictionaryKey;
            let licenseFileProductName = licenseFileDictionaryEntry.productName;
            let licenseFileSeatCount = licenseFileDictionaryEntry.seatCount;
            let licenseFileProductKey = licenseFileDictionaryEntry.productKey;
            let licenseFileLicenseOffering = licenseFileDictionaryEntry.licenseOffering;
            let licenseFileLicenseNumber = licenseFileDictionaryEntry.licenseNumber;
            let linesThatSubtractSeats = [];
            let originalLicenseFileSeatCount = licenseFileDictionaryEntry.originalLicenseFileSeatCount;

            // We use the license file's line index to detect if we've already tried subtracting seats. If we have, we should indicate so.
            // Failing to do so will result in an NNU-only license file with an INCLUDEALL line being stuck here forever.
            licenseLinesReached.push(licenseLineIndex);

            if (licenseLinesReached.includes(licenseLineIndex) && dictionaryToUseString === "INCLUDEALL") {
                firstAttemptToSubtractSeats = false;
            }

            // We start seat subtraction by checking to see if the product you're specifying exists in the license file.
            // Stupid JS being stupid.
            if (productName === undefined) {
                productName = "";
            }

            if (licenseFileProductName === undefined) {
                licenseFileProductName = "";
            }

            if ((productName.toLowerCase() ===  licenseFileProductName.toLowerCase()) || dictionaryToUseString === "INCLUDEALL") {
                matchingProductFoundInLicenseFile = true;

                if (licenseNumber === licenseFileLicenseNumber || productKey === licenseFileProductKey) {
                    usableLicenseNumberOrProductKeyFoundInLicenseFile = true;
                } else {
                    if (licenseNumber || licenseNumber.trim() || productKey || productKey.trim()) {
                        if (forceSeatSubtraction === true) { // Don't make sure search all day for something that doesn't exist, unless we must.
                            if (usableLicenseNumberOrProductKeyFoundInLicenseFile === false) {
                                if (licenseNumber || licenseNumber.trim()) {
                                    errorMessageFunction(`There is an issue with the options file: you have specified a license number, ${licenseNumber}, ` +
                                        `which does not exist in the license file for the product ${productName}.`);
                                    return;
                                } else if (productKey || productKey.trim()) {
                                    errorMessageFunction(`There is an issue with the options file: you have specified a product key, ${productKey}, ` +
                                        "which does not exist in the license file.");
                                    return;
                                }
                                errorMessageFunction("There is an issue with the options file: you have specified either a license number or product key, " +
                                    `(${licenseNumber}, ${productKey}), which does not exist in the license file.`);
                                return;
                            }
                        }
                        continue; // If the option entry in question has specified a license number or product key (ew), then we actually need to find a matching license number/product key.
                    } else { // If you're here, your option entry does not use a license number/product key...
                        // ...so we'll check if the current license file license number/product key has any remaining seats we can use/subtract from.
                        if (licenseFileSeatCount === 0 && forceSeatSubtraction === false) {
                            continue; // See if we can find another entry with the same product that does not have a seat count of 0.
                        } else {
                            if (dictionaryToUseString === "INCLUDEALL") {
                                window.unspecifiedLicenseOrProductKey = true;
                                usableLicenseNumberOrProductKeyFoundInLicenseFile = true;
                            }
                        }
                    }
                }

                switch (dictionaryToUseString) {
                    case "RESERVE":
                        if (firstAttemptToSubtractSeats) {
                            if (reserveSeatCount > licenseFileLicenseNumber && forceSeatSubtraction === false) {
                                // Subtract as much as possible from licenseFileSeatCount.
                                let seatsToSubtract = (reserveSeatCount - licenseFileSeatCount);
                                licenseFileSeatCount -= seatsToSubtract;

                                // Calculate the remaining seats that couldn't be subtracted, if any.
                                remainingSeatsToSubtract = (reserveSeatCount - seatsToSubtract);
                                firstAttemptToSubtractSeats = false;

                                // Record the line used to subtract this seat.
                                linesThatSubtractSeats.push(rawOptionLine);
                                linesThatHaveBeenRecorded.push([rawOptionLine, licenseFileProductKey]);

                                needToGoToNextEntry = true;
                            } else {
                                licenseFileSeatCount -= reserveSeatCount;

                                linesThatSubtractSeats.push(rawOptionLine);
                                linesThatHaveBeenRecorded.push([rawOptionLine, licenseFileProductKey]);

                                doneSubtractingSeats = true;
                                forceSeatSubtraction = false;
                            }
                        } else {
                            if (remainingSeatsToSubtract > licenseFileSeatCount) {
                                if (licenseFileSeatCount !== 0 || forceSeatSubtraction === true) {
                                    let seatsToSubtract = (remainingSeatsToSubtract - licenseFileSeatCount);
                                    licenseFileSeatCount -= remainingSeatsToSubtract;

                                    // Calculate the remaining seats that couldn't be subtracted.
                                    remainingSeatsToSubtract = (reserveSeatCount - seatsToSubtract);
                                    let needToSkipRawOptionLineRecording = false;

                                    for (let recordedLine of linesThatHaveBeenRecorded) {
                                        if (recordedLine[0] === rawOptionLine && recordedLine[1] === licenseFileProductKey) {
                                            needToSkipRawOptionLineRecording = true;
                                            break;
                                        }
                                    }

                                    if (needToSkipRawOptionLineRecording === false) {
                                        linesThatSubtractSeats.push(rawOptionLine);
                                        linesThatHaveBeenRecorded.push([rawOptionLine, licenseFileProductKey]);
                                    }
                                }
                            }
                        }
                        break;
                    case "INCLUDEALL":
                        break;
                    case "INCLUDE":
                        break;
                    default:
                        // What's the meaning of this? Who are you and how did you get in here?
                        return;

                }

                // Ready for entry into the object.
                licenseFileDictionary[licenseLineIndex] = {
                    productName: licenseFileProductName,
                    seatCount: licenseFileSeatCount,
                    productKey: licenseFileProductKey,
                    licenseOffering: licenseFileLicenseOffering,
                    licenseNumber: licenseFileLicenseNumber,
                    linesThatSubtractSeats,
                    originalLicenseFileSeatCount
                };

                if (needToGoToNextEntry === true) {
                    continue;
                }

                if (dictionaryToUseString !== "INCLUDEALL") {
                    break;
                }
            } else {
                if (forceSeatSubtraction === true) {
                    // You can't give away what you don't have.
                    if (matchingProductFoundInLicenseFile === false && dictionaryToUseString !== "INCLUDEALL") {
                        errorMessageFunction(`There is an issue with the options file: you specified a product, ${productName}, but this product is not in your license file. ` +
                            "Product names must match the ones found in the license file after the word INCREMENT. Any typos will result in this error being shown.");
                        return;
                    }
                }
            }
        }
        if (doneSubtractingSeats === false) {
            forceSeatSubtraction = true;
        }
    }
}