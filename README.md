# Options File Checker
A simple program designed to help you spot obvious errors in your options file used for MathWorks Products. You may find some precompiled builds in the Releases section of this page to download.

Notes for user:
- This tool is not created by nor associated with MathWorks.
- The top box displays warnings, the bottom displays the seat count and what is subtracting from it, if anything.
- Not all errors can be accounted for. For example, if you're getting an error -38, there isn't a way for this to detect that (or at least, not any good way IMO.)
- Seat count calculations ignore HOST, GROUP_HOST, INTERNET, PROJECT, and DISPLAY on non-RESERVE lines. Multiple people could be coming from these client types, so there's no way to calculate seat count with these.
- Yes, options= and port= aren't technically needed on the DAEMON line, but MathWorks says you should use them.
- I _think_ it's possible in the options file to specify multiple entries (ex: INCLUDE MATLAB USER rob) without creating a line break. This program does not support this type of formatting and probably never will.
- In general, with FlexNet, USERs are case-sensitive. Additionally, if you have the exact same INCLUDE line multiple times, each one will be counted separately and will subtract from the seat count. Because FlexNet does this, so does this program.
- Since spaces cannot be in USER names with FlexNet, this program will treat a space as an indicator of a separate entry.
- If you combined your license file with non-MathWorks products, this will not work. Support for other products is not planned.

To-do:
- Identify bad INCREMENT lines (possibly counting lineParts?)
- Setup bug testing.
