'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import DashboardLayout from '@/components/DashboardLayout';
import { Button, Input, Modal, Card, Skeleton } from '@/components/ui';

interface Contact {
  id: string;
  name: string;
  title?: string;
  email?: string;
  phone?: string;
  mobile?: string;
  isPrimary: boolean;
  customerId: string;
  createdAt: string;
  updatedAt?: string;
}

interface Customer {
  id: string;
  name: string;
}

interface FormData {
  name: string;
  title: string;
  email: string;
  phone: string;
  mobile: string;
  isPrimary: boolean;
}

interface FormErrors {
  name?: string;
  email?: string;
  phone?: string;
  customerId?: string;
  general?: string;
}

export default function ContactsPage() {
  const router = useRouter();

  const [authToken, setAuthToken] = useState<string | null>(null);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [selectedCustomerId, setSelectedCustomerId] = useState('');
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingContact, setEditingContact] = useState<Contact | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingContactId, setDeletingContactId] = useState<string | null>(null);
  const [formData, setFormData] = useState<FormData>({
    name: '',
    title: '',
    email: '',
    phone: '',
    mobile: '',
    isPrimary: false,
  });
  const [formErrors, setFormErrors] = useState<FormErrors>({});
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
  }, [router]);

  // Fetch contacts when customer is selected
  useEffect(() => {
    if (selectedCustomerId && authToken) {
      fetchContacts(authToken, selectedCustomerId);
    } else {
      setContacts([]);
      setLoading(false);
    }
  }, [selectedCustomerId, authToken]);

  // Fetch customers
  const fetchCustomers = useCallback(async (token: string) => {
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
              customers {
                id
                name
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
      
      // If no customer selected but we have customers, select the first one
      if (!selectedCustomerId && result.data.customers?.length > 0) {
        setSelectedCustomerId(result.data.customers[0].id);
      }
    } catch (error) {
      console.error('Error fetching customers:', error);
    }
  }, [selectedCustomerId, router]);

  // Fetch contacts for selected customer
  const fetchContacts = useCallback(async (token: string, customerId: string) => {
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
            query GetContactsByCustomer($customerId: Int!) {
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
          `,
          variables: {
            customerId: parseInt(customerId),
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error fetching contacts:', result.errors);
        return;
      }

      setContacts(result.data.contactsByCustomer || []);
    } catch (error) {
      console.error('Error fetching contacts:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  // Validate form
  const validateForm = useCallback((): boolean => {
    const errors: FormErrors = {};

    if (!formData.name.trim()) {
      errors.name = 'Contact name is required';
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Please enter a valid email address';
    }

    if (!selectedCustomerId) {
      errors.customerId = 'Please select a customer';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData, selectedCustomerId]);

  // Handle create contact
  const handleCreateContact = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken || !selectedCustomerId) return;

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
              name: formData.name,
              title: formData.title || null,
              email: formData.email || null,
              phone: formData.phone || null,
              mobile: formData.mobile || null,
              isPrimary: formData.isPrimary,
              customerId: selectedCustomerId,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setFormErrors({ general: result.errors[0].message });
        return;
      }

      setContacts([...contacts, result.data.createContact]);
      setShowCreateModal(false);
      resetForm();
    } catch (error) {
      setFormErrors({ general: 'Failed to create contact. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle update contact
  const handleUpdateContact = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken || !editingContact) return;

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
          `,
          variables: {
            input: {
              id: editingContact.id,
              name: formData.name,
              title: formData.title || null,
              email: formData.email || null,
              phone: formData.phone || null,
              mobile: formData.mobile || null,
              isPrimary: formData.isPrimary,
              customerId: selectedCustomerId,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setFormErrors({ general: result.errors[0].message });
        return;
      }

      setContacts(contacts.map(c => c.id === editingContact.id ? result.data.updateContact : c));
      setShowEditModal(false);
      setEditingContact(null);
      resetForm();
    } catch (error) {
      setFormErrors({ general: 'Failed to update contact. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle delete contact
  const handleDeleteContact = async () => {
    if (!authToken || !deletingContactId) return;

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
            mutation DeleteContact($id: ID!) {
              deleteContact(id: $id)
            }
          `,
          variables: {
            id: deletingContactId,
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error deleting contact:', result.errors);
        return;
      }

      setContacts(contacts.filter(c => c.id !== deletingContactId));
      setShowDeleteConfirm(false);
      setDeletingContactId(null);
    } catch (error) {
      console.error('Error deleting contact:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Reset form
  const resetForm = () => {
    setFormData({
      name: '',
      title: '',
      email: '',
      phone: '',
      mobile: '',
      isPrimary: false,
    });
    setFormErrors({});
  };

  // Open edit modal
  const openEditModal = (contact: Contact) => {
    setEditingContact(contact);
    setFormData({
      name: contact.name,
      title: contact.title || '',
      email: contact.email || '',
      phone: contact.phone || '',
      mobile: contact.mobile || '',
      isPrimary: contact.isPrimary,
    });
    setShowEditModal(true);
  };

  // Filter contacts
  const filteredContacts = contacts.filter(contact =>
    contact.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    contact.email?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    contact.phone?.includes(searchTerm) ||
    contact.mobile?.includes(searchTerm)
  );

  // Get selected customer name
  const selectedCustomer = customers.find(c => c.id === selectedCustomerId);

  if (loading && selectedCustomerId) {
    return (
      <DashboardLayout currentPage="contacts">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <div className="mb-8">
              <Skeleton variant="rectangular" height={40} width={200} className="mb-2" />
            </div>
            <div className="mb-6">
              <Skeleton variant="rectangular" height={60} />
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[1, 2, 3].map((i) => (
                <Card key={i} padding="md">
                  <Skeleton variant="text" width={150} className="mb-4" />
                  <Skeleton variant="text" width={100} className="mb-2" />
                  <Skeleton variant="text" width={120} />
                </Card>
              ))}
            </div>
          </div>
        </main>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout currentPage="contacts">
      <main className="py-8" style={{ background: 'var(--gray-50)' }}>
        <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
          {/* Page header */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-semibold mb-1" style={{ color: 'var(--gray-900)' }}>Contacts</h1>
                <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Manage customer contacts</p>
              </div>
              <Button
                onClick={() => {
                  setShowCreateModal(true);
                  resetForm();
                }}
                disabled={!selectedCustomerId}
                variant="primary"
                size="md"
              >
                <svg className="-ml-1 mr-2 h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                Add Contact
              </Button>
            </div>
          </div>

          {/* Customer selector */}
          <Card padding="md" className="mb-6">
            <label className="block text-sm font-medium mb-2" style={{ color: 'var(--gray-900)' }}>Select Customer</label>
            <select
              value={selectedCustomerId}
              onChange={(e) => setSelectedCustomerId(e.target.value)}
              className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
              style={{
                backgroundColor: 'var(--background)',
                color: 'var(--foreground)',
              }}
            >
              <option value="">-- Select a customer --</option>
              {customers.map(customer => (
                <option key={customer.id} value={customer.id}>{customer.name}</option>
              ))}
            </select>
          </Card>

          {selectedCustomerId ? (
            <>
              {/* Customer info */}
              <Card padding="md" className="mb-6" style={{ backgroundColor: 'rgba(0, 122, 255, 0.05)', borderColor: 'var(--blue)' }}>
                <p className="text-sm" style={{ color: 'var(--blue)' }}>
                  Viewing contacts for: <span className="font-semibold">{selectedCustomer?.name}</span>
                </p>
              </Card>

              {/* Search bar */}
              {contacts.length > 0 && (
                <div className="mb-6">
                  <Input
                    type="text"
                    placeholder="Search contacts by name, email, or phone..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                </div>
              )}

              {/* Contacts grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredContacts.length === 0 ? (
                  <div className="col-span-full">
                    <Card padding="lg">
                      <div className="text-center py-8">
                        <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.856-1.487M15 10a3 3 0 11-6 0 3 3 0 016 0z" />
                        </svg>
                        <h3 className="text-base font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>No contacts</h3>
                        <p className="text-sm mb-6" style={{ color: 'var(--gray-500)' }}>
                          {searchTerm ? 'No contacts match your search.' : 'Get started by adding your first contact.'}
                        </p>
                        {!searchTerm && (
                          <Button
                            onClick={() => {
                              setShowCreateModal(true);
                              resetForm();
                            }}
                            variant="primary"
                          >
                            Add Your First Contact
                          </Button>
                        )}
                      </div>
                    </Card>
                  </div>
                ) : (
                  filteredContacts.map((contact) => (
                    <Card key={contact.id} padding="md" hover>
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <h3 className="text-lg font-semibold" style={{ color: 'var(--gray-900)' }}>{contact.name}</h3>
                            {contact.isPrimary && (
                              <span 
                                className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
                                style={{ 
                                  backgroundColor: 'rgba(52, 199, 89, 0.1)',
                                  color: '#34C759',
                                }}
                              >
                                Primary
                              </span>
                            )}
                          </div>
                          {contact.title && <p className="text-sm" style={{ color: 'var(--gray-500)' }}>{contact.title}</p>}
                        </div>
                      </div>

                      <div className="space-y-2 mb-4">
                        {contact.email && (
                          <div className="text-sm">
                            <span style={{ color: 'var(--gray-500)' }}>Email:</span>
                            <a href={`mailto:${contact.email}`} className="ml-2 transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>{contact.email}</a>
                          </div>
                        )}
                        {contact.phone && (
                          <div className="text-sm">
                            <span style={{ color: 'var(--gray-500)' }}>Phone:</span>
                            <a href={`tel:${contact.phone}`} className="ml-2 transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>{contact.phone}</a>
                          </div>
                        )}
                        {contact.mobile && (
                          <div className="text-sm">
                            <span style={{ color: 'var(--gray-500)' }}>Mobile:</span>
                            <a href={`tel:${contact.mobile}`} className="ml-2 transition-colors hover:opacity-80" style={{ color: 'var(--blue)' }}>{contact.mobile}</a>
                          </div>
                        )}
                      </div>

                      <div className="flex gap-2 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                        <Button
                          onClick={() => openEditModal(contact)}
                          variant="secondary"
                          size="sm"
                          className="flex-1"
                        >
                          Edit
                        </Button>
                        <Button
                          onClick={() => {
                            setDeletingContactId(contact.id);
                            setShowDeleteConfirm(true);
                          }}
                          variant="danger"
                          size="sm"
                          className="flex-1"
                        >
                          Delete
                        </Button>
                      </div>
                    </Card>
                  ))
                )}
              </div>
            </>
          ) : (
            <Card padding="lg">
              <div className="text-center py-8">
                <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.856-1.487M15 10a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <h3 className="text-base font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>Select a customer</h3>
                <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Choose a customer from the dropdown above to view and manage their contacts.</p>
              </div>
            </Card>
          )}
        </div>

        {/* Create/Edit Modal */}
        <Modal
          isOpen={showCreateModal || showEditModal}
          onClose={() => {
            setShowCreateModal(false);
            setShowEditModal(false);
            setEditingContact(null);
            resetForm();
          }}
          title={showEditModal ? 'Edit Contact' : 'Add New Contact'}
          size="md"
        >
          <form onSubmit={showEditModal ? handleUpdateContact : handleCreateContact}>
            <div className="space-y-4">
              <Input
                label="Name"
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Contact name"
                error={formErrors.name}
                required
              />

              <Input
                label="Title"
                type="text"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                placeholder="e.g., CEO, Manager, Contact Person"
              />

              <Input
                label="Email"
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                placeholder="contact@example.com"
                error={formErrors.email}
              />

              <Input
                label="Phone"
                type="tel"
                value={formData.phone}
                onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                placeholder="Office phone"
              />

              <Input
                label="Mobile"
                type="tel"
                value={formData.mobile}
                onChange={(e) => setFormData({ ...formData, mobile: e.target.value })}
                placeholder="Mobile number"
              />

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="isPrimary"
                  checked={formData.isPrimary}
                  onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                  className="h-4 w-4 rounded border-gray-300 focus:ring-2 focus:ring-blue-500"
                  style={{ accentColor: 'var(--blue)' }}
                />
                <label htmlFor="isPrimary" className="ml-2 block text-sm" style={{ color: 'var(--gray-700)' }}>
                  Mark as primary contact
                </label>
              </div>

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
                  setEditingContact(null);
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
          setDeletingContactId(null);
        }}
        title="Delete Contact?"
        size="sm"
      >
        <p className="text-sm mb-6" style={{ color: 'var(--gray-600)' }}>
          Are you sure you want to delete this contact? This action cannot be undone.
        </p>

        <div className="flex justify-end gap-3">
          <Button
            variant="secondary"
            onClick={() => {
              setShowDeleteConfirm(false);
              setDeletingContactId(null);
            }}
          >
            Cancel
          </Button>
          <Button
            variant="danger"
            onClick={handleDeleteContact}
            isLoading={isSubmitting}
            disabled={isSubmitting}
          >
            Delete
          </Button>
        </div>
      </Modal>
    </main>
    </DashboardLayout>
  );
}
