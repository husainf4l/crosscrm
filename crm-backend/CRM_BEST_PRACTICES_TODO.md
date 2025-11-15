# CRM Best Practices & Enterprise Improvements TODO

**Project:** Enterprise CRM System Refactoring  
**Created:** January 2025  
**Focus:** Relationships, Business Logic Flow, Enterprise CRM Patterns

---

## Overview

This document outlines improvements needed to align the CRM system with enterprise best practices, focusing on:
- **Data Relationships & Integrity**
- **Business Logic Flow & Workflows**
- **Enterprise CRM Patterns**
- **Data Consistency & Auditability**

---

## Implementation Status Summary

**Last Updated:** January 2025

### ✅ Completed Phases
- **Phase 1: Core Relationship Improvements** - 100% Complete
- **Phase 2: Business Logic Flow & Workflows** - 100% Complete (Core functionality, background jobs pending)

### 📋 In Progress
- None

### ⏳ Pending Phases
- **Phase 3: Data Consistency & Auditability** - Not Started
- **Phase 4: Enterprise CRM Patterns** - Not Started
- **Phase 5: Advanced Relationships & Polymorphism** - Not Started
- **Phase 6: Service Layer Improvements** - Not Started
- **Phase 7: Data Aggregation & Analytics** - Not Started
- **Phase 8: Code Quality & Architecture** - Ongoing

### 📊 Progress Overview
- **Total Tasks:** 200+
- **Completed:** ~45 tasks (Phases 1-2)
- **Remaining:** ~155 tasks (Phases 3-8)
- **Completion Rate:** ~22% (Core functionality complete)

---

## Phase 1: Core Relationship Improvements (CRITICAL)

**Priority:** 🔴 CRITICAL  
**Estimated Time:** 1 week  
**Status:** ✅ COMPLETE (100%)

### 1.1 Missing Core Relationships

- [x] **Opportunity → Quote Relationship**
  - [x] Add `ICollection<Quote> Quotes` to Opportunity model
  - [x] Update Quote model to ensure OpportunityId is properly indexed
  - [x] Add business rule: Quote can only be created from Open Opportunity
  - [x] Add service method: `GetQuotesByOpportunityAsync()`

- [x] **Opportunity → Contract Relationship**
  - [x] Add `ICollection<Contract> Contracts` to Opportunity model
  - [x] Add business rule: Contract can only be created from Won Opportunity
  - [x] Update Contract creation to validate Opportunity status

- [x] **Quote → Invoice Conversion Tracking**
  - [x] Add `ConvertedToInvoiceId` to Quote model
  - [x] Add business rule: Quote must be Accepted before conversion
  - [x] Add service method: `ConvertQuoteToInvoiceAsync()`
  - [x] Update Quote status to "Converted" when invoice created

- [x] **Invoice → Contract Relationship**
  - [x] Add `ICollection<Contract> Contracts` to Invoice model
  - [x] Add business rule: Contract can reference multiple invoices
  - [x] Add service method: `LinkContractToInvoiceAsync()`

### 1.2 Lead Conversion Flow Enhancement

- [x] **Lead → Customer → Opportunity Tracking**
  - [x] Add `ConvertedFromLeadId` to Customer model
  - [x] Add `ConvertedFromLeadId` to Opportunity model
  - [x] Create service method: `ConvertLeadToCustomerAndOpportunityAsync()`
  - [x] Ensure Lead status updates to "Converted" automatically
  - [x] Copy Lead data to Customer/Opportunity on conversion

- [x] **Lead Source Tracking**
  - [x] Ensure LeadSource is properly linked to Lead
  - [x] Add LeadSource to Opportunity when created from Lead
  - [ ] Add analytics: Track conversion rates by LeadSource (Future enhancement)

### 1.3 Communication Entity Relationships

- [x] **Email → Opportunity/Quote/Invoice Links**
  - [x] Ensure Email model has proper OpportunityId, QuoteId, InvoiceId
  - [x] Add service methods to link emails to entities (via existing EmailService)
  - [x] Add email thread tracking (EmailId → ParentEmailId) (Already exists)

