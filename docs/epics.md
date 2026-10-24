---
status: draft
stepsCompleted:
  - 1
  - 2
  - 3
  - 4
inputDocuments:
  - "c:\\Associate .Net Developer - Test Task (1).pdf"
  - "docs/brief.md"
  - "docs/PRD.md"
  - "docs/architecture.md"
  - "docs/data-model.md"
  - "docs/open-questions.md"
---

# Horizon Survey Equipment Management - Epic Breakdown

## Overview

This draft records extracted requirements, an approved epic structure, and nine reviewed stories. The source PDF intentionally leaves details open; the inventory distinguishes source requirements from approved planning assumptions and unresolved behavior. Production readiness is not established.

## Requirements Inventory

### Functional Requirements

- **FR-1 — View current equipment and holders:** The owner and authorized office users can view equipment and its current holder or availability. The source does not define access roles.
- **FR-2 — Issue an individual asset or survey kit:** A team member can record equipment taken as an individual item or a survey kit. Kit contents and composition are unspecified.
- **FR-3 — Record expected and actual returns:** An equipment checkout can have an expected return and a recorded return. Due-date defaults, extensions, partial returns, and overdue threshold are unspecified.
- **FR-4 — Require approval for drone checkout:** A drone cannot be issued until Hamza has approved its checkout. Authentication, approval capture, rejection, and delegate behavior are unspecified.
- **FR-5 — Show and update remaining quantities:** Track four tripods and six field tablets by quantity rather than individual unit identity; record quantities taken and returned. Partial quantities and adjustments are unspecified.
- **FR-6 — Make overdue equipment identifiable:** The system can identify an open checkout whose expected return has passed. The date/time rule is unspecified.
- **FR-7 — Support reminders:** The source requests reminders but does not specify trigger, schedule, recipient, channel, or WhatsApp integration. The approved planning assumption is one in-app reminder to the holder at the expected return time, without repeats or WhatsApp integration.
- **FR-8 — Record and surface damage:** Provide a way to record damage and make it visible before a later checkout. Reporting, evidence, repair lifecycle, and automatic blocking are unspecified.
- **FR-9 — Review prior equipment assignments:** The owner can review what equipment was out and who held it during a previous month. Reporting boundaries and retention are unspecified.

### NonFunctional Requirements

- **NFR-1 — Backend technology:** C#, .NET 8 or .NET 9, and ASP.NET Core Web API.
- **NFR-2 — Persistence:** Use a relational database. SQLite and SQL Server LocalDB are examples acceptable for the task; production selection is open.
- **NFR-3 — Usability:** The frontend is the candidate's choice but must be usable; visual design is not assessed.
- **NFR-4 — Review startup:** A reviewer should be able to run the application within five minutes using one documented command or a short, accurate README.
- **NFR-5 — Low-friction field process:** Do not impose an unnecessarily complicated process or make it feel excessively controlling.
- **NFR-6 — Production quality targets:** Security, privacy, performance, availability, backup/recovery, observability, and support targets are not specified. Given production intent, these are blockers to architecture and release sign-off, not inferred requirements.

### Additional Requirements

- The test task asks candidates to prioritize and document what they leave out; not every client need must be implemented in working code.
- The task says not to contact Horizon for requirements clarification; document assumptions, alternatives, rationale, and what would change each decision.
- The requested backend is an ASP.NET Core Web API; browser/mobile form factor and frontend framework are not specified.
- Preserve historical issue/return information to support the owner's prior-month query.
- The source requests complete, unedited AI conversation logs in a readable format. No transcript is included in this planning bundle.
- Submission is expected as a private GitHub repository with reviewer access or a ZIP, with `/src`, `/docs`, `/conversations`, `/DECISIONS.md`, and `/README.md`; `/src` is intentionally absent in this planning-only phase.
- The task sets a seven-calendar-day completion window and submission 24 hours before the interview; the actual interview date is not supplied.
- The README must eventually describe run steps, completed and incomplete work, cuts and rationale, time spent, and AI tools. The candidate must be able to explain and change the implementation live.
- Use the current BMAD Method to establish requirements and context before implementation.
- No UX design contract was supplied; visual and interaction details beyond the qualitative simplicity constraint remain open.
- The proposed data model is documented separately and is not a client-prescribed schema.
- Rover approval remains unresolved and is not counted as a functional requirement.

