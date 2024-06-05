// License file prompts.
bool validLicenseFileGiven = false;
while (!validLicenseFileGiven)
{
    Console.WriteLine("Please enter the full path of your license file. Do not use quotes around your path.");
    Console.Write("> ");
    string? licenseFilePath = Console.ReadLine();

    if (licenseFilePath == null)
    {
        Console.WriteLine("There is an issue with the license file: it is either not a license file or it is corrupted.");
        continue;
    }

    // Read the file contents.
    string fileContents;
    try
    {
        fileContents = File.ReadAllText(licenseFilePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"There was an error reading the license file: {ex.Message}");
        continue;
    }

    // Check the file size indirectly via its content length.
    long fileSizeInBytes = fileContents.Length;
    const long FiftyMegabytes = 50L * 1024L * 1024L;

    if (fileSizeInBytes > FiftyMegabytes)
    {
        Console.WriteLine("There is an issue with the license file: it is over 50 MB and therefore, likely (hopefully) not a license file.");
        continue;
    }

    if (!fileContents.Contains("INCREMENT"))
    {
        Console.WriteLine("There is an issue with the license file: it is either not a license file or it is corrupted.");
        continue;
    }

    if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
    {
        Console.WriteLine("There is an issue with the license file: it contains an Individual or Designated Computer license, " +
            "which cannot use an options file.");
        continue;
    }

    if (fileContents.Contains("CONTRACT_ID="))
    {
        Console.WriteLine("There is an issue with the license file: it is not a MathWorks license file.");
        continue;
    }

    if (!fileContents.Contains("SERVER") || !fileContents.Contains("DAEMON"))
    {
        Console.WriteLine("There is an issue with the license file: it is missing the SERVER and/or DAEMON line.");
        continue;
    }

    validLicenseFileGiven = true;
}

// Options file propmts.
bool validOptionsFileGiven = false;

while (!validOptionsFileGiven)
{
    Console.WriteLine("Please enter the full path of your options file:");
    Console.Write("> ");
    string? optionsFilePath = Console.ReadLine();

    if (optionsFilePath == null)
    {
        Console.WriteLine("There is an issue with the license file: it is either not a license file or it is corrupted.");
        continue;
    }

    // Read the file contents.
    string fileContents;
    try
    {
        fileContents = File.ReadAllText(optionsFilePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"There was an error reading the license file: {ex.Message}");
        continue;
    }

    // Check the file size indirectly via its content length.
    long fileSizeInBytes = fileContents.Length;
    const long FiftyMegabytes = 50L * 1024L * 1024L;

    if (fileSizeInBytes > FiftyMegabytes)
    {
        Console.WriteLine("The selected file is over 50 MB, which is unexpectedly large for an options file. I will assume is this not an options file.");
        continue;
    }

    if (string.IsNullOrWhiteSpace(fileContents))
    {
        Console.WriteLine("There is an issue with the options file: it is either empty or only contains white space.");
        continue;
    }

    Console.WriteLine($"You entered: {optionsFilePath}");
}