- [x] **Task → Entity Relationships**
  - [x] Add TaskType enum (FollowUp, Call, Meeting, Email, Other)
  - [x] Ensure Task can link to Customer, Opportunity, Quote, Invoice, Contract
  - [x] Add service method: `GetTasksByEntityAsync(entityType, entityId)`

- [x] **Appointment → Entity Relationships**
  - [x] Ensure Appointment can link to Customer, Opportunity, Quote
  - [x] Add AppointmentCategory enum (Sales, Support, Demo, Meeting, Other)
  - [x] Add service method: `GetAppointmentsByEntityAsync()`

### 1.4 Team & Assignment Relationships

- [x] **Team → Opportunity Assignment**
  - [x] Add `AssignedToTeamId` to Opportunity model
  - [x] Add business rule: Opportunity can be assigned to User OR Team
  - [ ] Add service method: `AssignOpportunityToTeamAsync()` (Can be done via UpdateOpportunity)

- [x] **Team → Quote/Invoice Assignment**
  - [x] Add `AssignedToTeamId` to Quote and Invoice models
  - [ ] Add service methods for team assignment (Can be done via Update methods)

- [x] **Team → Customer Ownership**
  - [x] Add `AssignedToTeamId` to Customer model
  - [ ] Add service method: `AssignCustomerToTeamAsync()` (Can be done via UpdateCustomer)
  - [ ] Add business rule: Team members can access team customers (Future enhancement)

---

## Phase 2: Business Logic Flow & Workflows (CRITICAL)

**Priority:** 🔴 CRITICAL  
**Estimated Time:** 1.5 weeks  
**Status:** ✅ COMPLETE (100% - Core functionality complete, background jobs pending)

### 2.1 Sales Pipeline Workflow

- [x] **Opportunity Status Transitions**
  - [x] Create `OpportunityWorkflowService` with status transition validation
  - [x] Add validation: Only allow valid status transitions
  - [x] Add service method: `TransitionOpportunityStatusAsync()`
  - [x] Log all status changes in ActivityTimeline
  - [x] Trigger notifications on status changes

- [x] **Opportunity Won/Lost Workflow**
  - [x] When Opportunity marked as Won:
    - [x] Auto-create Quote if not exists
    - [x] Create ActivityTimeline entry
    - [x] Update Customer status
    - [x] Trigger notification to team
  - [x] When Opportunity marked as Lost:
    - [x] Require LostReason
    - [x] Create ActivityTimeline entry
    - [ ] Update LeadSource statistics (Future enhancement)
    - [x] Trigger notification

- [x] **Quote Workflow**
  - [x] Quote Status Transitions:
    - [x] Draft → Sent (requires valid email - validation in service)
    - [x] Sent → Accepted/Rejected/Expired
    - [x] Accepted → Converted (when invoice created)
  - [ ] Add Quote expiration check (background job) (Future enhancement)
  - [ ] Auto-update Quote status to Expired when ValidUntil passed (Future enhancement)

- [x] **Invoice Workflow**
  - [x] Invoice Status Transitions:
    - [x] Draft → Sent
    - [x] Sent → Paid/PartiallyPaid/Overdue
  - [x] Auto-calculate Invoice status based on Payments
  - [ ] Add overdue detection (background job) (Future enhancement)
  - [x] Auto-update Invoice status to Overdue when DueDate passed (in UpdateInvoiceStatusAsync)

- [x] **Payment Workflow**
  - [x] When Payment created:
    - [x] Update Invoice PaidAmount
    - [x] Update Invoice Status (Paid/PartiallyPaid)
    - [x] Create ActivityTimeline entry
    - [x] Update Opportunity if linked (logs activity)
  - [x] Add payment validation (amount <= balance)

### 2.2 Lead Management Workflow

- [x] **Lead Qualification Process**
  - [x] Add LeadScore field (0-100)
  - [x] Add LeadScoringService to calculate score (in LeadConversionService)
  - [ ] Auto-qualify leads based on score threshold (Future enhancement)
  - [ ] Add Lead qualification checklist (Future enhancement)

- [x] **Lead Conversion Workflow**
  - [x] Create `ConvertLeadDto` with options:
    - [x] Create Customer only
    - [x] Create Customer + Opportunity
    - [x] Link to existing Customer
  - [x] Copy Lead data to Customer/Opportunity
  - [x] Preserve Lead history
  - [x] Update Lead status and conversion tracking

