const analyzerBtn = document.getElementById('analyzerButton');
const outputTextbox = document.getElementById('outputTextbox');
if (analyzerBtn) {
    analyzerBtn.addEventListener('click', async () => {
        const licensePath = document.getElementById('licenseFileTextbox')?.value.trim();
        const optionsPath = document.getElementById('optionsFileTextbox')?.value.trim();

        if (!licensePath || !optionsPath) {
            alert('Please select both license and options files.');
            return;
        }

        gatherData()

        console.log(licenseFileText);
        console.log(optionsFileText)
    });
}
