'use client';

import { useState, useEffect, ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import Logo from '@/components/Logo';
import {
  DashboardIcon,
  CustomersIcon,
  ContactsIcon,
  TicketsIcon,
  CompaniesIcon,
  ReportsIcon,
  SettingsIcon,
  BellIcon,
  ChevronDownIcon,
  MenuIcon,
} from '@/components/icons';

interface User {
  id?: number;
  name: string;
  email: string;
  phone?: string;
  companyId?: string;
  createdAt?: string;
}

interface NavigationItem {
  name: string;
  href: string;
  icon?: React.ReactNode;
  current: boolean;
}

interface DashboardLayoutProps {
  children: ReactNode;
  currentPage: string;
}

export default function DashboardLayout({ children, currentPage }: DashboardLayoutProps) {
  const router = useRouter();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [user, setUser] = useState<User>({
    name: 'John Doe',
    email: 'john.doe@example.com',
  });

  useEffect(() => {
    // Get user data from localStorage
    const userData = localStorage.getItem('currentUser');
    const token = localStorage.getItem('authToken');

    if (!userData || !token) {
      // Redirect to login if not authenticated
      router.push('/login');
      return;
    }

    try {
      const parsedUser = JSON.parse(userData);
      setUser(parsedUser);
      
      // Fetch user data from protected me query
      const fetchUserData = async () => {
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
            })
          });

          const result = await response.json();

          if (result.errors) {
            console.error('Error fetching user data:', result.errors[0].message);
            // If token is invalid, logout
            if (result.errors[0].message.includes('Invalid') || result.errors[0].message.includes('Unauthorized')) {
              localStorage.removeItem('authToken');
              localStorage.removeItem('currentUser');
              router.push('/login');
            }
            return;
          }

          const userData = result.data.me;
          setUser(userData);
          
          // Update localStorage with fresh data from server
          localStorage.setItem('currentUser', JSON.stringify({
            id: userData.id,
            name: userData.name,
            email: userData.email,
            companyId: userData.companyId,
          }));
        } catch (error) {
          console.error('Error fetching user data from me query:', error);
        }
      };

      fetchUserData();
    } catch (error) {
      console.error('Error parsing user data:', error);
      router.push('/login');
    }
  }, [router]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (profileOpen) {
        const target = event.target as HTMLElement;
        if (!target.closest('[data-profile-dropdown]')) {
          setProfileOpen(false);
        }
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [profileOpen]);

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    router.push('/login');
  };

  const navigation: NavigationItem[] = [
    { name: 'Dashboard', href: '/dashboard', icon: <DashboardIcon />, current: currentPage === 'dashboard' },
    { name: 'Customers', href: '/dashboard/customers', icon: <CustomersIcon />, current: currentPage === 'customers' },
    { name: 'Contacts', href: '/dashboard/contacts', icon: <ContactsIcon />, current: currentPage === 'contacts' },
    { name: 'Tickets', href: '/dashboard/tickets', icon: <TicketsIcon />, current: currentPage === 'tickets' },
    { name: 'Companies', href: '/dashboard/companies', icon: <CompaniesIcon />, current: currentPage === 'companies' },
    { name: 'Reports', href: '/dashboard/reports', icon: <ReportsIcon />, current: currentPage === 'reports' },
    { name: 'Settings', href: '/dashboard/settings', icon: <SettingsIcon />, current: currentPage === 'settings' },
  ];

  return (
    <div className="min-h-screen" style={{ background: 'var(--gray-50)' }}>
      {/* Mobile sidebar overlay with backdrop blur */}
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 lg:hidden transition-opacity">
          <div 
            className="fixed inset-0 backdrop-blur-sm transition-opacity"
            style={{ backgroundColor: 'rgba(0, 0, 0, 0.3)' }}
            onClick={() => setSidebarOpen(false)}
          />
          <div 
            className="fixed inset-y-0 left-0 z-50 w-64 transition-transform"
            style={{
              background: 'rgba(255, 255, 255, 0.8)',
              backdropFilter: 'blur(20px) saturate(180%)',
              WebkitBackdropFilter: 'blur(20px) saturate(180%)',
              boxShadow: 'var(--shadow-lg)',
            }}
          >
            <SidebarContent navigation={navigation} user={user} onClose={() => setSidebarOpen(false)} />
          </div>
        </div>
      )}

      {/* Desktop sidebar with glassmorphism */}
      <div 
        className="hidden lg:fixed lg:inset-y-0 lg:left-0 lg:z-50 lg:block lg:w-64"
        style={{
          background: 'rgba(255, 255, 255, 0.7)',
          backdropFilter: 'blur(20px) saturate(180%)',
          WebkitBackdropFilter: 'blur(20px) saturate(180%)',
          borderRight: '1px solid var(--border)',
          boxShadow: 'var(--shadow-sm)',
        }}
      >
        <SidebarContent navigation={navigation} user={user} />
      </div>

      {/* Main content */}
      <div className="lg:pl-64">
        {/* Top navigation with glassmorphism */}
        <div 
          className="sticky top-0 z-40 transition-all"
          style={{
            background: 'rgba(255, 255, 255, 0.8)',
            backdropFilter: 'blur(20px) saturate(180%)',
            WebkitBackdropFilter: 'blur(20px) saturate(180%)',
            borderBottom: '1px solid var(--border)',
          }}
        >
          <div className="flex h-16 items-center justify-between px-4 sm:px-6 lg:px-8">
            <div className="flex items-center">
              <button
                type="button"
                className="lg:hidden p-2 rounded-md transition-all hover:bg-gray-100 active:scale-95"
                style={{ color: 'var(--gray-600)' }}
                onClick={() => setSidebarOpen(true)}
              >
                <MenuIcon size={20} />
              </button>
            </div>

            <div className="flex items-center gap-4">
              {/* Notifications */}
              <button 
                className="p-2 rounded-md transition-all hover:bg-gray-100 active:scale-95"
                style={{ color: 'var(--gray-600)' }}
              >
                <BellIcon size={20} />
              </button>

              {/* Profile dropdown */}
              <div className="flex items-center gap-3 relative" data-profile-dropdown>
                <div className="shrink-0">
                  <div 
                    className="h-9 w-9 rounded-full flex items-center justify-center text-sm font-medium transition-transform hover:scale-105"
                    style={{ 
                      background: 'var(--blue)',
                      color: 'white',
                    }}
                  >
                    {user.name.split(' ').map(n => n[0]).join('')}
                  </div>
                </div>
                <div className="hidden md:block">
                  <div className="text-sm font-medium" style={{ color: 'var(--gray-900)' }}>{user.name}</div>
                  <div className="text-xs" style={{ color: 'var(--gray-500)' }}>{user.email}</div>
                </div>
                <button 
                  onClick={() => setProfileOpen(!profileOpen)}
                  className="p-1 rounded-md transition-all hover:bg-gray-100 active:scale-95"
                  style={{ color: 'var(--gray-500)' }}
                >
                  <ChevronDownIcon 
                    size={16} 
                    className={`transition-transform ${profileOpen ? 'rotate-180' : ''}`}
                  />
                </button>

                {/* Profile dropdown menu with glassmorphism */}
                {profileOpen && (
                  <div 
                    className="absolute right-0 mt-2 w-56 rounded-xl py-2 z-50 top-12 transition-all animate-in fade-in slide-in-from-top-2"
                    style={{
                      background: 'rgba(255, 255, 255, 0.9)',
                      backdropFilter: 'blur(20px) saturate(180%)',
                      WebkitBackdropFilter: 'blur(20px) saturate(180%)',
                      border: '1px solid var(--border)',
                      boxShadow: 'var(--shadow-lg)',
                    }}
                  >
                    <div className="px-4 py-3 border-b" style={{ borderColor: 'var(--border)' }}>
                      <p className="text-sm font-medium" style={{ color: 'var(--gray-900)' }}>{user.name}</p>
                      <p className="text-xs mt-0.5" style={{ color: 'var(--gray-500)' }}>{user.email}</p>
                    </div>
                    <Link
                      href="/dashboard/settings"
                      className="block px-4 py-2.5 text-sm transition-colors hover:bg-gray-100"
                      style={{ color: 'var(--gray-700)' }}
                      onClick={() => setProfileOpen(false)}
                    >
                      Settings
                    </Link>
                    <button
                      onClick={handleLogout}
                      className="block w-full text-left px-4 py-2.5 text-sm transition-colors hover:bg-red-50"
                      style={{ color: 'var(--red)' }}
                    >
                      Sign Out
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Page content */}
        {children}
      </div>
    </div>
  );
}

