window.errorOccurred = false;
function errorMessageFunction(errorMessage) {
    if (errorMessage !== "No error message set.") {
        window.errorOccurred = true;
        outputTextbox.textContent = errorMessage;
        alert(errorMessage);
    }
}