### 2.3 Contract Lifecycle Management

- [x] **Contract Workflow**
  - [x] Contract Status Transitions:
    - [x] Draft → Sent → Signed → Active
    - [x] Active → Expired/Renewed/Cancelled
  - [ ] Add contract renewal workflow (Future enhancement)
  - [ ] Auto-renewal check (background job) (Future enhancement)
  - [ ] Contract expiration notifications (Future enhancement)

- [x] **Contract → Invoice Linking**
  - [x] Support linking contracts to invoices
  - [ ] Add ContractLineItem → InvoiceLineItem mapping (Future enhancement)
  - [ ] Track contract value vs invoiced amount (Future enhancement)

---

## Phase 3: Data Consistency & Auditability (HIGH)

**Priority:** 🟠 HIGH  
**Estimated Time:** 1 week

### 3.1 Soft Deletes

- [ ] **Implement Soft Delete Pattern**
  - [ ] Add `IsDeleted` and `DeletedAt` to all major entities:
    - [ ] Customer, Opportunity, Quote, Invoice, Contract
    - [ ] Lead, Campaign, Product
    - [ ] Team, Channel, Message
  - [ ] Add `DeletedByUserId` for audit trail
  - [ ] Update all queries to filter `IsDeleted == false`
  - [ ] Add service methods: `SoftDeleteAsync()`, `RestoreAsync()`, `PermanentlyDeleteAsync()`

### 3.2 Audit Trail & Change Tracking

- [ ] **Entity Change History**
  - [ ] Create `EntityChangeLog` model (EntityType, EntityId, FieldName, OldValue, NewValue, ChangedBy, ChangedAt)
  - [ ] Implement change tracking for:
    - [ ] Customer (name, email, status)
    - [ ] Opportunity (status, amount, stage)
    - [ ] Quote (status, amount)
    - [ ] Invoice (status, amount)
    - [ ] Contract (status, dates)
  - [ ] Add service: `IEntityChangeLogService`
  - [ ] Auto-log changes on entity updates

- [x] **Activity Timeline Enhancement**
  - [x] Ensure all major actions create ActivityTimeline entries:
    - [x] Opportunity status changes (via OpportunityWorkflowService)
    - [x] Quote creation/acceptance (via QuoteWorkflowService)
    - [x] Invoice creation/payment (via PaymentService)
    - [x] Contract signing (via ContractWorkflowService)
    - [x] Lead conversion (via LeadConversionService)
  - [x] Add ActivityType enum with all action types (implemented in ActivityTimeline model)
  - [x] Link ActivityTimeline to source entity (EntityType and EntityId fields)

### 3.3 Data Validation & Business Rules

- [x] **Financial Data Validation**
  - [x] Quote: SubTotal + Tax - Discount = TotalAmount (calculated in QuoteService)
  - [x] Invoice: SubTotal + Tax - Discount = TotalAmount (calculated in InvoiceService)
  - [x] Payment: Sum of Payments <= Invoice TotalAmount (validated in PaymentService)
  - [x] Opportunity: Amount >= 0, Probability 0-100 (validated in OpportunityService)

- [ ] **Date Validation**
  - [ ] Opportunity: ExpectedCloseDate >= CreatedAt (Future enhancement - add validation)
  - [ ] Quote: ValidUntil >= CreatedAt (Future enhancement - add validation)
  - [ ] Invoice: DueDate >= InvoiceDate (Future enhancement - add validation)
  - [ ] Contract: EndDate > StartDate (Future enhancement - add validation)
  - [ ] Appointment: EndTime > StartTime (Future enhancement - add validation)

- [x] **Status Validation**
  - [x] Cannot delete entities in certain statuses (e.g., Paid Invoice - InvoiceService checks for payments)
  - [ ] Cannot modify certain fields after status change (e.g., Quote amount after Sent) (Future enhancement)

### 3.4 Duplicate Detection

- [ ] **Customer Duplicate Detection**
  - [ ] Create `DuplicateDetectionService`
  - [ ] Check duplicates by: Email, Phone, Name+Address
  - [ ] Add duplicate detection on Customer creation
  - [ ] Add merge functionality for duplicates

