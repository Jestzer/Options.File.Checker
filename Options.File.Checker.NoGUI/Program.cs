﻿// Command-line only version of the program. .NET and Avalonia don't seem to support bundling this into a single executable. Sorry!
using Options.File.Checker; // I'm not bundling this separately and this will make updating the gatherer and analyzer classes easier.

if (args.Contains("-version"))
{
    Console.WriteLine("Version 0.2.5");
    Environment.Exit(0);
}

bool validLicenseFileGiven = false;
string? licenseFilePath = string.Empty;
string? optionsFilePath = string.Empty;

// License file prompts.
while (!validLicenseFileGiven)
{
    Console.WriteLine("Please enter the full path of your license file. You may or may not use quotes around your path.");
    Console.Write("> ");
    licenseFilePath = Console.ReadLine();

    if (licenseFilePath == null)
    {
        ErrorMessage("There is an issue with the license file: you either didn't enter a path or your input was registered as empty.");
        continue;
    }
    else
    {
        licenseFilePath = licenseFilePath.Trim('"');
    }

    if (licenseFilePath == string.Empty)
    {
        ErrorMessage("Please enter your license file path.");
        continue;
    }

    // Read the file contents.
    string? fileContents;
    try
    {
        fileContents = File.ReadAllText(licenseFilePath);
    }
    catch (Exception ex)
    {
        ErrorMessage($"There was an error reading the license file: {ex.Message}");
        continue;
    }

    // Check the file size indirectly via its content length.
    long fileSizeInBytes = fileContents.Length;
    const long FiftyMegabytes = 50L * 1024L * 1024L;

    if (fileSizeInBytes > FiftyMegabytes)
    {
        ErrorMessage("There is an issue with the license file: it is over 50 MB and therefore, likely (hopefully) not a license file.");
        continue;
    }

    if (!fileContents.Contains("INCREMENT"))
    {
        ErrorMessage("There is an issue with the license file: it is either not a license file or it is corrupted.");
        continue;
    }

    if (fileContents.Contains("lo=IN") || fileContents.Contains("lo=DC") || fileContents.Contains("lo=CIN"))
    {
        ErrorMessage("There is an issue with the license file: it contains an Individual or Designated Computer license, " +
            "which cannot use an options file.");
        continue;
    }

    if (fileContents.Contains("CONTRACT_ID="))
    {
        ErrorMessage("There is an issue with the license file: it contains at least 1 non-MathWorks product.");
        continue;
    }

    if (!fileContents.Contains("SERVER") || !fileContents.Contains("DAEMON"))
    {
        ErrorMessage("There is an issue with the license file: it is missing the SERVER and/or DAEMON line.");
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
    optionsFilePath = Console.ReadLine();

    if (optionsFilePath == null)
    {
        ErrorMessage("There is an issue with the license file: it is either not a license file or it is corrupted.");
        continue;
    }
    else
    {
        optionsFilePath = optionsFilePath.Trim('"');
    }

    if (optionsFilePath == string.Empty)
    {
        ErrorMessage("Please enter your options file path.");
        continue;
    }

    // Read the file contents.
    string? fileContents;
    try
    {
        fileContents = File.ReadAllText(optionsFilePath);
    }
    catch (Exception ex)
    {
        ErrorMessage($"There was an error reading the options file: {ex.Message}");
        continue;
    }

    // Check the file size indirectly via its content length.
    long fileSizeInBytes = fileContents.Length;
    const long FiftyMegabytes = 50L * 1024L * 1024L;

    if (fileSizeInBytes > FiftyMegabytes)
    {
        ErrorMessage("The selected file is over 50 MB, which is unexpectedly large for an options file. I will assume is this not an options file.");
        continue;
    }

    if (string.IsNullOrWhiteSpace(fileContents))
    {
        ErrorMessage("There is an issue with the options file: it is either empty or only contains white space.");
        continue;
    }

    // You've made it this far, but I still don't trust you didn't select a pptx.
    static bool ContainsNonPlaintext(string fileContents)
    {
        foreach (char c in fileContents)
        {
            // Check if the character is non-printable/control character (excluding newline and tab) or beyond the ASCII range.
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t' || c > 127)
            {
                return true;
            }
        }
        return false;
    }

    if (ContainsNonPlaintext(fileContents))
    {
        ErrorMessage("There is an issue with the options file: it contains non-plaintext characters and therefore, is likely not an options file.");
        return;
    }

    validOptionsFileGiven = true;
}
if (licenseFilePath != null && optionsFilePath != null)
{
    // Call the AnalyzeFiles method. I'm leaving these unused variables in case I want to use them later. You're welcome future me.
    var (serverLineHasPort,
        daemonLineHasPort,
        daemonPortIsCNUFriendly,
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

    // Space out output and options file path.
    Console.WriteLine();

    // Check if there was an error.
    if (!string.IsNullOrEmpty(err))
    {
        ErrorMessage(err);
        return;
    }


    if (!serverLineHasPort)
    {
        WarningMessage("Warning: you did not specify a port number on your SERVER line.\n");
    }

    if (!daemonLineHasPort)
    {
        WarningMessage("Warning: you did not specify a port number on your DAEMON line. This means random port will be chosen each time you restart FlexLM.\n");
    }

    if (caseSensitivity)
    {
        WarningMessage("Warning: case sensitivity is enabled for users defined in GROUPs and HOST_GROUPs.\n");
    }

    // Warn the user if they didn't specify a license number or product key in their seat-subtracting option entries.
    if (unspecifiedLicenseOrProductKey)
    {
        WarningMessage("Please note: you did not specify a license number or product key for either one of your INCLUDE or RESERVE lines. This means we will subtract the seat from the first " +
            "license the product appears on.\n");
    }

    if (optionsFileUsesMatlabParallelServer)
    {
        WarningMessage("Warning: you are including MATLAB Parallel Server in your options file. Keep in mind that the username must correspond to the username as it is on the cluster. " +
        "This does not prevent users from accessing the cluster.\n");
    }

    if (wildcardsAreUsed)
    {
        WarningMessage("Warning: you are using at least 1 wildcard in your options file. These may be unreliable or cause other issues.\n");
    }

    if (ipAddressesAreUsed)
    {
        WarningMessage("Warning: you are using an IP address in your options file. IP addresses are often dynamic and therefore cannot be reliably used to identify users.\n");
    }

    // Print seatCount.
    if (licenseFileDictionary != null)
    {
        bool overdraftCNWarningHit = false;
        bool includeAllNNUWarningHit = false;
        bool alreadyYelledToCNUAboutPORTFormat = false;

        foreach (var item in licenseFileDictionary)
        {
            string? seatOrSeats; // Grammar is important, I guess.

            if (item.Value.Item2 == 1)
            {
                seatOrSeats = "seat";
            }
            else
            {
                seatOrSeats = "seats";
            }

            if (!daemonPortIsCNUFriendly && daemonLineHasPort && item.Value.Item4.Contains("CNU"))
            {
                if (!alreadyYelledToCNUAboutPORTFormat)
                {
                    WarningMessage("Please note: your license file contains a CNU license and you've specified a DAEMON port, but you did not specifically specify your DAEMON port with \"PORT=\", which is case-sensitive and recommended to do so.\n");
                    alreadyYelledToCNUAboutPORTFormat = true;
                }
            }

            else if (item.Value.Item4.Contains("CN") && item.Value.Item2 < 0)
            {
                if (!overdraftCNWarningHit)
                {
                    WarningMessage($"\r\nWARNING: you have specified more users on license {item.Value.Item5} for the product {item.Value.Item1} than you have seats for. " +
                        $"If every user included was using the product at once then the seat count would technically be at {item.Value.Item2}. " +
                        "This is acceptable since it is a Concurrent license, but if all seats are being used, then a user you've specified to be able to use the product will not be able to " +
                        "access this product until a seat is available.\r\n\r\nTHE WARNING ABOVE WILL ONLY PRINT ONCE FOR THIS SINGLE PRODUCT.\r\n");
                    overdraftCNWarningHit = true;
                }
                else
                {
                    WarningMessage($"You have specified more users on Concurrent license {item.Value.Item5} for the product {item.Value.Item1} than you have seats for (technically counting at {item.Value.Item2} seats.)");
                }
            }
            else
            {
                if (item.Value.Item4.Contains("NNU")) // This is not an else if because I want the seat count to still print out the same.
                {
                    if (!includeAllNNUWarningHit)
                    {
                        if (includeAllDictionary != null)
                        {
                            if (includeAllDictionary.Count != 0)
                            {
                                WarningMessage("Warning: INCLUDEALL cannot be used NNU licenses and will not count towards their seat count.\n");
                            }
                        }
                        includeAllNNUWarningHit = true;
                    }
                }
                // Finally, print the stuff we want to see!
                Console.WriteLine($"{item.Value.Item1} has {item.Value.Item2} unassigned {seatOrSeats} on license number {item.Value.Item5} (product key {item.Value.Item3}).");
            }
        }
    }
    else
    {
        ErrorMessage("The license file dictionary is null. Please report this error on GitHub.");
        return;
    }
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nYour results are displayed above!");
Console.ResetColor();

static void ErrorMessage(string err)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(err);
    Console.ResetColor();
    GC.Collect();
    GC.WaitForPendingFinalizers();
}

static void WarningMessage(string message)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(message);
    Console.ResetColor();
}