### UX Design Requirements

No UX design document was supplied. The source requirement that field workflows take only a few seconds is retained as NFR-5; no specific screen, visual token, component, or accessibility requirement can be extracted.

### FR Coverage Map

FR-1: Epic 1 — View current equipment and holders.
FR-2: Epic 1 — Issue an individual asset or survey kit.
FR-3: Epic 1 — Record expected and actual returns.
FR-4: Epic 2 — Require Hamza's approval for drone checkout.
FR-5: Epic 1 — Show and update remaining quantities.
FR-6: Epic 3 — Make overdue equipment identifiable.
FR-7: Epic 3 — Support reminders.
FR-8: Epic 2 — Record and surface damage.
FR-9: Epic 3 — Review prior equipment assignments.

## Epic List

### Epic 1: Know What Equipment Is Available and Who Has It

Team members and office staff can see equipment availability and current holders, issue individual items or kits, record expected and actual returns, and track pooled quantities. This establishes the working inventory and checkout capability without depending on future epics.

**FRs covered:** FR-1, FR-2, FR-3, FR-5.

### Epic 2: Use Controlled and Condition-Aware Equipment Safely

Team members cannot complete a drone checkout until Hamza approves it, and reported damage is visible before later equipment checkout. This epic builds on Epic 1's inventory and checkout records and can deliver its safety/condition value without Epic 3.

**FRs covered:** FR-4, FR-8.

**Open policy dependency:** Whether damage blocks checkout, how Hamza is authenticated, and what happens if Hamza is unavailable remain unresolved. The rejected request state is a proposed workflow detail.

### Epic 3: Stay Ahead of Returns and Understand Past Usage

The owner and office staff can identify overdue checkouts, issue reminders under an approved policy, and review equipment assignments from a previous month. This epic builds on Epic 1's checkout history and does not depend on Epic 2.

**FRs covered:** FR-6, FR-7, FR-9.

**Open policy dependency:** The draft assumes one in-app reminder to the holder at the expected return time, with no repeats and no WhatsApp integration, and a previous-calendar-month default for history. Retry behavior, delivery reliability, timezone, and retention remain unresolved.

### Cross-Cutting Requirements

The production security, privacy, availability, recovery, observability, and support questions in NFR-6 apply across all epics. They are release gates, not technical-layer epics. NFR-1 through NFR-5 also apply across the stories they affect.

## Epic 1: Know What Equipment Is Available and Who Has It

Team members and office staff can see equipment availability and current holders, issue individual items or kits, record expected and actual returns, and track pooled quantities. This establishes the working inventory and checkout capability without depending on future epics.

**FRs covered:** FR-1, FR-2, FR-3, FR-5.

### Story 1.1: View Current Equipment and Holders

As the owner or office staff member,  
I want to see the current status and holder of equipment,  
So that I can answer where equipment is and whether it is available.

**Acceptance Criteria:**

**Given** an individually tracked asset is checked out to a team member  
**When** I view the current equipment list  
**Then** the asset and current holder are shown.

**Given** an individually tracked asset is not checked out  
**When** I view the current equipment list  
**Then** it is shown as available.

**Given** an item is quantity-tracked  
**When** I view its current status  
**Then** the available count is shown rather than invented individual-unit identities.

**Given** the inventory contains no equipment records  
**When** I open the list  
**Then** an empty state indicates that no inventory is recorded.

**FRs addressed:** FR-1; the quantity display supports FR-5.

**Open requirements:** How initial inventory records are created and which roles may access this view are not specified.

### Story 1.2: Record an Equipment Checkout

As a team member,  
I want to record an individual item or survey kit that I take,  
So that the office can see who has it.

**Acceptance Criteria:**

**Given** an individually tracked, available, non-drone asset  
**When** a checkout is recorded with the holder  
**Then** an open checkout is created and the current equipment view shows that holder.

