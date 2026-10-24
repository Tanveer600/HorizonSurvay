# Source Brief — Horizon Survey Services Test Task

**Source:** *Solvefy – Test Task, AI Engineer I (Associate)*, supplied PDF (4 pages). Page references below refer to the PDF's printed page order.

## Assignment

- Build a working application addressing Horizon's equipment-management problem: the company cannot reliably tell who has equipment, equipment is kept longer than needed, and damage can go unreported until it disrupts field work. (pp. 2–3)
- This is an associate-level AI-engineering hiring task. The stated completion window is seven calendar days, and submission is due 24 hours before the scheduled interview. (p. 1)
- The brief intentionally leaves requirements vague, missing, and potentially contradictory. The candidate is assessed on prioritization, build order, and the ability to explain what was left out. Not every mentioned need must be implemented in working code. (p. 1, p. 3)

## Client and operating context

- Horizon Survey Services is a land-surveying company in Lahore with about 25 people, roughly 14 of whom are field staff. It owns expensive equipment. (p. 2)
- Mr Tariq, the owner, wants to know who has what and have it returned on time; he also wants to view the information on a screen and review who had equipment in a past month. (p. 2)
- Saima is office staff who receives equipment-location questions. Field staff take whole kits or individual items and may rely on WhatsApp. (p. 2)
- Hamza is the only licensed drone operator and must approve drone use before anyone takes the drone. The owner has not decided whether the new rover also needs approval. (p. 2)
- Four tripods and six field tablets are not individually tracked; the client needs to know how many remain. (p. 2)
- Damage may not be reported when equipment returns and can be discovered at a job site. (p. 2)

## Technical and assessment constraints

- Backend: C#, .NET 8 or .NET 9, ASP.NET Core Web API. Use a relational database; SQLite and SQL Server LocalDB are acceptable examples for a task of this size. (p. 3)
- Frontend technology is the candidate's choice; it must be usable. Visual design is not assessed. The data model matters more than a polished interface with a confused model. (p. 3)
- A reviewer should be able to run the application within five minutes using one documented command or a short, accurate README. (p. 3)
- The task requests a repository or ZIP with `/src`, `/docs`, `/conversations`, `/DECISIONS.md`, and `/README.md`. Submission may be a private GitHub repository with reviewer access or a ZIP. (p. 3)
- Use the BMAD Method to develop requirements and context before software implementation; use the current method/release rather than relying uncritically on an older video. (p. 1, p. 3)
- Submit every AI conversation used, complete, unedited, and readable. A submission without conversation logs will not be interviewed. (pp. 1, 3–4)
- `DECISIONS.md` should record unresolved, missing, or contradictory requirements, quote the relevant line, state the decision, alternative, reasoning, and what would change the decision. The brief expects roughly 8–20 decisions. (p. 3)
- `README.md` should explain how to run the app; what is and is not done; what was cut and why; roughly how many hours were spent; and which AI tools were used. (pp. 3–4)
- The candidate must understand and be able to explain the code and change it live during interview. (p. 4)
- AI tools are allowed, but use must be disclosed and the work must remain directed and understood by the candidate. (p. 4)

## Current phase boundary

The user requested planning documents only and explicitly deferred project creation. Therefore this bundle does not create `/src`, a .NET solution, or a runnable application. The final submission layout and run instructions remain future work once implementation is authorized.

## Related planning documents

- [PRD](./PRD.md)
- [Epics and requirements inventory](./epics.md)
- [Architecture notes](./architecture.md)
- [Data model and properties](./data-model.md)
- [Missing requirements](./open-questions.md)
- [Provisional decisions](../DECISIONS.md)
