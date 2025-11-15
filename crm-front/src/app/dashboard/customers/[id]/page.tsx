'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter, useParams } from 'next/navigation';
import Link from 'next/link';
import DashboardLayout from '@/components/DashboardLayout';
import { Button, Input, Textarea, Modal, Card, Skeleton } from '@/components/ui';

interface Customer {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  companyId: string;
  companyName: string;
  createdAt: string;
  updatedAt?: string;
}

interface Ticket {
  id: string;
  title: string;
  description?: string;
  status: 'OPEN' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED';
  priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  createdAt: string;
}

interface Contact {
  id: string;
  name: string;
  title?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  isPrimary: boolean;
}

const STATUS_COLORS: Record<string, { bg: string; text: string }> = {
  OPEN: { bg: 'rgba(255, 193, 7, 0.1)', text: '#FF9500' },
  IN_PROGRESS: { bg: 'rgba(0, 122, 255, 0.1)', text: '#007AFF' },
  RESOLVED: { bg: 'rgba(52, 199, 89, 0.1)', text: '#34C759' },
  CLOSED: { bg: 'rgba(142, 142, 147, 0.1)', text: '#8E8E93' },
};

const PRIORITY_COLORS: Record<string, { bg: string; text: string }> = {
  LOW: { bg: 'rgba(52, 199, 89, 0.1)', text: '#34C759' },
  MEDIUM: { bg: 'rgba(255, 193, 7, 0.1)', text: '#FF9500' },
  HIGH: { bg: 'rgba(255, 149, 0, 0.1)', text: '#FF9500' },
  CRITICAL: { bg: 'rgba(255, 59, 48, 0.1)', text: '#FF3B30' },
};

