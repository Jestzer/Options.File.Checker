# Options File Checker
A simple program designed to help you spot obvious errors in your options file used for MathWorks Products.

You may find some precompiled builds in the Releases section of this page. Non-compiled builds include versions that will work on Windows XP and on computers that can only use a CLI. Ask for them, if needed.

Notes for user:
- This tool is not created by nor associated with MathWorks.
- Not all errors can be accounted for. If you're getting an error -38, there isn't a way for this to detect that (or at least, not any good way IMO.)
- Seat count calculations ignore HOST, GROUP_HOST, INTERNET, PROJECT, and DISPLAY on non-RESERVE lines. Multiple people could be coming from these client types, so there's no way to calculate seat count with these.
- Yes, options= and port= aren't technically needed on the DAEMON line, but MathWorks says you should use them.
- I _think_ it's possible in the options file to specify new entries (ex: INCLUDE MATLAB USER rob) without creating a line break. This program does not support this type of formatting and probably never will.
- Options and license files over 50 MB will not be able to be used. I will keep this limit until I see a need to lift it.
- Latest release uses .NET 8.0.

To do:
- Print warnings when wild cards & IP addresses are used.
- Print warning for MATLAB Parallel Server.
- Make an options file creator/editor (now a separate project, see its GitHub page for more information.)