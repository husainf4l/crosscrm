# API Implementation To-Do List

**Based on:** FRONTEND_INTEGRATION_REPORT.md  
**Created:** January 2025  
**Status:** 📋 Planning Phase

---

## 📊 Overview

This document outlines all features from the backend API that need to be implemented in the frontend, organized by priority and entity type.

---

## 🔴 CRITICAL PRIORITY (Core Infrastructure)

### 1. Fix Apollo Client Configuration
- [ ] **1.1** Enable auth link in `src/lib/graphql.ts`
  - Uncomment token retrieval
  - Uncomment authorization header
  - Test authentication flow

- [ ] **1.2** Create centralized API configuration
  - Create `.env.local` with `NEXT_PUBLIC_API_URL`
  - Create `src/lib/config.ts` for environment variables
  - Replace all hardcoded URLs with config

- [ ] **1.3** Migrate pages to use Apollo Client
  - Replace `fetch()` calls with `useQuery` and `useMutation`
  - Implement proper error handling
  - Add loading states

- [ ] **1.4** Set up GraphQL Code Generator
  - Install `@graphql-codegen/cli` and plugins
  - Create `codegen.yml` configuration
  - Generate TypeScript types from schema
  - Update imports to use generated types

### 2. Error Handling & User Experience
- [ ] **2.1** Implement global error boundary
  - Create `ErrorBoundary` component
  - Handle GraphQL errors globally
  - Display user-friendly error messages

- [ ] **2.2** Standardize error handling
  - Create error handler utility
  - Map GraphQL error codes to user messages
  - Use toast notifications consistently

- [ ] **2.3** Implement token refresh logic
  - Handle token expiration
  - Auto-refresh tokens if supported
  - Redirect to login on auth failure

---

## 🟠 HIGH PRIORITY (Core CRM Entities)

### 3. Opportunities Module (Sales Pipeline)
- [ ] **3.1** Create Opportunities List Page (`/dashboard/opportunities`)
  - Display all opportunities with filtering
  - Show status, amount, probability, weighted amount
  - Link to customers and pipeline stages

- [ ] **3.2** Create Opportunity Detail Page (`/dashboard/opportunities/[id]`)
  - Show full opportunity details
  - Display related quotes and contracts
  - Show activity timeline

- [ ] **3.3** Implement Opportunity CRUD Operations
  - Create opportunity mutation
  - Update opportunity mutation
  - Delete opportunity mutation
  - Status transition handling

- [ ] **3.4** Implement Pipeline Management
  - Display pipeline stages
  - Move opportunity to stage mutation
  - Visual pipeline view (Kanban board)
  - Status transition validation

- [ ] **3.5** Implement Weighted Pipeline
  - Display weighted pipeline value
  - Show probability calculations
  - Dashboard widget

- [ ] **3.6** Opportunity Status Workflow
  - Handle valid status transitions
  - Lost reason input (when status = Lost)
  - Activity logging on status changes

### 4. Quotes Module
- [ ] **4.1** Create Quotes List Page (`/dashboard/quotes`)
  - Display all quotes with filtering
  - Show quote number, amount, status, customer
  - Link to opportunities

- [ ] **4.2** Create Quote Detail Page (`/dashboard/quotes/[id]`)
  - Show full quote details
  - Display line items
  - Show valid until date
  - Link to opportunity

- [ ] **4.3** Implement Quote CRUD Operations
  - Create quote mutation (only from Open opportunities)
  - Update quote mutation
  - Delete quote mutation
  - Status management (Draft, Sent, Accepted, Rejected, Expired, Converted)

- [ ] **4.4** Implement Quote Line Items
  - Add/edit/remove line items
  - Calculate totals automatically
  - Product selection

- [ ] **4.5** Implement Quote to Invoice Conversion
  - Convert quote to invoice mutation
  - Validation (quote must be Accepted)
  - Update quote status to Converted
  - Link invoice to quote

### 5. Invoices Module (Financial Integration)
- [ ] **5.1** Create Invoices List Page (`/dashboard/invoices`)
  - Display all invoices (synced from external systems)
  - Show invoice number, amount, status, balance
  - Filter by sync status
  - Show external system info

