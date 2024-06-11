# Options File Checker
A simple program designed to help you spot obvious errors in your options file used for MathWorks Products.

Notes for user:
- This tool is not created by nor associated with MathWorks.
- Supports Windows, macOS, and Linux. Compiled builds in the releases section exist for Windows 11, Debian 12.5, macOS Sonoma (ARM), and macOS High Sierra (x64)
- If you don't already have the .NET 8.0 runtime download and installed, you will need to do so in order to use this program. This largely sets the platform compability; if it can't run .NET 8.0, it can't run this program (that means you, CentOS!)
- Project is built with .NET 8.0 and 7.0.
- Yes, options= and port= aren't technically needed on the DAEMON line, but MathWorks says you should use them.
- Not all errors can be accounted for. If you're getting an error -38, there isn't a way for this to detect that (or at least, not any good way IMO.)
- Seat count calculations ignore HOST, GROUP_HOST, INTERNET, PROJECT, and DISPLAY on non-RESERVE lines. Multiple people could be coming from these client types, so there's no way to calculate seat count with these.
- I _think_ it's possible in the options file to specify new entries (ex: INCLUDE MATLAB USER rob) without creating a line break. This program does not support this type of formatting and probably never will.
- Options and license files over 50 MB will not be able to be used. I will keep this limit until I see a need to lift it.

To do:
- Confirm how FlexLM actually handles RESERVE lines with NNU licenses and if needed, change code appropriately.
- Throw an error if the ISSUED date preceeds your computer's date.
- Print warnings when wild cards & IP addresses are used.
- Print warning for MATLAB Parallel Server.
- Make an options file creator/editor (now a separate project, see its GitHub page for more information.)
