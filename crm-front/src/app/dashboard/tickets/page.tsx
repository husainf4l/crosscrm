'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import DashboardLayout from '@/components/DashboardLayout';
import { Button, Input, Textarea, Modal, Card, Spinner, Skeleton } from '@/components/ui';

interface Ticket {
  id: string;
  title: string;
  description: string;
  status: 'OPEN' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED';
  priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  customerId: string;
  customerName: string;
  assignedUserId?: number;
  assignedUserName?: string;
  createdAt: string;
  updatedAt?: string;
  resolvedAt?: string;
  resolution?: string;
  tags?: string;
}

interface FormData {
  title: string;
  description: string;
  priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  customerId: string;
  assignedUserId?: string;
}

interface FormErrors {
  title?: string;
  description?: string;
  priority?: string;
  customerId?: string;
  general?: string;
}

interface Customer {
  id: string;
  name: string;
}

interface User {
  id: string;
  name: string;
}

const STATUS_OPTIONS = ['OPEN', 'IN_PROGRESS', 'RESOLVED', 'CLOSED'] as const;
const PRIORITY_OPTIONS = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'] as const;

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

export default function TicketsPage() {
  const router = useRouter();
  const [authToken, setAuthToken] = useState<string | null>(null);
  const [currentUser, setCurrentUser] = useState<any>(null);
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('');
  const [filterPriority, setFilterPriority] = useState<string>('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingTicket, setEditingTicket] = useState<Ticket | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [deletingTicketId, setDeletingTicketId] = useState<string | null>(null);
  const [formData, setFormData] = useState<FormData>({
    title: '',
    description: '',
    priority: 'MEDIUM',
    customerId: '',
    assignedUserId: '',
  });
  const [formErrors, setFormErrors] = useState<FormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Check authentication on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    const userStr = localStorage.getItem('currentUser');
    
    if (!token) {
      router.push('/login');
      return;
    }
    
    setAuthToken(token);
    if (userStr) {
      setCurrentUser(JSON.parse(userStr));
    }
    
    Promise.all([
      fetchTickets(token),
      fetchCustomers(token),
      fetchUsers(token),
    ]);
  }, [router]);

  // Fetch tickets
  const fetchTickets = useCallback(async (token: string) => {
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
          `,
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error fetching tickets:', result.errors);
        if (result.errors[0].message.includes('Invalid') || result.errors[0].message.includes('Unauthorized')) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('currentUser');
          router.push('/login');
        }
        return;
      }

      setTickets(result.data.tickets || []);
    } catch (error) {
      console.error('Error fetching tickets:', error);
    } finally {
      setLoading(false);
    }
  }, [router]);

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
      if (!result.errors) {
        setCustomers(result.data.customers || []);
      }
    } catch (error) {
      console.error('Error fetching customers:', error);
    }
  }, []);

  // Fetch users
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

    if (!formData.title.trim()) {
      errors.title = 'Ticket title is required';
    }

    if (!formData.description.trim()) {
      errors.description = 'Ticket description is required';
    }

    if (!formData.customerId) {
      errors.customerId = 'Customer is required';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  }, [formData]);

  // Handle create ticket
  const handleCreateTicket = async (e: React.FormEvent) => {
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
              title: formData.title,
              description: formData.description,
              priority: formData.priority,
              customerId: parseInt(formData.customerId),
              assignedUserId: formData.assignedUserId ? parseInt(formData.assignedUserId) : null,
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        setFormErrors({ general: result.errors[0].message });
        return;
      }

      setTickets([...tickets, result.data.createTicket]);
      setShowCreateModal(false);
      resetForm();
    } catch (error) {
      setFormErrors({ general: 'Failed to create ticket. Please try again.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  // Handle update ticket (status or assignment)
  const handleUpdateTicketStatus = async (ticket: Ticket, newStatus: string, newAssignedUserId?: number) => {
    if (!authToken) return;

    try {
      const response = await fetch('http://192.168.1.164:5196/graphql', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
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
              }
            }
          `,
          variables: {
            input: {
              id: ticket.id,
              status: newStatus,
              ...(newAssignedUserId !== undefined && { assignedUserId: newAssignedUserId }),
            }
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error updating ticket:', result.errors);
        return;
      }

      setTickets(tickets.map(t => t.id === ticket.id ? result.data.updateTicket : t));
    } catch (error) {
      console.error('Error updating ticket:', error);
    }
  };

  // Handle delete ticket
  const handleDeleteTicket = async () => {
    if (!authToken || !deletingTicketId) return;

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
            mutation DeleteTicket($id: ID!) {
              deleteTicket(id: $id)
            }
          `,
          variables: {
            id: deletingTicketId,
          }
        })
      });

      const result = await response.json();

      if (result.errors) {
        console.error('Error deleting ticket:', result.errors);
        return;
      }

      setTickets(tickets.filter(t => t.id !== deletingTicketId));
      setShowDeleteConfirm(false);
      setDeletingTicketId(null);
    } catch (error) {
      console.error('Error deleting ticket:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Reset form
  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      priority: 'MEDIUM',
      customerId: '',
      assignedUserId: '',
    });
    setFormErrors({});
  };

  // Open edit modal
  const openEditModal = (ticket: Ticket) => {
    setEditingTicket(ticket);
    setFormData({
      title: ticket.title,
      description: ticket.description,
      priority: ticket.priority,
      customerId: ticket.customerId,
      assignedUserId: ticket.assignedUserId?.toString() || '',
    });
    setShowEditModal(true);
  };

  // Filter tickets
  const filteredTickets = tickets.filter(ticket => {
    const matchesSearch = 
      ticket.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesStatus = !filterStatus || ticket.status === filterStatus;
    const matchesPriority = !filterPriority || ticket.priority === filterPriority;

    return matchesSearch && matchesStatus && matchesPriority;
  });

  if (loading) {
    return (
      <DashboardLayout currentPage="tickets">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <div className="mb-8">
              <Skeleton variant="rectangular" height={40} width={200} className="mb-2" />
              <Skeleton variant="text" width={300} />
            </div>
            <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
              <Skeleton variant="rectangular" height={40} />
              <Skeleton variant="rectangular" height={40} />
              <Skeleton variant="rectangular" height={40} />
            </div>
            <Card padding="md">
              <div className="space-y-4">
                {[1, 2, 3, 4, 5].map((i) => (
                  <div key={i} className="flex items-center gap-4">
                    <Skeleton variant="text" width={200} />
                    <Skeleton variant="text" width={150} />
                    <Skeleton variant="circular" width={60} height={24} />
                    <Skeleton variant="circular" width={80} height={24} />
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
    <DashboardLayout currentPage="tickets">
      <main className="py-8" style={{ background: 'var(--gray-50)' }}>
        <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
          {/* Page header */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-3xl font-semibold mb-1" style={{ color: 'var(--gray-900)' }}>Tickets</h1>
                <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Manage and track support tickets</p>
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
                Create Ticket
              </Button>
            </div>
          </div>
          {/* Filters */}
          <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
            <Input
              type="text"
              placeholder="Search by title, customer, or description..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
              style={{
                backgroundColor: 'var(--background)',
                color: 'var(--foreground)',
              }}
            >
              <option value="">All Status</option>
              {STATUS_OPTIONS.map(status => (
                <option key={status} value={status}>{status}</option>
              ))}
            </select>
            <select
              value={filterPriority}
              onChange={(e) => setFilterPriority(e.target.value)}
              className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
              style={{
                backgroundColor: 'var(--background)',
                color: 'var(--foreground)',
              }}
            >
              <option value="">All Priority</option>
              {PRIORITY_OPTIONS.map(priority => (
                <option key={priority} value={priority}>{priority}</option>
              ))}
            </select>
          </div>

          {/* Tickets table */}
          <Card padding="none">
            {filteredTickets.length === 0 ? (
              <div className="text-center py-16 px-6">
                <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--gray-300)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <h3 className="text-base font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>No tickets</h3>
                <p className="text-sm mb-6" style={{ color: 'var(--gray-500)' }}>
                  {searchTerm || filterStatus || filterPriority ? 'No tickets match your filters.' : 'Get started by creating your first ticket.'}
                </p>
                {!searchTerm && !filterStatus && !filterPriority && (
                  <Button
                    onClick={() => {
                      setShowCreateModal(true);
                      resetForm();
                    }}
                    variant="primary"
                  >
                    Create Your First Ticket
                  </Button>
                )}
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border)', background: 'var(--gray-50)' }}>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Title</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Customer</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Priority</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Status</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Assigned To</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Created</th>
                      <th className="px-6 py-4 text-left text-xs font-semibold uppercase tracking-wider" style={{ color: 'var(--gray-600)' }}>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredTickets.map((ticket) => {
                      const priorityColor = PRIORITY_COLORS[ticket.priority];
                      const statusColor = STATUS_COLORS[ticket.status];
                      return (
                        <tr 
                          key={ticket.id} 
                          className="transition-colors hover:bg-gray-50"
                          style={{ borderBottom: '1px solid var(--border)' }}
                        >
                          <td className="px-6 py-4">
                            <div className="text-sm font-medium truncate max-w-xs" style={{ color: 'var(--gray-900)' }}>{ticket.title}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{ticket.customerName}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span 
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                              style={{ 
                                backgroundColor: priorityColor.bg,
                                color: priorityColor.text,
                              }}
                            >
                              {ticket.priority}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <select
                              value={ticket.status}
                              onChange={(e) => handleUpdateTicketStatus(ticket, e.target.value)}
                              className="text-xs font-medium px-2.5 py-1 rounded-full border-0 focus:ring-2 focus:ring-blue-500 cursor-pointer transition-all"
                              style={{
                                backgroundColor: statusColor.bg,
                                color: statusColor.text,
                              }}
                            >
                              {STATUS_OPTIONS.map(status => (
                                <option key={status} value={status}>{status}</option>
                              ))}
                            </select>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <select
                              value={ticket.assignedUserId?.toString() || ''}
                              onChange={(e) => handleUpdateTicketStatus(ticket, ticket.status, e.target.value ? parseInt(e.target.value) : undefined)}
                              className="text-sm px-2 py-1 rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500 cursor-pointer"
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
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm" style={{ color: 'var(--gray-600)' }}>{new Date(ticket.createdAt).toLocaleDateString()}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center gap-2">
                              <button
                                onClick={() => openEditModal(ticket)}
                                className="text-sm font-medium transition-colors hover:opacity-80"
                                style={{ color: 'var(--blue)' }}
                              >
                                Edit
                              </button>
                              <button
                                onClick={() => {
                                  setDeletingTicketId(ticket.id);
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
                      );
                    })}
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
            setEditingTicket(null);
            resetForm();
          }}
          title={showEditModal ? 'Edit Ticket' : 'Create New Ticket'}
          size="md"
        >
          <form onSubmit={showEditModal ? (e) => {
            e.preventDefault();
            // Handle edit ticket update
            if (editingTicket && authToken) {
              handleUpdateTicketStatus(editingTicket, editingTicket.status);
            }
          } : handleCreateTicket}>
            <div className="space-y-4">
              <Input
                label="Title"
                type="text"
                value={formData.title}
                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                placeholder="Ticket title"
                error={formErrors.title}
                required
              />

              <Textarea
                label="Description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                rows={3}
                placeholder="Describe the issue..."
                error={formErrors.description}
                required
              />

              <div>
                <label className="block text-sm font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>Priority</label>
                <select
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: e.target.value as any })}
                  className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                  style={{
                    backgroundColor: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                >
                  {PRIORITY_OPTIONS.map(priority => (
                    <option key={priority} value={priority}>{priority}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>
                  Customer <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.customerId}
                  onChange={(e) => setFormData({ ...formData, customerId: e.target.value })}
                  className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                  style={{
                    backgroundColor: 'var(--background)',
                    color: 'var(--foreground)',
                  }}
                >
                  <option value="">Select a customer</option>
                  {customers.map(customer => (
                    <option key={customer.id} value={customer.id}>{customer.name}</option>
                  ))}
                </select>
                {formErrors.customerId && (
                  <p className="mt-1.5 text-sm" style={{ color: 'var(--red)' }}>{formErrors.customerId}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium mb-1.5" style={{ color: 'var(--gray-900)' }}>Assign To</label>
                <select
                  value={formData.assignedUserId || ''}
                  onChange={(e) => setFormData({ ...formData, assignedUserId: e.target.value })}
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
                  setEditingTicket(null);
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
            setDeletingTicketId(null);
          }}
          title="Delete Ticket?"
          size="sm"
        >
          <p className="text-sm mb-6" style={{ color: 'var(--gray-600)' }}>
            Are you sure you want to delete this ticket? This action cannot be undone.
          </p>

          <div className="flex justify-end gap-3">
            <Button
              variant="secondary"
              onClick={() => {
                setShowDeleteConfirm(false);
                setDeletingTicketId(null);
              }}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={handleDeleteTicket}
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
