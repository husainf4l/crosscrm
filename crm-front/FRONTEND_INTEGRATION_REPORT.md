# Frontend Integration Report
## Enterprise CRM Backend API Documentation

**Version:** 1.0  
**Last Updated:** January 2025  
**API Base URL:** `http://localhost:5000` (Development)  
**GraphQL Endpoint:** `/graphql`  
**REST API Endpoint:** `/api/ai-agent` (AI Agent only)

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Authentication & Authorization](#authentication--authorization)
3. [GraphQL API](#graphql-api)
4. [REST API (AI Agents)](#rest-api-ai-agents)
5. [Data Models & DTOs](#data-models--dtos)
6. [Business Rules & Workflows](#business-rules--workflows)
7. [Real-time Subscriptions](#real-time-subscriptions)
8. [Error Handling](#error-handling)
9. [Code Examples](#code-examples)
10. [Best Practices](#best-practices)

---

## System Overview

### Architecture
- **Framework:** ASP.NET Core 10.0
- **API Type:** GraphQL (Hot Chocolate) with REST endpoints for AI agents
- **Database:** PostgreSQL with Entity Framework Core 10.0
- **Authentication:** JWT Bearer Tokens
- **Real-time:** GraphQL Subscriptions (WebSocket)
- **Multi-tenancy:** Company-based data isolation

### Key Features
- ✅ Complete CRM entity management (Customers, Opportunities, Quotes, Invoices, Contracts)
- ✅ Sales pipeline management with workflow automation
- ✅ Lead management and conversion
- ✅ **Financial Integration** - Sync with external financial systems (QuickBooks, Odoo, SAP)
  - Sync Customers, Invoices, Orders, and Balances
  - Bidirectional sync support
  - Sync status tracking and conflict resolution
- ✅ Communication tracking (Emails, Tasks, Appointments)
- ✅ Multi-user collaboration (Teams, Channels, Messages)
- ✅ AI Agent integration with API keys and tools
- ✅ Real-time notifications and messaging
- ✅ Full-text search across entities
- ✅ Activity timeline and audit trail

### Financial Integration Architecture

**Important:** The CRM does NOT maintain a full financial model. Instead, it syncs financial data from external systems:

- **QuickBooks** - Sync customers, invoices, orders, and account balances
- **Odoo** - Sync customers, invoices, sales orders, and financial data
- **SAP Finance & Operations** - Sync customers, invoices, orders, and balances

**What Gets Synced:**
- Customer financial information
- Invoices (from external system to CRM)
- Orders/Sales Orders
- Account balances and payment status
- Payment history

**Sync Direction:**
- **Primary:** External System → CRM (read-only in CRM)
- **Secondary:** CRM → External System (for quotes and opportunities that convert to invoices)

---

## Authentication & Authorization

### JWT Authentication

All GraphQL queries, mutations, and subscriptions require a valid JWT token in the Authorization header.

**Header Format:**
```
Authorization: Bearer <jwt_token>
```

### Login Flow

**GraphQL Mutation:**
```graphql
mutation Login($email: String!, $password: String!) {
  login(email: $email, password: $password) {
    token
    user {
      id
      email
      name
      companyId
    }
  }
}
```

**Response:**
```json
{
  "data": {
    "login": {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "user": {
        "id": 1,
        "email": "user@example.com",
        "name": "John Doe",
        "companyId": 1
      }
    }
  }
}
```

### Company Context

The system automatically filters all data by the authenticated user's active company. Users can switch between companies if they belong to multiple.

**Set Active Company:**
```graphql
mutation SetActiveCompany($companyId: Int!) {
  setActiveCompany(companyId: $companyId) {
    id
    name
  }
}
```

### API Key Authentication (AI Agents)

AI agents use API keys for authentication. Include the API key in the `X-API-Key` header:

```
X-API-Key: <api_key>
```

---

## GraphQL API

### Base URL
```
POST /graphql
```

### Query Structure

All queries support:
- **Filtering:** `where: { field: { eq: value } }`
- **Sorting:** `order: { field: ASC|DESC }`
- **Projection:** Select specific fields
- **Pagination:** `skip` and `take` parameters

### Core Entities

#### 1. Customers

**Queries:**
```graphql
# Get all customers
query GetCustomers {
  getCustomers {
    id
    name
    email
    phone
    status
    companyId
    createdAt
  }
}

# Get customer by ID
query GetCustomer($id: Int!) {
  getCustomer(id: $id) {
    id
    name
    email
    contacts {
      id
      firstName
      lastName
      email
    }
    opportunities {
      id
      name
      amount
      status
    }
  }
}
```

**Mutations:**
```graphql
# Create customer
mutation CreateCustomer($input: CreateCustomerDto!) {
  createCustomer(input: $input) {
    id
    name
    email
  }
}

# Update customer
mutation UpdateCustomer($id: Int!, $input: UpdateCustomerDto!) {
  updateCustomer(id: $id, input: $input) {
    id
    name
    email
  }
}

# Delete customer
mutation DeleteCustomer($id: Int!) {
  deleteCustomer(id: $id)
}
```

#### 2. Opportunities

**Queries:**
```graphql
# Get all opportunities
query GetOpportunities {
  getOpportunities {
    id
    name
    amount
    currency
    probability
    weightedAmount
    status
    pipelineStageName
    customerName
    expectedCloseDate
    quotes {
      id
      quoteNumber
      totalAmount
      status
    }
    contracts {
      id
      contractNumber
      status
    }
  }
}

# Get weighted pipeline value
query GetWeightedPipelineValue {
  getWeightedPipelineValue
}
```

**Mutations:**
```graphql
# Create opportunity
mutation CreateOpportunity($input: CreateOpportunityDto!) {
  createOpportunity(input: $input) {
    id
    name
    amount
    status
  }
}

# Update opportunity
mutation UpdateOpportunity($id: Int!, $input: UpdateOpportunityDto!) {
  updateOpportunity(id: $id, input: $input) {
    id
    name
    amount
    status
  }
}

# Move opportunity to stage
mutation MoveOpportunityToStage($id: Int!, $pipelineStageId: Int!) {
  moveOpportunityToStage(id: $id, pipelineStageId: $pipelineStageId) {
    id
    pipelineStageName
  }
}
```

**Workflow Mutations:**
```graphql
# Transition opportunity status (via workflow service)
# Note: This is handled internally when updating status
# The system validates transitions and logs activities automatically
```

#### 3. Quotes

**Queries:**
```graphql
# Get all quotes
query GetQuotes {
  getQuotes {
    id
    quoteNumber
    title
    totalAmount
    currency
    status
    customerName
    opportunityName
    validUntil
    lineItems {
      id
      productName
      quantity
      unitPrice
      totalPrice
    }
  }
}

# Get quotes by opportunity
query GetQuotesByOpportunity($opportunityId: Int!) {
  getQuotes(where: { opportunityId: { eq: $opportunityId } }) {
    id
    quoteNumber
    totalAmount
    status
  }
}
```

**Mutations:**
```graphql
# Create quote
mutation CreateQuote($input: CreateQuoteDto!) {
  createQuote(input: $input) {
    id
    quoteNumber
    totalAmount
    status
  }
}

# Update quote
mutation UpdateQuote($id: Int!, $input: UpdateQuoteDto!) {
  updateQuote(id: $id, input: $input) {
    id
    status
  }
}

# Convert quote to invoice
mutation ConvertQuoteToInvoice($id: Int!) {
  convertQuoteToInvoice(id: $id) {
    id
    quoteNumber
    convertedToInvoiceId
    status
  }
}
```

**Business Rules:**
- Quote can only be created from an **Open** Opportunity
- Quote must be **Accepted** before conversion to Invoice
- Quote status automatically updates to **Converted** when invoice is created

#### 4. Invoices

**Note:** Invoices are primarily synced from external financial systems (QuickBooks, Odoo, SAP). The CRM displays invoice data but does not manage the full financial lifecycle.

**Queries:**
```graphql
# Get all invoices (synced from external systems)
query GetInvoices {
  getInvoices {
    id
    invoiceNumber
    totalAmount
    currency
    status
    paidAmount
    balanceAmount
    dueDate
    customerName
    # Integration fields
    externalSystemId
    externalSystemType
    syncStatus
    lastSyncedAt
    payments {
      id
      amount
      paymentDate
      paymentMethod
    }
  }
}

# Get invoices by external system
query GetInvoicesByExternalSystem($systemType: String!, $systemId: String!) {
  getInvoices(where: { 
    externalSystemType: { eq: $systemType }
    externalSystemId: { eq: $systemId }
  }) {
    id
    invoiceNumber
    totalAmount
    syncStatus
  }
}
```

**Mutations:**
```graphql
# Sync invoice from external system
mutation SyncInvoiceFromExternalSystem($input: SyncInvoiceDto!) {
  syncInvoiceFromExternalSystem(input: $input) {
    id
    invoiceNumber
    totalAmount
    syncStatus
    lastSyncedAt
  }
}

# Trigger manual sync for invoice
mutation SyncInvoice($id: Int!) {
  syncInvoice(id: $id) {
    id
    syncStatus
    lastSyncedAt
  }
}

# Note: Direct invoice creation/update is limited
# Invoices should be synced from external financial systems
```

**Business Rules:**
- Invoices are primarily synced from external systems (QuickBooks, Odoo, SAP)
- Invoice data is read-only in CRM (except for sync operations)
- Sync status tracks: `Pending`, `Synced`, `SyncFailed`, `Conflict`
- Cannot manually delete synced invoices (must be deleted in external system)
- Invoice status and balances are synced from external system

#### 5. Payments

**Note:** Payments are synced from external financial systems. The CRM displays payment data but does not process payments directly.

**Queries:**
```graphql
# Get all payments (synced from external systems)
query GetPayments {
  getPayments {
    id
    amount
    paymentDate
    paymentMethod
    invoiceId
    invoiceNumber
    # Integration fields
    externalSystemId
    externalSystemType
    syncStatus
    lastSyncedAt
  }
}

# Get payments by invoice
query GetPaymentsByInvoice($invoiceId: Int!) {
  getPaymentsByInvoice(invoiceId: $invoiceId) {
    id
    amount
    paymentDate
    paymentMethod
    syncStatus
  }
}
```

**Mutations:**
```graphql
# Sync payment from external system
mutation SyncPaymentFromExternalSystem($input: SyncPaymentDto!) {
  syncPaymentFromExternalSystem(input: $input) {
    id
    amount
    paymentDate
    syncStatus
  }
}

# Trigger manual sync for payment
mutation SyncPayment($id: Int!) {
  syncPayment(id: $id) {
    id
    syncStatus
    lastSyncedAt
  }
}

# Note: Payments are synced from external systems
# Direct payment creation is not supported in CRM
```

**Business Rules:**
- Payments are synced from external financial systems
- Payment data is read-only in CRM
- Payment sync automatically updates invoice balances
- Sync status tracks sync state and errors

#### 6. Leads

**Queries:**
```graphql
# Get all leads
query GetLeads {
  getLeads {
    id
    firstName
    lastName
    email
    phone
    companyName
    leadScore
    status
    sourceName
    convertedFromLeadId
  }
}
```

**Mutations:**
```graphql
# Create lead
mutation CreateLead($input: CreateLeadDto!) {
  createLead(input: $input) {
    id
    firstName
    lastName
    email
    leadScore
  }
}

# Convert lead to customer
mutation ConvertLeadToCustomer($leadId: Int!, $input: ConvertLeadDto) {
  convertLeadToCustomer(leadId: $leadId, input: $input) {
    id
    status
    convertedFromLeadId
  }
}

# Convert lead to opportunity
mutation ConvertLeadToOpportunity($leadId: Int!, $input: ConvertLeadDto) {
  convertLeadToOpportunity(leadId: $leadId, input: $input) {
    id
    status
    convertedFromLeadId
  }
}
```

**ConvertLeadDto Options:**
```graphql
input ConvertLeadDto {
  createCustomer: Boolean = true
  customerId: Int  # Link to existing customer
  createOpportunity: Boolean = true
  opportunityId: Int  # Link to existing opportunity
  customerName: String  # If different from lead's company name
}
```

#### 7. Contracts

**Queries:**
```graphql
# Get all contracts
query GetContracts {
  getContracts {
    id
    contractNumber
    title
    totalValue
    currency
    status
    startDate
    endDate
    customerName
    opportunityName
    invoiceId
  }
}
```

**Mutations:**
```graphql
# Create contract
mutation CreateContract($input: CreateContractDto!) {
  createContract(input: $input) {
    id
    contractNumber
    status
  }
}

# Update contract
mutation UpdateContract($id: Int!, $input: UpdateContractDto!) {
  updateContract(id: $id, input: $input) {
    id
    status
  }
}
```

**Business Rules:**
- Contract can only be created from a **Won** Opportunity

#### 8. Communication Entities

##### Tasks

**Queries:**
```graphql
# Get all tasks
query GetTasks {
  getTasks {
    id
    title
    description
    type
    status
    dueDate
    customerId
    opportunityId
    quoteId
    invoiceId
    contractId
  }
}

# Get tasks by entity
query GetTasksByEntity($entityType: String!, $entityId: Int!) {
  getTasksByEntity(entityType: $entityType, entityId: $entityId) {
    id
    title
    status
    dueDate
  }
}
```

**Task Types:**
- `FollowUp`
- `Call`
- `Meeting`
- `Email`
- `Other`

##### Appointments

**Queries:**
```graphql
# Get all appointments
query GetAppointments {
  getAppointments {
    id
    title
    description
    category
    startTime
    endTime
    customerId
    opportunityId
    quoteId
  }
}

# Get appointments by entity
query GetAppointmentsByEntity($entityType: String!, $entityId: Int!) {
  getAppointmentsByEntity(entityType: $entityType, entityId: $entityId) {
    id
    title
    category
    startTime
    endTime
  }
}
```

**Appointment Categories:**
- `Sales`
- `Support`
- `Demo`
- `Meeting`
- `Other`

##### Emails

**Queries:**
```graphql
# Get all emails
query GetEmails {
  getEmails {
    id
    subject
    body
    fromEmail
    toEmail
    status
    customerId
    opportunityId
    quoteId
    invoiceId
  }
}
```

#### 9. Collaboration

##### Teams

**Queries:**
```graphql
# Get all teams
query GetTeams {
  getTeams {
    id
    name
    description
    memberCount
    members {
      id
      userName
      role
    }
  }
}
```

**Mutations:**
```graphql
# Create team
mutation CreateTeam($input: CreateTeamDto!) {
  createTeam(input: $input) {
    id
    name
  }
}

# Add team member
mutation AddTeamMember($teamId: Int!, $userId: Int!, $role: String!) {
  addTeamMember(teamId: $teamId, userId: $userId, role: $role) {
    id
    userName
    role
  }
}
```

##### Channels & Messages

**Queries:**
```graphql
# Get channels
query GetChannels {
  getChannels {
    id
    name
    type
    memberCount
    unreadCount
    lastMessageAt
  }
}

# Get messages
query GetMessages($channelId: Int!, $skip: Int = 0, $take: Int = 50) {
  getMessages(channelId: $channelId, skip: $skip, take: $take) {
    id
    content
    contentType
    createdByUserName
    createdAt
    reactionsJson
    attachments {
      id
      fileName
      fileUrl
    }
  }
}
```

**Mutations:**
```graphql
# Create channel
mutation CreateChannel($input: CreateChannelDto!) {
  createChannel(input: $input) {
    id
    name
    type
  }
}

# Send message
mutation SendMessage($input: CreateMessageDto!) {
  sendMessage(input: $input) {
    id
    content
    createdAt
  }
}

# React to message
mutation ReactToMessage($messageId: Int!, $reaction: String!) {
  reactToMessage(messageId: $messageId, reaction: $reaction) {
    id
    reactionsJson
  }
}
```

**Channel Types:**
- `Direct` - One-on-one conversation
- `Group` - Group chat
- `Team` - Team channel
- `Customer` - Customer workspace channel

#### 10. AI Agents

**Queries:**
```graphql
# Get AI agents
query GetAIAgents {
  getAIAgents {
    id
    name
    description
    prompt
    status
    tools {
      id
      name
      description
      toolType
    }
  }
}

# Get AI agent API keys
query GetAIAgentApiKeys($agentId: Int!) {
  getAIAgentApiKeys(agentId: $agentId) {
    id
    name
    lastUsedAt
    expiresAt
    isActive
  }
}
```

**Mutations:**
```graphql
# Create AI agent
mutation CreateAIAgent($input: CreateAIAgentDto!) {
  createAIAgent(input: $input) {
    id
    name
    prompt
  }
}

# Generate API key
mutation GenerateAIAgentApiKey($input: CreateAIAgentApiKeyDto!) {
  generateAIAgentApiKey(input: $input) {
    apiKey
    apiKeyId
    expiresAt
  }
}

# Create AI agent tool
mutation CreateAIAgentTool($input: CreateAIAgentToolDto!) {
  createAIAgentTool(input: $input) {
    id
    name
    description
    toolType
  }
}
```

#### 11. Financial Integrations

**Queries:**
```graphql
# Get all financial integrations
query GetFinancialIntegrations {
  getFinancialIntegrations {
    id
    name
    systemType
    status
    isActive
    lastSyncAt
    syncFrequency
    companyId
  }
}

# Get integration by type
query GetFinancialIntegrationByType($systemType: String!) {
  getFinancialIntegrations(where: { systemType: { eq: $systemType } }) {
    id
    name
    status
    lastSyncAt
  }
}

# Get sync history
query GetSyncHistory($integrationId: Int!, $skip: Int = 0, $take: Int = 50) {
  getSyncHistory(integrationId: $integrationId, skip: $skip, take: $take) {
    id
    entityType
    entityId
    syncStatus
    syncedAt
    errorMessage
  }
}
```

**Mutations:**
```graphql
# Create financial integration
mutation CreateFinancialIntegration($input: CreateFinancialIntegrationDto!) {
  createFinancialIntegration(input: $input) {
    id
    name
    systemType
    status
  }
}

# Update financial integration
mutation UpdateFinancialIntegration($id: Int!, $input: UpdateFinancialIntegrationDto!) {
  updateFinancialIntegration(id: $id, input: $input) {
    id
    status
    isActive
  }
}

# Test integration connection
mutation TestFinancialIntegration($id: Int!) {
  testFinancialIntegration(id: $id) {
    success
    message
    connectionDetails
  }
}

# Trigger manual sync
mutation TriggerFinancialSync($integrationId: Int!, $entityType: String) {
  triggerFinancialSync(integrationId: $integrationId, entityType: $entityType) {
    success
    syncedCount
    errorCount
  }
}

# Disconnect integration
mutation DisconnectFinancialIntegration($id: Int!) {
  disconnectFinancialIntegration(id: $id) {
    success
    message
  }
}
```

**Supported Systems:**
- `QuickBooks` - QuickBooks Online/Desktop
- `Odoo` - Odoo ERP
- `SAP` - SAP Finance & Operations

**Sync Entity Types:**
- `Customer` - Customer financial data
- `Invoice` - Invoices and billing
- `Order` - Sales orders
- `Payment` - Payment records
- `Balance` - Account balances

#### 12. Search

**Queries:**
```graphql
# Full-text search
query Search(
  $query: String!
  $entityTypes: [String!]
  $startDate: DateTime
  $endDate: DateTime
  $createdByUserId: Int
  $teamId: Int
  $limit: Int
  $offset: Int
) {
  search(
    query: $query
    entityTypes: $entityTypes
    startDate: $startDate
    endDate: $endDate
    createdByUserId: $createdByUserId
    teamId: $teamId
    limit: $limit
    offset: $offset
  ) {
    totalCount
    results {
      entityType
      entityId
      title
      description
      highlights
      relevanceScore
    }
  }
}

# Search suggestions
query GetSearchSuggestions($query: String!, $limit: Int = 10) {
  getSearchSuggestions(query: $query, limit: $limit) {
    text
    entityType
    entityId
  }
}
```

**Searchable Entity Types:**
- `Customer`
- `Opportunity`
- `Quote`
- `Invoice`
- `Contract`
- `Lead`
- `Task`
- `Appointment`
- `Email`
- `Message`
- `Channel`

#### 13. Activity Timeline

**Queries:**
```graphql
# Get activity timeline
query GetActivityTimeline(
  $entityType: String!
  $entityId: Int!
  $skip: Int = 0
  $take: Int = 50
) {
  getActivityTimeline(
    entityType: $entityType
    entityId: $entityId
    skip: $skip
    take: $take
  ) {
    id
    activityType
    description
    performedByUserName
    performedAt
    metadata
  }
}
```

**Activity Types:**
- `Created`
- `Updated`
- `StatusChanged`
- `PaymentReceived`
- `QuoteAccepted`
- `QuoteRejected`
- `InvoicePaid`
- `ContractSigned`
- `LeadConverted`
- `OpportunityWon`
- `OpportunityLost`
- `MessageSent`
- `FileUploaded`
- `NoteAdded`

#### 14. Notifications

**Queries:**
```graphql
# Get notifications
query GetNotifications($skip: Int = 0, $take: Int = 50) {
  getNotifications(skip: $skip, take: $take) {
    id
    title
    message
    type
    isRead
    createdAt
    relatedEntityType
    relatedEntityId
  }
}

# Get unread notification count
query GetUnreadNotificationCount {
  getUnreadNotificationCount
}
```

**Mutations:**
```graphql
# Mark notification as read
mutation MarkNotificationAsRead($id: Int!) {
  markNotificationAsRead(id: $id) {
    id
    isRead
  }
}

# Mark all notifications as read
mutation MarkAllNotificationsAsRead {
  markAllNotificationsAsRead
}
```

---

## REST API (AI Agents)

### Base URL
```
/api/ai-agent
```

### Authentication
Include API key in header:
```
X-API-Key: <api_key>
```

### Endpoints

#### 1. Health Check
```
GET /api/ai-agent/health
```

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

#### 2. Get Available Tools
```
GET /api/ai-agent/tools
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "create_customer",
    "description": "Create a new customer",
    "toolType": "Create",
    "parameters": {
      "name": "string",
      "email": "string",
      "phone": "string"
    }
  }
]
```

#### 3. Execute Tool
```
POST /api/ai-agent/execute-tool
Content-Type: application/json
X-API-Key: <api_key>
```

**Request:**
```json
{
  "toolName": "create_customer",
  "parameters": {
    "name": "Acme Corp",
    "email": "contact@acme.com",
    "phone": "+1234567890"
  }
}
```

**Response:**
```json
{
  "success": true,
  "result": {
    "id": 123,
    "name": "Acme Corp",
    "email": "contact@acme.com"
  },
  "executionTime": 45
}
```

**Error Response:**
```json
{
  "success": false,
  "error": "Validation failed: Email is required",
  "executionTime": 12
}
```

---

## Real-time Subscriptions

### WebSocket Connection

Connect to GraphQL endpoint via WebSocket for real-time updates.

### Available Subscriptions

#### 1. Message Created
```graphql
subscription OnMessageCreated($channelId: Int!) {
  onMessageCreated(channelId: $channelId) {
    id
    content
    contentType
    createdByUserName
    createdAt
    channelId
  }
}
```

#### 2. Message Updated
```graphql
subscription OnMessageUpdated($channelId: Int!) {
  onMessageUpdated(channelId: $channelId) {
    id
    content
    isEdited
    editedAt
  }
}
```

#### 3. Channel Created
```graphql
subscription OnChannelCreated($companyId: Int!) {
  onChannelCreated(companyId: $companyId) {
    id
    name
    type
    createdByUserName
    createdAt
  }
}
```

#### 4. Notification Created
```graphql
subscription OnNotificationCreated {
  onNotificationCreated {
    id
    title
    message
    type
    isRead
    createdAt
  }
}
```

#### 5. Notification Count Changed
```graphql
subscription OnNotificationCountChanged {
  onNotificationCountChanged {
    unreadCount
    totalCount
  }
}
```

---

## Data Models & DTOs

### Common Fields

Most entities include:
- `id: Int` - Unique identifier
- `companyId: Int` - Company context
- `createdAt: DateTime` - Creation timestamp
- `updatedAt: DateTime?` - Last update timestamp

### Status Enums

#### OpportunityStatus
- `Open`
- `Qualified`
- `Proposal`
- `Negotiation`
- `Won`
- `Lost`
- `Cancelled`

#### QuoteStatus
- `Draft`
- `Sent`
- `Accepted`
- `Rejected`
- `Expired`
- `Converted`

#### InvoiceStatus
- `Draft`
- `Sent`
- `Paid`
- `PartiallyPaid`
- `Overdue`
- `Cancelled`

#### ContractStatus
- `Draft`
- `Sent`
- `Signed`
- `Active`
- `Expired`
- `Renewed`
- `Cancelled`

#### LeadStatus
- `New`
- `Contacted`
- `Qualified`
- `Converted`
- `Lost`

### Relationship Fields

#### Opportunity
- `customerId: Int` - Linked customer
- `assignedUserId: Int?` - Assigned user
- `assignedToTeamId: Int?` - Assigned team
- `pipelineStageId: Int` - Current pipeline stage
- `sourceId: Int?` - Lead source
- `convertedFromLeadId: Int?` - Source lead (if converted)
- `quotes: [Quote]` - Related quotes
- `contracts: [Contract]` - Related contracts

#### Quote
- `customerId: Int` - Linked customer
- `opportunityId: Int?` - Linked opportunity
- `assignedToTeamId: Int?` - Assigned team
- `convertedToInvoiceId: Int?` - Converted invoice (if converted)
- `lineItems: [QuoteLineItem]` - Quote line items

#### Invoice
- `customerId: Int` - Linked customer
- `opportunityId: Int?` - Linked opportunity
- `assignedToTeamId: Int?` - Assigned team
- `payments: [Payment]` - Related payments
- `contracts: [Contract]` - Related contracts

#### Customer
- `assignedToTeamId: Int?` - Assigned team
- `convertedFromLeadId: Int?` - Source lead (if converted)
- `contacts: [Contact]` - Related contacts
- `opportunities: [Opportunity]` - Related opportunities

#### Task
- `customerId: Int?` - Linked customer
- `opportunityId: Int?` - Linked opportunity
- `quoteId: Int?` - Linked quote
- `invoiceId: Int?` - Linked invoice
- `contractId: Int?` - Linked contract
- `ticketId: Int?` - Linked ticket
- `type: TaskType` - Task type enum

#### Appointment
- `customerId: Int?` - Linked customer
- `opportunityId: Int?` - Linked opportunity
- `quoteId: Int?` - Linked quote
- `category: AppointmentCategory` - Appointment category enum

#### Email
- `customerId: Int?` - Linked customer
- `opportunityId: Int?` - Linked opportunity
- `quoteId: Int?` - Linked quote
- `invoiceId: Int?` - Linked invoice

---

## Business Rules & Workflows

### Sales Pipeline Workflow

1. **Lead → Customer → Opportunity**
   - Lead can be converted to Customer and/or Opportunity
   - Conversion preserves lead history
   - Lead status updates to "Converted"

2. **Opportunity Status Transitions**
   - Valid transitions are enforced
   - Status changes are logged in ActivityTimeline
   - Notifications sent on status changes

3. **Opportunity Won**
   - Can create Contract from Won opportunity
   - Can create Quote from Open opportunity
   - Activity logged and notifications sent

4. **Opportunity Lost**
   - Requires `lostReason`
   - Activity logged and notifications sent

### Quote Workflow

1. **Quote Creation**
   - Can only be created from **Open** Opportunity
   - Status starts as `Draft`

2. **Quote Status Transitions**
   - `Draft` → `Sent` (requires valid email)
   - `Sent` → `Accepted` | `Rejected` | `Expired`
   - `Accepted` → `Converted` (when invoice created)

3. **Quote to Invoice Conversion**
   - Quote must be **Accepted** before conversion
   - Creates invoice with quote line items
   - Updates quote status to `Converted`
   - Links invoice to quote via `convertedToInvoiceId`

### Invoice Workflow

1. **Invoice Status Calculation**
   - Automatically calculated based on payments:
     - `Paid`: `paidAmount >= totalAmount`
     - `PartiallyPaid`: `paidAmount > 0 && paidAmount < totalAmount`
     - `Overdue`: `dueDate < today && status != Paid`

2. **Payment Processing**
   - Payment amount cannot exceed invoice balance
   - Creating payment automatically:
     - Updates invoice `paidAmount` and `status`
     - Logs activity timeline entry
     - Updates linked opportunity (if exists)

3. **Invoice Deletion**
   - Cannot delete invoice if it has associated payments

### Contract Workflow

1. **Contract Creation**
   - Can only be created from **Won** Opportunity

2. **Contract Status Transitions**
   - `Draft` → `Sent` → `Signed` → `Active`
   - `Active` → `Expired` | `Renewed` | `Cancelled`

3. **Contract to Invoice Linking**
   - Contracts can be linked to invoices
   - Supports multiple invoices per contract

### Lead Conversion Workflow

1. **Conversion Options**
   - Create Customer only
   - Create Customer + Opportunity
   - Link to existing Customer
   - Link to existing Opportunity

2. **Data Preservation**
   - Lead data copied to Customer/Opportunity
   - Lead history preserved
   - `convertedFromLeadId` tracked on Customer/Opportunity

---

## Error Handling

### GraphQL Errors

Errors are returned in the standard GraphQL error format:

```json
{
  "errors": [
    {
      "message": "Validation failed: Email is required",
      "extensions": {
        "code": "VALIDATION_ERROR",
        "field": "email"
      }
    }
  ]
}
```

### Common Error Codes

- `VALIDATION_ERROR` - Input validation failed
- `NOT_FOUND` - Entity not found
- `UNAUTHORIZED` - Authentication required
- `FORBIDDEN` - Access denied
- `BUSINESS_RULE_VIOLATION` - Business rule violation
- `INTERNAL_ERROR` - Server error

### Error Handling Example

```typescript
try {
  const result = await client.mutate({
    mutation: CREATE_OPPORTUNITY,
    variables: { input: opportunityData }
  });
  
  if (result.errors) {
    result.errors.forEach(error => {
      console.error(`Error: ${error.message}`);
      if (error.extensions?.code === 'VALIDATION_ERROR') {
        // Handle validation error
      }
    });
  }
} catch (error) {
  console.error('Network error:', error);
}
```

---

## Code Examples

### React/TypeScript Example

```typescript
import { ApolloClient, InMemoryCache, gql, useQuery, useMutation } from '@apollo/client';

// Initialize Apollo Client
const client = new ApolloClient({
  uri: 'http://localhost:5000/graphql',
  cache: new InMemoryCache(),
  headers: {
    authorization: `Bearer ${localStorage.getItem('token')}`
  }
});

// Query Hook
const GET_OPPORTUNITIES = gql`
  query GetOpportunities {
    getOpportunities {
      id
      name
      amount
      status
      customerName
      expectedCloseDate
    }
  }
`;

function OpportunitiesList() {
  const { loading, error, data } = useQuery(GET_OPPORTUNITIES);
  
  if (loading) return <p>Loading...</p>;
  if (error) return <p>Error: {error.message}</p>;
  
  return (
    <ul>
      {data.getOpportunities.map((opp: any) => (
        <li key={opp.id}>
          {opp.name} - ${opp.amount} - {opp.status}
        </li>
      ))}
    </ul>
  );
}

// Mutation Hook
const CREATE_OPPORTUNITY = gql`
  mutation CreateOpportunity($input: CreateOpportunityDto!) {
    createOpportunity(input: $input) {
      id
      name
      amount
    }
  }
`;

function CreateOpportunityForm() {
  const [createOpportunity, { loading, error }] = useMutation(CREATE_OPPORTUNITY);
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const result = await createOpportunity({
        variables: {
          input: {
            name: 'New Opportunity',
            amount: 10000,
            customerId: 1,
            pipelineStageId: 1
          }
        }
      });
      console.log('Created:', result.data.createOpportunity);
    } catch (err) {
      console.error('Error:', err);
    }
  };
  
  return (
    <form onSubmit={handleSubmit}>
      {/* Form fields */}
      <button type="submit" disabled={loading}>
        {loading ? 'Creating...' : 'Create'}
      </button>
    </form>
  );
}
```

### Subscription Example

```typescript
import { useSubscription } from '@apollo/client';

const MESSAGE_SUBSCRIPTION = gql`
  subscription OnMessageCreated($channelId: Int!) {
    onMessageCreated(channelId: $channelId) {
      id
      content
      createdByUserName
      createdAt
    }
  }
`;

function MessageList({ channelId }: { channelId: number }) {
  const { data, loading } = useSubscription(MESSAGE_SUBSCRIPTION, {
    variables: { channelId }
  });
  
  if (loading) return <p>Loading messages...</p>;
  
  return (
    <div>
      {data?.onMessageCreated && (
        <div>
          <strong>{data.onMessageCreated.createdByUserName}:</strong>
          {data.onMessageCreated.content}
        </div>
      )}
    </div>
  );
}
```

### REST API Example (AI Agent)

```typescript
async function executeTool(toolName: string, parameters: any) {
  const response = await fetch('http://localhost:5000/api/ai-agent/execute-tool', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-API-Key': process.env.AI_AGENT_API_KEY!
    },
    body: JSON.stringify({
      toolName,
      parameters
    })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Tool execution failed');
  }
  
  return await response.json();
}

// Usage
const result = await executeTool('create_customer', {
  name: 'Acme Corp',
  email: 'contact@acme.com'
});
```

---

## Best Practices

### 1. Authentication
- Always include JWT token in Authorization header
- Handle token expiration and refresh
- Store tokens securely (httpOnly cookies recommended for production)

### 2. Error Handling
- Always check for GraphQL errors in response
- Handle network errors gracefully
- Display user-friendly error messages

### 3. Caching
- Use Apollo Client cache for frequently accessed data
- Implement cache invalidation on mutations
- Use optimistic updates for better UX

### 4. Real-time Updates
- Subscribe to relevant subscriptions for live data
- Handle subscription errors and reconnection
- Unsubscribe when component unmounts

### 5. Performance
- Use GraphQL projections to fetch only needed fields
- Implement pagination for large lists
- Use filtering and sorting on the server side

### 6. Type Safety
- Generate TypeScript types from GraphQL schema
- Use typed GraphQL clients (Apollo, urql, etc.)
- Validate inputs before sending mutations

### 7. Multi-tenancy
- Always ensure company context is set
- Handle company switching gracefully
- Filter data by company on the frontend as well

### 8. Business Rules
- Understand and respect business rules
- Validate inputs before submission
- Handle business rule violations gracefully

---

## Financial Integration Details

### Integration Architecture

The CRM uses a **sync-based approach** for financial data rather than maintaining a full financial model:

1. **External Systems as Source of Truth**
   - QuickBooks, Odoo, and SAP are the primary financial systems
   - CRM syncs data from these systems for display and reporting

2. **What Gets Synced**
   - **Customers:** Financial information, credit limits, payment terms
   - **Invoices:** All invoice data including line items, status, balances
   - **Orders:** Sales orders and purchase orders
   - **Payments:** Payment records and transaction history
   - **Balances:** Account balances and outstanding amounts

3. **Sync Direction**
   - **Primary:** External System → CRM (read-only display)
   - **Secondary:** CRM → External System (when quotes convert to invoices)

4. **Sync Methods**
   - **Automatic:** Scheduled syncs (configurable frequency)
   - **Manual:** On-demand sync via API
   - **Real-time:** Webhook-based sync (if supported by external system)

### Integration Setup

1. **Connect External System**
   ```graphql
   mutation CreateFinancialIntegration($input: CreateFinancialIntegrationDto!) {
     createFinancialIntegration(input: {
       name: "QuickBooks Production"
       systemType: "QuickBooks"
       connectionSettings: {
         # OAuth tokens, API keys, etc.
       }
       syncFrequency: "Hourly"
       syncEntities: ["Customer", "Invoice", "Payment", "Order"]
     }) {
       id
       status
     }
   }
   ```

2. **Test Connection**
   ```graphql
   mutation TestFinancialIntegration($id: Int!) {
     testFinancialIntegration(id: $id) {
       success
       message
     }
   }
   ```

3. **Trigger Initial Sync**
   ```graphql
   mutation TriggerFinancialSync($integrationId: Int!) {
     triggerFinancialSync(integrationId: $integrationId) {
       success
       syncedCount
     }
   }
   ```

### Sync Status

- `Pending` - Waiting to sync
- `Syncing` - Currently syncing
- `Synced` - Successfully synced
- `SyncFailed` - Sync failed (check error message)
- `Conflict` - Data conflict detected (requires resolution)

### Conflict Resolution

When data conflicts occur:
1. System detects differences between CRM and external system
2. Conflict is logged with details
3. User can choose resolution strategy:
   - Use external system data (recommended)
   - Keep CRM data (if applicable)
   - Manual merge

---

## Additional Resources

### GraphQL Playground
Access GraphQL Playground at:
```
http://localhost:5000/graphql
```

### Schema Introspection
Query the schema directly:
```graphql
query IntrospectionQuery {
  __schema {
    types {
      name
      kind
    }
  }
}
```

### API Documentation
- GraphQL endpoint: `/graphql`
- REST API endpoint: `/api/ai-agent`
- OpenAPI/Swagger: Available at `/swagger` (if enabled)

---

## Support & Contact

For questions or issues:
1. Check this documentation
2. Review GraphQL schema in Playground
3. Contact backend team

---

**Document Version:** 1.0  
**Last Updated:** January 2025  
**Maintained By:** Backend Team

