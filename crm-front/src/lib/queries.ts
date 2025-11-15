import { gql } from '@apollo/client';

export const CREATE_USER = gql`
  mutation CreateUser($input: CreateUserDtoInput!) {
    createUser(input: $input) {
      id
      name
      email
      phone
      createdAt
      companyId
    }
  }
`;

export const LOGIN_USER = gql`
  mutation LoginUser($email: String!, $password: String!) {
    login(email: $email, password: $password) {
      token
      user {
        id
        name
        email
        phone
        companyId
      }
    }
  }
`;

export const GET_USERS = gql`
  query GetUsers {
    users {
      id
      name
      email
      phone
      createdAt
      companyId
    }
  }
`;

export const GET_USER = gql`
  query GetUser($id: ID!) {
    user(id: $id) {
      id
      name
      email
      phone
      createdAt
      companyId
    }
  }
`;

export const CREATE_COMPANY = gql`
  mutation CreateCompany($input: CreateCompanyDtoInput!) {
    createCompany(input: $input) {
      id
      name
      description
      createdAt
      userCount
    }
  }
`;

export const ADD_USER_TO_COMPANY = gql`
  mutation AddUserToCompany($userId: Int!, $companyId: Int!) {
    addUserToCompany(userId: $userId, companyId: $companyId)
  }
`;

export const GET_COMPANIES = gql`
  query GetCompanies {
    companies {
      id
      name
      description
      createdAt
      userCount
    }
  }
`;

export const GET_ME = gql`
  query GetMe {
    me {
      id
      name
      email
      phone
      companyId
      createdAt
    }
  }
`;

export const GET_CUSTOMERS = gql`
  query GetCustomers {
    customers {
      id
      name
      email
      phone
      address
      city
      country
      companyId
      companyName
      createdAt
      updatedAt
    }
  }
`;

export const GET_CUSTOMER = gql`
  query GetCustomer($id: ID!) {
    customer(id: $id) {
      id
      name
      email
      phone
      address
      city
      country
      companyId
      companyName
      createdAt
      updatedAt
    }
  }
`;

export const GET_CUSTOMER_WITH_CONTACTS = gql`
  query GetCustomerWithContacts($id: ID!) {
    customerWithContacts(id: $id) {
      id
      name
      email
      phone
      address
      city
      country
      companyId
      companyName
      createdAt
      updatedAt
    }
  }
`;

export const CREATE_CUSTOMER = gql`
  mutation CreateCustomer($input: CreateCustomerDtoInput!) {
    createCustomer(input: $input) {
      id
      name
      email
      phone
      address
      city
      country
      companyId
      companyName
      createdAt
      updatedAt
    }
  }
`;

export const UPDATE_CUSTOMER = gql`
  mutation UpdateCustomer($input: UpdateCustomerDtoInput!) {
    updateCustomer(input: $input) {
      id
      name
      email
      phone
      address
      city
      country
      companyId
      companyName
      createdAt
      updatedAt
    }
  }
`;

export const DELETE_CUSTOMER = gql`
  mutation DeleteCustomer($id: ID!) {
    deleteCustomer(id: $id)
  }
`;

export const GET_TICKETS = gql`
  query GetTickets {
    tickets {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const GET_TICKET = gql`
  query GetTicket($id: ID!) {
    ticket(id: $id) {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const GET_MY_ASSIGNED_TICKETS = gql`
  query GetMyAssignedTickets {
    myAssignedTickets {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const GET_TICKETS_BY_CUSTOMER = gql`
  query GetTicketsByCustomer($customerId: ID!) {
    ticketsByCustomer(customerId: $customerId) {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const CREATE_TICKET = gql`
  mutation CreateTicket($input: CreateTicketDtoInput!) {
    createTicket(input: $input) {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const UPDATE_TICKET = gql`
  mutation UpdateTicket($input: UpdateTicketDtoInput!) {
    updateTicket(input: $input) {
      id
      title
      description
      status
      priority
      customerId
      customerName
      assignedUserId
      assignedUserName
      createdAt
      updatedAt
      resolvedAt
      resolution
      tags
    }
  }
`;

export const DELETE_TICKET = gql`
  mutation DeleteTicket($id: ID!) {
    deleteTicket(id: $id)
  }
`;

export const GET_CONTACTS_BY_CUSTOMER = gql`
  query GetContactsByCustomer($customerId: ID!) {
    contactsByCustomer(customerId: $customerId) {
      id
      name
      title
      email
      phone
      mobile
      isPrimary
      customerId
      createdAt
      updatedAt
    }
  }
`;

export const CREATE_CONTACT = gql`
  mutation CreateContact($input: CreateContactDtoInput!) {
    createContact(input: $input) {
      id
      name
      title
      email
      phone
      mobile
      isPrimary
      customerId
      createdAt
      updatedAt
    }
  }
`;

export const UPDATE_CONTACT = gql`
  mutation UpdateContact($input: UpdateContactDtoInput!) {
    updateContact(input: $input) {
      id
      name
      title
      email
      phone
      mobile
      isPrimary
      customerId
      createdAt
      updatedAt
    }
  }
`;

export const DELETE_CONTACT = gql`
  mutation DeleteContact($id: ID!) {
    deleteContact(id: $id)
  }
`;

export const GET_COMPANY = gql`
  query GetCompany($id: Int!) {
    company(id: $id) {
      id
      name
      description
      createdAt
      userCount
    }
  }
`;

export const UPDATE_COMPANY = gql`
  mutation UpdateCompany($input: UpdateCompanyDtoInput!) {
    updateCompany(input: $input) {
      id
      name
      description
      createdAt
      userCount
    }
  }
`;

export const REMOVE_USER_FROM_COMPANY = gql`
  mutation RemoveUserFromCompany($userId: Int!, $companyId: Int!) {
    removeUserFromCompany(userId: $userId, companyId: $companyId)
  }
`;

export const GET_COMPANY_USERS = gql`
  query GetCompanyUsers {
    users {
      id
      name
      email
      phone
      createdAt
      companyId
    }
  }
`;

export const UPDATE_USER = gql`
  mutation UpdateUser($input: UpdateUserDtoInput!) {
    updateUser(input: $input) {
      id
      name
      email
      phone
      companyId
      createdAt
    }
  }
`;

export const CHANGE_PASSWORD = gql`
  mutation ChangePassword($currentPassword: String!, $newPassword: String!) {
    changePassword(currentPassword: $currentPassword, newPassword: $newPassword) {
      success
      message
    }
  }
`;

export const DELETE_ACCOUNT = gql`
  mutation DeleteAccount($password: String!) {
    deleteAccount(password: $password) {
      success
      message
    }
  }
`;

export const SET_ACTIVE_COMPANY = gql`
  mutation SetActiveCompany($userId: Int!, $companyId: Int!) {
    setActiveCompany(userId: $userId, companyId: $companyId)
  }
`;