import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';

export interface CustomerContact {
  id?: string;
  type: 'email' | 'phone' | 'mobile' | 'fax' | 'website' | 'linkedin' | 'other';
  value: string;
  isPrimary?: boolean;
  label?: string;
}

export interface CustomerAddress {
  id?: string;
  type: 'billing' | 'shipping' | 'office' | 'warehouse' | 'other';
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  postalCode?: string;
  country: string;
  isPrimary?: boolean;
  label?: string;
}

export interface CustomerNote {
  id?: string;
  content: string;
  type: 'general' | 'meeting' | 'call' | 'email' | 'task' | 'reminder';
  createdAt?: string;
  createdBy?: string;
}

export interface CustomerTag {
  id?: string;
  name: string;
  color?: string;
}

export interface Customer {
  id?: string;
  name?: string;
  email?: string;
  phone?: string;
  companyName: string;
  contactPersonName?: string;
  customerType: 'lead' | 'prospect' | 'customer' | 'client' | 'enterprise' | 'partner' | 'dormant';
  industry?: string;
  website?: string;
  description?: string;
  source: 'website' | 'referral' | 'social' | 'advertising' | 'trade_show' | 'cold_call' | 'other';
  
  annualRevenue?: number;
  employeeCount?: number;
  taxId?: string;
  
  status: 'active' | 'purchasing' | 'inactive' | 'lost' | 'blacklisted';
  priority: 'low' | 'medium' | 'high' | 'critical';
  assignedUserId?: string;
  assignedUserName?: string;
  assignedToTeamId?: string;
  assignedToUserId?: string;
  assignedTeam?: { id: string; name: string; };
  assignedToUser?: { id: string; name: string; email: string; };
  
  parentCustomerId?: string;
  subsidiaryIds?: string[];
  firstPurchaseDate?: string;
  lastPurchaseDate?: string;
  averageOrderValue?: number;
  totalOrders?: number;
  customerLifetimeValue?: number;
  relationshipStartDate?: string;
  
  tags: CustomerTag[];
  notes: CustomerNote[];
  totalDeals?: number;
  totalRevenue?: number;
  lastContactDate?: string;
  nextFollowUpDate?: string;
  
  contacts: CustomerContact[];
  addresses: CustomerAddress[];
  
