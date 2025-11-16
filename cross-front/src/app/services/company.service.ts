import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';

export interface Company {
  id: string;
  name: string;
  description?: string;
  website?: string;
  industry?: string;
  size?: string;
  address?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  logo?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CompanyUser {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  role?: string;
  status: 'active' | 'pending' | 'inactive';
  joinedAt?: string;
  lastLoginAt?: string;
  managerId?: string;
  manager?: {
    id: string;
    name: string;
    email: string;
  };
  directReports?: CompanyUser[];
  teams?: {
    id: string;
    name: string;
    role: string;
  }[];
}

export interface UserInvitation {
  id: number;
  email: string;
  invitationToken: string;
  companyId: number;
  companyName: string;
  invitedByUserId: number;
  invitedByUserName: string;
  roleId?: number;
  roleName?: string;
  teamId?: number;
  teamName?: string;
  status: 'PENDING' | 'ACCEPTED' | 'DECLINED' | 'EXPIRED' | 'CANCELLED';
  createdAt: string;
  expiresAt: string;
  acceptedAt?: string;
  acceptedByUserId?: number;
  acceptedByUserName?: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private companySubject = new BehaviorSubject<Company | null>(null);
  private usersSubject = new BehaviorSubject<CompanyUser[]>([]);
  private invitationsSubject = new BehaviorSubject<UserInvitation[]>([]);

  public company$ = this.companySubject.asObservable();
  public users$ = this.usersSubject.asObservable();
  public invitations$ = this.invitationsSubject.asObservable();

  constructor(private apiService: ApiService, private authService: AuthService) {}

  // ==================== COMPANY PROFILE ====================

