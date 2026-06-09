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

## Industry-format documents (v1.0)

These six standalone documents present the system in professional, industry-standard format
(mirroring real BRS / SRS / architecture deliverables). They are the detailed companions to
`CET254_Documentation.html`.

| File | What it is |
|------|------------|
| `CEMS_UML_v1.0.md` | All six UML models (use case, activity, class *without* & *with* relationships, ERD, sequence) as themed Mermaid diagrams. |
| `CEMS_Architecture_v1.0.md` | System design / architecture: layers, component diagram, design patterns with file evidence. |
| `CEMS_BRS_v1.0.md` | Business Requirements Specification — 15 numbered business rules with traceability. |
| `CEMS_SRS_v1.0.md` | System Requirements Specification — 13 functional requirements, NFRs, DB schema, traceability matrix. |
| `CEMS_Test_Plan_v1.0.md` | Test plan: strategy, 43 manual test cases, the 43 automated tests mapped, and the Playwright E2E scenarios. |
| `CEMS_Rubric_Mapping_v1.0.md` | Criterion-by-criterion mapping of how the submission targets first-class marks, with file evidence. |
| `CEMS_Getting_Started.md` | From-scratch guide: prerequisites → run on localhost → run the unit, Selenium and Playwright tests, with troubleshooting. |

> The diagrams render automatically on GitHub and in VS Code (Mermaid). To get a PNG for Word, paste
> a diagram into <https://mermaid.live> and export it.

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

- `/Solution/` — the full Visual Studio solution (the `CommunityEventManagement`,
  `CommunityEventManagement.Tests`, `CommunityEventManagement.SeleniumTests` and
  `CommunityEventManagement.E2ETests` projects).
- `/Documentation/` — the finished `.docx` plus the diagram images.
- `bi95ss_Thapa_Sagar.txt` — a text file containing the Panopto video link.
