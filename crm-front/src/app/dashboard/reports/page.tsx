'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import DashboardLayout from '@/components/DashboardLayout';

interface Customer {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  city?: string;
  createdAt: string;
}

interface Ticket {
  id: string;
  title: string;
  status: 'OPEN' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED';
  priority: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
  customerId: string;
  createdAt: string;
  updatedAt?: string;
  resolvedAt?: string;
}

interface AnalyticsData {
  totalCustomers: number;
  newCustomersThisMonth: number;
  totalTickets: number;
  openTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  inProgressTickets: number;
  criticalTickets: number;
  highPriorityTickets: number;
  averageResolutionTime: number;
  customersByMonth: Array<{ month: string; count: number }>;
  ticketsByPriority: { LOW: number; MEDIUM: number; HIGH: number; CRITICAL: number };
}

const API_ENDPOINT = 'http://192.168.1.164:5196/graphql';

export default function ReportsPage() {
  const router = useRouter();

  const [authToken, setAuthToken] = useState<string | null>(null);
  const [analytics, setAnalytics] = useState<AnalyticsData>({
    totalCustomers: 0,
    newCustomersThisMonth: 0,
    totalTickets: 0,
    openTickets: 0,
    resolvedTickets: 0,
    closedTickets: 0,
    inProgressTickets: 0,
    criticalTickets: 0,
    highPriorityTickets: 0,
    averageResolutionTime: 0,
    customersByMonth: [],
    ticketsByPriority: { LOW: 0, MEDIUM: 0, HIGH: 0, CRITICAL: 0 },
  });
  const [loading, setLoading] = useState(true);

  // Check authentication on mount
  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      router.push('/login');
      return;
    }
    setAuthToken(token);
    fetchAnalytics(token);
  }, [router]);

  const fetchAnalytics = useCallback(async (token: string) => {
    try {
      setLoading(true);

      // Fetch customers
      const customersResponse = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetCustomers {
              customers {
                id
                name
                email
                phone
                city
                createdAt
              }
            }
          `,
        }),
      });

      const customersData = await customersResponse.json();
      if (customersData.errors) {
        console.error('Error fetching customers:', customersData.errors);
        return;
      }

      const customers: Customer[] = customersData.data?.customers || [];

      // Fetch tickets
      const ticketsResponse = await fetch(API_ENDPOINT, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          query: `
            query GetTickets {
              tickets {
                id
                title
                status
                priority
                customerId
                createdAt
                updatedAt
                resolvedAt
              }
            }
          `,
        }),
      });

      const ticketsData = await ticketsResponse.json();
      if (ticketsData.errors) {
        console.error('Error fetching tickets:', ticketsData.errors);
        return;
      }

      const tickets: Ticket[] = ticketsData.data?.tickets || [];

      // Calculate analytics
      const now = new Date();
      const thisMonth = new Date(now.getFullYear(), now.getMonth(), 1);

      const newCustomersThisMonth = customers.filter(
        (c) => new Date(c.createdAt) >= thisMonth
      ).length;

      const openTickets = tickets.filter((t) => t.status === 'OPEN').length;
      const inProgressTickets = tickets.filter((t) => t.status === 'IN_PROGRESS').length;
      const resolvedTickets = tickets.filter((t) => t.status === 'RESOLVED').length;
      const closedTickets = tickets.filter((t) => t.status === 'CLOSED').length;
      const criticalTickets = tickets.filter((t) => t.priority === 'CRITICAL').length;
      const highPriorityTickets = tickets.filter((t) => t.priority === 'HIGH').length;

      // Calculate average resolution time (in hours)
      const resolvedTicketsWithTime = tickets.filter(
        (t) => t.resolvedAt && t.createdAt
      );
      let averageResolutionTime = 0;
      if (resolvedTicketsWithTime.length > 0) {
        const totalTime = resolvedTicketsWithTime.reduce((sum, ticket) => {
          const created = new Date(ticket.createdAt).getTime();
          const resolved = new Date(ticket.resolvedAt!).getTime();
          return sum + (resolved - created);
        }, 0);
        averageResolutionTime = Math.round(totalTime / resolvedTicketsWithTime.length / (1000 * 60 * 60));
      }

      // Calculate customers by month (last 12 months)
      const customersByMonth: { [key: string]: number } = {};
      for (let i = 11; i >= 0; i--) {
        const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
        const key = date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
        customersByMonth[key] = 0;
      }

      customers.forEach((customer) => {
        const date = new Date(customer.createdAt);
        const key = date.toLocaleDateString('en-US', { month: 'short', year: '2-digit' });
        if (key in customersByMonth) {
          customersByMonth[key]++;
        }
      });

      // Calculate tickets by priority
      const ticketsByPriority = {
        LOW: tickets.filter((t) => t.priority === 'LOW').length,
        MEDIUM: tickets.filter((t) => t.priority === 'MEDIUM').length,
        HIGH: tickets.filter((t) => t.priority === 'HIGH').length,
        CRITICAL: tickets.filter((t) => t.priority === 'CRITICAL').length,
      };

      setAnalytics({
        totalCustomers: customers.length,
        newCustomersThisMonth,
        totalTickets: tickets.length,
        openTickets,
        resolvedTickets,
        closedTickets,
        inProgressTickets,
        criticalTickets,
        highPriorityTickets,
        averageResolutionTime,
        customersByMonth: Object.entries(customersByMonth).map(([month, count]) => ({
          month,
          count,
        })),
        ticketsByPriority,
      });
    } catch (error) {
      console.error('Error fetching analytics:', error);
    } finally {
      setLoading(false);
    }
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-gray-500">Loading analytics...</div>
      </div>
    );
  }

  // Calculate resolution rate
  const totalResolvedOrClosed = analytics.resolvedTickets + analytics.closedTickets;
  const resolutionRate = analytics.totalTickets > 0
    ? Math.round((totalResolvedOrClosed / analytics.totalTickets) * 100)
    : 0;

  return (
    <DashboardLayout currentPage="reports">
      <main className="py-8">
        <div className="px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-gray-900">Reports & Analytics</h1>
            <p className="text-gray-600 mt-1">Track your CRM performance metrics</p>
          </div>

          {/* Key Metrics Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {/* Total Customers */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-start justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">Total Customers</p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">{analytics.totalCustomers}</p>
                  <p className="text-sm text-green-600 mt-2">
                +{analytics.newCustomersThisMonth} this month
              </p>
            </div>
            <div className="p-3 bg-blue-100 rounded-lg">
              <svg className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-2a6 6 0 0112 0v2zm0 0h6v-2a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Total Tickets */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Total Tickets</p>
              <p className="text-3xl font-bold text-gray-900 mt-2">{analytics.totalTickets}</p>
              <p className="text-sm text-orange-600 mt-2">
                {analytics.openTickets} open now
              </p>
            </div>
            <div className="p-3 bg-orange-100 rounded-lg">
              <svg className="h-6 w-6 text-orange-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Resolution Rate */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Resolution Rate</p>
              <p className="text-3xl font-bold text-gray-900 mt-2">{resolutionRate}%</p>
              <p className="text-sm text-green-600 mt-2">
                {totalResolvedOrClosed} resolved/closed
              </p>
            </div>
            <div className="p-3 bg-green-100 rounded-lg">
              <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
          </div>
        </div>

        {/* Avg Resolution Time */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm font-medium text-gray-600">Avg Resolution Time</p>
              <p className="text-3xl font-bold text-gray-900 mt-2">{analytics.averageResolutionTime}h</p>
              <p className="text-sm text-blue-600 mt-2">
                hours per ticket
              </p>
            </div>
            <div className="p-3 bg-purple-100 rounded-lg">
              <svg className="h-6 w-6 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
          </div>
        </div>
      </div>

      {/* Ticket Status Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Ticket Status Distribution */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Ticket Status Distribution</h2>
          <div className="space-y-4">
            {[
              { label: 'Open', value: analytics.openTickets, color: 'bg-red-500', lightColor: 'bg-red-100', textColor: 'text-red-700' },
              { label: 'In Progress', value: analytics.inProgressTickets, color: 'bg-blue-500', lightColor: 'bg-blue-100', textColor: 'text-blue-700' },
              { label: 'Resolved', value: analytics.resolvedTickets, color: 'bg-green-500', lightColor: 'bg-green-100', textColor: 'text-green-700' },
              { label: 'Closed', value: analytics.closedTickets, color: 'bg-gray-500', lightColor: 'bg-gray-100', textColor: 'text-gray-700' },
            ].map((status) => (
              <div key={status.label}>
                <div className="flex items-center justify-between mb-1">
                  <span className="text-sm font-medium text-gray-700">{status.label}</span>
                  <span className={`text-sm font-bold ${status.textColor}`}>{status.value}</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className={`${status.color} h-2 rounded-full transition-all duration-300`}
                    style={{
                      width: `${analytics.totalTickets > 0 ? (status.value / analytics.totalTickets) * 100 : 0}%`,
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Priority Distribution */}
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Tickets by Priority</h2>
          <div className="space-y-4">
            {[
              { label: 'Critical', value: analytics.ticketsByPriority.CRITICAL, color: 'text-red-600', bgColor: 'bg-red-50', icon: '🔴' },
              { label: 'High', value: analytics.ticketsByPriority.HIGH, color: 'text-orange-600', bgColor: 'bg-orange-50', icon: '🟠' },
              { label: 'Medium', value: analytics.ticketsByPriority.MEDIUM, color: 'text-yellow-600', bgColor: 'bg-yellow-50', icon: '🟡' },
              { label: 'Low', value: analytics.ticketsByPriority.LOW, color: 'text-green-600', bgColor: 'bg-green-50', icon: '🟢' },
            ].map((priority) => (
              <div key={priority.label} className={`${priority.bgColor} rounded-lg p-4 flex items-center justify-between`}>
                <div className="flex items-center space-x-3">
                  <span className="text-lg">{priority.icon}</span>
                  <span className="font-medium text-gray-900">{priority.label}</span>
                </div>
                <span className={`${priority.color} font-bold text-lg`}>{priority.value}</span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Customer Growth Chart */}
      <div className="bg-white rounded-lg shadow p-6 mb-8">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Customer Growth (Last 12 Months)</h2>
        <div className="flex items-end justify-between h-64 gap-2">
          {analytics.customersByMonth.map((data) => (
            <div key={data.month} className="flex-1 flex flex-col items-center">
              <div className="relative w-full h-full flex items-end justify-center">
                {data.count > 0 && (
                  <div
                    className="w-full bg-linear-to-t from-blue-500 to-blue-400 rounded-t"
                    style={{
                      height: `${Math.max(20, (data.count / Math.max(...analytics.customersByMonth.map(m => m.count), 1)) * 100)}%`,
                    }}
                    title={`${data.count} customers`}
                  />
                )}
              </div>
              <p className="text-xs text-gray-600 mt-2 text-center font-medium">{data.month}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-linear-to-br from-blue-50 to-indigo-50 rounded-lg shadow p-6 border border-blue-100">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">Ticket Performance</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Urgent Issues</span>
              <span className="font-bold text-red-600">{analytics.criticalTickets}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">High Priority</span>
              <span className="font-bold text-orange-600">{analytics.highPriorityTickets}</span>
            </div>
            <div className="flex justify-between pt-2 border-t border-blue-200">
              <span className="text-sm text-gray-600">Total Urgent</span>
              <span className="font-bold text-gray-900">{analytics.criticalTickets + analytics.highPriorityTickets}</span>
            </div>
          </div>
        </div>

        <div className="bg-linear-to-br from-green-50 to-emerald-50 rounded-lg shadow p-6 border border-green-100">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">Customer Health</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Total Customers</span>
              <span className="font-bold text-blue-600">{analytics.totalCustomers}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">New This Month</span>
              <span className="font-bold text-green-600">{analytics.newCustomersThisMonth}</span>
            </div>
            <div className="flex justify-between pt-2 border-t border-green-200">
              <span className="text-sm text-gray-600">Growth Rate</span>
              <span className="font-bold text-gray-900">
                {analytics.totalCustomers > 0 
                  ? Math.round((analytics.newCustomersThisMonth / analytics.totalCustomers) * 100) 
                  : 0}%
              </span>
            </div>
          </div>
        </div>

        <div className="bg-linear-to-br from-purple-50 to-pink-50 rounded-lg shadow p-6 border border-purple-100">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">Support Metrics</h3>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Resolution Rate</span>
              <span className="font-bold text-green-600">{resolutionRate}%</span>
            </div>
            <div className="flex justify-between">
              <span className="text-sm text-gray-600">Avg Response Time</span>
              <span className="font-bold text-purple-600">{analytics.averageResolutionTime}h</span>
            </div>
            <div className="flex justify-between pt-2 border-t border-purple-200">
              <span className="text-sm text-gray-600">Tickets Resolved</span>
              <span className="font-bold text-gray-900">{totalResolvedOrClosed}</span>
            </div>
          </div>
        </div>
      </div>
      </div>
    </main>
    </DashboardLayout>
  );
}