**Given** an asset already has an open checkout  
**When** another checkout is attempted  
**Then** the duplicate issue is rejected and the existing holder remains unchanged.

**Given** a survey kit is selected  
**When** the checkout is recorded  
**Then** the checkout identifies the kit and holder; fixed kit composition is not assumed.

**Given** a drone is selected before its approval flow is available  
**When** checkout is attempted  
**Then** it cannot be completed as issued without Hamza approval. Full approval handling is covered by Epic 2.

**FRs addressed:** FR-2.

**Open requirements:** Kit composition is undefined. In Epic 1, drone issue remains unavailable until its approval workflow is delivered.

### Story 1.3: Record Expected and Actual Returns

As a team member,  
I want an expected return and the actual return recorded for a checkout,  
So that equipment status reflects whether it is still out or has come back.

**Acceptance Criteria:**

**Given** a checkout is being recorded and an expected return is supplied  
**When** it is saved  
**Then** the expected return is retained and shown with the checkout; no default duration is inferred.

**Given** a checkout is open  
**When** the return is recorded  
**Then** the actual return time is retained, the checkout is no longer open, and the individual asset becomes available again.

**Given** a checkout has not been returned  
**When** its status is viewed  
**Then** it remains open and the holder is still shown.

**FRs addressed:** FR-3.

**Open requirements:** Whether a due date is mandatory, extensions, timezone, and partial returns remain undecided.

### Story 1.4: Track Pooled Equipment Quantities

As office staff,  
I want to see and update counts for quantity-tracked equipment,  
So that I know how many units remain without tracking each physical unit.

**Acceptance Criteria:**

**Given** the initial inventory is entered  
**When** I view pooled stock  
**Then** it shows four tripods and six field tablets as counts, not individual assets.

**Given** a quantity checkout requests a specified positive amount no greater than the available count  
**When** it is recorded  
**Then** the available count decreases by that amount.

**Given** a quantity checkout requests more than the available count  
**When** it is submitted  
**Then** it is rejected and the available count does not become negative.

**Given** a quantity checkout is returned  
**When** the return is recorded  
**Then** the returned amount is added back to the available count.

**FRs addressed:** FR-5.

**Open requirements:** The treatment of damaged/missing stock, count corrections, and concurrent updates remains unresolved.

## Epic 2: Use Controlled and Condition-Aware Equipment Safely

Team members cannot complete a drone checkout until Hamza approves it, and reported damage is visible before later equipment checkout. This epic builds on Epic 1's inventory and checkout records and can deliver its safety/condition value without Epic 3.

**FRs covered:** FR-4, FR-8.

### Story 2.1: Require Hamza Approval for Drone Checkout

As a team member,  
I want a drone checkout to wait for Hamza's approval,  
So that only approved drone use is recorded as issued.

**Acceptance Criteria:**

**Given** a team member requests an available drone  
**When** the request is submitted  
**Then** it is recorded as pending approval and is not an active checkout.

**Given** a drone request is pending  
**When** Hamza approves it  
**Then** the approval is recorded and the checkout can be marked active.

**Given** a drone request is pending  
**When** Hamza rejects it  
**Then** it remains unissued and the request is recorded as rejected.

**Given** a drone request has no recorded Hamza approval  
**When** checkout status is evaluated  
**Then** it cannot be marked active.

**FRs addressed:** FR-4.

**Open requirements:** Approval identity verification, delegation, and what happens if Hamza is unavailable remain unresolved. The rejected state is a proposed workflow detail.

### Story 2.2: Record and Surface Equipment Damage

As a team member,  
I want to report damage and have it visible to the next person,  
So that damage is not first discovered at a field site.

**Acceptance Criteria:**

**Given** damage is observed on individually tracked equipment  
**When** a damage report with an equipment reference and description is submitted  
**Then** a report is recorded with its reported time.

**Given** equipment has an unresolved damage report  
**When** a user views its availability before a later checkout  
**Then** the damage status is visible.

**Given** damage is visible on equipment  
**When** a user attempts a later checkout  
**Then** the damage information is shown before the checkout is completed.