export default function CustomerDetailPage() {
  const router = useRouter();
  const params = useParams();
  const customerId = params.id as string;

  const [authToken, setAuthToken] = useState<string | null>(null);
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [loading, setLoading] = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [showTicketModal, setShowTicketModal] = useState(false);
  const [showContactModal, setShowContactModal] = useState(false);
  const [users, setUsers] = useState<{ id: string; name: string }[]>([]);

  interface FormData {
    name: string;
    email: string;
    phone: string;
    address: string;
    city: string;
    country: string;
  }

  interface FormErrors {
    name?: string;
    email?: string;
  }

  const [formData, setFormData] = useState<FormData>({
    name: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    country: '',
  });
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  interface TicketFormData {
    title: string;
    description: string;
    priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
    assignedUserId?: string;
  }

  interface ContactFormData {
    name: string;
    title: string;
    email: string;
    phone: string;
    mobile: string;
    isPrimary: boolean;
  }

  const [ticketFormData, setTicketFormData] = useState<TicketFormData>({
    title: '',
    description: '',
    priority: 'MEDIUM',
    assignedUserId: '',
  });

  const [contactFormData, setContactFormData] = useState<ContactFormData>({
    name: '',
    title: '',
    email: '',
    phone: '',
    mobile: '',
    isPrimary: false,
  });

  const [ticketFormErrors, setTicketFormErrors] = useState<{ title?: string; description?: string; general?: string }>({});
  const [contactFormErrors, setContactFormErrors] = useState<{ name?: string; general?: string }>({});

  // Check authentication on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }
    setAuthToken(token);
    fetchUsers(token);
  }, [router]);

  // Fetch users for ticket assignment
  const fetchUsers = useCallback(async (token: string) => {
    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query {
              users {
                id
                name
              }
            }
          `,
        })
      });

      const result = await response.json();
      if (!result.errors) {
        setUsers(result.data.users || []);
      }
    } catch (error) {
      console.error('Error fetching users:', error);
    }
  }, []);

  // Fetch customer details
  const fetchCustomerData = useCallback(async (token: string) => {
    try {
      setLoading(true);
      
      // Fetch customer info
      const customerResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetCustomer($id: Int!) {
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
          `,
          variables: { id: parseInt(customerId) }
        })
      });

      const customerResult = await customerResponse.json();
      if (customerResult.data?.customer) {
        const customerData = customerResult.data.customer;
        setCustomer(customerData);
        setFormData({
          name: customerData.name,
          email: customerData.email || '',
          phone: customerData.phone || '',
          address: customerData.address || '',
          city: customerData.city || '',
          country: customerData.country || '',
        });
      }

      // Fetch tickets for this customer
      const ticketsResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetTicketsByCustomer($customerId: Int!) {
              ticketsByCustomer(customerId: $customerId) {
                id
                title
                description
                status
                priority
                createdAt
              }
            }
          `,
          variables: { customerId: parseInt(customerId) }
        })
      });

      const ticketsResult = await ticketsResponse.json();
      setTickets(ticketsResult.data?.ticketsByCustomer || []);

      // Fetch contacts for this customer
      const contactsResponse = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetContactsByCustomer($customerId: Int!) {
              contactsByCustomer(customerId: $customerId) {
                id
                name
                title
                email
                phone
                mobile
                isPrimary
              }
            }
          `,
          variables: { customerId: parseInt(customerId) }
        })
      });

      const contactsResult = await contactsResponse.json();
      setContacts(contactsResult.data?.contactsByCustomer || []);
    } catch (error) {
      console.error('Error fetching customer details:', error);
    } finally {
      setLoading(false);
    }
  }, [customerId]);

  // Load data on mount
  useEffect(() => {
    if (authToken) {
      fetchCustomerData(authToken);
    }
  }, [authToken, fetchCustomerData]);

  // Validate form
  const validateForm = (): boolean => {
    const errors: FormErrors = {};

    if (!formData.name.trim()) {
      errors.name = 'Customer name is required';
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Please enter a valid email address';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Handle update customer
  const handleUpdateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken || !customer) return;

    setIsSubmitting(true);
    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
            mutation UpdateCustomer($id: Int!, $input: UpdateCustomerDtoInput!) {
              updateCustomer(id: $id, input: $input) {
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
          `,
          variables: {
            id: parseInt(customer.id),
            input: {
              name: formData.name,
              email: formData.email || null,
              phone: formData.phone || null,
              address: formData.address || null,
              city: formData.city || null,
              country: formData.country || null,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setFormErrors({ email: result.errors[0].message });
        return;
      }

      setCustomer(result.data.updateCustomer);
      setShowEditModal(false);
    } catch (error) {
      console.error('Error updating customer:', error);
      setFormErrors({ email: 'Failed to update customer' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle delete customer
  const handleDeleteCustomer = async () => {
    if (!authToken || !customer) return;

    setIsSubmitting(true);
    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
            mutation DeleteCustomer($id: Int!) {
              deleteCustomer(id: $id)
            }
          `,
          variables: { id: parseInt(customer.id) }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error deleting customer:', result.errors);
        return;
      }

      router.push('/dashboard/customers');
    } catch (error) {
      console.error('Error deleting customer:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle create ticket
  const handleCreateTicket = async (e: React.FormEvent) => {
    e.preventDefault();

    const errors: { title?: string; description?: string; general?: string } = {};
    if (!ticketFormData.title.trim()) {
      errors.title = 'Ticket title is required';
    }
    if (!ticketFormData.description.trim()) {
      errors.description = 'Ticket description is required';
    }

    setTicketFormErrors(errors);
    if (Object.keys(errors).length > 0 || !authToken || !customer) return;

    setIsSubmitting(true);
    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
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
              }
            }
          `,
          variables: {
            input: {
              title: ticketFormData.title,
              description: ticketFormData.description,
              priority: ticketFormData.priority,
              customerId: parseInt(customer.id),
              assignedUserId: ticketFormData.assignedUserId ? parseInt(ticketFormData.assignedUserId) : null,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setTicketFormErrors({ general: result.errors[0].message });
        return;
      }

      // Refresh tickets list
      if (authToken) {
        fetchCustomerData(authToken);
      }
      setShowTicketModal(false);
      setTicketFormData({
        title: '',
        description: '',
        priority: 'MEDIUM',
        assignedUserId: '',
      });
      setTicketFormErrors({});
    } catch (error) {
      setTicketFormErrors({ general: 'Failed to create ticket. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle create contact
  const handleCreateContact = async (e: React.FormEvent) => {
    e.preventDefault();

    const errors: { name?: string; general?: string } = {};
    if (!contactFormData.name.trim()) {
      errors.name = 'Contact name is required';
    }

    setContactFormErrors(errors);
    if (Object.keys(errors).length > 0 || !authToken || !customer) return;

    setIsSubmitting(true);
    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
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
          `,
          variables: {
            input: {
              name: contactFormData.name,
              title: contactFormData.title || null,
              email: contactFormData.email || null,
              phone: contactFormData.phone || null,
              mobile: contactFormData.mobile || null,
              isPrimary: contactFormData.isPrimary,
              customerId: parseInt(customer.id),
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setContactFormErrors({ general: result.errors[0].message });
        return;
      }

      // Refresh contacts list
      if (authToken) {
        fetchCustomerData(authToken);
      }
      setShowContactModal(false);
      setContactFormData({
        name: '',
        title: '',
        email: '',
        phone: '',
        mobile: '',
        isPrimary: false,
      });
      setContactFormErrors({});
    } catch (error) {
      setContactFormErrors({ general: 'Failed to create contact. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <DashboardLayout currentPage="customers">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <div className="mb-8">
              <Skeleton variant="rectangular" height={40} width={200} className="mb-2" />
              <Skeleton variant="text" width={300} />
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <Card padding="md">
                <Skeleton variant="text" width={150} className="mb-4" />
                <div className="space-y-4">
                  {[1, 2, 3, 4, 5].map((i) => (
                    <div key={i}>
                      <Skeleton variant="text" width={80} className="mb-1" />
                      <Skeleton variant="text" width={150} />
                    </div>
                  ))}
                </div>
              </Card>
              <div className="lg:col-span-2 space-y-6">
                <Card padding="md">
                  <Skeleton variant="text" width={120} className="mb-4" />
                  <Skeleton variant="rectangular" height={100} />
                </Card>
                <Card padding="md">
                  <Skeleton variant="text" width={120} className="mb-4" />
                  <Skeleton variant="rectangular" height={100} />
                </Card>
              </div>
            </div>
          </div>
        </main>
      </DashboardLayout>
    );
  }

  if (!customer) {
    return (
      <DashboardLayout currentPage="customers">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <Card padding="lg">
              <div className="text-center py-12">
                <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p className="text-base font-medium mb-4" style={{ color: 'var(--gray-900)' }}>Customer not found</p>
                <Link 
                  href="/dashboard/customers" 
                  className="text-sm font-medium transition-colors hover:opacity-80"
                  style={{ color: 'var(--blue)' }}
                >
                  ← Back to Customers
                </Link>
              </div>
            </Card>
          </div>
        </main>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout currentPage="customers">
      <main className="py-8" style={{ background: 'var(--gray-50)' }}>
        <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
          {/* Header */}
          <div className="flex items-center justify-between mb-8">
            <div className="flex items-center gap-4">
              <Link 
                href="/dashboard/customers" 
                className="text-sm font-medium transition-colors hover:opacity-80"
                style={{ color: 'var(--blue)' }}
              >
                ← Back to Customers
              </Link>
              <div>
                <h1 className="text-3xl font-semibold mb-1" style={{ color: 'var(--gray-900)' }}>{customer.name}</h1>
                <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Customer details and related information</p>
              </div>
            </div>
            <div className="flex gap-3">
              <Button
                onClick={() => setShowEditModal(true)}
                variant="primary"
                size="md"
              >
                Edit Customer
              </Button>
              <Button
                onClick={() => setShowDeleteConfirm(true)}
                variant="danger"
                size="md"
              >
                Delete Customer
              </Button>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Customer Information Card */}
            <Card padding="md">
              <h2 className="text-lg font-semibold mb-4" style={{ color: 'var(--gray-900)' }}>Customer Information</h2>
              <div className="space-y-4">
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Email</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.email || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Phone</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.phone || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Address</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.address || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>City</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.city || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Country</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.country || '-'}</p>
                </div>
                <div className="pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Company</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{customer.companyName || '-'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-500)' }}>Created</p>
                  <p className="text-base" style={{ color: 'var(--gray-900)' }}>{new Date(customer.createdAt).toLocaleDateString()}</p>
                </div>
              </div>
            </Card>

            {/* Tickets and Contacts */}
            <div className="lg:col-span-2 space-y-6">
              {/* Tickets */}
              <Card padding="md">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold" style={{ color: 'var(--gray-900)' }}>Tickets ({tickets.length})</h2>
                  <Button
                    onClick={() => setShowTicketModal(true)}
                    variant="primary"
                    size="sm"
                  >
                    <svg className="-ml-1 mr-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                    New Ticket
                  </Button>
                </div>
                {tickets.length === 0 ? (
                  <div className="text-center py-8">
                    <svg className="mx-auto h-12 w-12 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <p className="text-sm mb-4" style={{ color: 'var(--gray-500)' }}>No tickets for this customer</p>
                    <Button
                      onClick={() => setShowTicketModal(true)}
                      variant="primary"
                      size="sm"
                    >
                      Create First Ticket
                    </Button>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {tickets.map((ticket) => {
                      const statusColor = STATUS_COLORS[ticket.status];
                      const priorityColor = PRIORITY_COLORS[ticket.priority];
                      return (
                        <Card key={ticket.id} padding="md" hover>
                          <div className="flex items-start justify-between mb-2">
                            <h3 className="font-medium" style={{ color: 'var(--gray-900)' }}>{ticket.title}</h3>
                            <p className="text-xs" style={{ color: 'var(--gray-500)' }}>{new Date(ticket.createdAt).toLocaleDateString()}</p>
                          </div>
                          {ticket.description && (
                            <p className="text-sm mb-3" style={{ color: 'var(--gray-600)' }}>{ticket.description}</p>
                          )}
                          <div className="flex gap-2">
                            <span 
                              className="text-xs px-2.5 py-1 rounded-full font-medium"
                              style={{ 
                                backgroundColor: statusColor.bg,
                                color: statusColor.text,
                              }}
                            >
                              {ticket.status}
                            </span>
                            <span 
                              className="text-xs px-2.5 py-1 rounded-full font-medium"
                              style={{ 
                                backgroundColor: priorityColor.bg,
                                color: priorityColor.text,
                              }}
                            >
                              {ticket.priority}
                            </span>
                          </div>
                        </Card>
                      );
                    })}
                  </div>
                )}
              </Card>

              {/* Contacts */}
              <Card padding="md">
                <div className="flex items-center justify-between mb-4">
                  <h2 className="text-lg font-semibold" style={{ color: 'var(--gray-900)' }}>Contacts ({contacts.length})</h2>
                  <Button
                    onClick={() => setShowContactModal(true)}
                    variant="primary"
                    size="sm"
                  >
                    <svg className="-ml-1 mr-1 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                    New Contact
                  </Button>
                </div>
                {contacts.length === 0 ? (
                  <div className="text-center py-8">
                    <svg className="mx-auto h-12 w-12 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.856-1.487M15 10a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    <p className="text-sm mb-4" style={{ color: 'var(--gray-500)' }}>No contacts for this customer</p>
                    <Button
                      onClick={() => setShowContactModal(true)}
                      variant="primary"
                      size="sm"
                    >
                      Add First Contact
                    </Button>
                  </div>
                ) : (
                  <div className="space-y-3">
                    {contacts.map((contact) => (
                      <Card key={contact.id} padding="md" hover>
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center gap-2">
                            <h3 className="font-medium" style={{ color: 'var(--gray-900)' }}>{contact.name}</h3>
                            {contact.isPrimary && (
                              <span 
                                className="text-xs px-2 py-1 rounded-full font-medium"
                                style={{ 
                                  backgroundColor: 'rgba(52, 199, 89, 0.1)',
                                  color: '#34C759',
                                }}
                              >
                                Primary
                              </span>
                            )}
                          </div>
                        </div>
                        {contact.title && (
                          <p className="text-sm mb-2" style={{ color: 'var(--gray-500)' }}>{contact.title}</p>
                        )}
                        <div className="flex flex-col gap-1 text-sm">
                          {contact.email && (
                            <p style={{ color: 'var(--gray-600)' }}>
                              <span className="font-medium">Email:</span>{' '}
                              <a href={`mailto:${contact.email}`} className="transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>
                                {contact.email}
                              </a>
                            </p>
                          )}
                          {contact.phone && (
                            <p style={{ color: 'var(--gray-600)' }}>
                              <span className="font-medium">Phone:</span>{' '}
                              <a href={`tel:${contact.phone}`} className="transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>
                                {contact.phone}
                              </a>
                            </p>
                          )}
                          {contact.mobile && (
                            <p style={{ color: 'var(--gray-600)' }}>
                              <span className="font-medium">Mobile:</span>{' '}
                              <a href={`tel:${contact.mobile}`} className="transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>
                                {contact.mobile}
                              </a>
                            </p>
                          )}
                        </div>
                      </Card>
                    ))}
                  </div>
                )}
              </Card>
            </div>
          </div>
        </div>

        {/* Edit Modal */}
        <Modal
          isOpen={showEditModal}
          onClose={() => setShowEditModal(false)}
          title="Edit Customer"
          size="md"
        >
          <form onSubmit={handleUpdateCustomer}>
            <div className="space-y-4">
              <Input
                label="Name"
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Customer name"
                error={formErrors.name}
                required
              />

              <Input
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                placeholder="customer@example.com"
                error={formErrors.email}
              />

              <Input
                label="Phone"
                type="tel"
                value={formData.phone}
                onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                placeholder="+1 (555) 123-4567"
              />

              <Input
                label="Address"
                type="text"
                value={formData.address}
                onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                placeholder="Street address"
              />

              <Input
                label="City"
                type="text"
                value={formData.city}
                onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                placeholder="City"
              />

              <Input
                label="Country"
                type="text"
                value={formData.country}
                onChange={(e) => setFormData({ ...formData, country: e.target.value })}
                placeholder="Country"
              />
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Button
                type="button"
                variant="secondary"
                onClick={() => setShowEditModal(false)}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="primary"
                isLoading={isSubmitting}
                disabled={isSubmitting}
              >
                Update
              </Button>
            </div>
          </form>
        </Modal>

        {/* Delete Confirmation Modal */}
        <Modal
          isOpen={showDeleteConfirm}
          onClose={() => setShowDeleteConfirm(false)}
          title="Delete Customer?"
          size="sm"
        >
          <p className="text-sm mb-6" style={{ color: 'var(--gray-600)' }}>
            Are you sure you want to delete this customer? This action cannot be undone.
          </p>

          <div className="flex justify-end gap-3">
            <Button
              variant="secondary"
              onClick={() => setShowDeleteConfirm(false)}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={handleDeleteCustomer}
              isLoading={isSubmitting}
              disabled={isSubmitting}
            >
              Delete
            </Button>
          </div>
        </Modal>

        {/* Create Ticket Modal */}
        <Modal
          isOpen={showTicketModal}
          onClose={() => {
            setShowTicketModal(false);
            setTicketFormData({
              title: '',
              description: '',
              priority: 'MEDIUM',
              assignedUserId: '',
            });
            setTicketFormErrors({});
          }}
          title={customer ? `New Ticket for ${customer.name}` : 'New Ticket'}
          size="md"
        >
          <form onSubmit={handleCreateTicket}>
            <div className="space-y-4">
              <Input
                label="Title"
                type="text"
                value={ticketFormData.title}
                onChange={(e) => setTicketFormData({ ...ticketFormData, title: e.target.value })}
                placeholder="Ticket title"
                error={ticketFormErrors.title}
                required
              />

              <Textarea
                label="Description"
                value={ticketFormData.description}
                onChange={(e) => setTicketFormData({ ...ticketFormData, description: e.target.value })}
                placeholder="Describe the issue..."
                rows={3}
                error={ticketFormErrors.description}
                required
              />

              <div>
                <label className="block text-sm font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>Priority</label>
                <select
                  value={ticketFormData.priority}
                  onChange={(e) => setTicketFormData({ ...ticketFormData, priority: e.target.value as any })}
                  className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                  style={{
                    backgroundColor: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                >
                  <option value="LOW">LOW</option>
                  <option value="MEDIUM">MEDIUM</option>
                  <option value="HIGH">HIGH</option>
                  <option value="CRITICAL">CRITICAL</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>Assign To</label>
                <select
                  value={ticketFormData.assignedUserId || ''}
                  onChange={(e) => setTicketFormData({ ...ticketFormData, assignedUserId: e.target.value })}
                  className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                  style={{
                    backgroundColor: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                >
                  <option value="">Unassigned</option>
                  {users.map(user => (
                    <option key={user.id} value={user.id}>{user.name}</option>
                  ))}
                </select>
              </div>

              {ticketFormErrors.general && (
                <p className="text-sm" style={{ color: 'var(--red)' }}>{ticketFormErrors.general}</p>
              )}
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Button
                type="button"
                variant="secondary"
                onClick={() => {
                  setShowTicketModal(false);
                  setTicketFormData({
                    title: '',
                    description: '',
                    priority: 'MEDIUM',
                    assignedUserId: '',
                  });
                  setTicketFormErrors({});
                }}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="primary"
                isLoading={isSubmitting}
                disabled={isSubmitting}
              >
                Create Ticket
              </Button>
            </div>
          </form>
        </Modal>

        {/* Create Contact Modal */}
        <Modal
          isOpen={showContactModal}
          onClose={() => {
            setShowContactModal(false);
            setContactFormData({
              name: '',
              title: '',
              email: '',
              phone: '',
              mobile: '',
              isPrimary: false,
            });
            setContactFormErrors({});
          }}
          title={customer ? `New Contact for ${customer.name}` : 'New Contact'}
          size="md"
        >
          <form onSubmit={handleCreateContact}>
            <div className="space-y-4">
              <Input
                label="Name"
                type="text"
                value={contactFormData.name}
                onChange={(e) => setContactFormData({ ...contactFormData, name: e.target.value })}
                placeholder="Contact name"
                error={contactFormErrors.name}
                required
              />

              <Input
                label="Title"
                type="text"
                value={contactFormData.title}
                onChange={(e) => setContactFormData({ ...contactFormData, title: e.target.value })}
                placeholder="e.g., CEO, Manager"
              />

              <Input
                label="Email"
                type="email"
                value={contactFormData.email}
                onChange={(e) => setContactFormData({ ...contactFormData, email: e.target.value })}
                placeholder="contact@example.com"
              />

              <Input
                label="Phone"
                type="tel"
                value={contactFormData.phone}
                onChange={(e) => setContactFormData({ ...contactFormData, phone: e.target.value })}
                placeholder="+1 (555) 123-4567"
              />

              <Input
                label="Mobile"
                type="tel"
                value={contactFormData.mobile}
                onChange={(e) => setContactFormData({ ...contactFormData, mobile: e.target.value })}
                placeholder="+1 (555) 123-4567"
              />

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isPrimary"
                  checked={contactFormData.isPrimary}
                  onChange={(e) => setContactFormData({ ...contactFormData, isPrimary: e.target.checked })}
                  className="h-4 w-4 rounded border-gray-300 focus:ring-2 focus:ring-blue-500"
                  style={{ accentColor: 'var(--blue)' }}
                />
                <label htmlFor="isPrimary" className="ml-2 block text-sm" style={{ color: 'var(--gray-700)' }}>
                  Primary Contact
                </label>
              </div>

              {contactFormErrors.general && (
                <p className="text-sm" style={{ color: 'var(--red)' }}>{contactFormErrors.general}</p>
              )}
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Button
                type="button"
                variant="secondary"
                onClick={() => {
                  setShowContactModal(false);
                  setContactFormData({
                    name: '',
                    title: '',
                    email: '',
                    phone: '',
                    mobile: '',
                    isPrimary: false,
                  });
                  setContactFormErrors({});
                }}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="primary"
                isLoading={isSubmitting}
                disabled={isSubmitting}
              >
                Create Contact
              </Button>
            </div>
          </form>
        </Modal>
      </main>
    </DashboardLayout>
  );
}


