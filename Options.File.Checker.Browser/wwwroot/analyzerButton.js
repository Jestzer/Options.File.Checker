const analyzerBtn = document.getElementById('analyzerButton');
const treeRoot = document.querySelector('.output-tree');
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
        outputTextbox.textContent = "";
        document.querySelector('.output-tree').innerHTML = '';

        gatherData()

        if (!window.errorOccurred) {
            analyzeData()
            if (!window.errorOccurred) {

                if (window.serverLineHasPort === false) {
                    outputTextbox.textContent = "Warning: you did not specify a port number on your SERVER line.\n";
                }

                if (daemonLineHasPort === false) {
                    outputTextbox.textContent += "Warning: you did not specify a port number on your DAEMON line. This means random port will be chosen each time you restart FlexLM.\n";
                }

                Object.entries(licenseFileDictionary).forEach(([idx, obj]) => {
                    const details = document.createElement('details');
                    const summary = document.createElement('summary');
                    summary.textContent = `${obj.productName} Seats remaining: ${obj.seatCount}. Original seat count: ${obj.originalLicenseFileSeatCount}. ${obj.licenseOffering}. ${obj.licenseNumber}. Product Key: ${obj.productKey}.`;
                    details.appendChild(summary);

                    // Children for linesThatSubtractSeats.
                    if (obj.linesThatSubtractSeats?.length) {
                        const ul = document.createElement('ul');
                        obj.linesThatSubtractSeats.forEach(line => {
                            const li = document.createElement('li');
                            li.textContent = line;          // one <li> per array element
                            ul.appendChild(li);
                        });
                        details.appendChild(ul);
                    }
                    treeRoot.appendChild(details);
                });

            }
        }
    });
}