**FRs addressed:** FR-8.

**Open requirements:** Reporter identity, required evidence, whether damage to pooled stock is reported, and whether a damaged item is blocked or may be issued after acknowledgement remain unresolved.

## Epic 3: Stay Ahead of Returns and Understand Past Usage

The owner and office staff can identify overdue checkouts, issue reminders under an approved policy, and review equipment assignments from a previous month. This epic builds on Epic 1's checkout history and does not depend on Epic 2.

**FRs covered:** FR-6, FR-7, FR-9.

### Story 3.1: Identify Overdue Equipment

As the owner or office staff,  
I want open equipment checkouts past their expected return to be marked overdue,  
So that I can follow up before equipment is kept too long.

**Acceptance Criteria:**

**Given** an open checkout has an expected return earlier than the applicable current date/time  
**When** checkout status is viewed  
**Then** it is identified as overdue.

**Given** an open checkout has an expected return in the future  
**When** checkout status is viewed  
**Then** it is not identified as overdue.

**Given** a checkout has no expected return recorded  
**When** checkout status is viewed  
**Then** no overdue status is inferred solely from the absence of a date.

**Given** a checkout is returned  
**When** its status is viewed  
**Then** it is not shown as currently overdue.

**FRs addressed:** FR-6.

**Open requirements:** The local timezone and whether due values are date-only or date/time remain unresolved.

### Story 3.2: Remind the Holder at the Expected Return Time

As the holder of checked-out equipment,  
I want an in-app reminder when the expected return time arrives,  
So that I am prompted to return equipment on time.

**Acceptance Criteria:**

**Given** a checkout is still open and has an expected return time  
**When** that time arrives  
**Then** one in-app reminder is created for the holder.

**Given** a checkout is returned before its expected return time  
**When** that time arrives  
**Then** no reminder is created.

**Given** a checkout has no expected return time  
**When** reminders are evaluated  
**Then** no reminder is created.

**Given** a reminder is created  
**When** the holder views reminders  
**Then** it identifies the related equipment and expected return.

**FRs addressed:** FR-7.

**Approved planning assumption:** One in-app reminder at the due time, with no repeat schedule and no WhatsApp integration.

**Open requirements:** Delivery retry/reliability and timezone behavior remain unresolved.

### Story 3.3: Review Equipment Assignments for a Prior Month

As the owner,  
I want to review which equipment was out and who held it during a selected month,  
So that I can understand past equipment use.

**Acceptance Criteria:**

**Given** checkout history exists  
**When** I select a calendar month  
**Then** the report shows equipment whose checkout interval overlapped that month and identifies its holder.

**Given** an included checkout was returned  
**When** its history is shown  
**Then** the checkout and return dates are displayed.

**Given** an included checkout has not been returned  
**When** its history is shown  
**Then** it is identified as still open.

**Given** the report opens with no month selected  
**When** it loads  
**Then** it defaults to the previous calendar month.

**FRs addressed:** FR-9.

**Approved planning assumption:** “Last month” means the previous calendar month; a checkout is included if its interval overlaps that month.

**Open requirements:** Timezone, filters, output format, and retention remain unresolved.

## Final Validation

**Status: draft — not yet implementation-ready for production.**

### Passed

- All nine functional requirements map to at least one story with Given/When/Then acceptance criteria.
- No architecture starter-template requirement was present. The mandated backend stack and relational-database constraint are recorded.
- Stories are organized by user value; no forward dependency between stories was identified.
- No application project or source scaffold was created.

### Open checks

1. **Initial data and account setup:** Stories assume equipment and team-member records exist. The process for entering the initial inventory, identifying assets, and creating accounts is not specified or covered by a story.
2. **Production readiness:** Identity/access, security/privacy, deployment, backup/recovery, availability, observability, and support requirements remain undefined.
3. **Product policies:** Kit composition, due-date/timezone rules, damaged-item and pooled-stock handling, reminder delivery reliability, and history retention need decisions before the affected stories are fully implementation-ready.

Resolve these checks before treating this breakdown as ready for production implementation.