  /**
   * Get company profile
   */
  getCompanyProfile(companyId: string): Observable<Company> {
    const query = `
      query GetCompany($id: Int!) {
        company(id: $id) {
          id
          name
          description
          website
          industry
          size
          address
          city
          country
          phone
          email
          logo
          createdAt
          updatedAt
        }
      }
    `;

    const variables = { id: parseInt(companyId) };

    return this.apiService.graphql<{ company: Company }>(query, variables).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to fetch company profile');
        }
        const company = response.data!.company;
        this.companySubject.next(company);
        return company;
      }),
      catchError(error => {
        console.error('Error fetching company profile:', error);
        return throwError(() => new Error(error.message || 'Failed to fetch company profile'));
      })
    );
  }

  /**
   * Update company profile
   */
  updateCompanyProfile(companyId: string, companyData: Partial<Company>): Observable<Company> {
    const mutation = `
      mutation UpdateCompany($id: Int!, $input: UpdateCompanyDtoInput!) {
        updateCompany(id: $id, input: $input) {
          id
          name
          description
          website
          industry
          size
          address
          city
          country
          phone
          email
          logo
          updatedAt
        }
      }
    `;

    return this.apiService.graphql<{ updateCompany: Company }>(
      mutation, 
      { id: parseInt(companyId), input: companyData }
    ).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to update company profile');
        }
        const updatedCompany = response.data!.updateCompany;
        this.companySubject.next(updatedCompany);
        return updatedCompany;
      }),
      catchError(error => {
        console.error('Error updating company profile:', error);
        return throwError(() => new Error(error.message || 'Failed to update company profile'));
      })
    );
  }

  // ==================== COMPANY USERS ====================

  /**
   * Get all users in the company
   */
  getCompanyUsers(): Observable<CompanyUser[]> {
    const query = `
      query GetCompanyUsers {
        users {
          id
          name
          email
          avatar
          role
          status
          joinedAt
          lastLoginAt
          managerId
          manager {
            id
            name
            email
            avatar
          }
          directReports {
            id
            name
            email
            avatar
          }
          teams {
            id
            teamId
            teamName
            name
            role
            isActive
            joinedAt
          }
        }
      }
    `;

    return this.apiService.graphql<{ users: CompanyUser[] }>(query).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return [];
        }
        const users = response.data?.users || [];
        this.usersSubject.next(users);
        return users;
      }),
      catchError(error => {
        console.error('Error fetching company users:', error);
        return of([]);
      })
    );
  }

  /**
   * Update user role or status
   */
  updateUser(userId: string, userData: Partial<CompanyUser>): Observable<CompanyUser> {
    const mutation = `
      mutation UpdateUser($id: Int!, $input: UpdateUserInput!) {
        updateUser(id: $id, input: $input) {
          id
          name
          email
          role
          status
          managerId
          manager {
            id
            name
            email
          }
        }
      }
    `;

    return this.apiService.graphql<{ updateUser: CompanyUser }>(
      mutation,
      { id: parseInt(userId), input: userData }
    ).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to update user');
        }
        const updatedUser = response.data!.updateUser;
        // Refresh users list
        this.getCompanyUsers().subscribe();
        return updatedUser;
      }),
      catchError(error => {
        console.error('Error updating user:', error);
        return throwError(() => new Error(error.message || 'Failed to update user'));
      })
    );
  }

  /**
   * Remove user from company
   */
  removeUserFromCompany(userId: string): Observable<boolean> {
    const mutation = `
      mutation RemoveUserFromCompany($userId: Int!, $companyId: Int!) {
        removeUserFromCompany(userId: $userId, companyId: $companyId)
      }
    `;

    // Get the current user's company ID from auth service
    const currentUser = this.authService.getCurrentUser();
    const companyId = currentUser?.companyId;

    if (!companyId) {
      console.error('No company ID found for current user:', currentUser);
      return throwError(() => new Error('No company ID found. Please ensure you are logged in with a company account.'));
    }

    console.log('Removing user:', { userId, companyId });

    return this.apiService.graphql<{ removeUserFromCompany: boolean }>(
      mutation,
      { userId: parseInt(userId), companyId: parseInt(companyId.toString()) }
    ).pipe(
      map(response => {
        console.log('Remove user response:', response);
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          throw new Error(response.errors[0]?.message || 'Failed to remove user');
        }
        const success = response.data!.removeUserFromCompany;
        if (success) {
          console.log('User removed successfully, refreshing users list');
          // Refresh users list
          this.getCompanyUsers().subscribe();
        }
        return success;
      }),
      catchError(error => {
        console.error('Error removing user from company:', error);
        return throwError(() => new Error(error.message || 'Failed to remove user from company'));
      })
    );
  }

  // ==================== USER INVITATIONS ====================

  /**
   * Send user invitation
   */
  sendUserInvitation(invitationData: {
    email: string;
    companyId: number;
    roleId?: number;
    teamId?: number;
    notes?: string;
  }): Observable<UserInvitation> {
    const mutation = `
      mutation InviteUser($input: InviteUserDtoInput!) {
        inviteUser(input: $input) {
          id
          email
          invitationToken
          companyId
          companyName
          invitedByUserId
          invitedByUserName
          roleId
          roleName
          teamId
          teamName
          status
          createdAt
          expiresAt
          notes
        }
      }
    `;

    // Ensure proper data types for GraphQL
    const input = {
      email: invitationData.email,
      companyId: invitationData.companyId,
      roleId: invitationData.roleId ? parseInt(invitationData.roleId.toString()) : undefined,
      teamId: invitationData.teamId ? parseInt(invitationData.teamId.toString()) : undefined,
      notes: invitationData.notes
    };

    return this.apiService.graphql<{ inviteUser: UserInvitation }>(mutation, { input }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to send invitation');
        }
        const invitation = response.data!.inviteUser;
        // Refresh invitations list
        this.getInvitations().subscribe();
        return invitation;
      }),
      catchError(error => {
        console.error('Error sending invitation:', error);
        return throwError(() => new Error(error.message || 'Failed to send invitation'));
      })
    );
  }

  /**
   * Get company invitations
   */
  getInvitations(): Observable<UserInvitation[]> {
    const query = `
      query GetCompanyInvitations {
        companyInvitations {
          id
          email
          status
          companyName
          invitedByUserName
          roleName
          teamName
          createdAt
          expiresAt
          acceptedAt
          acceptedByUserName
          notes
        }
      }
    `;

    return this.apiService.graphql<{ companyInvitations: UserInvitation[] }>(query).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return [];
        }
        const invitations = response.data?.companyInvitations || [];
        this.invitationsSubject.next(invitations);
        return invitations;
      }),
      catchError(error => {
        console.error('Error fetching invitations:', error);
        return of([]);
      })
    );
  }

  /**
   * Cancel invitation
   */
  cancelInvitation(invitationId: number): Observable<boolean> {
    const mutation = `
      mutation CancelInvitation($invitationId: Int!) {
        cancelInvitation(invitationId: $invitationId)
      }
    `;

    return this.apiService.graphql<{ cancelInvitation: boolean }>(
      mutation,
      { invitationId }
    ).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to cancel invitation');
        }
        const success = response.data!.cancelInvitation;
        if (success) {
          // Refresh invitations list
          this.getInvitations().subscribe();
        }
        return success;
      }),
      catchError(error => {
        console.error('Error canceling invitation:', error);
        return throwError(() => new Error(error.message || 'Failed to cancel invitation'));
      })
    );
  }

  /**
   * Resend invitation
   */
  resendInvitation(invitationId: number): Observable<boolean> {
    const mutation = `
      mutation ResendInvitation($invitationId: Int!) {
        resendInvitation(invitationId: $invitationId)
      }
    `;

    return this.apiService.graphql<{ resendInvitation: boolean }>(
      mutation,
      { invitationId }
    ).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to resend invitation');
        }
        const success = response.data!.resendInvitation;
        if (success) {
          // Refresh invitations list
          this.getInvitations().subscribe();
        }
        return success;
      }),
      catchError(error => {
        console.error('Error resending invitation:', error);
        return throwError(() => new Error(error.message || 'Failed to resend invitation'));
      })
    );
  }

  /**
   * Send test email
   */
  sendTestEmail(email: string): Observable<boolean> {
    const mutation = `
      mutation SendTestEmail($toEmail: String!) {
        sendTestEmail(toEmail: $toEmail)
      }
    `;

    return this.apiService.graphql<{ sendTestEmail: boolean }>(mutation, { toEmail: email }).pipe(
      map(response => {
        if (response.errors) {
          throw new Error(response.errors[0]?.message || 'Failed to send test email');
        }
        return response.data!.sendTestEmail;
      }),
      catchError(error => {
        console.error('Error sending test email:', error);
        return throwError(() => new Error(error.message || 'Failed to send test email'));
      })
    );
  }

  // ==================== HELPER METHODS ====================

  /**
   * Get available user roles
   */
  getUserRoles(): string[] {
    return ['admin', 'manager', 'member', 'viewer'];
  }

  /**
   * Get company industry options
   */
  getIndustryOptions(): string[] {
    return [
      'Technology',
      'Healthcare',
      'Finance',
      'Education',
      'Manufacturing',
      'Retail',
      'Construction',
      'Real Estate',
      'Transportation',
      'Energy',
      'Media',
      'Consulting',
      'Other'
    ];
  }

  /**
   * Get company size options
   */
  getCompanySizeOptions(): string[] {
    return [
      '1-10 employees',
      '11-50 employees',
      '51-200 employees',
      '201-500 employees',
      '501-1000 employees',
      '1000+ employees'
    ];
  }
}