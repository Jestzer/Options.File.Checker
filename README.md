# Options File Checker
A simple program designed to help you spot obvious errors in your options file used for MathWorks Products.

Notes for user:
- This tool is not created by nor associated with MathWorks.
- Supports Windows, macOS, and Linux. Compiled builds in the releases section exist for Windows XP-11, Debian 12.5 and macOS Sonoma (ARM).
- Desktop version uses both .NET 7.0 and 8.0. Non-GUI version uses .NET 8.0. "XP Edition" uses .NET Framework 4.0.
- Desktop and non-GUI releases are self-contained, meaning you shouldn't need to install anything before using the program. XP Edition needs .NET Framework 4.0 installed beforehand.
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

## Release Breakdown
Keep in mind that the information below is just relevant to the Releases I've made. Platforms "tested on" work, unless it is in the incompatible section. Just because a certain setup wasn't tested, doesn't mean it won't work.
| v0.2.3 Release Info | Desktop           | Non-GUI  | XP Edition   |
|---------------------|-------------------|----------|--------------|
| .NET version        | 7.0 & 8.0         | 8.0      | Framework 4.0|
| GUI Framework       | Avalonia          | —        | WinForms     |
| AOT?                | Yes               | Yes      | No           |
| Self-contained?     | Yes               | Yes      | No           |
| Architecture        | x64, ARM (macOS)  | x64      | x86, works on x64 |
| Linux Distros tested on | Arch, Debian 12, CentOS 7.9, Ubuntu 18.04 | Debian 12 | —      |
| Windows version tested on | Windows 11 | None     | XP, 11      |
| macOS versions tested on | Sonoma (ARM), Catalina (x64)       | None     | —            |
| Incompatible platforms tested on | Ubuntu 6.06.2
