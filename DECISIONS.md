# Decisions and Assumptions

**Status:** Provisional planning decisions based on the supplied test-task brief. These are not client-approved. The brief instructs the candidate to record unresolved, missing, or contradictory requirements rather than contact Horizon for clarification. Update each decision if interview evidence or implementation constraints change it.

## D-01 — Treat the brief as intentionally incomplete

- **Quote:** “some requirements are deliberately vague, missing details, and contain at least one contradiction.”
- **Decision:** Mark source facts separately from assumptions and keep unresolved items in [docs/open-questions.md](./docs/open-questions.md).
- **Alternative:** Fill gaps silently with conventional defaults.
- **Reasoning:** The task explicitly evaluates prioritization and how the candidate handles ambiguity.
- **Revisit if:** A decision-maker provides authoritative clarification.

## D-02 — Use equipment visibility and timely return as the product thesis

- **Quote:** “I want to know who has what. That’s it really. And I want it back on time.”
- **Decision:** Center the PRD on current holder/availability and return tracking.
- **Alternative:** Treat damage, reporting, or approvals as the sole primary workflow.
- **Reasoning:** These are the owner's concise summary of the central need.
- **Revisit if:** Stakeholder evidence changes the priority.

## D-03 — Keep field workflows low-friction

- **Quote:** “anything that takes more than a few seconds and they’ll just go back to WhatsApp.”
- **Decision:** Treat simplicity as a product constraint; do not infer a specific screen design or measured threshold.
- **Alternative:** Add a detailed approval form to every checkout.
- **Reasoning:** Staff adoption depends on a light process and the owner rejects an overly controlling feel.
- **Revisit if:** Field-staff testing identifies a different workable flow.

## D-04 — Enforce drone approval only

- **Quote:** “The drone is different. Only Hamza is licensed. If anyone wants to take the drone, Hamza has to say yes first.”
- **Decision:** Drone checkout requires Hamza approval. Do not extend this gate to all equipment.
- **Alternative:** Require approval for all equipment or no equipment.
- **Reasoning:** The drone rule is explicit while the broad team rule should otherwise remain intact.
- **Revisit if:** The owner defines additional restricted equipment.

## D-05 — Keep rover approval undecided

- **Quote:** “Same for the new rover maybe, I will decide later.”
- **Decision:** Record rover approval as unresolved; do not assert yes or no in the requirements.
- **Alternative:** Apply the drone rule to the rover by default.
- **Reasoning:** The owner explicitly defers the policy.
- **Revisit if:** The owner decides whether the rover needs approval.

## D-06 — Represent count-tracked inventory separately from individual assets

- **Quote:** “Four tripods, six field tablets. He doesn’t care which one, just how many are left.”
- **Decision:** The proposed model distinguishes individually tracked assets from pooled quantity stock.
- **Alternative:** Assign a unique identity to every tripod and tablet.
- **Reasoning:** The source explicitly says those items are not individually tracked.
- **Revisit if:** Physical unit traceability becomes necessary.

## D-07 — Do not infer a one-day due-date default

- **Quote:** “Sometimes a guy takes something for one day and keeps it for three weeks.”
- **Decision:** Record that expected return is needed, but leave default duration and overdue threshold open.
- **Alternative:** Set a universal one-day checkout period.
- **Reasoning:** The one-day phrasing is an example, not a specified policy.
- **Revisit if:** Horizon supplies a return policy.

## D-08 — Do not assume WhatsApp integration

- **Quote:** “We mostly use WhatsApp” and “It would be good if the system reminds people.”
- **Decision:** Treat reminders as a desired capability; leave delivery channel open and do not claim a WhatsApp integration.
- **Alternative:** Build a WhatsApp integration in the first release.
- **Reasoning:** The source does not explicitly require WhatsApp delivery or integration.
- **Revisit if:** A selected reminder policy requires that channel.

## D-09 — Surface damage before later checkout; leave blocking policy open

- **Quote:** “the next person only finds out when they’re at a site and it doesn’t work.”
- **Decision:** Damage records must be visible before a later checkout; automatic blocking is unresolved.
- **Alternative:** Always block checkout of damaged items or allow checkout without warning.
- **Reasoning:** The desired outcome is clear, while the operational repair/clearance workflow is absent.
- **Revisit if:** Inspection and repair policy is defined.

