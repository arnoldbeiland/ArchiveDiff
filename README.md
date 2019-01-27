# ArchiveDiff
WPF GUI utility to compare .zip archive contents (e.g. for Open Packaging Convention files). Targets .Net 4.5.

Might still be buggy and unstable.

## Guide
* Build the solution and run the generated executable.
* Open (or drag & drop) the files you want to compare.
* Double-clicking any row in the grid will run the program specified at the bottom of the 
  window (e.g. for diffing the contents).
* Hitting *Refresh* will reload the original files in case of changes.

## Side effects
* Creates directories prefixed with `ArchiveDiff_` into the current user's 
  temporary directory (e.g. C:\Users\UserName\AppData\Local\Temp\). These have to 
  be deleted manually in case of a crash.
* Creates a file called `ArchiveDiffSettings.xml` near the executable for storing user settings.
