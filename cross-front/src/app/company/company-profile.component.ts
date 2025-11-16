import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, combineLatest } from 'rxjs';

import { CompanyService, Company, CompanyUser, UserInvitation } from '../services/company.service';
import { AuthService, User } from '../services/auth.service';
import { TeamService } from '../services/team.service';

interface InviteUserForm {
  email: string;
  roleId?: number;
  teamId?: number;
  notes?: string;
}

@Component({
  selector: 'app-company-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="min-h-screen bg-gray-50">
      <!-- Header -->
      <div class="bg-white shadow-sm border-b border-gray-200">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between items-center py-6">
            <div>
              <h1 class="text-2xl font-semibold text-gray-900">Company Profile</h1>
              <p class="text-sm text-gray-600 mt-1">Manage your company information and team members</p>
            </div>
            <button 
              (click)="showInviteModal = true"
              class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors flex items-center space-x-2">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
              </svg>
              <span>Invite User</span>
            </button>
          </div>
        </div>
      </div>

      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- Company Information Card -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 mb-8">
          <div class="px-6 py-4 border-b border-gray-200">
            <h2 class="text-lg font-semibold text-gray-900">Company Information</h2>
          </div>
          
          <div class="p-6" *ngIf="company">
            <form [formGroup]="companyForm" (ngSubmit)="updateCompanyProfile()" class="space-y-6">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <!-- Company Name -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Company Name</label>
                  <input 
                    type="text" 
                    formControlName="name"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter company name">
                </div>

                <!-- Industry -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Industry</label>
                  <select 
                    formControlName="industry"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                    <option value="">Select industry</option>
                    <option *ngFor="let industry of industryOptions" [value]="industry">{{ industry }}</option>
                  </select>
                </div>

                <!-- Company Size -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Company Size</label>
                  <select 
                    formControlName="size"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
                    <option value="">Select company size</option>
                    <option *ngFor="let size of companySizeOptions" [value]="size">{{ size }}</option>
                  </select>
                </div>

                <!-- Website -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Website</label>
                  <input 
                    type="url" 
                    formControlName="website"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="https://example.com">
                </div>

                <!-- Phone -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                  <input 
                    type="tel" 
                    formControlName="phone"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="+1 (555) 123-4567">
                </div>

                <!-- Email -->
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input 
                    type="email" 
                    formControlName="email"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="contact@example.com">
                </div>
              </div>

              <!-- Description -->
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Description</label>
                <textarea 
                  formControlName="description"
                  rows="4"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  placeholder="Describe your company..."></textarea>
              </div>

              <!-- Address -->
              <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div class="md:col-span-2">
                  <label class="block text-sm font-medium text-gray-700 mb-2">Address</label>
                  <input 
                    type="text" 
                    formControlName="address"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Street address">
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">City</label>
                  <input 
                    type="text" 
                    formControlName="city"
                    class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="City">
                </div>
              </div>

              <!-- Update Button -->
              <div class="flex justify-end">
                <button 
                  type="submit" 
                  [disabled]="companyForm.invalid || isUpdatingCompany"
                  class="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center space-x-2">
                  <svg *ngIf="isUpdatingCompany" class="w-4 h-4 animate-spin" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V5z" clip-rule="evenodd"></path>
                  </svg>
                  <span>{{ isUpdatingCompany ? 'Updating...' : 'Update Company' }}</span>
                </button>
              </div>
            </form>
          </div>
        </div>

        <!-- Team Members & Invitations Tabs -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200">
          <div class="border-b border-gray-200">
            <nav class="flex space-x-8 px-6">
              <button 
                (click)="activeTab = 'members'"
                [class]="'py-4 px-1 border-b-2 font-medium text-sm transition-colors ' + (activeTab === 'members' ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300')">
                Team Members
                <span class="ml-2 bg-gray-100 text-gray-900 py-0.5 px-2.5 rounded-full text-xs font-medium">{{ users.length }}</span>
              </button>
              <button 
                (click)="activeTab = 'invitations'"
                [class]="'py-4 px-1 border-b-2 font-medium text-sm transition-colors ' + (activeTab === 'invitations' ? 'border-blue-500 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300')">
                Pending Invitations
                <span class="ml-2 bg-gray-100 text-gray-900 py-0.5 px-2.5 rounded-full text-xs font-medium">{{ invitations.length }}</span>
              </button>
            </nav>
          </div>

          <!-- Team Members Tab -->
          <div *ngIf="activeTab === 'members'" class="p-6">
            <div *ngIf="users.length === 0" class="text-center py-8">
              <svg class="w-12 h-12 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m9 5.197v1M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path>
              </svg>
              <h3 class="text-lg font-medium text-gray-900 mb-2">No team members</h3>
              <p class="text-gray-500">Start building your team by inviting new members.</p>
            </div>

            <div *ngIf="users.length > 0" class="space-y-4">
              <div *ngFor="let user of users" class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div class="flex items-center space-x-4">
                  <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                    <span class="text-sm font-medium text-blue-600">{{ getUserInitials(user.name) }}</span>
                  </div>
                  <div>
                    <h3 class="text-sm font-medium text-gray-900">{{ user.name }}</h3>
                    <p class="text-sm text-gray-500">{{ user.email }}</p>
                    <div class="flex items-center space-x-4 mt-1">
                      <span *ngIf="user.role" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {{ user.role }}
                      </span>
                      <span [class]="'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ' + getUserStatusClass(user.status)">
                        {{ user.status }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="flex items-center space-x-2">
                  <button 
                    (click)="editUser(user)"
                    class="text-gray-400 hover:text-gray-600 transition-colors">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"></path>
                    </svg>
                  </button>
                  <button 
                    *ngIf="currentUser && user.id !== currentUser.id"
                    (click)="removeUser(user)"
                    class="text-red-400 hover:text-red-600 transition-colors">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Invitations Tab -->
          <div *ngIf="activeTab === 'invitations'" class="p-6">
            <div *ngIf="invitations.length === 0" class="text-center py-8">
              <svg class="w-12 h-12 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
              </svg>
              <h3 class="text-lg font-medium text-gray-900 mb-2">No pending invitations</h3>
              <p class="text-gray-500">All invitations have been accepted or expired.</p>
            </div>

            <div *ngIf="invitations.length > 0" class="space-y-4">
              <div *ngFor="let invitation of invitations" class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <h3 class="text-sm font-medium text-gray-900">{{ invitation.email }}</h3>
                  <div class="flex items-center space-x-4 mt-1">
                    <span *ngIf="invitation.roleName" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {{ invitation.roleName }}
                    </span>
                    <span *ngIf="invitation.teamName" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                      {{ invitation.teamName }}
                    </span>
                    <span [class]="'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ' + getInvitationStatusClass(invitation.status)">
                      {{ invitation.status }}
                    </span>
                  </div>
                  <p class="text-xs text-gray-500 mt-1">
                    Invited {{ formatDate(invitation.createdAt) }} • Expires {{ formatDate(invitation.expiresAt) }}
                  </p>
                </div>
                <div class="flex items-center space-x-2">
                  <button 
                    *ngIf="invitation.status === 'PENDING'"
                    (click)="resendInvitation(invitation.id)"
                    class="text-blue-600 hover:text-blue-800 text-xs font-medium">
                    Resend
                  </button>
                  <button 
                    *ngIf="invitation.status === 'PENDING'"
                    (click)="cancelInvitation(invitation.id)"
                    class="text-red-600 hover:text-red-800 text-xs font-medium">
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Invite User Modal -->
    <div *ngIf="showInviteModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div class="bg-white rounded-lg shadow-xl max-w-md w-full">
        <div class="px-6 py-4 border-b border-gray-200">
          <h3 class="text-lg font-semibold text-gray-900">Invite New User</h3>
        </div>
        
        <form [formGroup]="inviteForm" (ngSubmit)="sendInvitation()" class="p-6 space-y-4">
          <!-- Email -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Email Address *</label>
            <input 
              type="email" 
              formControlName="email"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="user@example.com">
            <p *ngIf="inviteForm.get('email')?.errors?.['required'] && inviteForm.get('email')?.touched" 
               class="mt-1 text-sm text-red-600">Email is required</p>
            <p *ngIf="inviteForm.get('email')?.errors?.['email'] && inviteForm.get('email')?.touched" 
               class="mt-1 text-sm text-red-600">Please enter a valid email</p>
          </div>

          <!-- Role -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Role</label>
            <select 
              formControlName="roleId"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
              <option value="">Select a role (optional)</option>
              <option *ngFor="let role of userRoles" [value]="role.id">{{ role.name }}</option>
            </select>
          </div>

          <!-- Team -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Team</label>
            <select 
              formControlName="teamId"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500">
              <option value="">Select a team (optional)</option>
              <option *ngFor="let team of teams" [value]="team.id">{{ team.name }}</option>
            </select>
          </div>

          <!-- Notes -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Welcome Message</label>
            <textarea 
              formControlName="notes"
              rows="3"
              maxlength="500"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="Optional welcome message for the new user..."></textarea>
            <p class="text-xs text-gray-500 mt-1">{{ inviteForm.get('notes')?.value?.length || 0 }}/500 characters</p>
          </div>

          <!-- Error Message -->
          <div *ngIf="errorMessage" class="bg-red-50 border border-red-200 rounded-lg p-3">
            <p class="text-sm text-red-600">{{ errorMessage }}</p>
          </div>

          <!-- Success Message -->
          <div *ngIf="successMessage" class="bg-green-50 border border-green-200 rounded-lg p-3">
            <p class="text-sm text-green-600">{{ successMessage }}</p>
          </div>

          <!-- Buttons -->
          <div class="flex justify-end space-x-3 pt-4">
            <button 
              type="button" 
              (click)="closeInviteModal()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors">
              Cancel
            </button>
            <button 
              type="submit" 
              [disabled]="inviteForm.invalid || isSendingInvite"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-lg transition-colors flex items-center space-x-2">
              <svg *ngIf="isSendingInvite" class="w-4 h-4 animate-spin" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V5z" clip-rule="evenodd"></path>
              </svg>
              <span>{{ isSendingInvite ? 'Sending...' : 'Send Invitation' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class CompanyProfileComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  company: Company | null = null;
  users: CompanyUser[] = [];
  invitations: UserInvitation[] = [];
  teams: any[] = [];
  userRoles: any[] = [
    { id: 1, name: 'Admin' },
    { id: 2, name: 'Manager' }, 
    { id: 3, name: 'Member' },
    { id: 4, name: 'Viewer' }
  ];

  currentUser: User | null = null;
  activeTab: 'members' | 'invitations' = 'members';
  showInviteModal = false;

  // Form groups
  companyForm: FormGroup;
  inviteForm: FormGroup;

  // Loading states
  isLoading = true;
  isUpdatingCompany = false;
  isSendingInvite = false;

  // Messages
  errorMessage = '';
  successMessage = '';

  // Options
  industryOptions: string[] = [];
  companySizeOptions: string[] = [];

  constructor(
    private companyService: CompanyService,
    private authService: AuthService,
    private teamService: TeamService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.companyForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      website: [''],
      industry: [''],
      size: [''],
      address: [''],
      city: [''],
      phone: [''],
      email: ['', Validators.email]
    });

    this.inviteForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      roleId: [''],
      teamId: [''],
      notes: ['', Validators.maxLength(500)]
    });

    this.industryOptions = this.companyService.getIndustryOptions();
    this.companySizeOptions = this.companyService.getCompanySizeOptions();
  }

  ngOnInit() {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser || !this.currentUser.companyId) {
      this.router.navigate(['/company-setup']);
      return;
    }

    this.loadData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadData() {
    const companyId = this.currentUser?.companyId;
    if (!companyId) return;

    // Load all data in parallel
    combineLatest([
      this.companyService.getCompanyProfile(companyId.toString()),
      this.companyService.getCompanyUsers(),
      this.companyService.getInvitations(),
      this.teamService.getTeams()
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: ([company, users, invitations, teams]) => {
        this.company = company;
        this.users = users;
        this.invitations = invitations;
        this.teams = teams;
        this.isLoading = false;

        // Populate company form
        if (company) {
          this.companyForm.patchValue(company);
        }
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.errorMessage = 'Failed to load company data';
        this.isLoading = false;
      }
    });
  }

  updateCompanyProfile() {
    if (this.companyForm.invalid || !this.company) return;

    this.isUpdatingCompany = true;
    this.errorMessage = '';

    this.companyService.updateCompanyProfile(this.company.id, this.companyForm.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedCompany) => {
          this.company = updatedCompany;
          this.successMessage = 'Company profile updated successfully';
          this.isUpdatingCompany = false;
          
          // Clear success message after 3 seconds
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (error) => {
          console.error('Error updating company:', error);
          this.errorMessage = error.message || 'Failed to update company profile';
          this.isUpdatingCompany = false;
        }
      });
  }

  sendInvitation() {
    if (this.inviteForm.invalid || !this.currentUser?.companyId) return;

    this.isSendingInvite = true;
    this.errorMessage = '';
    this.successMessage = '';

    const invitationData = {
      ...this.inviteForm.value,
      companyId: this.currentUser.companyId,
      roleId: this.inviteForm.value.roleId || undefined,
      teamId: this.inviteForm.value.teamId || undefined
    };

    this.companyService.sendUserInvitation(invitationData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invitation) => {
          this.successMessage = `Invitation sent to ${invitation.email} successfully!`;
          this.inviteForm.reset();
          this.isSendingInvite = false;
          
          // Refresh invitations
          this.companyService.getInvitations().subscribe(invitations => {
            this.invitations = invitations;
          });

          // Close modal after 2 seconds
          setTimeout(() => {
            this.closeInviteModal();
          }, 2000);
        },
        error: (error) => {
          console.error('Error sending invitation:', error);
          this.errorMessage = error.message || 'Failed to send invitation';
          this.isSendingInvite = false;
        }
      });
  }

  cancelInvitation(invitationId: number) {
    if (!confirm('Are you sure you want to cancel this invitation?')) return;

    this.companyService.cancelInvitation(invitationId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.invitations = this.invitations.filter(inv => inv.id !== invitationId);
          this.successMessage = 'Invitation cancelled successfully';
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          console.error('Error cancelling invitation:', error);
          this.errorMessage = error.message || 'Failed to cancel invitation';
        }
      });
  }

  resendInvitation(invitationId: number) {
    this.companyService.resendInvitation(invitationId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.successMessage = 'Invitation resent successfully';
          setTimeout(() => this.successMessage = '', 3000);
        },
        error: (error) => {
          console.error('Error resending invitation:', error);
          this.errorMessage = error.message || 'Failed to resend invitation';
        }
      });
  }

  editUser(user: CompanyUser) {
    // TODO: Implement user editing modal
    console.log('Edit user:', user);
  }

  removeUser(user: CompanyUser) {
    if (!confirm(`Are you sure you want to remove ${user.name} from the company?`)) return;

    console.log('Removing user:', user);

    this.companyService.removeUserFromCompany(user.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (success) => {
          console.log('Remove user result:', success);
          if (success) {
            this.users = this.users.filter(u => u.id !== user.id);
            this.successMessage = `${user.name} has been removed from the company`;
            setTimeout(() => this.successMessage = '', 3000);
          } else {
            this.errorMessage = 'Failed to remove user - operation returned false';
          }
        },
        error: (error) => {
          console.error('Error removing user:', error);
          this.errorMessage = error.message || 'Failed to remove user';
        }
      });
  }

  closeInviteModal() {
    this.showInviteModal = false;
    this.inviteForm.reset();
    this.errorMessage = '';
    this.successMessage = '';
  }

  getUserInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
  }

  getUserStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800';
      case 'inactive': return 'bg-gray-100 text-gray-800';
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getInvitationStatusClass(status: string): string {
    switch (status) {
      case 'PENDING': return 'bg-yellow-100 text-yellow-800';
      case 'ACCEPTED': return 'bg-green-100 text-green-800';
      case 'DECLINED': return 'bg-red-100 text-red-800';
      case 'EXPIRED': return 'bg-gray-100 text-gray-800';
      case 'CANCELLED': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric', 
      year: 'numeric' 
    });
  }
}