- [ ] **Lead Duplicate Detection**
  - [ ] Check duplicates by: Email, Phone
  - [ ] Suggest merge or link to existing Lead

---

## Phase 4: Enterprise CRM Patterns (HIGH)

**Priority:** 🟠 HIGH  
**Estimated Time:** 2 weeks  
**Status:** ✅ COMPLETE (Models & Relationships)

### 4.1 Account vs Contact Model (B2B)

- [x] **Account Model**
  - [x] Create `Account` model (Company/Organization)
    - [x] Name, Industry, Website, AnnualRevenue, EmployeeCount
    - [x] BillingAddress, ShippingAddress
    - [x] AccountType (Customer, Partner, Competitor, Prospect)
    - [x] ParentAccountId (for account hierarchy)
  - [x] Update `Contact` model:
    - [x] Link Contact to Account (not just Customer)
    - [x] Add ContactRole (Decision Maker, Influencer, User, etc.)
  - [x] Update Customer model:
    - [x] Link Customer to Account
    - [x] Customer represents the relationship, Account is the company

- [x] **Account Relationships**
  - [x] Account → Contacts (one-to-many)
  - [x] Account → Opportunities (one-to-many)
  - [x] Account → Contracts (one-to-many)
  - [x] Account → Cases/Tickets (one-to-many)

### 4.2 Territory & Region Management

- [x] **Territory Model**
  - [x] Create `Territory` model:
    - [x] Name, Description, Region, Country
    - [x] AssignedToUserId (Territory Manager)
    - [x] AssignedToTeamId
  - [x] Link Territory to:
    - [x] Customers
    - [x] Opportunities
    - [x] Accounts

### 4.3 Product Catalog & Pricing

- [x] **Product Catalog Enhancement**
  - [x] Add ProductCategory model
  - [x] Add ProductFamily model
  - [x] Add Product attributes (SKU, Barcode, Weight, Dimensions)
  - [x] Add Product images
  - [x] Add Product variants (Size, Color, etc.)

- [x] **Pricing Management**
  - [x] Create `PriceBook` model (Standard, Volume, Contract)
  - [x] Create `PriceBookEntry` model (Product, Price, Currency)
  - [x] Link QuoteLineItem to PriceBookEntry
  - [x] Add discount rules (Percentage, Fixed, Tiered)

### 4.4 Sales Forecasting

- [x] **Forecast Model**
  - [x] Create `SalesForecast` model:
    - [x] Period (Month, Quarter, Year)
    - [x] ForecastAmount, ActualAmount
    - [x] ForecastBy (User, Team, Territory)
  - [x] Create `ForecastItem` model:
    - [x] OpportunityId, ForecastedAmount, Probability
  - [ ] Add service: `IForecastService` (Future enhancement)
  - [ ] Auto-generate forecasts from Opportunities (Future enhancement)

### 4.5 Approval Workflows

- [x] **Approval Process Model**
  - [x] Create `ApprovalProcess` model:
    - [x] Name, EntityType (Quote, Contract, etc.)
    - [x] ApprovalSteps (sequential or parallel)
  - [x] Create `ApprovalStep` model:
    - [x] StepOrder, ApproverRole, ApproverUserId
    - [x] Required, CanDelegate
  - [x] Create `ApprovalRequest` model:
    - [x] EntityType, EntityId, Status, CurrentStep
    - [x] RequestedBy, RequestedAt
  - [x] Create `ApprovalResponse` model:
    - [x] ApprovalRequestId, Step, Approved/Rejected
    - [x] Comments, RespondedBy, RespondedAt

### 4.6 Document & Email Templates

- [x] **Document Template Model**
  - [x] Create `DocumentTemplate` model:
    - [x] Name, Type (Quote, Invoice, Contract)
    - [x] TemplateContent (HTML/PDF)
    - [x] Variables/Placeholders
  - [ ] Add service: `IDocumentTemplateService` (Future enhancement)
  - [ ] Add method: `GenerateDocumentFromTemplateAsync()` (Future enhancement)

