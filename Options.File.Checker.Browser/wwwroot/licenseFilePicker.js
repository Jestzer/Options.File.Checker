function initializeLicenseFilePicker() {
    const licenseFilePicker = document.getElementById('licenseFilePicker');
    const LicenseFileBrowseButton = document.getElementById('LicenseFileBrowseButton');
    const textBox = document.getElementById('licenseFileTextbox');

    if (LicenseFileBrowseButton && licenseFilePicker) {
        LicenseFileBrowseButton.addEventListener('click', (event) => {
            event.preventDefault();
            licenseFilePicker.click();
        });

        licenseFilePicker.addEventListener('change', (event) => {
            const files = event.target.files;
            if (files.length > 0 && textBox) {
                textBox.value = files[0].name;
            } else {
                errorMessageFunction("The license file you've chosen appears to be empty.")
                return;
            }

            const reader = new FileReader();
            reader.onload = (e) => {
                window.licenseFileRawText = e.target.result;
            };
            reader.readAsText(files[0]);
        });
    } else {
        errorMessageFunction("Could not find required elements in the DOM for the license file.")
        // return; Not needed at the moment.
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeLicenseFilePicker);
} else {
    initializeLicenseFilePicker();
}
