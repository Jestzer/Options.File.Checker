function initializeOptionsFilePicker() {
    const optionsFilePicker = document.getElementById('optionsFilePicker');
    const OptionsFileBrowseButton = document.getElementById('OptionsFileBrowseButton');
    const textBox = document.getElementById('optionsFileTextbox');

    if (OptionsFileBrowseButton && optionsFilePicker) {
        OptionsFileBrowseButton.addEventListener('click', (event) => {
            event.preventDefault();
            optionsFilePicker.click();
        });

        optionsFilePicker.addEventListener('change', (event) => {
            const files = event.target.files;
            if (files.length > 0 && textBox) {
                textBox.value = files[0].name;
            } else {
                errorMessageFunction("The options file you've chosen appears to be empty.")
                return;
            }

            const reader = new FileReader();
            reader.onload = (e) => {
                window.optionsFileRawText = e.target.result;
            };
            reader.readAsText(files[0]);
        });
    } else {
        errorMessageFunction("Could not find required elements in the DOM for the options file.")
        // return; Not needed at the moment.
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeOptionsFilePicker);
} else {
    initializeOptionsFilePicker();
}
