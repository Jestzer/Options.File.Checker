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
    const portEqualsRegex = /port=/i;
    const optionsEqualsRegex = /options=/i;
    const commentedBeginLineRegex = /# BEGIN--------------/i;
    const keyEqualsRegex = /key=/i;
    const assetInfoRegex = /asset_info=/i;
    const licenseNumberRegex = /^[^Rab_\d]+$/;
    const ipAddressRegex = /\d{2,3}\./;
    const whiteSpaceRegex = /\s+/;

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

            if (lineParts.length < 3) {
                errorMessageFunction("There is an issue with the license file: you are missing information from your SERVER line. See documentation online on how to format it.")
                return;
            }


        } else if (window.currentLine.trimEnd().startsWith("DAEMON") || window.currentLine.trimEnd().startsWith("VENDOR")) {

        } else if (window.currentLine.trimEnd().startsWith("INCREMENT")) {

        } else if (window.currentLine.trimEnd().startsWith("#") || window.currentLine.trim()) {

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