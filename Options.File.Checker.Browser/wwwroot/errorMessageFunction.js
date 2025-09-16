function errorMessageFunction(errorMessage) {
    if (errorMessage !== "No error message set.") {
        outputTextbox.textContent = errorMessage;
        alert(errorMessage);
    }
}