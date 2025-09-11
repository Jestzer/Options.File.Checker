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
            }
        });
    } else {
        console.error("Could not find required elements in the DOM.");
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeLicenseFilePicker);
} else {
    initializeLicenseFilePicker();
}
