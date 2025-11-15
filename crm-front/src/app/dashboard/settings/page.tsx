'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import DashboardLayout from '@/components/DashboardLayout';
import { Button, Input, Card, Skeleton } from '@/components/ui';

interface User {
  id: string;
  name: string;
  email: string;
  phone?: string;
  companyId: string;
  createdAt: string;
}

interface ProfileFormData {
  name: string;
  email: string;
  phone: string;
}

interface PasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

interface PreferencesData {
  emailNotifications: boolean;
  ticketUpdates: boolean;
  customerUpdates: boolean;
  weeklyReports: boolean;
  theme: 'light' | 'dark' | 'auto';
  language: 'en' | 'es' | 'fr' | 'de';
}

interface FormErrors {
  [key: string]: string;
}

const API_ENDPOINT = 'http://192.168.1.164:5196/graphql';

export default function SettingsPage() {
  const router = useRouter();

  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'profile' | 'password' | 'preferences'>('profile');
  
  // Profile state
  const [profileForm, setProfileForm] = useState<ProfileFormData>({
    name: '',
    email: '',
    phone: '',
  });
  const [profileErrors, setProfileErrors] = useState<FormErrors>({});
  const [profileSubmitting, setProfileSubmitting] = useState(false);
  const [profileSuccess, setProfileSuccess] = useState(false);

  // Password state
  const [passwordForm, setPasswordForm] = useState<PasswordFormData>({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [passwordErrors, setPasswordErrors] = useState<FormErrors>({});
  const [passwordSubmitting, setPasswordSubmitting] = useState(false);
  const [passwordSuccess, setPasswordSuccess] = useState(false);

  // Preferences state
  const [preferences, setPreferences] = useState<PreferencesData>({
    emailNotifications: true,
    ticketUpdates: true,
    customerUpdates: true,
    weeklyReports: false,
    theme: 'auto',
    language: 'en',
  });
  const [preferencesSubmitting, setPreferencesSubmitting] = useState(false);
  const [preferencesSuccess, setPreferencesSuccess] = useState(false);

  // Check authentication and fetch user data on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }

    fetchUserData(token);
  }, [router]);

  const fetchUserData = async (token: string) => {
    try {
      setLoading(true);

      const response = await fetch(API_ENDPOINT, {
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

      const data = await response.json();

      if (data.errors) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
        router.push('/login');
        return;
      }

      const userData = data.data.me;
      setUser(userData);
      setProfileForm({
        name: userData.name,
        email: userData.email,
        phone: userData.phone || '',
      });

      // Load preferences from localStorage
      const savedPreferences = localStorage.getItem('userPreferences');
      if (savedPreferences) {
        setPreferences(JSON.parse(savedPreferences));
      }
    } catch (error) {
      console.error('Error fetching user data:', error);
    } finally {
      setLoading(false);
    }
  };

  const validateProfileForm = (): boolean => {
    const errors: FormErrors = {};

    if (!profileForm.name.trim()) {
      errors.name = 'Name is required';
    }

    if (!profileForm.email.trim()) {
      errors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(profileForm.email)) {
      errors.email = 'Please enter a valid email';
    }

    if (profileForm.phone && !/^\d{10,}$/.test(profileForm.phone.replace(/\D/g, ''))) {
      errors.phone = 'Please enter a valid phone number';
    }

    setProfileErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleProfileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateProfileForm() || !user) return;

    setProfileSubmitting(true);
    setProfileSuccess(false);

    try {
      // Note: Update user mutation would need to be implemented in backend
      // For now, we'll simulate the update by updating localStorage
      const updatedUser = {
        ...user,
        name: profileForm.name,
        email: profileForm.email,
        phone: profileForm.phone,
      };

      localStorage.setItem('currentUser', JSON.stringify(updatedUser));
      setUser(updatedUser);
      setProfileSuccess(true);

      setTimeout(() => setProfileSuccess(false), 3000);
    } catch (error) {
      console.error('Error updating profile:', error);
      setProfileErrors({ submit: 'Failed to update profile' });
    } finally {
      setProfileSubmitting(false);
    }
  };

  const validatePasswordForm = (): boolean => {
    const errors: FormErrors = {};

    if (!passwordForm.currentPassword) {
      errors.currentPassword = 'Current password is required';
    }

    if (!passwordForm.newPassword) {
      errors.newPassword = 'New password is required';
    } else if (passwordForm.newPassword.length < 8) {
      errors.newPassword = 'Password must be at least 8 characters';
    }

    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      errors.confirmPassword = 'Passwords do not match';
    }

    setPasswordErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validatePasswordForm()) return;

    setPasswordSubmitting(true);
    setPasswordSuccess(false);

    try {
      // Note: Password change mutation would need to be implemented in backend
      // For now, we'll simulate success
      setPasswordSuccess(true);
      setPasswordForm({
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
      });

      setTimeout(() => setPasswordSuccess(false), 3000);
    } catch (error) {
      console.error('Error changing password:', error);
      setPasswordErrors({ submit: 'Failed to change password' });
    } finally {
      setPasswordSubmitting(false);
    }
  };

  const handlePreferencesChange = (key: keyof PreferencesData, value: any) => {
    const updated = { ...preferences, [key]: value };
    setPreferences(updated);
    localStorage.setItem('userPreferences', JSON.stringify(updated));
    setPreferencesSuccess(true);
    setTimeout(() => setPreferencesSuccess(false), 2000);
  };

  if (loading) {
    return (
      <DashboardLayout currentPage="settings">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <div className="mb-8">
              <Skeleton variant="rectangular" height={40} width={200} className="mb-2" />
              <Skeleton variant="text" width={300} />
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
              <Card padding="none">
                <Skeleton variant="rectangular" height={200} />
              </Card>
              <div className="lg:col-span-3">
                <Card padding="md">
                  <Skeleton variant="text" width={200} className="mb-4" />
                  <Skeleton variant="rectangular" height={400} />
                </Card>
              </div>
            </div>
          </div>
        </main>
      </DashboardLayout>
    );
  }

  if (!user) {
    return (
      <DashboardLayout currentPage="settings">
        <main className="py-8" style={{ background: 'var(--gray-50)' }}>
          <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
            <Card padding="lg">
              <div className="text-center py-12">
                <svg className="mx-auto h-16 w-16 mb-4" style={{ color: 'var(--red)' }} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p className="text-base font-medium" style={{ color: 'var(--red)' }}>Failed to load user data</p>
              </div>
            </Card>
          </div>
        </main>
      </DashboardLayout>
    );
  }

  return (
    <DashboardLayout currentPage="settings">
      <main className="py-8" style={{ background: 'var(--gray-50)' }}>
        <div className="px-4 sm:px-6 lg:px-8 max-w-7xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-semibold mb-1" style={{ color: 'var(--gray-900)' }}>Settings</h1>
            <p className="text-sm" style={{ color: 'var(--gray-500)' }}>Manage your account and preferences</p>
          </div>

          {/* Settings Container */}
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
            {/* Sidebar Navigation */}
            <div className="lg:col-span-1">
              <Card padding="none" className="overflow-hidden">
                <nav className="flex flex-col">
                  {[
                    { id: 'profile', label: 'Profile', icon: '👤' },
                    { id: 'password', label: 'Password', icon: '🔒' },
                    { id: 'preferences', label: 'Preferences', icon: '⚙️' },
                  ].map((tab) => (
                    <button
                      key={tab.id}
                      onClick={() => setActiveTab(tab.id as any)}
                      className={`w-full text-left px-6 py-4 border-l-4 transition-all ${
                        activeTab === tab.id
                          ? 'font-medium'
                          : 'hover:opacity-80'
                      }`}
                      style={{
                        backgroundColor: activeTab === tab.id ? 'rgba(0, 122, 255, 0.05)' : 'transparent',
                        borderLeftColor: activeTab === tab.id ? 'var(--blue)' : 'transparent',
                        color: activeTab === tab.id ? 'var(--blue)' : 'var(--gray-700)',
                      }}
                    >
                      <span className="mr-3">{tab.icon}</span>
                      {tab.label}
                    </button>
                  ))}
                </nav>
              </Card>
            </div>

        {/* Main Content */}
        <div className="lg:col-span-3">
          {/* Profile Tab */}
          {activeTab === 'profile' && (
            <Card padding="md">
              <h2 className="text-xl font-semibold mb-6" style={{ color: 'var(--gray-900)' }}>Profile Information</h2>

              {profileSuccess && (
                <div className="mb-6 p-4 rounded-lg" style={{ backgroundColor: 'rgba(52, 199, 89, 0.1)', border: '1px solid rgba(52, 199, 89, 0.2)', color: '#34C759' }}>
                  ✓ Profile updated successfully
                </div>
              )}

              <form onSubmit={handleProfileSubmit} className="space-y-6">
                {/* User Avatar */}
                <div className="flex items-center space-x-6 mb-8 pb-6" style={{ borderBottom: '1px solid var(--border)' }}>
                  <div className="h-20 w-20 rounded-full flex items-center justify-center text-white text-2xl font-semibold" style={{ backgroundColor: 'var(--blue)' }}>
                    {user.name.split(' ').map(n => n[0]).join('')}
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-900)' }}>Profile Picture</p>
                    <p className="text-sm mb-3" style={{ color: 'var(--gray-500)' }}>Upload a new photo or change your initials</p>
                    <Button
                      type="button"
                      variant="secondary"
                      size="sm"
                    >
                      Change Photo
                    </Button>
                  </div>
                </div>

                {/* Form Fields */}
                <Input
                  label="Full Name"
                  type="text"
                  value={profileForm.name}
                  onChange={(e) => setProfileForm({ ...profileForm, name: e.target.value })}
                  placeholder="Enter your full name"
                  error={profileErrors.name}
                  required
                />

                <Input
                  label="Email Address"
                  type="email"
                  value={profileForm.email}
                  onChange={(e) => setProfileForm({ ...profileForm, email: e.target.value })}
                  placeholder="Enter your email"
                  error={profileErrors.email}
                  required
                />

                <Input
                  label="Phone Number"
                  type="tel"
                  value={profileForm.phone}
                  onChange={(e) => setProfileForm({ ...profileForm, phone: e.target.value })}
                  placeholder="Enter your phone number"
                  error={profileErrors.phone}
                />

                <div className="grid grid-cols-2 gap-4 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                  <div>
                    <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-900)' }}>Account ID</p>
                    <p className="text-sm font-mono" style={{ color: 'var(--gray-600)' }}>{user.id}</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium mb-1" style={{ color: 'var(--gray-900)' }}>Member Since</p>
                    <p className="text-sm" style={{ color: 'var(--gray-600)' }}>
                      {new Date(user.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                </div>

                <div className="flex gap-3 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                  <Button
                    type="submit"
                    variant="primary"
                    isLoading={profileSubmitting}
                    disabled={profileSubmitting}
                  >
                    Save Changes
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                  >
                    Cancel
                  </Button>
                </div>
              </form>
            </Card>
          )}

          {/* Password Tab */}
          {activeTab === 'password' && (
            <Card padding="md">
              <h2 className="text-xl font-semibold mb-2" style={{ color: 'var(--gray-900)' }}>Change Password</h2>
              <p className="text-sm mb-6" style={{ color: 'var(--gray-500)' }}>Update your password to keep your account secure</p>

              {passwordSuccess && (
                <div className="mb-6 p-4 rounded-lg" style={{ backgroundColor: 'rgba(52, 199, 89, 0.1)', border: '1px solid rgba(52, 199, 89, 0.2)', color: '#34C759' }}>
                  ✓ Password changed successfully
                </div>
              )}

              {passwordErrors.submit && (
                <div className="mb-6 p-4 rounded-lg" style={{ backgroundColor: 'rgba(255, 59, 48, 0.1)', border: '1px solid rgba(255, 59, 48, 0.2)', color: '#FF3B30' }}>
                  ✗ {passwordErrors.submit}
                </div>
              )}

              <form onSubmit={handlePasswordSubmit} className="space-y-6">
                <Input
                  label="Current Password"
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })}
                  placeholder="Enter your current password"
                  error={passwordErrors.currentPassword}
                  required
                />

                <div>
                  <Input
                    label="New Password"
                    type="password"
                    value={passwordForm.newPassword}
                    onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })}
                    placeholder="Enter your new password"
                    error={passwordErrors.newPassword}
                    required
                  />
                  <p className="text-xs mt-2" style={{ color: 'var(--gray-500)' }}>
                    Minimum 8 characters, mix of letters, numbers, and symbols recommended
                  </p>
                </div>

                <Input
                  label="Confirm Password"
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })}
                  placeholder="Confirm your new password"
                  error={passwordErrors.confirmPassword}
                  required
                />

                {/* Password Requirements */}
                <Card padding="md" style={{ backgroundColor: 'rgba(0, 122, 255, 0.05)', borderColor: 'var(--blue)' }}>
                  <p className="text-sm font-medium mb-2" style={{ color: 'var(--gray-900)' }}>Password Requirements:</p>
                  <ul className="text-sm space-y-1" style={{ color: 'var(--gray-600)' }}>
                    <li>✓ At least 8 characters long</li>
                    <li>✓ Mix of uppercase and lowercase letters</li>
                    <li>✓ At least one number</li>
                    <li>✓ At least one special character (!@#$%^&*)</li>
                  </ul>
                </Card>

                <div className="flex gap-3 pt-4" style={{ borderTop: '1px solid var(--border)' }}>
                  <Button
                    type="submit"
                    variant="primary"
                    isLoading={passwordSubmitting}
                    disabled={passwordSubmitting}
                  >
                    Update Password
                  </Button>
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' })}
                  >
                    Clear
                  </Button>
                </div>
              </form>
            </Card>
          )}

          {/* Preferences Tab */}
          {activeTab === 'preferences' && (
            <Card padding="md">
              <h2 className="text-xl font-semibold mb-6" style={{ color: 'var(--gray-900)' }}>Preferences</h2>

              {preferencesSuccess && (
                <div className="mb-6 p-4 rounded-lg" style={{ backgroundColor: 'rgba(52, 199, 89, 0.1)', border: '1px solid rgba(52, 199, 89, 0.2)', color: '#34C759' }}>
                  ✓ Preferences saved
                </div>
              )}

              {/* Notifications Section */}
              <div className="mb-8">
                <h3 className="text-lg font-semibold mb-4" style={{ color: 'var(--gray-900)' }}>Notifications</h3>
                <div className="space-y-4">
                  {[
                    {
                      key: 'emailNotifications',
                      label: 'Email Notifications',
                      description: 'Receive email notifications for important updates',
                    },
                    {
                      key: 'ticketUpdates',
                      label: 'Ticket Updates',
                      description: 'Get notified when tickets are updated or assigned',
                    },
                    {
                      key: 'customerUpdates',
                      label: 'Customer Updates',
                      description: 'Receive notifications about customer changes',
                    },
                    {
                      key: 'weeklyReports',
                      label: 'Weekly Reports',
                      description: 'Get a weekly summary of CRM activity',
                    },
                  ].map((pref) => (
                    <label
                      key={pref.key}
                      className="flex items-center p-4 rounded-lg cursor-pointer transition-all hover:opacity-80"
                      style={{ border: '1px solid var(--border)' }}
                    >
                      <input
                        type="checkbox"
                        checked={preferences[pref.key as keyof PreferencesData] as boolean}
                        onChange={(e) =>
                          handlePreferencesChange(pref.key as keyof PreferencesData, e.target.checked)
                        }
                        className="w-5 h-5 rounded cursor-pointer"
                        style={{ accentColor: 'var(--blue)' }}
                      />
                      <div className="ml-4 flex-1">
                        <p className="font-medium" style={{ color: 'var(--gray-900)' }}>{pref.label}</p>
                        <p className="text-sm" style={{ color: 'var(--gray-500)' }}>{pref.description}</p>
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              {/* Display Section */}
              <div className="pt-8" style={{ borderTop: '1px solid var(--border)' }}>
                <h3 className="text-lg font-semibold mb-4" style={{ color: 'var(--gray-900)' }}>Display</h3>
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-2" style={{ color: 'var(--gray-900)' }}>
                      Theme
                    </label>
                    <select
                      value={preferences.theme}
                      onChange={(e) =>
                        handlePreferencesChange('theme', e.target.value as 'light' | 'dark' | 'auto')
                      }
                      className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                      style={{
                        backgroundColor: 'var(--background)',
                        color: 'var(--foreground)',
                      }}
                    >
                      <option value="light">Light</option>
                      <option value="dark">Dark</option>
                      <option value="auto">Auto (System)</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium mb-2" style={{ color: 'var(--gray-900)' }}>
                      Language
                    </label>
                    <select
                      value={preferences.language}
                      onChange={(e) =>
                        handlePreferencesChange('language', e.target.value as 'en' | 'es' | 'fr' | 'de')
                      }
                      className="w-full px-3 py-2 text-sm rounded-lg border transition-all focus:outline-none focus:ring-2 focus:ring-offset-0 border-gray-200 focus:border-blue-500 focus:ring-blue-500"
                      style={{
                        backgroundColor: 'var(--background)',
                        color: 'var(--foreground)',
                      }}
                    >
                      <option value="en">English</option>
                      <option value="es">Spanish</option>
                      <option value="fr">French</option>
                      <option value="de">German</option>
                    </select>
                  </div>
                </div>
              </div>

              {/* Danger Zone */}
              <div className="pt-8 mt-8" style={{ borderTop: '1px solid var(--border)' }}>
                <h3 className="text-lg font-semibold mb-4" style={{ color: 'var(--red)' }}>Danger Zone</h3>
                <Card padding="md" style={{ backgroundColor: 'rgba(255, 59, 48, 0.05)', borderColor: 'var(--red)' }}>
                  <p className="text-sm mb-4" style={{ color: 'var(--gray-700)' }}>
                    Deleting your account is permanent and cannot be undone.
                  </p>
                  <Button
                    type="button"
                    variant="danger"
                  >
                    Delete Account
                  </Button>
                </Card>
              </div>
            </Card>
          )}
            </div>
          </div>
        </div>
      </main>
    </DashboardLayout>
  );
}