- [ ] **5.2** Create Invoice Detail Page (`/dashboard/invoices/[id]`)
  - Show full invoice details
  - Display payments
  - Show sync status and last synced time
  - Link to customer and opportunity

- [ ] **5.3** Implement Invoice Sync Operations
  - Sync invoice from external system mutation
  - Manual sync trigger
  - Display sync history
  - Handle sync conflicts

- [ ] **5.4** Display Payment Information
  - Show payment history
  - Display paid amount vs total
  - Show balance amount
  - Payment status indicators

- [ ] **5.5** Invoice Status Display
  - Auto-calculate status (Paid, PartiallyPaid, Overdue)
  - Show due dates
  - Visual status indicators

### 6. Contracts Module
- [ ] **6.1** Create Contracts List Page (`/dashboard/contracts`)
  - Display all contracts
  - Show contract number, value, status, dates
  - Link to customers and opportunities

- [ ] **6.2** Create Contract Detail Page (`/dashboard/contracts/[id]`)
  - Show full contract details
  - Display start/end dates
  - Link to invoices
  - Show renewal information

- [ ] **6.3** Implement Contract CRUD Operations
  - Create contract mutation (only from Won opportunities)
  - Update contract mutation
  - Delete contract mutation
  - Status management

- [ ] **6.4** Contract Status Workflow
  - Handle status transitions (Draft → Sent → Signed → Active)
  - Expiration handling
  - Renewal workflow

### 7. Leads Module
- [ ] **7.1** Create Leads List Page (`/dashboard/leads`)
  - Display all leads with filtering
  - Show lead score, status, source
  - Filter by conversion status

- [ ] **7.2** Create Lead Detail Page (`/dashboard/leads/[id]`)
  - Show full lead details
  - Display conversion history
  - Show related customer/opportunity

- [ ] **7.3** Implement Lead CRUD Operations
  - Create lead mutation
  - Update lead mutation
  - Delete lead mutation
  - Lead scoring display

- [ ] **7.4** Implement Lead Conversion
  - Convert to customer mutation
  - Convert to opportunity mutation
  - Convert to both customer and opportunity
  - Link to existing customer/opportunity
  - Preserve lead history

---

## 🟡 MEDIUM PRIORITY (Communication & Collaboration)

### 8. Tasks Module
- [ ] **8.1** Create Tasks List Page (`/dashboard/tasks`)
  - Display all tasks with filtering
  - Filter by type, status, due date
  - Link to related entities

- [ ] **8.2** Implement Task CRUD Operations
  - Create task mutation
  - Update task mutation
  - Delete task mutation
  - Task types: FollowUp, Call, Meeting, Email, Other

- [ ] **8.3** Task Management Features
  - Due date management
  - Status tracking
  - Assign to users/teams
  - Link to customers, opportunities, quotes, invoices, contracts

### 9. Appointments Module
- [ ] **9.1** Create Appointments Calendar View (`/dashboard/appointments`)
  - Calendar view with time slots
  - Filter by category, date range
  - Link to related entities

- [ ] **9.2** Implement Appointment CRUD Operations
  - Create appointment mutation
  - Update appointment mutation
  - Delete appointment mutation
  - Categories: Sales, Support, Demo, Meeting, Other

- [ ] **9.3** Appointment Management Features
  - Start/end time management
  - Category selection
  - Link to customers, opportunities, quotes

### 10. Emails Module
- [ ] **10.1** Create Emails List Page (`/dashboard/emails`)
  - Display all emails with filtering
  - Show subject, from/to, status
  - Link to related entities

- [ ] **10.2** Implement Email Operations
  - View email details
  - Link emails to customers, opportunities, quotes, invoices
  - Email status tracking

### 11. Teams & Collaboration
- [ ] **11.1** Create Teams Management Page (`/dashboard/teams`)
  - Display all teams
  - Show team members and roles
  - Team member count

- [ ] **11.2** Implement Team Operations
  - Create team mutation
  - Add team member mutation
  - Remove team member mutation
  - Assign entities to teams

