    function updateAnalyzeButtonState() {
        const licensePath = document.getElementById('licenseTextBox')?.value.trim();
        const optionsPath = document.getElementById('optionsTextBox')?.value.trim();
        const analyzeBtn = document.getElementById('analyzeButton');

        if (analyzeBtn) {
            // Enable button only if both paths are non-empty
            analyzeBtn.disabled = !(licensePath && optionsPath);
        }
    }

    // Set up event listeners for text box changes
    document.addEventListener('DOMContentLoaded', () => {
        const licenseTextBox = document.getElementById('licenseTextBox');
        const optionsTextBox = document.getElementById('optionsTextBox');
        const analyzeBtn = document.getElementById('analyzeButton');

        if (licenseTextBox) {
            licenseTextBox.addEventListener('input', updateAnalyzeButtonState);
        }
        if (optionsTextBox) {
            optionsTextBox.addEventListener('input', updateAnalyzeButtonState);
        }

        if (analyzeBtn) {
            analyzeBtn.addEventListener('click', async () => {
                const licensePath = document.getElementById('licenseTextBox')?.value.trim();
                const optionsPath = document.getElementById('optionsTextBox')?.value.trim();

                if (!licensePath || !optionsPath) {
                    console.error("License or Options file path is missing.");
                    alert("Please select both license and options files.");
                    return;
                }

                console.log(`Calling C# AnalyzeData with License: ${licensePath}, Options: ${optionsPath}`);

                try {
                    // Call the exported C# method
                    // The method name is prefixed with the assembly name (usually your project name)
                    await DotNet.invokeMethodAsync('Options.File.Checker', 'LicenseAndOptionsFileDataAnalyzer', licensePath, optionsPath);
                    console.log("C# AnalyzeFilesAsync call completed.");
                    // You could update UI here based on results if they were returned
                } catch (error) {
                    console.error("Error calling C# method:", error);
                    alert("An error occurred during analysis. Check the console for details.");
                }
            });
        }

        // Initial state check
        updateAnalyzeButtonState();
    });