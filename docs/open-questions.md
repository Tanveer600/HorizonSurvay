# Missing Requirements and Open Questions

**Source:** supplied Solvefy associate-level test task for Horizon Survey Services. The brief says requirements are intentionally incomplete and contain at least one contradiction, but does not identify it. These items are decision points for the candidate/product owner; the task says not to contact Horizon to resolve requirements.

## Production launch blockers

1. **Identity and access:** Who can sign in? Are authentication and role-based authorization required? Which users can issue, approve, edit inventory, see history, or manage accounts?
2. **Security and privacy:** What personal/operational data is stored? What encryption, access logging, data residency, retention, export, and deletion rules apply?
3. **Deployment and operations:** Where will the system run? Who supports it? What availability, backup/restore, recovery, monitoring, and incident response targets apply?
4. **Data integrity:** What happens when concurrent users attempt to issue the same asset or last remaining unit? Which actions must be auditable and immutable?

## Product behavior decisions

5. **Rover approval:** The owner says, “Same for the new rover maybe, I will decide later.” Decide whether approval is required; do not present either option as source-approved.
6. **Drone approval:** Hamza must approve drone use. Define identity verification, request/approval/rejection states, delegation, and behavior when Hamza is unavailable.
7. **Survey kits:** Define what a kit contains, whether composition changes, how component availability is calculated, and whether a kit can be issued partially.
8. **Quantity-tracked equipment:** Four tripods and six field tablets are counted rather than individually identified. Define partial quantities, unit labels, count correction, reservation, damage, loss, and how remaining count is calculated.
9. **Expected return:** Define whether a due date/time is mandatory, default duration, extension rules, partial return, local timezone, and the moment an item becomes overdue. The one-day example is not a stated default.
10. **Reminders:** The current draft uses the user's approved planning assumption of one in-app reminder to the holder at the expected return time, without repeats or WhatsApp integration. Confirm whether this is appropriate for production; define opt-out, delivery failure/retry behavior, and timezone.
11. **Damage:** Define who may report/confirm damage, required details/evidence, severity, review/repair lifecycle, whether an affected item is blocked, and who can clear the block.
12. **History:** The current draft assumes the previous calendar month and includes checkouts whose interval overlaps that month. Confirm this interpretation for production; define timezone, filters, report/export format, and retention period.
13. **Access boundary:** “Anyone in the team can take anything” conflicts in scope with the later drone-approval exception only if read literally; clarify whether drone approval is the sole restricted-item rule or one example among more restrictions.
14. **Other workflows:** The brief mentions demand conflicts but does not specify reservations or conflict resolution. Loss/theft, maintenance, replacement, and damaged quantity-stock procedures are also undefined.
15. **Platform:** The owner wants information “on a screen,” but web, desktop, mobile, offline use, browsers, and device support are unspecified.
16. **Success criteria:** Define measurable targets for reduced time spent locating equipment, on-time returns, damage discovered before a field job, workflow completion time, and adoption.
17. **Initial data setup:** Define who enters the equipment catalog and team-member records, how existing equipment is identified, and whether initial counts are loaded manually or imported. The current stories assume inventory records already exist.

## Source-stated technical/assessment constraints

- Backend is C# with .NET 8 or 9 and ASP.NET Core Web API.
- Use a relational database; SQLite or SQL Server LocalDB are acceptable examples for the task.
- Frontend is the candidate's choice but must be usable.
- Reviewers should be able to run the app within five minutes using documented instructions.
- Candidate must document scope and decisions, not implement every need indiscriminately.
- The source requests complete, unedited AI conversation logs in a readable format. Actual conversation exports must be supplied; a summary is not a substitute.

## Potential contradiction / tension

The source says there is at least one contradiction but does not identify it. “Anyone in the team can take anything” and the explicit drone approval rule can be reconciled as a general rule plus an exception; they are a tension, not a proven contradiction. The individual-versus-quantity tracking distinction and its interaction with kits is also underspecified. Do not claim either is definitively the intended contradiction.