  createdAt?: string;
  updatedAt?: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface CustomerFilters {
  search?: string;
  customerType?: string[];
  status?: string[];
  priority?: string[];
  industry?: string[];
  source?: string[];
  assignedUserId?: string;
  tags?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private customersSubject = new BehaviorSubject<Customer[]>([]);
  public customers$ = this.customersSubject.asObservable();

  constructor(private apiService: ApiService) {}

  getCustomers(
    first: number = 20, 
    after?: string, 
    filters?: CustomerFilters
  ): Observable<{ customers: Customer[], pageInfo: any, totalCount: number }> {
    const query = `query GetCustomers($first: Int, $after: String, $search: String, $filters: CustomerFiltersDtoInput) {
      customers(first: $first, after: $after, search: $search, filters: $filters) {
        edges {
          node {
            id
            name
            email
            phone
            address
            city
            country
            status
            companyId
            companyName
            contactPersonName
            customerType
            industry
            website
            priority
            totalActivities
            lastActivity
            createdAt
            updatedAt
            assignedToTeamId
            assignedToUserId
            assignedTeam {
              id
              name
            }
            assignedToUser {
              id
              name
              email
            }
          }
        }
        pageInfo {
          hasNextPage
          hasPreviousPage
          startCursor
          endCursor
        }
        totalCount
      }
    }`;

    // Build filters object, taking first value for each array since backend expects single values
    const backendFilters: any = {};
    
    if (filters?.customerType && filters.customerType.length > 0) {
      backendFilters.customerType = filters.customerType[0];
    }
    if (filters?.status && filters.status.length > 0) {
      backendFilters.status = filters.status[0];  
    }
    if (filters?.priority && filters.priority.length > 0) {
      backendFilters.priority = filters.priority[0];
    }
    if (filters?.industry && filters.industry.length > 0) {
      backendFilters.industry = filters.industry[0];
    }

    const variables = {
      first,
      after,
      search: filters?.search,
      filters: Object.keys(backendFilters).length > 0 ? backendFilters : null
    };

    return this.apiService.graphql<any>(query, variables).pipe(
      map(response => {
        if (response.errors) {
          console.warn('GraphQL errors:', response.errors);
          return { customers: [], pageInfo: {}, totalCount: 0 };
        }
        
        const customers = response.data?.customers?.edges?.map((edge: any) => ({
          id: edge.node.id?.toString(),
          name: edge.node.contactPersonName || edge.node.name,
          email: edge.node.email,
          phone: edge.node.phone,
          companyName: edge.node.name, // Backend 'name' field is the company name
          contactPersonName: edge.node.contactPersonName,
          customerType: edge.node.customerType || 'customer',
          industry: edge.node.industry,
          website: edge.node.website,
          status: edge.node.status || 'active',
          priority: edge.node.priority || 'medium',
          createdAt: edge.node.createdAt,
          updatedAt: edge.node.updatedAt,
          totalDeals: edge.node.totalActivities,
          lastContactDate: edge.node.lastActivity,
          addresses: edge.node.address ? [{
            addressLine1: edge.node.address,
            city: edge.node.city,
            country: edge.node.country,
            type: 'office' as const
          }] : [],
          contacts: [],
          tags: [],
          notes: [],
          source: 'website' as const
        })) || [];

        // Apply client-side filtering for multiple selections
        let filteredCustomers = customers;
        
        if (filters?.customerType && filters.customerType.length > 1) {
          filteredCustomers = filteredCustomers.filter((c: Customer) => 
            filters.customerType!.includes(c.customerType)
          );
        }
        
        if (filters?.status && filters.status.length > 1) {
          filteredCustomers = filteredCustomers.filter((c: Customer) => 
            filters.status!.includes(c.status)
          );
        }
        
        if (filters?.priority && filters.priority.length > 1) {
          filteredCustomers = filteredCustomers.filter((c: Customer) => 
            filters.priority!.includes(c.priority)
          );
        }
        
        if (filters?.industry && filters.industry.length > 1) {
          filteredCustomers = filteredCustomers.filter((c: Customer) => 
            c.industry && filters.industry!.includes(c.industry)
          );
        }
        
        const pageInfo = response.data?.customers?.pageInfo || {};
        const totalCount = response.data?.customers?.totalCount || 0;
        
        this.customersSubject.next(filteredCustomers);
        return { customers: filteredCustomers, pageInfo, totalCount };
      }),
      catchError(error => {
        console.error('Error fetching customers:', error);
        const mockCustomers: Customer[] = [
          {
            id: '1',
            name: 'John Doe',
            email: 'john@example.com',
            phone: '+1-555-0123',
            companyName: 'Example Corp',
            customerType: 'customer',
            status: 'active',
            priority: 'high',
            industry: 'Technology',
            source: 'website',
            createdAt: new Date().toISOString(),
            contacts: [],
            addresses: [],
            tags: [],
            notes: []
          },
          {
            id: '2',
            name: 'Jane Smith',
            email: 'jane@techstart.com',
            phone: '+1-555-0456',
            companyName: 'TechStart Inc',
            customerType: 'prospect',
            status: 'purchasing',
            priority: 'medium',
            industry: 'Software',
            source: 'referral',
            createdAt: new Date().toISOString(),
            contacts: [],
            addresses: [],
            tags: [],
            notes: []
          }
        ];
        
        this.customersSubject.next(mockCustomers);
        return of({ customers: mockCustomers, pageInfo: {}, totalCount: mockCustomers.length });
      })
    );
  }

  getCustomerById(id: string): Observable<Customer> {
    const query = `query GetCustomer($id: Int!) {
      customer(id: $id) {
        id
        name
        email
        phone
        address
        city
        country
        status
        companyId
        companyName
        contactPersonName
        customerType
        industry
        website
        priority
        totalActivities
        lastActivity
        createdAt
        updatedAt
        assignedToTeamId
        assignedToUserId
        assignedTeam {
          id
          name
        }
        assignedToUser {
          id
          name
          email
        }
      }
    }`;

    return this.apiService.graphql<{ customer: any }>(query, { id: parseInt(id) }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0].message);
        }
        return {
          id: response.data!.customer.id?.toString(),
          name: response.data!.customer.contactPersonName || response.data!.customer.name,
          email: response.data!.customer.email,
          phone: response.data!.customer.phone,
          companyName: response.data!.customer.name || 'Unknown Company', // Backend 'name' field is company name
          contactPersonName: response.data!.customer.contactPersonName,
          customerType: response.data!.customer.customerType || 'customer',
          industry: response.data!.customer.industry,
          website: response.data!.customer.website,
          status: response.data!.customer.status || 'active',
          priority: response.data!.customer.priority || 'medium',
          createdAt: response.data!.customer.createdAt,
          updatedAt: response.data!.customer.updatedAt,
          addresses: response.data!.customer.address ? [{
            addressLine1: response.data!.customer.address,
            city: response.data!.customer.city,
            country: response.data!.customer.country,
            type: 'office' as const
          }] : [],
          contacts: [],
          tags: [],
          notes: [],
          source: 'website' as const
        } as Customer;
      }),
      catchError(error => {
        console.error('Error fetching customer:', error);
        return of({
          id: id,
          companyName: 'Sample Customer',
          customerType: 'customer' as const,
          status: 'active' as const,
          priority: 'medium' as const,
          source: 'website' as const,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          contacts: [],
          addresses: [],
          tags: [],
          notes: []
        } as Customer);
      })
    );
  }

  createCustomer(customerData: Partial<Customer>): Observable<Customer> {
    const mutation = `mutation CreateCustomer($input: CreateCustomerDtoInput!) {
      createCustomer(input: $input) {
        id
        name
        email
        phone
        address
        city
        country
        status
        companyId
        companyName
        contactPersonName
        customerType
        industry
        website
        priority
        createdAt
        assignedToTeamId
        assignedToUserId
        assignedTeam {
          id
          name
        }
        assignedToUser {
          id
          name
          email
        }
      }
    }`;

    // Map our Customer interface to backend CreateCustomerDtoInput
    const input: any = {
      name: customerData.companyName || customerData.name, // Backend 'name' field is company name
      email: customerData.email,
      phone: customerData.phone,
      address: customerData.addresses?.[0]?.addressLine1,
      city: customerData.addresses?.[0]?.city,
      country: customerData.addresses?.[0]?.country,
      contactPersonName: customerData.contactPersonName || customerData.name,
      customerType: customerData.customerType,
      industry: customerData.industry,
      website: customerData.website,
      priority: customerData.priority
    };

    // Add team and user assignments if provided
    if (customerData.assignedToTeamId) {
      input.assignedToTeamId = parseInt(customerData.assignedToTeamId);
    }
    if (customerData.assignedToUserId) {
      input.assignedToUserId = parseInt(customerData.assignedToUserId);
    }

    return this.apiService.graphql<{ createCustomer: Customer }>(mutation, { input }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0].message);
        }
        const newCustomer = {
          ...response.data!.createCustomer,
          contacts: [],
          addresses: [],
          tags: [],
          notes: []
        };
        
        const current = this.customersSubject.value;
        this.customersSubject.next([newCustomer, ...current]);
        
        return newCustomer;
      }),
      catchError(error => {
        console.error('Error creating customer:', error);
        const mockCustomer = {
          id: Date.now().toString(),
          ...customerData,
          createdAt: new Date().toISOString(),
          contacts: customerData.contacts || [],
          addresses: customerData.addresses || [],
          tags: [],
          notes: []
        } as Customer;
        
        const current = this.customersSubject.value;
        this.customersSubject.next([mockCustomer, ...current]);
        
        return of(mockCustomer);
      })
    );
  }

  updateCustomer(id: string, customerData: Partial<Customer>): Observable<Customer> {
    const mutation = `mutation UpdateCustomer($id: Int!, $input: UpdateCustomerDtoInput!) {
      updateCustomer(id: $id, input: $input) {
        id
        name
        email
        phone
        address
        city
        country
        status
        companyId
        companyName
        contactPersonName
        customerType
        industry
        website
        priority
        updatedAt
        assignedToTeamId
        assignedToUserId
        assignedTeam {
          id
          name
        }
        assignedToUser {
          id
          name
          email
        }
      }
    }`;

    // Map our Customer interface to backend UpdateCustomerDtoInput
    const input: any = {
      name: customerData.companyName || customerData.name,
      email: customerData.email,
      phone: customerData.phone,
      address: customerData.addresses?.[0]?.addressLine1,
      city: customerData.addresses?.[0]?.city,
      country: customerData.addresses?.[0]?.country,
      status: customerData.status,
      contactPersonName: customerData.contactPersonName,
      customerType: customerData.customerType,
      industry: customerData.industry,
      website: customerData.website,
      priority: customerData.priority
    };

    // Add team and user assignments if provided
    if (customerData.assignedToTeamId !== undefined) {
      input.assignedToTeamId = customerData.assignedToTeamId ? parseInt(customerData.assignedToTeamId) : null;
    }
    if (customerData.assignedToUserId !== undefined) {
      input.assignedToUserId = customerData.assignedToUserId ? parseInt(customerData.assignedToUserId) : null;
    }

    return this.apiService.graphql<{ updateCustomer: Customer }>(mutation, { id: parseInt(id), input }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0].message);
        }
        
        const updatedCustomer = {
          ...response.data!.updateCustomer,
          contacts: [],
          addresses: [],
          tags: [],
          notes: []
        };
        
        const current = this.customersSubject.value;
        const updated = current.map(c => c.id === id ? updatedCustomer : c);
        this.customersSubject.next(updated);
        
        return updatedCustomer;
      }),
      catchError(error => {
        console.error('Error updating customer:', error);
        return of({
          ...customerData,
          id,
          updatedAt: new Date().toISOString()
        } as Customer);
      })
    );
  }

  deleteCustomer(id: string): Observable<boolean> {
    const mutation = `mutation DeleteCustomer($id: Int!) {
      deleteCustomer(id: $id)
    }`;

    return this.apiService.graphql<{ deleteCustomer: boolean }>(mutation, { id: parseInt(id) }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0].message);
        }
        
        const current = this.customersSubject.value;
        const filtered = current.filter(c => c.id !== id);
        this.customersSubject.next(filtered);
        
        return true;
      }),
      catchError(error => {
        console.error('Error deleting customer:', error);
        return of(false);
      })
    );
  }

  getCustomersByType(customerType: string): Observable<Customer[]> {
    return this.getCustomers(100, undefined, { customerType: [customerType] })
      .pipe(map(result => result.customers));
  }

  searchCustomers(searchTerm: string): Observable<Customer[]> {
    return this.getCustomers(50, undefined, { search: searchTerm })
      .pipe(map(result => result.customers));
  }

  getCustomerStats(): Observable<any> {
    return this.customers$.pipe(
      map(customers => ({
        total: customers.length,
        active: customers.filter(c => c.status === 'active').length,
        prospects: customers.filter(c => c.customerType === 'prospect').length,
        clients: customers.filter(c => c.customerType === 'customer' || c.customerType === 'client').length
      }))
    );
  }
}