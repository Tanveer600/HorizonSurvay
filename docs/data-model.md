# Draft Data Model and Property Requirements

**Status:** Proposed planning model; not specified or approved by Horizon. The test-task PDF names operational concepts but defines no database schema, property names, types, identifiers, validation rules, or relationships. Every property below is a proposed starting point so that missing requirements are visible. Resolve the open questions before implementation.

Source-derived concepts include team members, equipment, survey kits, checkouts/returns, quantity-tracked stock, drone approvals, damage, reminders, and checkout history. See [PRD](./PRD.md) and [Open Questions](./open-questions.md).

## 1. Proposed entities

### TeamMember

| Property | Proposed type | Required? | Notes |
|---|---|---:|---|
| `Id` | `Guid` | Yes | Proposed stable identifier; source does not specify identity format. |
| `DisplayName` | `string` | Yes | Name shown in holder/history views. |
| `Role` | `string` or enum | Yes | Proposed operational role; exact roles and permissions require a decision. |
| `IsActive` | `bool` | Yes | Proposed deactivation rather than deletion to preserve history. |
| `ExternalIdentityId` | `string?` | TBD | Needed only if an identity provider is selected. |

**Missing requirements:** authentication, account provisioning, role definitions, uniqueness of names, personal-data policy, and whether the operator's license status is stored.

### EquipmentAsset

For individually tracked equipment.

| Property | Proposed type | Required? | Notes |
|---|---|---:|---|
| `Id` | `Guid` | Yes | Proposed stable identifier. |
| `Name` | `string` | Yes | Human-readable equipment label. |
| `Category` | `string` or enum | Yes | Examples in source include total stations, GPS rovers, drones, laptops, tripods, and field tablets. |
| `SerialNumber` | `string?` | TBD | Useful for unique physical identification; not required by the source. |
| `TrackingMode` | enum | Yes | Proposed values: `Individual` or `Quantity`; confirms which tracking rules apply. |
| `Status` | enum | Yes | Proposed values: `Available`, `CheckedOut`, `Damaged`, `Retired`; exact lifecycle needs approval. |
| `Condition` | enum or `string?` | TBD | Inspection/condition semantics are not specified. |
| `IsApprovalRequired` | `bool` | Yes | Proposed configurable rule; drone starts as required from the explicit policy, rover remains undecided. |
| `CreatedAtUtc` / `UpdatedAtUtc` | `DateTimeOffset` | Proposed | Audit metadata; retention and clock rules require agreement. |

### QuantityStock

For pooled inventory not identified by unique physical unit.

| Property | Proposed type | Required? | Notes |
|---|---|---:|---|
| `Id` | `Guid` | Yes | Proposed identifier. |
| `Name` | `string` | Yes | For example, tripods or field tablets. |
| `TotalQuantity` | `int` | Yes | Proposed initial values: tripods = 4; field tablets = 6. |
| `AvailableQuantity` | `int` | Yes | Proposed count available to issue. |
| `UnitLabel` | `string` | TBD | Whether counts mean pieces, sets, or another unit is not specified. |
| `UpdatedAtUtc` | `DateTimeOffset` | Proposed | Audit metadata. |

**Proposed validation:** quantities are non-negative integers; available quantity cannot exceed total quantity. How damaged, missing, reserved, and checked-out quantities affect the total is unresolved.

### SurveyKit and SurveyKitComponent

| Entity/property | Proposed type | Notes |
|---|---|---|
| `SurveyKit.Id` | `Guid` | Proposed identifier. |
| `SurveyKit.Name` | `string` | Source mentions a whole survey kit but gives no name or count. |
| `SurveyKit.IsActive` | `bool` | Proposed lifecycle property. |
| `SurveyKitComponent.KitId` | `Guid` | Proposed relationship to kit. |
| `SurveyKitComponent.EquipmentAssetId` | `Guid?` | For an individually tracked asset; optional only if the component is pooled stock. |
| `SurveyKitComponent.QuantityStockId` | `Guid?` | For quantity-tracked kit components. |
| `SurveyKitComponent.Quantity` | `int` | Proposed component count; validation and partial-kit rules need confirmation. |

**Proposed validation:** each component references exactly one asset or stock category. Whether kits are fixed, configurable, or simply a label for a checkout group is unknown.

### Checkout and CheckoutLine

