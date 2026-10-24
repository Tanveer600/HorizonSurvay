---
title: Horizon Survey Equipment Management
status: draft
created: 2026-10-21
updated: 2026-10-21
---

# PRD: Horizon Survey Equipment Management

## 0. Document Purpose

This draft translates the supplied Solvefy associate-level test task into product requirements for Horizon Survey Services. It is for the product owner, architecture and delivery planning, and the candidate who will later implement the application. The source deliberately leaves requirements incomplete and asks the candidate to document decisions. Source-stated requirements are distinguished from assumptions and proposals; this document is not client-approved.

Related documents: [Source Brief](./brief.md), [Architecture](./architecture.md), [Data Model](./data-model.md), [Open Questions](./open-questions.md), and the root [DECISIONS.md](../DECISIONS.md). No application code or project scaffold is included in this planning phase.

## 1. Vision

Horizon Survey Services needs a reliable, low-friction way to know which team member has each piece of surveying equipment, when it is expected back, and whether it is ready for use. The current trust-based process leaves the office unable to answer basic availability questions, allows equipment to be kept longer than needed, and can allow damage to go unnoticed until a field job is disrupted.

The proposed system gives the office and owner a current equipment view, records issues and returns, preserves enough history to answer who had what, and makes damaged or approval-controlled equipment visible before it is issued. The field process must remain quick and proportionate rather than feeling punitive.

Production launch is the intended rigor level selected for this planning draft. Security, privacy, availability, recovery, support, and deployment targets are not defined by the source and remain launch blockers rather than silent assumptions.

## 2. Target Users

### 2.1 Jobs to Be Done

- **Owner (Mr Tariq):** See who has what, know when it should come back, and review equipment assignments from a prior period.
- **Field staff:** Take the right equipment to a site using a quick process and return it without unnecessary administration.
- **Office staff (Saima):** Answer equipment-location questions and coordinate availability.
- **Licensed drone operator (Hamza):** Approve drone use before it is issued.

### 2.2 Non-Users

The source does not identify external customer access or public access. Excluding those audiences is a draft boundary, not a confirmed policy.

### 2.3 Key User Journeys

- **UJ-1. A field staff member checks out equipment.** A team member selects a survey kit or individual equipment, records who is taking it and when it is expected back, and can see whether approval is needed. Exact date defaults and kit contents are unresolved.
- **UJ-2. Hamza approves a drone checkout.** A drone request remains unissued until Hamza approves it. The identity, notification, rejection, and unavailability flows are unresolved.
- **UJ-3. Office staff find equipment status.** Saima or the owner views current holders, availability, and remaining quantity for count-tracked items.
- **UJ-4. A team member returns equipment and reports damage.** The return is recorded; damage is captured and surfaced before the next issue. Whether damaged equipment is automatically blocked is unresolved.
- **UJ-5. The owner reviews prior assignments.** Mr Tariq selects a previous month and sees which items were out and who held them. Date boundaries and report details are unresolved.

## 3. Glossary

- **Asset:** An individually identifiable piece of equipment.
- **Checkout:** A recorded issue of an asset, kit, or quantity of stock to a team member.
- **Quantity-tracked stock:** Equipment managed by available counts rather than unique unit identities.
- **Survey kit:** A set of equipment taken together; the source does not define its contents or whether its composition is fixed.
- **Team member:** A person in Horizon's team who may participate in an equipment workflow.

## 4. Features and Functional Requirements

The source provides needs, not formal acceptance criteria. The consequences below make the needs testable where possible; any proposed behavior is marked as an assumption or remains an open question.

### 4.1 Equipment visibility and issue/return

**Description:** Provide a current view of equipment and its holder, and capture issuing and returning equipment as an individual asset or survey kit. Supports UJ-1 and UJ-3.

#### FR-1: View current equipment and holders

The owner and authorized office users can view equipment and its current holder or availability.

**Consequences:**
- For each individually tracked asset, the view identifies whether it is available or checked out and, when checked out, its current holder.
- The view shows the remaining count for quantity-tracked stock.
- **[OPEN QUESTION]** The source does not define access roles, filters, whether field staff can view the same information, or how initial inventory/team-member records are created.

#### FR-2: Issue an individual asset or survey kit

A team member can record equipment being taken as a single item or as a survey kit.

**Consequences:**
- A checkout is associated with a holder and the equipment issued.
- The system can distinguish a kit checkout from an individual-item checkout.
- **[ASSUMPTION]** A kit's contents must be represented sufficiently to avoid double-issuing its component assets. The source does not define kit composition or whether contents are fixed.

#### FR-3: Record expected and actual returns

An equipment checkout can have an expected return and a recorded return.

**Consequences:**
- The record can distinguish equipment still out from equipment returned.
- The owner or office user can identify checkouts whose expected return has passed.
- **[OPEN QUESTION]** The due-date rule, default duration, extension behavior, partial returns, and overdue threshold are not specified.

### 4.2 Approval-controlled equipment

**Description:** Enforce the explicit drone approval rule without generalizing it to other equipment. Supports UJ-2.

#### FR-4: Require approval for drone checkout

A drone cannot be issued until Hamza has approved its checkout.

**Consequences:**
- A pending drone request is distinguishable from an approved request.
- An unapproved request cannot be recorded as a completed drone checkout.
- **[OPEN QUESTION]** The source does not define how Hamza is authenticated, how approval is captured, or what happens if he is unavailable or declines.

**[OPEN QUESTION]** The owner has not decided whether the new rover requires approval. This is not treated as a build-ready functional requirement.

### 4.3 Quantity-tracked stock

