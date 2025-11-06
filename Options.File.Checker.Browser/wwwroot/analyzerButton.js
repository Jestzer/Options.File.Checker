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
                //console.log(licenseFileText)
                //console.log(optionsFileText)
                console.log("licenseFileDicitionary:")
                console.log(licenseFileDictionary);
                console.log("includeDicitionary:")
                console.log(includeDictionary);
                // console.log(groupDictionary);
                // console.log(hostGroupDictionary);
                console.log("reserveDicitionary:")
                console.log(reserveDictionary);

                if (window.serverLineHasPort === false) {
                    outputTextbox.textContent = "Warning: you did not specify a port number on your SERVER line.\n";
                }

                if (daemonLineHasPort === false) {
                    outputTextbox.textContent += "Warning: you did not specify a port number on your DAEMON line. This means random port will be chosen each time you restart FlexLM.\n";
                }

                Object.entries(licenseFileDictionary).forEach(([idx, obj]) => {
                    const details = document.createElement('details');
                    const summary = document.createElement('summary');
                    summary.textContent = `${obj.productName} Seats remaining: ${obj.seatCount}. Original seat count: ${obj.originalLicenseFileSeatCount}. ${obj.licenseOffering}. ${obj.licenseNumber}.`; // The label.
                    details.appendChild(summary);

                    const ul = document.createElement('ul');
                    Object.entries(obj).forEach(([productDetail, productDetailValue]) => {
                        if (productDetail === 'productName' || productDetail === 'seatCount' || productDetail === 'originalLicenseFileSeatCount'|| productDetail === 'licenseOffering'|| productDetail === 'licenseNumber') return;
                        const li = document.createElement('li');
                        li.textContent = `${productDetail}: ${productDetailValue}`;
                        ul.appendChild(li); // The children!!! Think about the children!!!
                    });
                    details.appendChild(ul);
                    treeRoot.appendChild(details);
                });
            }
        }
    });
}