- [x] **Email Template Model**
  - [x] Create `EmailTemplate` model:
    - [x] Name, Subject, Body, Type
    - [x] Variables/Placeholders
  - [ ] Add service: `IEmailTemplateService` (Future enhancement)
  - [ ] Add method: `SendEmailFromTemplateAsync()` (Future enhancement)

---

## Phase 5: Advanced Relationships & Polymorphism (MEDIUM)

**Priority:** 🟡 MEDIUM  
**Estimated Time:** 1 week

### 5.1 Polymorphic Relationships

- [ ] **FileAttachment Polymorphism**
  - [ ] Update FileAttachment to support:
    - [ ] Customer, Opportunity, Quote, Invoice, Contract
    - [ ] Lead, Campaign, Product
    - [ ] Message, Note, Strategy, Idea
  - [ ] Add EntityType and EntityId fields
  - [ ] Update service to handle all entity types

- [ ] **ActivityLog Polymorphism**
  - [ ] Update ActivityLog to support all entity types
  - [ ] Add EntityType and EntityId fields
  - [ ] Consolidate with ActivityTimeline or merge

- [ ] **NoteComment Enhancement**
  - [ ] Ensure NoteComment supports all commentable entities
  - [ ] Add support for: Quote, Invoice, Contract comments

### 5.2 Many-to-Many Relationships

- [ ] **Tag System**
  - [ ] Create `Tag` model (Name, Color, Category)
  - [ ] Create `EntityTag` junction model (EntityType, EntityId, TagId)
  - [ ] Support tagging for: Customer, Opportunity, Lead, Quote, Invoice

- [ ] **Campaign → Opportunity Linking**
  - [ ] Create `CampaignOpportunity` junction model
  - [ ] Track which Opportunities came from Campaigns
  - [ ] Add ROI tracking

### 5.3 Hierarchical Relationships

- [ ] **Account Hierarchy**
  - [ ] Add ParentAccountId to Account
  - [ ] Add service method: `GetAccountHierarchyAsync()`
  - [ ] Add business rule: Prevent circular references

- [ ] **Opportunity Hierarchy**
  - [ ] Add ParentOpportunityId for split opportunities
  - [ ] Add service method: `GetRelatedOpportunitiesAsync()`

---

## Phase 6: Service Layer Improvements (MEDIUM)

**Priority:** 🟡 MEDIUM  
**Estimated Time:** 1 week

### 6.1 Domain Events

- [ ] **Event System**
  - [ ] Create `IDomainEvent` interface
  - [ ] Create domain events:
    - [ ] `OpportunityWonEvent`, `OpportunityLostEvent`
    - [ ] `QuoteAcceptedEvent`, `QuoteRejectedEvent`
    - [ ] `InvoicePaidEvent`, `InvoiceOverdueEvent`
    - [ ] `LeadConvertedEvent`
    - [ ] `ContractSignedEvent`
  - [ ] Implement event handlers for:
    - [ ] ActivityTimeline creation
    - [ ] Notification sending
    - [ ] Analytics updates

### 6.2 Business Rule Validation Service

- [ ] **Business Rules Engine**
  - [ ] Create `IBusinessRuleService`
  - [ ] Add rule validation methods:
    - [ ] `ValidateOpportunityStatusTransition()`
    - [ ] `ValidateQuoteConversion()`
    - [ ] `ValidatePaymentAmount()`
    - [ ] `ValidateContractDates()`
  - [ ] Centralize all business rules

### 6.3 Workflow Orchestration

- [ ] **Workflow Service**
  - [ ] Create `IWorkflowService`
  - [ ] Add workflow methods:
    - [ ] `ExecuteLeadConversionWorkflowAsync()`
    - [ ] `ExecuteOpportunityWonWorkflowAsync()`
    - [ ] `ExecuteQuoteToInvoiceWorkflowAsync()`
  - [ ] Chain multiple operations atomically

### 6.4 Integration Points

- [ ] **External System Integration**
  - [ ] Create `IIntegrationService` interface
  - [ ] Add integration points:
    - [ ] Email service (SendGrid, AWS SES)
    - [ ] Payment gateway (Stripe, PayPal)
    - [ ] Document generation (Puppeteer, iText)
    - [ ] Calendar sync (Google Calendar, Outlook)