- [ ] **11.3** Create Channels & Messages Module
  - Channel list view
  - Channel types: Direct, Group, Team, Customer
  - Message display
  - Send message mutation
  - React to messages
  - File attachments

- [ ] **11.4** Real-time Messaging (WebSocket)
  - Set up GraphQL subscriptions
  - Subscribe to message created/updated
  - Subscribe to channel created
  - Real-time message updates

---

## 🟢 LOW PRIORITY (Advanced Features)

### 12. Financial Integrations
- [ ] **12.1** Create Financial Integrations Page (`/dashboard/integrations`)
  - Display all integrations
  - Show integration status
  - Supported systems: QuickBooks, Odoo, SAP

- [ ] **12.2** Implement Integration Management
  - Create integration mutation
  - Update integration mutation
  - Test connection mutation
  - Disconnect integration mutation

- [ ] **12.3** Sync Management
  - Trigger manual sync mutation
  - View sync history
  - Handle sync conflicts
  - Display sync status

- [ ] **12.4** Integration Settings
  - Configure sync frequency
  - Select entities to sync
  - Connection settings UI

### 13. AI Agents
- [ ] **13.1** Create AI Agents Page (`/dashboard/ai-agents`)
  - Display all AI agents
  - Show agent status and tools
  - API key management

- [ ] **13.2** Implement AI Agent Operations
  - Create AI agent mutation
  - Update agent mutation
  - Generate API key mutation
  - Create agent tool mutation

- [ ] **13.3** API Key Management
  - Display API keys
  - Show last used time
  - Expiration management
  - Revoke keys

### 14. Search & Discovery
- [ ] **14.1** Implement Full-text Search
  - Global search bar
  - Search across all entity types
  - Search results page
  - Highlight search terms

- [ ] **14.2** Search Features
  - Filter by entity type
  - Date range filtering
  - Filter by creator
  - Filter by team
  - Relevance scoring

- [ ] **14.3** Search Suggestions
  - Auto-complete suggestions
  - Recent searches
  - Popular searches

### 15. Activity Timeline
- [ ] **15.1** Create Activity Timeline Component
  - Display activities for any entity
  - Show activity type, description, performer, timestamp
  - Filter by activity type

- [ ] **15.2** Activity Types Support
  - Created, Updated, StatusChanged
  - PaymentReceived, QuoteAccepted, QuoteRejected
  - InvoicePaid, ContractSigned
  - LeadConverted, OpportunityWon, OpportunityLost
  - MessageSent, FileUploaded, NoteAdded

- [ ] **15.3** Activity Timeline Integration
  - Add to customer detail page
  - Add to opportunity detail page
  - Add to quote detail page
  - Add to invoice detail page
  - Add to contract detail page

### 16. Notifications System
- [ ] **16.1** Create Notifications Component
  - Notification bell icon
  - Unread count badge
  - Notification dropdown/list

- [ ] **16.2** Implement Notification Operations
  - Get notifications query
  - Mark as read mutation
  - Mark all as read mutation
  - Get unread count query

- [ ] **16.3** Real-time Notifications (WebSocket)
  - Subscribe to notification created
  - Subscribe to notification count changed
  - Real-time notification updates
  - Desktop notifications (if supported)

---

## 🔵 ENHANCEMENTS (Existing Features)

### 17. Customers Module Enhancements
- [ ] **17.1** Add Lead Conversion Tracking
  - Show `convertedFromLeadId` if exists
  - Link to source lead
  - Display conversion history

- [ ] **17.2** Add Team Assignment
  - Assign customer to team
  - Display assigned team
  - Team filtering

- [ ] **17.3** Enhanced Customer Details
  - Show all related opportunities
  - Show all related quotes
  - Show all related invoices
  - Show all related contracts

### 18. Dashboard Enhancements
- [ ] **18.1** Create Main Dashboard Page (`/dashboard`)
  - Key metrics cards
  - Pipeline overview
  - Recent activities
  - Quick actions

- [ ] **18.2** Dashboard Widgets
  - Weighted pipeline value
  - Opportunity funnel
  - Revenue chart
  - Activity feed

### 19. Company Management
- [ ] **19.1** Implement Company Switching
  - `setActiveCompany` mutation
  - Company switcher UI
  - Handle data refresh on switch

