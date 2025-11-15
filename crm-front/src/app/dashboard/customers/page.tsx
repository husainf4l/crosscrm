'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import DashboardLayout from '@/components/DashboardLayout';
import { Button, Input, Textarea, Modal, Card, Spinner, Skeleton } from '@/components/ui';

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
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  general?: string;
}

interface TicketFormData {
  title: string;
  description: string;
  priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  customerId: string;
  assignedUserId?: string;
}

interface ContactFormData {
  name: string;
  title: string;
  email: string;
  phone: string;
  mobile: string;
  isPrimary: boolean;
  customerId: string;
}

interface User {
  id: string;
  name: string;
}

export default function CustomersPage() {
  const router = useRouter();
  const [authToken, setAuthToken] = useState<string | null>(null);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingCustomerId, setDeletingCustomerId] = useState<string | null>(null);
  const [showTicketModal, setShowTicketModal] = useState(false);
  const [showContactModal, setShowContactModal] = useState(false);
  const [selectedCustomerForTicket, setSelectedCustomerForTicket] = useState<Customer | null>(null);
  const [selectedCustomerForContact, setSelectedCustomerForContact] = useState<Customer | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [formData, setFormData] = useState<FormData>({
    name: '',
    email: '',
    phone: '',
    address: '',
    city: '',
    country: '',
  });
  const [ticketFormData, setTicketFormData] = useState<TicketFormData>({
    title: '',
    description: '',
    priority: 'MEDIUM',
    customerId: '',
    assignedUserId: '',
  });
  const [contactFormData, setContactFormData] = useState<ContactFormData>({
    name: '',
    title: '',
    email: '',
    phone: '',
    mobile: '',
    isPrimary: false,
    customerId: '',
  });
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const [ticketFormErrors, setTicketFormErrors] = useState<{ title?: string; description?: string; general?: string }>({});
  const [contactFormErrors, setContactFormErrors] = useState<{ name?: string; general?: string }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Check authentication on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }
    setAuthToken(token);
    fetchCustomers(token);
    fetchUsers(token);
  }, [router]);

  // Fetch customers from GraphQL
  const fetchCustomers = useCallback(async (token: string) => {
    try {
      setLoading(true);
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query {
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
          `,
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error fetching customers:', result.errors);
        if (result.errors[0].message.includes('Invalid') || result.errors[0].message.includes('Unauthorized')) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('currentUser');
          router.push('/login');
        }
        return;
      }

      setCustomers(result.data.customers || []);
    } catch (error) {
      console.error('Error fetching customers:', error);
    } finally {
      setLoading(false);
    }
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

  // Validate form
  const validateForm = useCallback((): boolean => {
    const errors: FormErrors = {};

    if (!formData.name.trim()) {
      errors.name = 'Customer name is required';
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Please enter a valid email address';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData]);

  // Handle create customer
  const handleCreateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken) return;

    setIsSubmitting(true);
    try {
      const userData = localStorage.getItem('currentUser');
      const user = userData ? JSON.parse(userData) : null;
      const companyId = user?.companyId;

      if (!companyId) {
        setFormErrors({ general: 'No active company. Please create or select a company first.' });
        setIsSubmitting(false);
        return;
      }

      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
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
          `,
          variables: {
            input: {
              name: formData.name,
              email: formData.email || null,
              phone: formData.phone || null,
              address: formData.address || null,
              city: formData.city || null,
              country: formData.country || null,
              companyId: parseInt(companyId),
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setFormErrors({ general: result.errors[0].message });
        return;
      }

      // Add new customer to list
      setCustomers([...customers, result.data.createCustomer]);
      setShowCreateModal(false);
      resetForm();
    } catch (error) {
      setFormErrors({ general: 'Failed to create customer. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle update customer
  const handleUpdateCustomer = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken || !editingCustomer) return;

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
          `,
          variables: {
            input: {
              id: editingCustomer.id,
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
        setFormErrors({ general: result.errors[0].message });
        return;
      }

      // Update customer in list
      setCustomers(customers.map(c => c.id === editingCustomer.id ? result.data.updateCustomer : c));
      setShowEditModal(false);
      setEditingCustomer(null);
      resetForm();
    } catch (error) {
      setFormErrors({ general: 'Failed to update customer. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle delete customer
  const handleDeleteCustomer = async () => {
    if (!authToken || !deletingCustomerId) return;

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
            mutation DeleteCustomer($id: ID!) {
              deleteCustomer(id: $id)
            }
          `,
          variables: {
            id: deletingCustomerId,
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error deleting customer:', result.errors);
        return;
      }

      // Remove customer from list
      setCustomers(customers.filter(c => c.id !== deletingCustomerId));
      setShowDeleteConfirm(false);
      setDeletingCustomerId(null);
    } catch (error) {
      console.error('Error deleting customer:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Reset form
  const resetForm = () => {
    setFormData({
      name: '',
      email: '',
      phone: '',
      address: '',
      city: '',
      country: '',
    });
    setFormErrors({});
  };

  // Reset ticket form
  const resetTicketForm = () => {
    setTicketFormData({
      title: '',
      description: '',
      priority: 'MEDIUM',
      customerId: '',
      assignedUserId: '',
    });
    setTicketFormErrors({});
  };

  // Reset contact form
  const resetContactForm = () => {
    setContactFormData({
      name: '',
      title: '',
      email: '',
      phone: '',
      mobile: '',
      isPrimary: false,
      customerId: '',
    });
    setContactFormErrors({});
  };

  // Open ticket modal
  const openTicketModal = (customer: Customer) => {
    setSelectedCustomerForTicket(customer);
    setTicketFormData({
      title: '',
      description: '',
      priority: 'MEDIUM',
      customerId: customer.id,
      assignedUserId: '',
    });
    setShowTicketModal(true);
  };

  // Open contact modal
  const openContactModal = (customer: Customer) => {
    setSelectedCustomerForContact(customer);
    setContactFormData({
      name: '',
      title: '',
      email: '',
      phone: '',
      mobile: '',
      isPrimary: false,
      customerId: customer.id,
    });
    setShowContactModal(true);
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
    if (Object.keys(errors).length > 0 || !authToken) return;

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
              customerId: parseInt(ticketFormData.customerId),
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

      setShowTicketModal(false);
      resetTicketForm();
      setSelectedCustomerForTicket(null);
      // Optionally redirect to tickets page or show success message
      router.push('/dashboard/tickets');
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
    if (Object.keys(errors).length > 0 || !authToken) return;

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
              customerId: parseInt(contactFormData.customerId),
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setContactFormErrors({ general: result.errors[0].message });
        return;
      }

      setShowContactModal(false);
      resetContactForm();
      setSelectedCustomerForContact(null);
      // Optionally redirect to customer detail page or show success message
      router.push(`/dashboard/customers/${contactFormData.customerId}`);
    } catch (error) {
      setContactFormErrors({ general: 'Failed to create contact. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Open edit modal
  const openEditModal = (customer: Customer) => {
    setEditingCustomer(customer);
    setFormData({
      name: customer.name,
      email: customer.email || '',
      phone: customer.phone || '',
      address: customer.address || '',
      city: customer.city || '',
      country: customer.country || '',
    });
    setShowEditModal(true);
  };

  // Filter customers
  const filteredCustomers = customers.filter(customer =>
    customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    customer.phone?.includes(searchTerm)
  );

  if (loading) {
    return (
      <DashboardLayout currentPage="customers">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <div className="mb-8">
              <Skeleton variant="rectangular" height={40} width={200} className="mb-2" />
              <Skeleton variant="text" width={300} />
            </div>
            <div className="mb-6">
              <Skeleton variant="rectangular" height={40} />
            </div>
            <Card padding="md">
              <div className="space-y-4">
                {[1, 2, 3, 4, 5].map((i) => (
                  <div key={i} className="flex items-center gap-4">
                    <Skeleton variant="text" width={150} />
                    <Skeleton variant="text" width={200} />
                    <Skeleton variant="text" width={120} />
                    <Skeleton variant="text" width={100} />
                    <Skeleton variant="text" width={80} />
                  </div>
                ))}
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
          {/* Page header */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-semibold mb-1" style={{ color: 'var(--gray-900)' }}>Customers</h1>
                <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Manage your customer relationships</p>
              </div>
              <Button
                onClick={() => {
                  setShowCreateModal(true);
                  resetForm();
                }}
                variant="primary"
                size="md"
              >
                <svg className="-ml-1 mr-2 h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                Add Customer
              </Button>
            </div>
          </div>

          {/* Search bar */}
          <div className="mb-6">
            <Input
              type="text"
              placeholder="Search customers by name, email, or phone..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          {/* Customers table */}
          <Card padding="none">
            {filteredCustomers.length === 0 ? (
              <div className="text-center py-16 px-6">
                <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.856-1.487M15 10a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <h3 className="text-base font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>No customers</h3>
                <p className="text-sm mb-6" style={{ color: 'var(--gray-500)' }}>
                  {searchTerm ? 'No customers match your search.' : 'Get started by creating your first customer.'}
                </p>
                {!searchTerm && (
                  <Button
                    onClick={() => {
                      setShowCreateModal(true);
                      resetForm();
                    }}
                    variant="primary"
                  >
                    Add Your First Customer
                  </Button>
                )}
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border)', background: 'var(--gray-50)' }}>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Name</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Email</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Phone</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>City</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Created</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredCustomers.map((customer) => (
                      <tr 
                        key={customer.id} 
                        className="transition-colors hover:bg-gray-50"
                        style={{ borderBottom: '1px solid var(--border)' }}
                      >
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium" style={{ color: 'var(--gray-900)' }}>{customer.name}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{customer.email || '-'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{customer.phone || '-'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{customer.city || '-'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{new Date(customer.createdAt).toLocaleDateString()}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center gap-2">
                            <Link
                              href={`/dashboard/customers/${customer.id}`}
                              className="text-sm font-medium transition-colors hover:opacity-80"
                              style={{ color: 'var(--green)' }}
                            >
                              View
                            </Link>
                            <button
                              onClick={() => openEditModal(customer)}
                              className="text-sm font-medium transition-colors hover:opacity-80"
                              style={{ color: 'var(--blue)' }}
                            >
                              Edit
                            </button>
                            <button
                              onClick={() => openTicketModal(customer)}
                              className="text-sm font-medium transition-colors hover:opacity-80"
                              style={{ color: 'var(--purple)' }}
                              title="New Ticket"
                            >
                              Ticket
                            </button>
                            <button
                              onClick={() => openContactModal(customer)}
                              className="text-sm font-medium transition-colors hover:opacity-80"
                              style={{ color: 'var(--purple)' }}
                              title="New Contact"
                            >
                              Contact
                            </button>
                            <button
                              onClick={() => {
                                setDeletingCustomerId(customer.id);
                                setShowDeleteConfirm(true);
                              }}
                              className="text-sm font-medium transition-colors hover:opacity-80"
                              style={{ color: 'var(--red)' }}
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </div>

        {/* Create/Edit Modal */}
        <Modal
          isOpen={showCreateModal || showEditModal}
          onClose={() => {
            setShowCreateModal(false);
            setShowEditModal(false);
            setEditingCustomer(null);
            resetForm();
          }}
          title={showEditModal ? 'Edit Customer' : 'Add New Customer'}
          size="md"
        >
          <form onSubmit={showEditModal ? handleUpdateCustomer : handleCreateCustomer}>
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

              {formErrors.general && (
                <p className="text-sm" style={{ color: 'var(--red)' }}>{formErrors.general}</p>
              )}
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Button
                type="button"
                variant="secondary"
                onClick={() => {
                  setShowCreateModal(false);
                  setShowEditModal(false);
                  setEditingCustomer(null);
                  resetForm();
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
                {showEditModal ? 'Update' : 'Create'}
              </Button>
            </div>
          </form>
        </Modal>

      {/* Delete Confirmation Modal */}
      <Modal
        isOpen={showDeleteConfirm}
        onClose={() => {
          setShowDeleteConfirm(false);
          setDeletingCustomerId(null);
        }}
        title="Delete Customer?"
        size="sm"
      >
        <p className="text-sm mb-6" style={{ color: 'var(--gray-600)' }}>
          Are you sure you want to delete this customer? This action cannot be undone.
        </p>

        <div className="flex justify-end gap-3">
          <Button
            variant="secondary"
            onClick={() => {
              setShowDeleteConfirm(false);
              setDeletingCustomerId(null);
            }}
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
          resetTicketForm();
          setSelectedCustomerForTicket(null);
        }}
        title={selectedCustomerForTicket ? `New Ticket for ${selectedCustomerForTicket.name}` : 'New Ticket'}
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
              rows={3}
              placeholder="Describe the issue..."
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
                resetTicketForm();
                setSelectedCustomerForTicket(null);
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
          resetContactForm();
          setSelectedCustomerForContact(null);
        }}
        title={selectedCustomerForContact ? `New Contact for ${selectedCustomerForContact.name}` : 'New Contact'}
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
                resetContactForm();
                setSelectedCustomerForContact(null);
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


