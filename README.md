# Campus Lost and Found — Windows Forms

A C# Windows Forms staff application for registering found items, handling lost reports, reviewing ownership claims and recording collection.

## Run on Windows

1. Choose **Code → Download ZIP** on GitHub, then extract the entire ZIP to a folder. Use the solution and launch scripts at the repository root.
2. Install the **.NET 8 SDK for Windows** if it is not installed (a runtime alone cannot build source).
3. Double-click **START-HERE.cmd**. The first build may need internet access.

Alternatively, open `CampusLostAndFound.sln` in Visual Studio with the **.NET desktop development** workload and .NET 8 targeting support. Set **CampusLostAndFound.Desktop** as the startup project, then press F5.

This archive contains source code, not a prebuilt EXE. To create a Windows x64 application with its runtime included, run **BUILD-WINDOWS-APP.cmd**. After successful publishing, open `WindowsApp/CampusLostAndFound.exe`. Keep the entire WindowsApp folder together when sharing it.

## Features

- Overview with record counts and recent items; sample data available for an empty workspace.
- Found items: add, search, filter by status, view details and attach photos.
- Lost reports: create, view and close.
- Claims: submit ownership evidence, link a report, approve or reject, and record collection.
- Collections: view and export text receipts.
- People and places: add students, staff, locations and categories; remove unused students/staff. Linked history is protected.
- Inbox: read notifications and mark all as read.
- Local JSON persistence and backup on save.

The GUI uses `System.Windows.Forms.Form`, `DataGridView`, `TabControl`, and standard WinForms dialogs. The desktop project sets `<UseWindowsForms>true</UseWindowsForms>` and contains no WPF/XAML interface.
The interface is built in C# code, so visual designer files are not required. The existing domain class library, console demo and domain tests are retained.

## Data

Records are saved to `%LOCALAPPDATA%/CampusLostAndFound/workspace.json`; imported files are in the adjacent `attachments` folder. The format remains compatible with the earlier application. Close the old application before starting this version. Back up that folder before testing with important records. Data is stored locally; notifications are in-app only.

## Verification status and checks

The conversion was reviewed statically and the archive checked for integrity. The build, automated domain tests and Windows GUI have **not been executed in the conversion environment**, which has no .NET SDK or Windows desktop. Run **VERIFY.cmd** on Windows before submission; it builds the solution and runs the included domain tests. Then complete these manual checks:

1. Start the GUI and visit all seven navigation pages. Resize the window and check that controls remain usable.
2. Add a student and staff member. Add temporary unused records and remove them.
3. Record a found item, search for it, add a photo and view its details.
4. Submit a lost report and a claim linked to the same student/report, with ownership evidence.
5. Approve the claim with a staff decision note, then record collection.
6. View/export the collection receipt; confirm the item is Returned and the linked report is closed.
7. Check notifications. Close and reopen the application; verify data and photos persist.
8. Try blank required fields and actions with no selected row; check for helpful errors.
9. Try removing a student/staff member linked to history; verify removal is blocked.

## Video and GitHub submission

Upload the extracted source folder contents to GitHub, keeping the solution, all four project folders, README, scripts and .gitignore. Do not upload only the executable or ZIP. Build outputs are ignored.

Suggested demonstration: introduce the project, show the dashboard, add people and an item, search it, submit/review a claim, record collection, export a receipt, demonstrate removal of an unused person, and reopen to show saved data. Use sample names and no private records in the video.

After testing, upload the screen recording and check that its link is accessible to your lecturer. Only the group leader submits the **video link and GitHub repository link**. The WinForms source is included in this repository. The demonstration video still needs to be recorded and uploaded.

## Earlier materials

The `CampusLostAndFound-12-class-repo` folder contains the earlier class library and report. Use the root solution for the current WinForms application. Earlier dummy files remain for reference.
