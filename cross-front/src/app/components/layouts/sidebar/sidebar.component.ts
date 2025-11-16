import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { LogoComponent } from '../logo/logo.component';

interface MenuItem {
  id: string;
  label: string;
  icon: string;
  route: string;
  badge?: string;
  children?: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, LogoComponent],
  template: `
    <div class="flex h-screen bg-gray-50/50">
      <!-- Desktop Sidebar -->
      <div class="hidden lg:flex lg:w-64 lg:flex-col lg:fixed lg:inset-y-0">
        <div class="flex flex-col flex-grow overflow-y-auto bg-white border-r border-gray-200/80 backdrop-blur-sm">
          <!-- Logo section -->
          <div class="flex items-center flex-shrink-0 h-16 px-6 border-b border-gray-200/50">
            <app-logo class="flex items-center"></app-logo>
          </div>

          <!-- Navigation -->
          <nav class="flex-1 px-4 space-y-2 mt-6">
            <div *ngFor="let item of menuItems" class="space-y-1">
              <!-- Menu item without children -->
              <a *ngIf="!item.children"
                 [routerLink]="item.route"
                 routerLinkActive="bg-blue-50 text-blue-700 shadow-sm border border-blue-200/50"
                 [routerLinkActiveOptions]="{exact: false}"
                 class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200 border border-transparent">
                <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500 flex-shrink-0" 
                     [innerHTML]="item.icon">
                </svg>
                <span class="truncate">{{ item.label }}</span>
                <span *ngIf="item.badge" 
                      class="ml-auto inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-50 text-blue-600 border border-blue-200">
                  {{ item.badge }}
                </span>
              </a>

              <!-- Menu item with children (expandable) -->
              <div *ngIf="item.children" class="space-y-1">
                <button (click)="toggleSubmenu(item.id)"
                        class="group w-full flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200 border border-transparent hover:border-gray-200/50">
                  <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500 flex-shrink-0" 
                       [innerHTML]="item.icon">
                  </svg>
                  <span class="truncate">{{ item.label }}</span>
                  <svg class="ml-auto w-4 h-4 text-gray-400 transition-transform duration-200 flex-shrink-0"
                       [class.rotate-90]="expandedMenus.has(item.id)"
                       fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                  </svg>
                </button>
                
                <!-- Submenu items -->
                <div *ngIf="expandedMenus.has(item.id)" class="ml-8 space-y-1">
                  <a *ngFor="let child of item.children"
                     [routerLink]="child.route"
                     routerLinkActive="bg-blue-50 text-blue-700"
                     class="group flex items-center px-3 py-2 text-sm font-medium text-gray-600 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200">
                    <span class="truncate">{{ child.label }}</span>
                  </a>
                </div>
              </div>
            </div>
          </nav>

          <!-- Bottom section with settings and help -->
          <div class="px-4 pb-4 space-y-3 border-t border-gray-200/50 pt-4">
            <!-- Settings and Help -->
            <a href="#" class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200">
              <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
              </svg>
              <span class="truncate">Settings</span>
            </a>
            <a href="#" class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200">
              <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
              <span class="truncate">Help & Support</span>
            </a>
          </div>
        </div>
      </div>

      <!-- Mobile sidebar overlay -->
      <div *ngIf="isMobileSidebarOpen" class="fixed inset-0 z-50 lg:hidden">
        <div class="fixed inset-0 bg-black/50 backdrop-blur-sm" (click)="closeMobileSidebar()"></div>
        
        <!-- Mobile sidebar -->
        <div class="relative flex-1 flex flex-col max-w-xs w-full bg-white shadow-xl">
          <div class="absolute top-0 right-0 -mr-12 pt-4">
            <button (click)="closeMobileSidebar()" 
                    class="ml-1 flex items-center justify-center h-10 w-10 rounded-full focus:outline-none focus:ring-2 focus:ring-inset focus:ring-white bg-white/10 backdrop-blur-sm">
              <span class="sr-only">Close sidebar</span>
              <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>
          
          <!-- Mobile sidebar content (same structure as desktop) -->
          <div class="flex-1 h-0 overflow-y-auto">
            <!-- Mobile logo -->
            <div class="flex items-center flex-shrink-0 h-16 px-6 border-b border-gray-200/50">
              <app-logo class="flex items-center"></app-logo>
            </div>
            
            <!-- Mobile navigation (same as desktop but optimized for mobile) -->
            <div class="px-4 py-4">
              <!-- Mobile menu items -->
              <nav class="space-y-2">
                <div *ngFor="let item of menuItems" class="space-y-1">
                  <a *ngIf="!item.children"
                     [routerLink]="item.route"
                     (click)="closeMobileSidebar()"
                     routerLinkActive="bg-blue-50 text-blue-700"
                     class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 transition-all duration-200">
                    <svg class="mr-3 w-5 h-5 text-gray-400" [innerHTML]="item.icon"></svg>
                    {{ item.label }}
                    <span *ngIf="item.badge" class="ml-auto inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-50 text-blue-600">
                      {{ item.badge }}
                    </span>
                  </a>
                </div>
              </nav>
              
              <!-- Mobile Settings and Help -->
              <div class="mt-6 pt-6 border-t border-gray-200/50 space-y-2">
                <a href="#" class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200">
                  <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                  </svg>
                  Settings
                </a>
                <a href="#" class="group flex items-center px-3 py-2.5 text-sm font-medium text-gray-700 rounded-lg hover:bg-gray-50 hover:text-gray-900 transition-all duration-200">
                  <svg class="mr-3 w-5 h-5 text-gray-400 group-hover:text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                  </svg>
                  Help & Support
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Main content area -->
      <div class="flex-1 lg:ml-64">
        <ng-content></ng-content>
      </div>
    </div>
  `
})
export class SidebarComponent implements OnInit, OnDestroy {
  isMobileSidebarOpen = false;
  expandedMenus = new Set<string>();
  private subscriptions = new Subscription();