- [ ] **19.2** Multi-company Support
  - Display user's companies
  - Switch between companies
  - Company-specific data isolation

---

## 🛠️ TECHNICAL IMPROVEMENTS

### 20. Performance Optimization
- [ ] **20.1** Implement Apollo Client Caching
  - Proper cache configuration
  - Cache invalidation strategies
  - Optimistic updates

- [ ] **20.2** Implement Pagination
  - Add pagination to all list pages
  - Use `skip` and `take` parameters
  - Infinite scroll or page numbers

- [ ] **20.3** Implement Filtering & Sorting
  - Server-side filtering
  - Server-side sorting
  - Filter UI components

### 21. Type Safety
- [ ] **21.1** Generate TypeScript Types
  - Set up GraphQL Code Generator
  - Generate types from schema
  - Update all components to use generated types

- [ ] **21.2** Type Validation
  - Validate inputs before mutations
  - Type-safe error handling
  - Type-safe query results

### 22. Testing
- [ ] **22.1** Unit Tests
  - Test API utilities
  - Test GraphQL queries/mutations
  - Test error handling

- [ ] **22.2** Integration Tests
  - Test full user flows
  - Test API integration
  - Test error scenarios

### 23. Documentation
- [ ] **23.1** API Integration Docs
  - Document all queries/mutations used
  - Document error handling
  - Document best practices

- [ ] **23.2** Component Documentation
  - Document reusable components
  - Document hooks and utilities
  - Add JSDoc comments

---

## 📋 QUICK REFERENCE: Entity Status

| Entity | Status | Priority | Notes |
|--------|--------|----------|-------|
| Customers | ✅ Implemented | - | Needs enhancements |
| Tickets | ✅ Implemented | - | Basic implementation |
| Contacts | ✅ Implemented | - | Basic implementation |
| Opportunities | ❌ Missing | 🔴 High | Core CRM feature |
| Quotes | ❌ Missing | 🔴 High | Core CRM feature |
| Invoices | ❌ Missing | 🔴 High | Financial integration |
| Contracts | ❌ Missing | 🟠 Medium | Sales workflow |
| Leads | ❌ Missing | 🟠 Medium | Sales pipeline |
| Tasks | ❌ Missing | 🟡 Medium | Communication |
| Appointments | ❌ Missing | 🟡 Medium | Communication |
| Emails | ❌ Missing | 🟡 Medium | Communication |
| Teams | ❌ Missing | 🟡 Medium | Collaboration |
| Channels | ❌ Missing | 🟡 Medium | Collaboration |
| Messages | ❌ Missing | 🟡 Medium | Collaboration |
| Financial Integrations | ❌ Missing | 🟢 Low | Advanced feature |
| AI Agents | ❌ Missing | 🟢 Low | Advanced feature |
| Search | ❌ Missing | 🟢 Low | Discovery |
| Activity Timeline | ❌ Missing | 🟢 Low | Audit trail |
| Notifications | ❌ Missing | 🟢 Low | User experience |

---

## 🎯 Implementation Phases

### Phase 1: Foundation (Week 1-2)
- Fix Apollo Client
- Centralized API config
- Error handling
- Type generation

### Phase 2: Core CRM (Week 3-6)
- Opportunities module
- Quotes module
- Invoices module
- Contracts module
- Leads module

### Phase 3: Communication (Week 7-8)
- Tasks module
- Appointments module
- Emails module

### Phase 4: Collaboration (Week 9-10)
- Teams module
- Channels & Messages
- Real-time subscriptions

### Phase 5: Advanced Features (Week 11-12)
- Financial integrations
- AI Agents
- Search
- Activity timeline
- Notifications

---

## 📝 Notes

- All mutations should validate inputs before submission
- All queries should handle loading and error states
- All pages should use consistent UI components
- All API calls should use Apollo Client (after migration)
- All types should be generated from GraphQL schema
- All real-time features should use GraphQL subscriptions

---

**Last Updated:** January 2025  
**Total Tasks:** 100+  
**Estimated Completion:** 12 weeks (with dedicated development)



