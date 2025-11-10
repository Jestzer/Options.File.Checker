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

                if (serverLineHasPort === false) {
                    outputTextbox.textContent = "Warning: you did not specify a port number on your SERVER line.\n";
                }

                if (daemonLineHasPort === false) {
                    outputTextbox.textContent += "Warning: you did not specify a port number on your DAEMON line. This means random port will be chosen each time you restart FlexLM.\n";
                }

                if (caseSensitivity === true) {
                    outputTextbox.textContent += "Warning: case sensitivity is enabled for users defined in GROUPs and HOST_GROUPs.\n";
                }

                if (unspecifiedLicenseOrProductKey === true) {
                    outputTextbox.textContent += "Please note: you did not specify a license number or product key for either one of your INCLUDE or RESERVE lines. This means we will subtract the seat from the first " +
                        "license the product appears on.\n";
                }

                if (optionsFileUsesMatlabParallelServer === true) {
                    outputTextbox.textContent += "Warning: you are including MATLAB Parallel Server in your options file. Keep in mind that the username must correspond to the username as it is on the cluster. " +
                        "This does not prevent users from accessing the cluster.\n";
                }

                if (wildCardsAreUsed === true) {
                    outputTextbox.textContent += "Warning: you are using at least 1 wildcard in your options file. These may be unreliable or cause other issues.\n";
                }

                if (ipAddressesAreUsed === true) {
                    outputTextbox.textContent += "Warning: you are using an IP address in your options file. IP addresses are often dynamic and therefore cannot be reliably used to identify users.\n";
                }

                let nnuOverdraftWarningHasBeenDisplayed = false;

                Object.entries(licenseFileDictionary).forEach(([idx, obj]) => {
                    const details = document.createElement('details');
                    const summary = document.createElement('summary');
                    summary.textContent = `${obj.productName} Seats remaining: ${obj.seatCount}. Original seat count: ${obj.originalLicenseFileSeatCount}. ${obj.licenseOffering}. ${obj.licenseNumber}. Product Key: ${obj.productKey}.`;

                    // Wait!! I might have some things to tell you...
                    if (obj.licenseOffering === "lo=CN" && obj.seatCount < 0 && nnuOverdraftWarningHasBeenDisplayed === false) {
                        outputTextbox.textContent += "Warning: you have specified more users on a CN license than the number of seats available. " +
                            "This is introduces the possibility of License Manager Error -4 appearing, since there is not a seat available for every user to use at once.\n"
                        nnuOverdraftWarningHasBeenDisplayed = true;
                    }

                    // Okay, back to the tree view construction.
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