| Entity/property | Proposed type | Required? | Notes |
|---|---|---:|---|
| `Checkout.Id` | `Guid` | Yes | Proposed identifier. |
| `Checkout.HolderId` | `Guid` | Yes | Team member taking the equipment. |
| `Checkout.CheckedOutAtUtc` | `DateTimeOffset` | Yes | Proposed event timestamp. |
| `Checkout.DueAtUtc` | `DateTimeOffset?` | TBD | Needed to identify overdue items; source does not define whether due date/time is mandatory. |
| `Checkout.ReturnedAtUtc` | `DateTimeOffset?` | No | Null while open; proposed return timestamp. |
| `Checkout.Status` | enum | Yes | Proposed: `PendingApproval`, `Open`, `Returned`, `Cancelled`; exact transitions need confirmation. |
| `CheckoutLine.Id` | `Guid` | Yes | Proposed identifier. |
| `CheckoutLine.EquipmentAssetId` | `Guid?` | Conditional | Set for an individual asset. |
| `CheckoutLine.QuantityStockId` | `Guid?` | Conditional | Set for pooled stock. |
| `CheckoutLine.Quantity` | `int` | Yes | Proposed as 1 for individually tracked equipment. |
| `CheckoutLine.SurveyKitId` | `Guid?` | No | Optional reference for a kit-level issue. |

**Proposed validation:** a line references one equipment/stock target; an asset cannot have more than one active checkout; an issue cannot exceed available quantity. Kit expansion and partial returns are unresolved.

### ApprovalRecord

| Property | Proposed type | Notes |
|---|---|---|
| `Id` | `Guid` | Proposed identifier. |
| `CheckoutId` | `Guid` | Proposed relationship to the requested issue. |
| `ApproverId` | `Guid` | Hamza for drone approval, subject to identity confirmation. |
| `Status` | enum | Proposed: `Pending`, `Approved`, `Rejected`, `Cancelled`. |
| `RequestedAtUtc` | `DateTimeOffset` | Proposed audit timestamp. |
| `DecidedAtUtc` | `DateTimeOffset?` | Null until decision. |
| `Comment` | `string?` | Optional rationale; retention and privacy policy TBD. |

### DamageReport

| Property | Proposed type | Notes |
|---|---|---|
| `Id` | `Guid` | Proposed identifier. |
| `EquipmentAssetId` | `Guid` | Proposed association to individually identified equipment. |
| `CheckoutId` | `Guid?` | Optional link to the checkout during which damage was noticed. |
| `ReportedById` | `Guid` | Proposed reporter identity. |
| `ReportedAtUtc` | `DateTimeOffset` | Proposed timestamp. |
| `Description` | `string` | Proposed summary of the damage. |
| `Severity` | enum or `string?` | Not specified; do not define categories without stakeholder input. |
| `Status` | enum | Proposed lifecycle such as `Reported`, `UnderInspection`, `Resolved`; needs decision. |
| `EvidenceReference` | `string?` | Optional image/file reference; storage and privacy rules TBD. |

Whether a damage report blocks checkout is a policy decision, not a source requirement.

### ReminderEvent (optional)

| Property | Proposed type | Notes |
|---|---|---|
| `Id` | `Guid` | Proposed identifier. |
| `CheckoutId` | `Guid` | The checkout needing a reminder. |
| `RecipientId` | `Guid` | Proposed recipient, likely the holder but not confirmed. |
| `Channel` | enum | Email, in-app, WhatsApp, or other channel must be decided. |
| `ScheduledAtUtc` | `DateTimeOffset` | Trigger/schedule is unresolved. |
| `SentAtUtc` | `DateTimeOffset?` | Proposed delivery record. |
| `DeliveryStatus` | enum | Proposed result tracking; retry policy unresolved. |

This entity is only needed if reminders are in scope and delivery attempts must be auditable. The current planning assumption is one in-app reminder to the holder at the expected return time, without repeats.

## 2. Relationships

- A team member can have multiple historical checkouts; a checkout has one holder.
- A checkout has one or more checkout lines.
- A line represents an individual asset or a quantity of pooled stock; a kit checkout may expand into component lines, subject to the kit policy.
- A checkout may require one or more approval records; the only explicitly required rule is Hamza's approval for drone use.
- An equipment asset may have multiple historical damage reports.
- Reminder events, if implemented, relate to a checkout and recipient.

## 3. Cross-cutting data requirements to decide

- Primary-key format, unique constraints, deletion/deactivation, and audit history.
- Local timezone and date-boundary rules for due dates and monthly reports.
- Retention period and export/deletion policy for personal and operational history.
- Concurrency behavior when multiple people issue the same asset or stock simultaneously.
- Whether logs must record edits and who made them.
- Whether evidence attachments are required and how they are stored and secured.
