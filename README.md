# Option File Checker
Notes for user:
- This tool is not created by nor associated with MathWorks.
- Yes, options= isn't technically needed on the DAEMON line, but MathWorks says you should use it.
- Not all errors can be accounted for. If you're getting an error -38, there isn't a way for this to detect that (or at least, not any good way IMO.)
- Seat count calculations ignore HOST, GROUP_HOST, INTERNET, PROJECT, and DISPLAY. Multiple people could be coming from these client types, so there's no way to calculate seat count with these.
- I _think_ it's possible in the options file to specify new entries (ex: INCLUDE MATLAB USER rob) without creating a line break. This program does not support this type of formatting and probably never will.

To do:
- Remove case sensitivity for key=
- Redo logic for DAEMON line to _not_ use lineParts since it's a mess (even if it works.) This will more easily allow for line breaks to be supported.
- Print warnings when wild cards & IP addresses.