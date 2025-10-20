function gatherData() {
    // Set/reset everything.
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
    window.currentLine = "window.currentLine has not been set. :("

    // Variables that don't need to be set app-wide.

    // Regex.
    const portEqualsRegex = /port=/gi;
    const optionsEqualsRegex = /options=/gi;
    const commentedBeginLineRegex = /# BEGIN--------------/gi;
    const keyEqualsRegex = /key=/gi;
    const assetInfoRegex = /asset_info=/gi;
    const assetInfoWithNumberRegex = /asset_info=(\S+)/i;
    const licenseNumberRegex = /^[^Rab_\d]+$/g;
    const licenseNumberSnRegex = /SN=(\S+)/i;
    const ipAddressRegex = /\d{2,3}\./g;
    const whiteSpaceRegex = /\s+/g;

    // Other.
    let containsPLP = false;
    let serverLineCount = 0;
    let daemonLineCount = 0;
    let productLinesHaveBeenReached = false;
    const masterProductList = ["Aerospace_Blockset", "Aerospace_Toolbox", "Antenna_Toolbox", "Audio_System_Toolbox", "Automated_Driving_Toolbox", "AUTOSAR_Blockset", "Bioinformatics_Toolbox", "Bluetooth_Toolbox", "C2000_Blockset", "Communication_Blocks", "Communication_Toolbox", "Compiler", "Control_Toolbox", "Curve_Fitting_Toolbox", "Data_Acq_Toolbox", "Database_Toolbox", "DDS_Blockset", "Deep_Learning_HDL_Toolbox", "Dial_and_Gauge_Blocks", "Distrib_Computing_Toolbox", "DSP_HDL_Toolbox", "Econometrics_Toolbox", "EDA_Simulator_Link", "Embedded_IDE_Link", "Embedded_Target_c2000", "Embedded_Target_MPC555", "Excel_Link", "Extend_Symbolic_Toolbox", "Filter_Design_HDL_Coder", "Filter_Design_Toolbox", "Fin_Derivatives_Toolbox", "Fin_Instruments_Toolbox", "Financial_Toolbox", "Fixed_Income_Toolbox", "Fixed_Point_Toolbox", "Fixed-Point_Blocks", "Fuzzy_Toolbox", "GADS_Toolbox", "Garch_Toolbox", "GPU_Coder", "IDE_Link_MU", "Identification_Toolbox", "Image_Acquisition_Toolbox", "Image_Toolbox", "Instr_Control_Toolbox", "Lidar_Toolbox", "Link_for_Incisive", "Link_for_VisualDSP", "LTE_HDL_Toolbox", "LTE_Toolbox", "MAP_Toolbox", "MATLAB", "MATLAB_5G_Toolbox", "MATLAB_Builder_for_dot_Net", "MATLAB_Builder_for_Java", "MATLAB_Coder", "MATLAB_Distrib_Comp_Engine", "MATLAB_Excel_Builder", "MATLAB_Production_Server", "MATLAB_Report_Gen", "MATLAB_Test", "MATLAB_Web_App_Server", "MBC_Toolbox", "Medical_Imaging_Toolbox", "Mixed_Signal_Blockset", "Motor_Control_Blockset", "MPC_Toolbox", "Navigation_Toolbox", "NCD_Toolbox", "Neural_Network_Toolbox", "OPC_Toolbox", "Optimization_Toolbox", "PDE_Toolbox", "Phased_Array_System_Toolbox", "Polyspace_BF", "Polyspace_BF_Access", "Polyspace_BF_Server", "PolySpace_Bug_Finder", "PolySpace_Bug_Finder_Engine", "PolySpace_Client_ADA", "PolySpace_Client_C_CPP", "Polyspace_CP_Access", "Polyspace_CP_Server", "PolySpace_Server_ADA", "PolySpace_Server_C_CPP", "Polyspace_Test", "PolySpace_Model_Link_SL", "PolySpace_Model_Link_TL", "PolySpace_UML_Link_RH", "Power_System_Blocks", "Powertrain_Blockset", "Pred_Maintenance_Toolbox", "Radar_Toolbox", "Real-Time_Win_Target", "Real-Time_Workshop", "Reinforcement_Learn_Toolbox", "RF_Blockset", "RF_PCB_Toolbox", "RF_Toolbox", "Risk_Management_Toolbox", "RoadRunner", "RoadRunner_Asset_Library", "RoadRunner_HD_Scene_Builder", "RoadRunner_Scenario", "Robotics_System_Toolbox", "Robust_Toolbox", "ROS_Toolbox", "RTW_Embedded_Coder", "Satellite_Comm_Toolbox", "Sensor_Fusion_and_Tracking", "SerDes_Toolbox", "Signal_Blocks", "Signal_Integrity_Toolbox", "Signal_Toolbox", "SimBiology", "SimDriveline", "SimElectronics", "SimEvents", "SimHydraulics", "SimMechanics", "Simscape", "Simscape_Battery", "SIMULINK", "Simulink_Accelerator", "Simulink_Code_Inspector", "Simulink_Compiler", "Simulink_Control_Design", "Simulink_Coverage", "Simulink_Design_Optim", "Simulink_Design_Verifier", "Simulink_HDL_Coder", "Simulink_Param_Estimation", "Simulink_PLC_Coder", "SIMULINK_Report_Gen", "Simulink_Requirements", "Simulink_Test", "SL_Verification_Validation", "SoC_Blockset", "Spline_Toolbox", "Stateflow", "Stateflow_Coder", "Statistics_Toolbox", "Symbolic_Toolbox", "System_Composer", "SystemTest", "Target_Support_Package", "Text_Analytics_Toolbox", "TMW_Archive", "Trading_Toolbox", "UAV_Toolbox", "Vehicle_Dynamics_Blockset", "Vehicle_Network_Toolbox", "Video_and_Image_Blockset", "Virtual_Reality_Toolbox", "Vision_HDL_Toolbox", "Wavelet_Toolbox", "Wireless_Testbench", "WLAN_System_Toolbox", "XPC_Embedded_Option", "XPC_Target"]

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

    // No valid options found in options file.
    if (!window.optionsFileText.includes("INCLUDE") && !window.optionsFileText.includes("EXCLUDE") && !window.optionsFileText.includes("RESERVE")
        && !window.optionsFileText.includes("MAX") && !window.optionsFileText.includes("LINGER") && !window.optionsFileText.includes("LOG") &&
        !window.optionsFileText.includes("TIMEOUT")) {
        errorMessageFunction("There is an issue with the options file: it is likely not an options file or contains no usable content.")
        return;
    }

    // License file does not contain any products.
    if (!window.licenseFileText.includes("INCREMENT")) {
        errorMessageFunction("There is an issue with the license file: it is either not a license file or is corrupted.")
        return;
    }

    // See the error message for details.
    if (window.licenseFileText.includes("lo=IN") || window.licenseFileText.includes("lo=DC") || window.licenseFileText.includes("lo=CIN")) {
        errorMessageFunction("There is an issue with the license file: it contains an Individual or Designated Computer license, " +
            "which cannot use an options file.")
        return;
    }

    // See the error message for details.
    if (window.licenseFileText.includes("CONTRACT_ID=")) {
        errorMessageFunction("There is an issue with the license file: it contains at least 1 non-MathWorks product.")
        return;
    }

    // Now that you've passed, we'll go through each line of the license file to gather information needed for analysis.
    for (let licenseLineIndex = 0; licenseLineIndex < licenseFileContentsLines.length; licenseLineIndex++) {
        window.currentLine = licenseFileContentsLines[licenseLineIndex];
        let productName = "No productName :(";
        let seatCount = 0;
        let plpLicenseNumber = "No plpLicenseNumber :(";

        if (window.currentLine.trimStart().startsWith("SERVER")) {
            if (productLinesHaveBeenReached) {
                errorMessageFunction("There is an issue with the license file: your SERVER line(s) are listed after a product.")
                return;
            }
            serverLineCount++
            let lineParts = currentLine.split(" ");
            let theWordServer = lineParts[0];
            let serverHostID = lineParts[2];

            if (theWordServer !== "SERVER") {
                errorMessageFunction("There is an issue with the license file: it does not start with the word SERVER.")
                return;
            }

            if (serverHostID === "27000" || serverHostID === "27001" || serverHostID === "27010") {
                errorMessageFunction("There is an issue with the license file: you have likely omitted your Host ID and attempted to specify a SERVER port number. " +
                    "Because you have omitted the Host ID, the port you've attempted to specify will not be used.")
                return;
            }

            switch (lineParts.length) {
                case 0:
                case 1:
                case 2:
                    errorMessageFunction("There is an issue with the license file: you are missing information from your SERVER line. See documentation online on how to format it.")
                    return;
                case 3:
                    window.serverLineHasPort = false;
                    break;
                case 4:
                    let serverPort = Number(lineParts[3]);
                    if (!Number.isInteger(serverPort)) {
                        errorMessageFunction("There is an issue with the license file: you have stray information on your SERVER line.")
                        return;
                    }

                    if (!serverHostID.includes('INTERNET=') && serverHostID.length !== 12) {
                        errorMessageFunction("There is an issue with the license file: you have not specified your Host ID correctly.")
                        return;
                    }
                    // Congrats, you /likely/ have not made any mistakes on your SERVER line.
                    break;
                case 5:
                    if (lineParts[4] === "") {
                        continue; // Your stray space shall be ignored... for now.
                    } else {
                        errorMessageFunction("There is an issue with the license file: you have stray information on your SERVER line.")
                        return;
                    }
                default:
                    errorMessageFunction("There is an issue with the license file: you have stray information on your SERVER line.")
                    return;
            }
        } else if (window.currentLine.trimEnd().startsWith("DAEMON") || window.currentLine.trimEnd().startsWith("VENDOR")) {
            if (productLinesHaveBeenReached) {
                errorMessageFunction("There is an issue with the license file: your DAEMON line is listed after a product.")
                return;
            }

            // There should only be 1 DAEMON line.
            daemonLineCount++
            if (daemonLineCount > 1) {
                errorMessageFunction("There is an issue with the license file: you have more than 1 DAEMON line.")
                return;
            }

            const countPortEquals = (window.currentLine.match(portEqualsRegex) || []).length;
            const countOptionsEquals = (window.currentLine.match(optionsEqualsRegex) || []).length;
            const countCommentedBeginLines = (window.currentLine.match(commentedBeginLineRegex) || []).length;

            // For all the homies out there using CNU.
            if (window.currentLine.includes("PORT=")) {
                window.daemonPortIsCnuFriendly = true;
            }

            if (countCommentedBeginLines > 0) {
                errorMessageFunction("There is an issue with the license file: it has content that is intended to be commented out in your DAEMON line.")
                return;
            }

            if (countPortEquals > 1) {
                errorMessageFunction("There is an issue with the license file: you have specified more than 1 port number for MLM.")
                return;
            }

            if (countOptionsEquals > 1) {
                errorMessageFunction("There is an issue with the license file: you have specified the path to more than 1 options file.")
                return;
            }

            if (countOptionsEquals === 0) {
                errorMessageFunction("There is an issue with the license file: you did not specify the path to the options file. " +
                    "If you included the path, but did not use options= to specify it, MathWorks licenses ask that you do so, even if they technically work without options=.")
                return;
            }

            // daemonProperty1 and 2 could either be a port number or path to an options file.
            let lineParts = currentLine.split(" ");

            // Just having the word "DAEMON" isn't enough.
            if (lineParts.length === 1) {
                errorMessageFunction("There is an issue with the license file: you have a DAEMON line, but did not specify the daemon to be used (MLM) nor the path to it.")
                return;
            }

            // Checking out how you could've possibly messed up the vendor daemon.
            let daemonVendor = lineParts[1];

            // Try using this as a replacement for IsNullorWhiteSpace.
            if (!daemonVendor || !daemonVendor.trim()) {
                errorMessageFunction("There is an issue with the license file: there are too many spaces between \"DAEMON\" and \"MLM\".")
                return;
            }

            // The vendor daemon needs to read exactly as MLM. Not as a lowercase "mlm", or anything else.
            if (daemonVendor !== "MLM") {
                errorMessageFunction("There is an issue with the license file: you have incorrectly specified the vendor daemon MLM. " +
                    "It must read exactly as \"MLM\", with all uppercase letters.")
                return;
            }

            switch (lineParts.length) {
                case 2:
                    errorMessageFunction("There is an issue with the license file: you did not specify the path to the vendor daemon MLM.")
                    return;
                case 3:
                    errorMessageFunction("There is an issue with the license file: you did not specify the path to the options file.")
                    return;
                default:
                    break;
            }

            if (countOptionsEquals === 1) {
                window.daemonLineHasPort = true;
            }
        } else if (window.currentLine.trimEnd().startsWith("INCREMENT")) {
            productLinesHaveBeenReached = true;
            let lineParts = currentLine.split(" ");

            // Please get rid of the blank garbage. THANK YOU.
            lineParts = lineParts.filter(part => part && part.trim());


            // The stuff we really care about!
            productName = lineParts[1];
            let productVersion = Number(lineParts[3]);
            window.productExpirationDate = lineParts[4];
            let rawSeatCount = Number(lineParts[5]);
            let productKey = lineParts[6].trim();
            let licenseOffering;
            let licenseNumber = "No plpLicenseNumber :(";

            if (productKey.length > 20) {
                errorMessageFunction("There is an issue with the license file: one of your product keys is greater than 20 characters long. This means it's likely been " +
                    `tampered with. This is what the product key is being read as: ${productKey}.`)
                return;
            } else if (productKey.length < 10) {
                errorMessageFunction("There is an issue with the license file: one of your product keys is shorter than 10 characters long. This means it's likely been " +
                    `tampered with. This is what the product key is being read as: ${productKey}.`)
                return;
            }

            // License number.
            if (currentLine.includes("asset_info=")) {
                let match = currentLine.match(assetInfoWithNumberRegex);
                if (match) {
                    licenseNumber = match[1];
                }
            } else if (currentLine.includes("SN=")) {
                let match = currentLine.match(licenseNumberSnRegex)
                if (match) {
                    licenseNumber = match[1];
                }
                if (productName === "TMW_Archive") {
                    containsPLP = true;
                    plpLicenseNumber = licenseNumber;
                    continue;
                }
                // This is the best guess we can make if you're using a PLP-era product.
            } else if (containsPLP && productName.includes("PolySpace")) {
                licenseNumber = containsPLP
            } else {
                if (!licenseNumber || !licenseNumber.trim()) {
                    errorMessageFunction(`There is an issue with the license file: the license number was not found for the product {productName} with the product key ${productKey}. ` +
                        "Your license file has likely been tampered with. Please regenerate it for this product before proceeding.")
                    return;
                }
                errorMessageFunction(`There is an issue with the license file: the license number {licenseNumber} was not found for the product ${productName}.`)
                return;
            }

            // License offering.
            if (currentLine.includes("lo=")) {
                if (currentLine.includes("lo=CN:")) {
                    licenseOffering = "lo=CN";
                } else if (currentLine.includes("lo=CNU")) {
                    licenseOffering = "CNU";
                } else if (currentLine.includes("lo=NNU")) {
                    licenseOffering = "NNU";
                } else if (currentLine.includes("lo=TH")) {
                    if (!currentLine.includes("USER_BASED")) {
                        licenseOffering = "lo=CN";
                    } else {
                        errorMessageFunction(`There is an issue with the license file: it is formatted incorrectly. ${productName}'s license offering is being read as ` +
                            "Total Headcount, but also Network Named User, which doesn't exist.")
                        return;
                    }
                } else {
                    errorMessageFunction(`There is an issue with the license file: the product ${productName} has an invalid license offering.`)
                    return;
                }
                // Figure out your trial or PLP's license offering.
            } else if (currentLine.includes("lr=") || containsPLP || !currentLine.includes("asset_info=")) {
                if (seatCount > 0) {
                    if (currentLine.includes("USER_BASED")) {
                        licenseOffering = "NNU";
                    } else {
                        if (containsPLP && !currentLine.includes("asset_info=") && !currentLine.includes("ISSUED=")) {
                            licenseOffering = "lo=DC"; // See PLP-era explanation below.
                        } else {
                            licenseOffering = "lo=CN";
                        }
                    }
                }
                // This means you're likely using a macOS or Linux PLP-era license, which CAN use an options file... I think it has to.
                else if (containsPLP && !currentLine.includes("asset_info=")) {
                    licenseOffering = "lo=IN";
                    seatCount = 1;
                } else {
                    errorMessageFunction(`There is an issue with the license file: the product ${productName} comes from an Individual ` +
                        "or Designated Computer license, which cannot use an options file.")
                    return;
                }
            } else {
                if (currentLine.includes("PLATFORMS=x")) {
                    errorMessageFunction(`There is an issue with the license file: the product ${productName} comes from an Individual ` +
                        "or Designated Computer license generated from a PLP on Windows, which cannot use an options file.")
                    return;
                } else {
                    if (productKey.length === 20) {
                        if (!licenseFileContentsLines.includes("TMW_Archive")) {
                            errorMessageFunction("There is an issue with the license file: it either is a Windows Individual license generated from a PLP " +
                                "or you are missing the TMW_Archive product in order to make your pre-R2008a product(s) work.")
                            return;
                        } else {
                            // It's possible it's from an IN Windows PLP, but there's really no way to tell AFAIK.
                            licenseOffering = "lo=DC";
                            containsPLP = true;
                        }
                    } else {
                        errorMessageFunction(`There is an issue with the license file: the product ${productName} has an valid license offering.`)
                        return;
                    }
                }
            }

            // Check the product's expiration date. Year 0000 means perpetual.
            if (window.productExpirationDate === "01-jan-0000") {
                window.productExpirationDate = "01-jan-2999";
            }

            const expirationDate = parseDdMMMyyyy(window.productExpirationDate);

            // Will come to midnight, since FlexLM + MathWorks doesn't seem to care about the time.
            const currentDate = new Date(new Date().toDateString());

            if (expirationDate < currentDate) {
                errorMessageFunction(`There is an issue with the license file: The product ${productName} on license number ` +
                    `${licenseNumber} expired on ${window.productExpirationDate}. Please update your license file appropriately before proceeding.`)
                return;
            }

            if (licenseOffering.includes("NNU")) {
                if (seatCount !== 1 && !containsPLP) {
                    seatCount /= 2;
                }
            }

            if (licenseOffering === "lo=CN" && (seatCount === 0) && licenseNumber === "220668") {
                if ((productVersion <= 18) || (productName.includes("Polyspace") && productVersion <= 22)) {
                    errorMessageFunction(`There is an issue with the license file: it contains a Designated Computer license, ${licenseNumber}, " +
                        "that is incorrectly labeled as a Concurrent license.`)
                } else {
                    errorMessageFunction(`There is an issue with the license file: The product ${productName} on license number ` +
                        `${licenseNumber} expired on ${window.productExpirationDate}. Please update your license file appropriately before proceeding.`)
                }
                return;
            }

        } else if (window.currentLine.trimEnd().startsWith("#") || window.currentLine === "") {

        } else if (window.currentLine.trimEnd().startsWith("USE_SERVER")) {
            // Someday, there will be code here to yell at your for your sins, no matter how minor they are.
        } else {
            errorMessageFunction("There is an issue with the license file: it has an unrecognized line. You likely manually edited the license file " +
                "and likely need to regenerate it. The lines' contents are the following: " + window.currentLine)
            return;
        }

        if (serverLineCount > 3 || serverLineCount === 2) {
            errorMessageFunction("There is an issue with the license file: it has too many SERVER lines. Only 1 or 3 are accepted.")
            return;
        } else if (serverLineCount === 0) {
            errorMessageFunction("There is an issue with the license file: it has no SERVER lines. You must insert one manually. See instructions online on how to do so.")
            return;
        }
    }
}

// Parse "dd-MMM-yyyy" into Date.
function parseDdMMMyyyy(str) {
    const months = {
        jan: 0, feb: 1, mar: 2, apr: 3, may: 4, jun: 5,
        jul: 6, aug: 7, sep: 8, oct: 9, nov: 10, dec: 11
    };
    const [day, mon, yr] = str.split('-');
    return new Date(parseInt(yr, 10),
        months[mon.toLowerCase()],
        parseInt(day, 10));
}