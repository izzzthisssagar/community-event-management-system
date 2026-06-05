# Documentation folder

This folder contains everything for the written part of the submission.

| File | What it is |
|------|------------|
| `CET254_Documentation.html` | The full submission document. **Open this in Microsoft Word** and use *File → Save As → Word Document (.docx)*. The diagrams embed automatically because the PNGs are in this same folder. |
| `class-diagram.png` | Rendered UML class diagram (used by the document). |
| `erd.png` | Rendered Entity Relationship Diagram (used by the document). |
| `architecture.png` | Rendered layered-architecture diagram (Repository + Service patterns). |
| `sequence-registration.png` | Rendered sequence diagram of the registration use case. |
| `01-class-diagram.md` | The Mermaid source for the class diagram (re-render or edit at <https://mermaid.live>). |
| `02-erd.md` | The Mermaid source for the ERD. |
| `03-test-plan.md` | The test plan (43 tests) in Markdown. |
| `04-architecture-diagram.md` | The Mermaid source for the architecture diagram. |
| `05-sequence-diagram.md` | The Mermaid source for the sequence diagram. |
| `DEMO-SCRIPT.md` | A timed 10-minute script for the Panopto demonstration, mapped to each marking area. |

## Steps to finish the Word document

1. Open `CET254_Documentation.html` in Word and save it as `THAPA_Sagar.docx` (or similar).
2. Run `dotnet test` and take a screenshot of the green Test Explorer; paste it into Section 8
   where the placeholder is.
3. (Optional but recommended) Take screenshots of the running app — login, admin event list, the
   debounced browse/search page, and a successful registration — and paste them into Section 9.
4. Record the 10-minute demo video with OBS, upload it to the Panopto folder, and put the link in
   `bi95ss_Thapa_Sagar.txt` at the root of the submission ZIP.

## Final submission ZIP

The ZIP (named `THAPA_Sagar.zip`) should contain:

- `/Solution/` — the full Visual Studio solution (the `CommunityEventManagement` and
  `CommunityEventManagement.Tests` projects).
- `/Documentation/` — the finished `.docx` plus the diagram images.
- `bi95ss_Thapa_Sagar.txt` — a text file containing the Panopto video link.