  menuItems: MenuItem[] = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h0a2 2 0 012 2v0a2 2 0 01-2 2H10a2 2 0 01-2-2v0z"></path>`,
      route: '/dashboard'
    },
    {
      id: 'customers',
      label: 'Customers',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path>`,
      route: '/customers',
      badge: '24'
    },
    {
      id: 'leads',
      label: 'Leads',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>`,
      route: '/leads'
    },
    {
      id: 'sales',
      label: 'Sales',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1"></path>`,
      route: '/sales',
      children: [
        { id: 'opportunities', label: 'Opportunities', icon: '', route: '/sales/opportunities' },
        { id: 'quotes', label: 'Quotes', icon: '', route: '/sales/quotes' },
        { id: 'invoices', label: 'Invoices', icon: '', route: '/sales/invoices' }
      ]
    },
    {
      id: 'marketing',
      label: 'Marketing',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 3.055A9.001 9.001 0 1020.945 13H11V3.055z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.488 9H15V3.512A9.025 9.025 0 0120.488 9z"></path>`,
      route: '/marketing',
      children: [
        { id: 'campaigns', label: 'Campaigns', icon: '', route: '/marketing/campaigns' },
        { id: 'emails', label: 'Email Marketing', icon: '', route: '/marketing/emails' },
        { id: 'analytics', label: 'Analytics', icon: '', route: '/marketing/analytics' }
      ]
    },
    {
      id: 'reports',
      label: 'Reports',
      icon: `<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>`,
      route: '/reports'
    }
  ];

  constructor(
    private router: Router
  ) {}

  ngOnInit(): void {
    // Component initialization
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  toggleSubmenu(menuId: string): void {
    if (this.expandedMenus.has(menuId)) {
      this.expandedMenus.delete(menuId);
    } else {
      this.expandedMenus.add(menuId);
    }
  }

  public toggleMobileSidebar(): void {
    this.isMobileSidebarOpen = !this.isMobileSidebarOpen;
  }

  public closeMobileSidebar(): void {
    this.isMobileSidebarOpen = false;
  }
}