## D-10 — Preserve checkout history for prior-month review

- **Quote:** “I just want to look back at last month and see what was out and who had it.”
- **Decision:** Model historical checkout/holder records; exact reporting periods and retention remain open.
- **Alternative:** Keep only current holder state.
- **Reasoning:** Current state alone cannot answer the owner's historical question.
- **Revisit if:** Reporting or retention requirements are clarified.

## D-11 — Treat survey kits as a real workflow, not a fixed schema decision

- **Quote:** “Sometimes they take the whole survey kit. Sometimes they just take one thing.”
- **Decision:** The requirements support both kit and individual issue flows; contents and representation remain proposed.
- **Alternative:** Treat every kit as a hard-coded fixed bundle.
- **Reasoning:** The source does not define kit composition or variability.
- **Revisit if:** Actual kit inventories are supplied.

## D-12 — Follow the mandated backend stack

- **Quote:** “.NET 8 or .NET 9, C#, ASP.NET Core Web API.”
- **Decision:** Any later implementation should use this backend stack.
- **Alternative:** Choose another backend stack.
- **Reasoning:** It is an explicit assessment constraint.
- **Revisit if:** The task specification changes.

## D-13 — Choose a relational database, but do not select a production vendor yet

- **Quote:** “Any relational database. SQLite or SQL Server LocalDB are both fine.”
- **Decision:** Keep relational persistence as a constraint; defer production vendor selection.
- **Alternative:** Treat SQLite or LocalDB as a confirmed production choice.
- **Reasoning:** The examples are acceptable for the task and do not define production hosting needs.
- **Revisit if:** Deployment, concurrency, backup, and operational needs are known.

## D-14 — Keep the interface platform undecided

- **Quote:** “I want to see it on a screen.”
- **Decision:** The source requires a usable interface but does not establish web, mobile, or desktop.
- **Alternative:** Assume a browser application without recording the assumption.
- **Reasoning:** Form factor affects field usability and deployment.
- **Revisit if:** Target devices and connectivity are confirmed.

## D-15 — Treat production security and operations as launch blockers

- **Quote:** No security, privacy, availability, backup, hosting, or support targets are specified in the brief.
- **Decision:** Do not invent production controls or claim production readiness; obtain decisions before launch.
- **Alternative:** Assume generic defaults are sufficient.
- **Reasoning:** The user selected production launch as the intended rigor level; unspecified operational controls materially affect release safety.
- **Revisit if:** Stakeholders approve concrete controls and service objectives.

## D-16 — Do not create application source or scaffold in this phase

- **Quote:** User instruction: “projects yet should not be create.”
- **Decision:** This phase contains planning documents only; no application code, solution, or project scaffold is created.
- **Alternative:** Begin implementation alongside documentation.
- **Reasoning:** The user explicitly deferred project creation.
- **Revisit if:** The user provides a follow-up implementation prompt.

## D-17 — Use a single in-app reminder at the due time in the planning draft

- **Quote:** “It would be good if the system reminds people.”
- **Decision:** For the draft story, create one in-app reminder for the holder when the expected return time arrives; do not repeat and do not integrate WhatsApp.
- **Alternative:** Send earlier or repeated reminders, or use WhatsApp/email.
- **Reasoning:** The user approved this narrow, testable provisional policy; the brief does not specify channel or schedule.
- **Revisit if:** Field-staff use, delivery needs, or production notification policy calls for another channel or cadence.

## D-18 — Interpret “last month” as the previous calendar month

- **Quote:** “I just want to look back at last month and see what was out and who had it.”
- **Decision:** For the planning draft, default the history view to the previous calendar month and include checkouts whose interval overlaps that month.
- **Alternative:** Use a rolling 30-day window or require a manually entered date range.
- **Reasoning:** Calendar months provide a clear, testable interpretation of the owner's wording; the user approved it as a planning assumption.
- **Revisit if:** Horizon defines a different reporting period, timezone, filter, or retention policy.
