'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import DashboardLayout from '@/components/DashboardLayout';

interface Company {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  userCount: number;
}

interface User {
  id: string;
  name: string;
  email: string;
  phone?: string;
  createdAt: string;
  companyId: string;
}

interface FormData {
  name: string;
  description: string;
}

interface FormErrors {
  name?: string;
  description?: string;
}

const API_ENDPOINT = 'http://192.168.1.164:5196/graphql';

export default function CompaniesPage() {
  const router = useRouter();

  const [authToken, setAuthToken] = useState<string | null>(null);
  const [company, setCompany] = useState<Company | null>(null);
  const [teamMembers, setTeamMembers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showRemoveConfirm, setShowRemoveConfirm] = useState(false);
  const [removingUserId, setRemovingUserId] = useState<string | null>(null);
  const [formData, setFormData] = useState<FormData>({
    name: '',
    description: '',
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
    fetchCompanyData(token);
  }, [router]);

  const fetchCompanyData = useCallback(async (token: string) => {
    try {
      setLoading(true);

      // Fetch current user to get company ID
      const userResponse = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
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
          `,
        }),
      });

      const userData = await userResponse.json();
      if (userData.errors) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
        router.push('/login');
        return;
      }

      const companyId = userData.data.me.companyId;

      // Fetch company details
      const companyResponse = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetCompany($id: Int!) {
              company(id: $id) {
                id
                name
                description
                createdAt
                userCount
              }
            }
          `,
          variables: {
            id: parseInt(companyId),
          },
        }),
      });

      const companyData = await companyResponse.json();
      if (companyData.data?.company) {
        setCompany(companyData.data.company);
        setFormData({
          name: companyData.data.company.name,
          description: companyData.data.company.description || '',
        });
      }

      // Fetch team members (all users in company)
      const usersResponse = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
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
          `,
        }),
      });

      const usersData = await usersResponse.json();
      if (usersData.data?.users) {
        // Filter to only users in this company
        const filteredUsers = usersData.data.users.filter(
          (user: User) => user.companyId === companyId
        );
        setTeamMembers(filteredUsers);
      }
    } catch (error) {
      console.error('Error fetching company data:', error);
    } finally {
      setLoading(false);
    }
  }, [router]);

  const validateForm = (): boolean => {
    const errors: FormErrors = {};

    if (!formData.name.trim()) {
      errors.name = 'Company name is required';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleUpdateCompany = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm() || !authToken || !company) return;

    setIsSubmitting(true);

    try {
      const response = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
            mutation UpdateCompany($input: UpdateCompanyDtoInput!) {
              updateCompany(input: $input) {
                id
                name
                description
                createdAt
                userCount
              }
            }
          `,
          variables: {
            input: {
              id: company.id,
              name: formData.name,
              description: formData.description,
            },
          },
        }),
      });

      const data = await response.json();

      if (data.errors) {
        setFormErrors({
          name: data.errors[0]?.message || 'Failed to update company',
        });
        return;
      }

      if (data.data?.updateCompany) {
        setCompany(data.data.updateCompany);
        setShowEditModal(false);
      }
    } catch (error) {
      console.error('Error updating company:', error);
      setFormErrors({
        name: 'Failed to update company. Please try again.',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveUser = async () => {
    if (!authToken || !company || !removingUserId) return;

    setIsSubmitting(true);

    try {
      const response = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${authToken}`,
        },
        body: JSON.stringify({
          query: `
            mutation RemoveUserFromCompany($userId: Int!, $companyId: Int!) {
              removeUserFromCompany(userId: $userId, companyId: $companyId)
            }
          `,
          variables: {
            userId: parseInt(removingUserId),
            companyId: parseInt(company.id),
          },
        }),
      });

      const data = await response.json();

      if (data.errors) {
        console.error('Error removing user:', data.errors);
        setIsSubmitting(false);
        return;
      }

      // Refresh team members
      await fetchCompanyData(authToken);
      setShowRemoveConfirm(false);
      setRemovingUserId(null);
    } catch (error) {
      console.error('Error removing user:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-gray-500">Loading company data...</div>
      </div>
    );
  }

  if (!company) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-red-500">Failed to load company data</div>
      </div>
    );
  }

  return (
    <DashboardLayout currentPage="companies">
      <main className="py-8">
        <div className="px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="flex items-center justify-between mb-8">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Company Settings</h1>
              <p className="text-gray-600 mt-1">Manage company information and team</p>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Company Info Card */}
            <div className="lg:col-span-2">
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-xl font-semibold text-gray-900">Company Information</h2>
              <button
                onClick={() => setShowEditModal(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Edit Settings
              </button>
            </div>

            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-700">Company Name</label>
                <p className="text-lg text-gray-900 mt-1">{company.name}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-700">Description</label>
                <p className="text-gray-600 mt-1">
                  {company.description || 'No description provided'}
                </p>
              </div>

              <div className="grid grid-cols-2 gap-4 pt-4 border-t">
                <div>
                  <label className="text-sm font-medium text-gray-700">Team Members</label>
                  <p className="text-2xl font-bold text-gray-900 mt-1">{company.userCount}</p>
                </div>

                <div>
                  <label className="text-sm font-medium text-gray-700">Created</label>
                  <p className="text-gray-600 mt-1">
                    {new Date(company.createdAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Stats Card */}
        <div className="bg-linear-to-br from-blue-50 to-indigo-50 rounded-lg shadow p-6 border border-blue-100">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">Quick Stats</h3>
          <div className="space-y-4">
            <div>
              <div className="text-3xl font-bold text-blue-600">{company.userCount}</div>
              <div className="text-sm text-gray-600">Team Members</div>
            </div>
            <div className="pt-4 border-t border-blue-200">
              <div className="text-sm text-gray-600">
                Company ID: <span className="font-mono text-gray-900">{company.id}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Team Members Section */}
      <div className="mt-8">
        <h2 className="text-xl font-semibold text-gray-900 mb-6">Team Members</h2>

        {teamMembers.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-lg shadow">
            <p className="text-gray-500">No team members found</p>
          </div>
        ) : (
          <div className="bg-white rounded-lg shadow overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-200">
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                      Email
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                      Phone
                    </th>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-gray-900">
                      Joined
                    </th>
                    <th className="px-6 py-3 text-center text-sm font-semibold text-gray-900">
                      Action
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {teamMembers.map((member) => (
                    <tr key={member.id} className="hover:bg-gray-50 transition-colors">
                      <td className="px-6 py-4 text-sm text-gray-900 font-medium">
                        {member.name}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-600">{member.email}</td>
                      <td className="px-6 py-4 text-sm text-gray-600">
                        {member.phone || '-'}
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-600">
                        {new Date(member.createdAt).toLocaleDateString()}
                      </td>
                      <td className="px-6 py-4 text-center">
                        <button
                          onClick={() => {
                            setRemovingUserId(member.id);
                            setShowRemoveConfirm(true);
                          }}
                          className="text-red-600 hover:text-red-700 text-sm font-medium transition-colors"
                        >
                          Remove
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>

      {/* Edit Company Modal */}
      {showEditModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-lg font-semibold text-gray-900">Edit Company Settings</h3>
            </div>

            <form onSubmit={handleUpdateCompany} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Company Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) =>
                    setFormData({ ...formData, name: e.target.value })
                  }
                  placeholder="Enter company name"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                {formErrors.name && (
                  <p className="text-red-600 text-sm mt-1">{formErrors.name}</p>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) =>
                    setFormData({ ...formData, description: e.target.value })
                  }
                  placeholder="Enter company description"
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="flex gap-3 pt-4 border-t">
                <button
                  type="button"
                  onClick={() => {
                    setShowEditModal(false);
                    setFormErrors({});
                  }}
                  className="flex-1 px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  {isSubmitting ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Remove User Confirmation Modal */}
      {showRemoveConfirm && removingUserId && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-lg max-w-md w-full">
            <div className="p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Remove Team Member
              </h3>
              <p className="text-gray-600 mb-6">
                Are you sure you want to remove this team member from the company?
              </p>

              <div className="flex gap-3">
                <button
                  onClick={() => {
                    setShowRemoveConfirm(false);
                    setRemovingUserId(null);
                  }}
                  className="flex-1 px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={handleRemoveUser}
                  disabled={isSubmitting}
                  className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  {isSubmitting ? 'Removing...' : 'Remove'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
        </div>
      </main>
    </DashboardLayout>
  );
}
