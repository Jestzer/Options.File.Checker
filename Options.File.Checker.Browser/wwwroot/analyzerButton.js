const analyzerBtn = document.getElementById('analyzerButton');
let outputTextbox = document.getElementById('outputTextbox');
if (analyzerBtn) {
    analyzerBtn.addEventListener('click', async () => {
        const licensePath = document.getElementById('licenseFileTextbox')?.value.trim();
        const optionsPath = document.getElementById('optionsFileTextbox')?.value.trim();

        if (!licensePath || !optionsPath) {
            alert('Please select both license and options files.');
            return;
        }

        window.errorOccurred = false;
        gatherData()
        analyzeData()

        if (!window.errorOccurred) {
            // console.log(licenseFileText)
            // console.log(optionsFileText)
            console.log("licenseFileDictionary:");
            console.log(licenseFileDictionary);
            console.log("includeDictionary:");
            console.log(includeDictionary);
            // console.log(groupDictionary);
            // console.log(hostGroupDictionary);
            // console.log(reserveDictionary);
        }
    });
}
