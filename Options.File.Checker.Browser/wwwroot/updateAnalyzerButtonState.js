setInterval(() => {
    const licensePath = document.getElementById('licenseFileTextbox')?.value.trim();
    const optionsPath = document.getElementById('optionsFileTextbox')?.value.trim();

    const licenseAndOptionsFileSelected = licensePath && optionsPath;
    analyzerBtn.disabled = !licenseAndOptionsFileSelected;
    //console.log(licenseAndOptionsFileSelected ? 'enabled' : 'disabled');
}, 200); // Update 5 times per second.