---

## Phase 7: Data Aggregation & Analytics (LOW)

**Priority:** 🟢 LOW  
**Estimated Time:** 1 week

### 7.1 Customer 360 View

- [ ] **Customer Summary Service**
  - [ ] Create `ICustomerSummaryService`
  - [ ] Aggregate customer data:
    - [ ] Total Opportunities, Won/Lost count
    - [ ] Total Revenue, Average Deal Size
    - [ ] Active Quotes, Invoices, Contracts
    - [ ] Recent Activities, Upcoming Tasks
    - [ ] Communication History

### 7.2 Sales Performance Metrics

- [ ] **Sales Metrics Service**
  - [ ] Create `ISalesMetricsService`
  - [ ] Calculate metrics:
    - [ ] Win Rate, Average Sales Cycle
    - [ ] Pipeline Value, Weighted Pipeline
    - [ ] Conversion Rate by Stage
    - [ ] Revenue by Product, Territory, Team

### 7.3 Reporting & Dashboards

- [ ] **Report Models**
  - [ ] Create `Report` model (Name, Type, Parameters)
  - [ ] Create `ReportExecution` model (ReportId, ExecutedBy, Results)
  - [ ] Add common reports:
    - [ ] Sales Pipeline Report
    - [ ] Revenue Report
    - [ ] Customer Lifetime Value
    - [ ] Lead Conversion Report

---

## Phase 8: Code Quality & Architecture (ONGOING)

**Priority:** 🟡 MEDIUM  
**Estimated Time:** Ongoing

### 8.1 Repository Pattern

- [ ] **Implement Repository Pattern**
  - [ ] Create `IRepository<T>` interface
  - [ ] Create `GenericRepository<T>` implementation
  - [ ] Replace direct DbContext usage in services
  - [ ] Add unit of work pattern

### 8.2 Specification Pattern

- [ ] **Query Specifications**
  - [ ] Create `ISpecification<T>` interface
  - [ ] Create specification implementations for complex queries
  - [ ] Improve query reusability and testability

### 8.3 Caching Strategy

- [ ] **Add Caching Layer**
  - [ ] Cache frequently accessed data:
    - [ ] Products, PipelineStages, LeadSources
    - [ ] User permissions, Team memberships
  - [ ] Implement cache invalidation on updates

### 8.4 Background Jobs

- [ ] **Background Processing**
  - [ ] Add Hangfire or Quartz.NET
  - [ ] Create background jobs:
    - [ ] Quote expiration check
    - [ ] Invoice overdue detection
    - [ ] Contract renewal reminders
    - [ ] Email sending queue
    - [ ] Report generation

---

## Summary

**Total Phases:** 8  
**Total Estimated Time:** 8-10 weeks  
**Completed:** Phases 1-2 (2.5 weeks) ✅  
**Remaining:** Phases 3-8 (5.5-7.5 weeks)  
**Critical Items:** Phases 1-2 (2.5 weeks) ✅ COMPLETE  
**High Priority:** Phases 3-4 (3 weeks) - Pending  
**Medium Priority:** Phases 5-6 (2 weeks) - Pending  
**Low Priority:** Phase 7 (1 week) - Pending  
**Ongoing:** Phase 8 - Pending

**Key Focus Areas:**
1. ✅ Complete relationship mapping (Phase 1)
2. ✅ Implement proper business workflows (Phase 2)
3. ⏳ Add audit trail and data consistency (Phase 3)
4. ⏳ Enterprise CRM patterns (Account, Territory, Forecasting) (Phase 4)
5. ⏳ Service layer improvements (Events, Rules, Workflows) (Phase 6)

**Recent Completions:**
- ✅ All core entity relationships implemented
- ✅ Opportunity, Quote, Invoice, Payment, Contract workflows complete
- ✅ Lead conversion workflow with scoring
- ✅ Activity timeline logging for all major actions
- ✅ Payment workflow with invoice and opportunity updates
- ✅ Appointment and Task entity relationship queries

**Next Steps:**
- Phase 3: Implement soft deletes and audit trail
- Phase 4: Add Account model and Territory management
- Phase 8: Set up background job infrastructure for quote/invoice expiration

