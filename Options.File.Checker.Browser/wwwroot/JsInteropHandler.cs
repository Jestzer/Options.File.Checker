using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public static partial class JsInteropHandler
{
    // This method will be called from JavaScript
    // It takes the file paths as strings and returns a Task
    // indicating the operation is asynchronous.
    [JSExport] // This attribute makes the method callable from JS
    internal static async Task AnalyzeFilesAsync(string licenseFilePath, string optionsFilePath)
    {
        try
        {
            // Call your existing C# logic
            var (serverLineHasPort,
                daemonLineHasPort,
                daemonPortIsCnuFriendly,
                caseSensitivity,
                unspecifiedLicenseOrProductKey,
                optionsFileUsesMatlabParallelServer,
                wildcardsAreUsed,
                ipAddressesAreUsed,
                licenseFileDictionary,
                includeDictionary,
                includeBorrowDictionary,
                includeAllDictionary,
                excludeDictionary,
                excludeBorrowDictionary,
                excludeAllDictionary,
                reserveDictionary,
                maxDictionary,
                groupDictionary,
                hostGroupDictionary,
                err) = LicenseAndOptionsFileDataAnalyzer.AnalyzeData(licenseFilePath, optionsFilePath);

             System.Diagnostics.Debug.WriteLine($"Analysis completed. Error: {err ?? "None"}");
             Console.WriteLine($"Analysis completed. Error: {err ?? "None"}"); // Also log to browser console via JS interop if needed

        }
        catch (Exception ex)
        {
            // Handle exceptions that might occur during analysis
            string errorMessage = $"Error during analysis: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            Console.WriteLine(errorMessage); // Log to browser console
            // Optionally, send error back to JS
            // await JS.InvokeVoidAsync("window.handleAnalysisError", errorMessage);
        }
    }
}