interface SidebarContentProps {
  navigation: NavigationItem[];
  user: User;
  onClose?: () => void;
}

// Sidebar component with Apple-style design
function SidebarContent({ navigation, user, onClose }: SidebarContentProps) {
  return (
    <div className="flex flex-col h-full">
      {/* Logo */}
      <div 
        className="flex items-center justify-center h-16 px-4"
        style={{ borderBottom: '1px solid var(--border)' }}
      >
        <Logo />
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-1">
        {navigation.map((item) => (
          <Link
            key={item.name}
            href={item.href}
            onClick={onClose}
            className={`group flex items-center gap-3 px-3 py-2.5 text-sm font-medium rounded-lg transition-all ${
              item.current
                ? 'text-blue'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
            style={{
              backgroundColor: item.current ? 'rgba(0, 122, 255, 0.1)' : 'transparent',
              color: item.current ? 'var(--blue)' : 'var(--gray-700)',
            }}
          >
            <span 
              className="shrink-0 transition-transform group-hover:scale-110"
              style={{ 
                color: item.current ? 'var(--blue)' : 'var(--gray-600)',
              }}
            >
              {item.icon}
            </span>
            <span className="flex-1">{item.name}</span>
            {item.current && (
              <div 
                className="h-1.5 w-1.5 rounded-full"
                style={{ backgroundColor: 'var(--blue)' }}
              />
            )}
          </Link>
        ))}
      </nav>

      {/* User info */}
      <div 
        className="p-4"
        style={{ borderTop: '1px solid var(--border)' }}
      >
        <div className="flex items-center gap-3">
          <div className="shrink-0">
            <div 
              className="h-9 w-9 rounded-full flex items-center justify-center text-sm font-medium"
              style={{ 
                background: 'var(--blue)',
                color: 'white',
              }}
            >
              {user.name.split(' ').map((n: string) => n[0]).join('')}
            </div>
          </div>
          <div className="min-w-0 flex-1">
            <p 
              className="text-sm font-medium truncate"
              style={{ color: 'var(--gray-900)' }}
            >
              {user.name}
            </p>
            <p 
              className="text-xs truncate mt-0.5"
              style={{ color: 'var(--gray-500)' }}
            >
              {user.email}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
