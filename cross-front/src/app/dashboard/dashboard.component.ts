import { Component, OnInit, OnDestroy, HostListener, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SidebarComponent } from '../components/layouts/sidebar/sidebar.component';
import { AuthService } from '../services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, SidebarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild(SidebarComponent) sidebar!: SidebarComponent;
  
  currentUser: any = null;
  userAvatar = 'https://via.placeholder.com/32';
  isUserMenuOpen = false;
  private subscriptions = new Subscription();
  
  stats = {
    totalCustomers: 247,
    activeDeals: 18,
    revenue: 45890,
    conversionRate: 14.2
  };

  recentCustomers = [
    { name: 'John Doe', email: 'john@example.com', value: 12500, status: 'Active' },
    { name: 'Sarah Wilson', email: 'sarah@example.com', value: 8900, status: 'Negotiation' },
    { name: 'Michael Chen', email: 'michael@example.com', value: 15600, status: 'Closed' },
    { name: 'Emma Thompson', email: 'emma@example.com', value: 7200, status: 'Follow-up' },
    { name: 'David Lee', email: 'david@example.com', value: 22100, status: 'Active' }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // Test the provided JWT token
    this.testJWTToken();
  }

  ngOnInit() {
    this.subscriptions.add(
      this.authService.currentUser$.subscribe(user => {
        if (!user) {
          this.router.navigate(['/signin']);
          return;
        }
        this.currentUser = user;
        if (user?.avatar) {
          this.userAvatar = user.avatar;
        } else if (user) {
          // Generate a default avatar based on user's initials
          this.userAvatar = this.generateDefaultAvatar(user.name || 'User');
        }
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.relative')) {
      this.isUserMenuOpen = false;
    }
  }

  private generateDefaultAvatar(name: string): string {
    const initials = name
      .split(' ')
      .map(n => n.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
    
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&size=32&background=2563eb&color=ffffff&rounded=true`;
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  toggleSidebar(): void {
    if (this.sidebar) {
      this.sidebar.toggleMobileSidebar();
    }
  }

  closeUserMenu(): void {
    this.isUserMenuOpen = false;
  }

  logout(): void {
    this.authService.signOut();
    this.router.navigate(['/']);
  }

  private testJWTToken(): void {
    // Test the NEW JWT token with company ID
    const testToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhbC1odXNzZWluQHBhcGF5YXRyYWRpbmcuY29tIiwibmFtZSI6ImFsLWh1c3NlaW4iLCJjb21wYW55SWQiOiI2IiwiZXhwIjoxNzYzMzM2Mzg2LCJpc3MiOiJjcm0tYmFja2VuZCIsImF1ZCI6ImNybS1jbGllbnQifQ.rj1dDFXtA7jnXa4cWkeF0LiZSdL6Cpjf1lsCyFn3Ats';
    
    console.log('='.repeat(60));
    console.log('� TESTING NEW JWT TOKEN WITH COMPANY ID');
    console.log('='.repeat(60));
    
    (this.authService as any).testToken(testToken);
    
    console.log('\n🏢 Company Info Available - Ready for Customer Module!');
    console.log('='.repeat(60));
  }
}