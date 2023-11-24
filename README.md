# Option File Checker
Note: This tool is not created by or associated with MathWorks.

To do:
- Make output more useful. Print out seat counts for products on licenses. Make a note that INCLUDE and RESERVE lines will subtract from the first license.
- Remove case sensitivity for key=
- Make UI look nicer.
- Redo logic for DAEMON line to _not_ use lineParts since it's a mess (even if it works.) This will more easily allow for line breaks to be supported.

Notes for user:
- Yes, options= isn't technically needed on the DAEMON line, but MathWorks says you should use it.
- Not all errors can be accounted for. If you're getting an error -38, there isn't a way for this to detect that (or at least, not any good way IMO.)
- Seat count calculations ignore HOST, GROUP_HOST, INTERNET, PROJECT, and DISPLAY. Multiple people could be coming from these client types, so there's no way to calculate seat count with these.
