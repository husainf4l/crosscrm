'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import DashboardLayout from '@/components/DashboardLayout';

interface User {
  id: string;
  name: string;
  email: string;
  companyId: string;
  companyName: string;
}

interface DashboardStats {
  totalCustomers: number;
  totalTickets: number;
  totalContacts: number;
  openTickets: number;
}

const API_ENDPOINT = 'http://192.168.1.164:5196/graphql';

export default function DashboardPage() {
  const router = useRouter();
  const [user, setUser] = useState<User | null>(null);
  const [stats, setStats] = useState<DashboardStats>({
    totalCustomers: 0,
    totalTickets: 0,
    totalContacts: 0,
    openTickets: 0,
  });
  const [loading, setLoading] = useState(true);

  // Check authentication on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    const userStr = localStorage.getItem('currentUser');

    if (!token) {
      router.push('/login');
      return;
    }

    if (userStr) {
      setUser(JSON.parse(userStr));
    }

    fetchDashboardData(token);
  }, [router]);

  const fetchDashboardData = async (token: string) => {
    try {
      setLoading(true);

      // Fetch aggregated data
      const response = await fetch(API_ENDPOINT, {
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
              }
              tickets {
                id
                status
              }
              contacts {
                id
              }
            }
          `,
        })
      });

      const result = await response.json();

      if (result.data) {
        const customers = result.data.customers || [];
        const tickets = result.data.tickets || [];
        const contacts = result.data.contacts || [];

        setStats({
          totalCustomers: customers.length,
          totalTickets: tickets.length,
          totalContacts: contacts.length,
          openTickets: tickets.filter((t: any) => t.status === 'OPEN').length,
        });
      }
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <DashboardLayout currentPage="dashboard">
      <main className="flex-1 overflow-auto">
        <div className="py-6 px-4 sm:px-6 lg:px-8">
          {/* Welcome Section */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900 mb-2">
              Welcome, {user?.name || 'User'}!
            </h1>
            <p className="text-gray-500">
              {user?.companyName && `You're managing ${user.companyName}`}
            </p>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {/* Total Customers */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm font-medium">Total Customers</p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">{stats.totalCustomers}</p>
                </div>
                <div className="text-3xl">👥</div>
              </div>
              <Link href="/dashboard/customers" className="text-blue-600 hover:text-blue-900 text-sm mt-4 inline-block">
                View all →
              </Link>
            </div>

            {/* Total Tickets */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm font-medium">Total Tickets</p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">{stats.totalTickets}</p>
                </div>
                <div className="text-3xl">🎫</div>
              </div>
              <Link href="/dashboard/tickets" className="text-blue-600 hover:text-blue-900 text-sm mt-4 inline-block">
                View all →
              </Link>
            </div>

            {/* Open Tickets */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm font-medium">Open Tickets</p>
                  <p className="text-3xl font-bold text-orange-600 mt-2">{stats.openTickets}</p>
                </div>
                <div className="text-3xl">⚠️</div>
              </div>
              <Link href="/dashboard/tickets" className="text-blue-600 hover:text-blue-900 text-sm mt-4 inline-block">
                View open →
              </Link>
            </div>

            {/* Total Contacts */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm font-medium">Total Contacts</p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">{stats.totalContacts}</p>
                </div>
                <div className="text-3xl">📇</div>
              </div>
              <Link href="/dashboard/contacts" className="text-blue-600 hover:text-blue-900 text-sm mt-4 inline-block">
                View all →
              </Link>
            </div>
          </div>

          {/* Quick Actions */}
          <div className="bg-white rounded-lg shadow p-6 mb-8">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Quick Actions</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <Link
                href="/dashboard/customers"
                className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition"
              >
                <span className="text-2xl">➕</span>
                <div>
                  <p className="font-medium text-gray-900">Manage Customers</p>
                  <p className="text-sm text-gray-500">View and manage</p>
                </div>
              </Link>

              <Link
                href="/dashboard/tickets"
                className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition"
              >
                <span className="text-2xl">🎫</span>
                <div>
                  <p className="font-medium text-gray-900">Manage Tickets</p>
                  <p className="text-sm text-gray-500">Create and track</p>
                </div>
              </Link>

              <Link
                href="/dashboard/contacts"
                className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition"
              >
                <span className="text-2xl">📇</span>
                <div>
                  <p className="font-medium text-gray-900">Manage Contacts</p>
                  <p className="text-sm text-gray-500">Add and organize</p>
                </div>
              </Link>

              <Link
                href="/dashboard/reports"
                className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-blue-500 hover:bg-blue-50 transition"
              >
                <span className="text-2xl">📊</span>
                <div>
                  <p className="font-medium text-gray-900">View Reports</p>
                  <p className="text-sm text-gray-500">Analytics</p>
                </div>
              </Link>
            </div>
          </div>

          {/* Getting Started */}
          <div className="bg-linear-to-r from-blue-50 to-indigo-50 rounded-lg border border-blue-200 p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-2">Getting Started</h3>
            <p className="text-gray-600 mb-4">
              Welcome to your CRM dashboard! Here's what you can do:
            </p>
            <ul className="space-y-2 text-gray-600">
              <li>✅ Organize your customers and keep track of their information</li>
              <li>✅ Create and manage support tickets for issues</li>
              <li>✅ Store and manage contact details for each customer</li>
              <li>✅ View detailed analytics and reports about your business</li>
              <li>✅ Manage your account settings and preferences</li>
            </ul>
            <p className="text-gray-600 mt-4">
              Start by clicking on any of the menu items in the sidebar to get started!
            </p>
          </div>
        </div>
      </main>
    </DashboardLayout>
  );
}
