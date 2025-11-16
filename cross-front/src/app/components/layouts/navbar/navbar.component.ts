import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { LogoComponent } from '../logo/logo.component';
import { AuthService, User } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, LogoComponent, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {
  isLoggedIn = false;
  currentUser: User | null = null;
  userAvatar = 'https://via.placeholder.com/32';
  isUserMenuOpen = false;
  isMobileMenuOpen = false;
  isProfileLoading = false;
  private subscriptions = new Subscription();

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Subscribe to authentication state
    this.subscriptions.add(
      this.authService.currentUser$.subscribe(user => {
        this.currentUser = user;
        this.isLoggedIn = !!user;
        
        // Update avatar - use user's avatar or fallback to placeholder
        if (user?.avatar) {
          this.userAvatar = user.avatar;
        } else if (user) {
          // Generate a default avatar based on user's initials
          this.userAvatar = this.generateDefaultAvatar(user.name || 'User');
        } else {
          this.userAvatar = 'https://via.placeholder.com/32';
        }
      })
    );

    // Subscribe to route changes to close mobile menu
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.closeMobileMenu();
        this.closeUserMenu();
      })
    );

    // Fetch fresh user profile if authenticated
    if (this.authService.isAuthenticated()) {
      this.refreshUserProfile();
    }
  }

  private generateDefaultAvatar(name: string): string {
    // Generate a simple default avatar URL with user initials
    const initials = name
      .split(' ')
      .map(n => n.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
    
    // Using a service like UI Avatars for default avatars
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&size=32&background=6366f1&color=ffffff&rounded=true`;
  }

  private refreshUserProfile(): void {
    this.isProfileLoading = true;
    this.authService.fetchCurrentUserProfile().subscribe({
      next: (user) => {
        // User profile updated automatically via the observable
        this.isProfileLoading = false;
      },
      error: (error) => {
        console.warn('Could not refresh user profile:', error);
        this.isProfileLoading = false;
        // Don't show error to user for profile refresh failures
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  signIn() {
    this.router.navigate(['/signin']);
    this.closeMobileMenu();
  }

  register() {
    this.router.navigate(['/register']);
    this.closeMobileMenu();
  }

  logout() {
    this.authService.signOut();
    this.closeUserMenu();
    this.closeMobileMenu();
    this.router.navigate(['/']);
  }

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  closeUserMenu() {
    this.isUserMenuOpen = false;
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
    this.closeUserMenu();
    this.closeMobileMenu();
  }

  refreshProfile() {
    this.refreshUserProfile();
    this.closeUserMenu();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    
    // Handle user menu
    if (this.isUserMenuOpen) {
      const userMenuButton = target.closest('[data-user-menu-toggle]');
      const userMenuDropdown = target.closest('[data-user-menu-dropdown]');
      
      // Don't close if clicking on the toggle button or inside the dropdown
      if (!userMenuButton && !userMenuDropdown) {
        this.closeUserMenu();
      }
    }

    // Handle mobile menu
    if (this.isMobileMenuOpen) {
      const mobileMenuButton = target.closest('[data-mobile-menu-toggle]');
      const mobileMenuDropdown = target.closest('[data-mobile-menu]');
      
      // Don't close if clicking on the toggle button or inside the mobile menu
      if (!mobileMenuButton && !mobileMenuDropdown) {
        this.closeMobileMenu();
      }
    }
  }
}