**Description:** Track tripods and field tablets by count, not by individual unit. Supports UJ-3.

#### FR-5: Show and update remaining quantities

An authorized user can see how many quantity-tracked units remain and record quantities taken and returned.

**Consequences:**
- Initial stated counts are four tripods and six field tablets.
- A quantity checkout reduces the available count, and a return increases it, subject to a defined quantity rule.
- **[OPEN QUESTION]** Partial quantities, reservations, damaged/missing units, and count correction permissions are not specified.

### 4.4 Returns, reminders, and damage

**Description:** Reduce late returns and surface condition issues before they disrupt field work. Supports UJ-4.

#### FR-6: Make overdue equipment identifiable

The system can identify an open checkout whose expected return has passed.

**Consequences:**
- An open checkout with a due date before the applicable current date is identifiable as overdue.
- **[ASSUMPTION]** Calendar/time-zone rules and whether due dates include a time must be decided before implementation.

#### FR-7: Support reminders

The system supports reminders for equipment that needs to be returned.

**Consequences:**
- A reminder can be associated with an equipment checkout and its holder.
- The source leaves trigger, schedule, recipient, channel, delivery failure handling, and WhatsApp integration unspecified. Mention of WhatsApp as the team's usual communication tool is not itself an integration requirement.
- **[ASSUMPTION]** For the planning draft, the user approved one in-app reminder to the holder at the expected return time, with no repeat schedule and no WhatsApp integration. Delivery retries and timezone behavior remain open.

#### FR-8: Record and surface damage

The system provides a way to record equipment damage and make that information visible before a later checkout.

**Consequences:**
- A damage record is associated with the affected equipment and is visible to a person checking availability.
- **[OPEN QUESTION]** Reporter, evidence, severity, review/repair lifecycle, and whether damaged equipment is blocked from checkout are unspecified.

### 4.5 Historical review

**Description:** Preserve checkout information so the owner can review a past period. Supports UJ-5.

#### FR-9: Review prior equipment assignments

The owner can review what equipment was out and who held it during a previous month.

**Consequences:**
- Historical records identify the equipment, holder, and period during which it was out.
- The source does not define month boundaries, report format, filters, or retention duration.
- **[ASSUMPTION]** For the planning draft, the user approved a previous-calendar-month default and inclusion of checkouts whose interval overlaps the selected month. Timezone and retention remain open.

## 5. Cross-Cutting Constraints and Non-Functional Requirements

### Source-stated constraints

- The backend must use .NET 8 or .NET 9, C#, and ASP.NET Core Web API.
- A relational database is required. SQLite and SQL Server LocalDB are cited as sensible choices for the test task; the production database is not selected.
- The frontend is the candidate's choice and must be usable; visual design is not assessed.
- Reviewers should be able to start the application in five minutes or less using one documented command or a short, accurate README.
- The field process should be quick and should not feel excessively controlling.
- **NFR-1 — Low-friction field process:** Issue and return workflows should be simple enough for field staff who may rely on WhatsApp and are unlikely to use a complicated process taking more than a few seconds. This is qualitative; validation method and measurable threshold are unspecified.

### Production requirements not yet defined

The source does not set security, authentication, authorization, privacy, data residency, performance, availability, backup/restore, observability, support, deployment, or disaster-recovery requirements. Because production launch is the intended rigor level, these are blocking questions; this PRD does not invent thresholds.

## 6. Non-Goals and MVP Scope

### 6.1 Candidate MVP scope

**[ASSUMPTION]** A defensible first release should prioritize current inventory/holder visibility, simple issue and return capture, the explicit drone approval gate, quantity tracking, damage visibility, and historical checkout review. Overdue reminders are desired but their policy and delivery channel require decisions.

The brief explicitly says the candidate must prioritize rather than implement every mentioned need. This list is a planning proposal, not a client-approved scope.

### 6.2 Not specified / not committed

- WhatsApp integration is not committed by the source.
- Rover approval is not committed by the source.
- Reservations, demand-conflict resolution, loss/theft handling, maintenance, and external customer access are not defined.
- Authentication and role-based access are not defined and must be resolved for production.
- No visual design, hosting provider, frontend framework, or production database vendor is selected.

## 7. Success Measures

The source gives no product success targets. Proposed measures for stakeholder approval:

- **SM-1:** A user can determine the current holder and availability of an individually tracked asset. Validates FR-1.
- **SM-2:** Quantity-tracked checkout and return operations produce a correct remaining count. Validates FR-5.
- **SM-3:** No drone checkout is recorded as complete without Hamza's approval. Validates FR-4.
- **SM-4:** A recorded damage issue is visible before a later checkout. Validates FR-8.
- **Counter-metric:** Reduce administrative effort without increasing unrecorded issues, overdue equipment, or field workflow abandonment. Validates FR-2, FR-3, FR-6, NFR-1.

Targets, measurement windows, and responsible owners remain open.

## 8. Open Questions

See [Open Questions](./open-questions.md). Production launch must not be approved until launch-blocking security, access, deployment, recovery, and operational questions are answered.

## 9. Assumptions Index

- §2.3: External and public access are outside the initial audience.
- §4.1 FR-2: Kit contents need enough representation to prevent double-issuing components.
- §4.4 FR-6: Overdue calculations use a defined local date/time rule.
- §5 NFR-1: Usability should be validated with field staff.
- §6.1: The listed candidate MVP is a prioritization proposal, not an approved scope.
- §4.4 FR-7: One in-app reminder at the expected return time; no repeats or WhatsApp integration.
- §4.5 FR-9: “Last month” means the previous calendar month and includes overlapping checkouts.
