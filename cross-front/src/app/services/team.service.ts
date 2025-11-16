import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, map, of } from 'rxjs';
import { ApiService } from './api.service';

export interface User {
  id: string;
  name: string;
  email: string;
  managerId?: string;
  manager?: User;
  directReports?: User[];
  roles?: Array<{
    roleId: string;
    roleName: string;
    isActive: boolean;
    assignedAt: string;
  }>;
}

export interface Team {
  id: string;
  name: string;
  description?: string;
  type: 'SALES' | 'SUPPORT' | 'MANAGEMENT' | 'CROSS_FUNCTIONAL';
  managerUserId?: string;
  manager?: User;
  members?: TeamMember[];
  companyId: string;
  createdAt?: string;
}

export interface TeamMember {
  id: string;
  userId: string;
  teamId: string;
  role: 'MANAGER' | 'MEMBER' | 'OBSERVER';
  userName?: string;
  userEmail?: string;
  user?: User; // For backwards compatibility with component
  team?: Team;
}

export interface CustomerAssignment {
  customerId: string;
  assignedToTeamId?: string;
  assignedToUserId?: string;
  assignedTeam?: Team;
  assignedToUser?: User;
}

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private teamsSubject = new BehaviorSubject<Team[]>([]);
  public teams$ = this.teamsSubject.asObservable();

  private usersSubject = new BehaviorSubject<User[]>([]);
  public users$ = this.usersSubject.asObservable();

  constructor(private apiService: ApiService) {}

  // ==================== TEAM QUERIES ====================

  /**
   * Get all teams for the company
   */
  getTeams(): Observable<Team[]> {
    const query = `query GetTeams {
      teams {
        id
        name
        description
        type
        managerUserId
        manager {
          id
          name
          email
        }
        members {
          id
          userId
          teamId
          role
          userName
          userEmail
        }
        companyId
        createdAt
      }
    }`;

    return this.apiService.graphql<{ teams: Team[] }>(query).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return [];
        }
        const teams = response.data?.teams || [];
        this.teamsSubject.next(teams);
        return teams;
      }),
      catchError(error => {
        console.error('Error fetching teams:', error);
        return of([]);
      })
    );
  }

  /**
   * Get single team by ID with full details
   */
  getTeamById(id: string): Observable<Team | null> {
    const query = `query GetTeam($id: Int!) {
      team(id: $id) {
        id
        name
        description
        type
        managerUserId
        manager {
          id
          name
          email
        }
        members {
          id
          userId
          teamId
          role
          userName
          userEmail
        }
        companyId
        createdAt
      }
    }`;

    return this.apiService.graphql<{ team: Team }>(query, { id: parseInt(id) }).pipe(
      map(response => response.data?.team || null),
      catchError(error => {
        console.error('Error fetching team:', error);
        return of(null);
      })
    );
  }

  /**
   * Get teams managed by a specific user
   */
  getTeamsByManager(userId: string): Observable<Team[]> {
    const query = `query GetTeamsByManager($userId: Int!) {
      teams(where: { managerUserId: { eq: $userId } }) {
        id
        name
        description
        type
        members {
          id
          userId
          teamId
          role
          userName
          userEmail
        }
      }
    }`;

    return this.apiService.graphql<{ teams: Team[] }>(query, { userId: parseInt(userId) }).pipe(
      map(response => response.data?.teams || []),
      catchError(error => {
        console.error('Error fetching teams by manager:', error);
        return of([]);
      })
    );
  }

  // ==================== USER QUERIES ====================

  /**
   * Get all users in the company with hierarchy
   */
  getUsers(): Observable<User[]> {
    const query = `query GetUsers {
      users {
        id
        name
        email
        managerId
        manager {
          id
          name
          email
        }
        directReports {
          id
          name
          email
        }
        roles {
          roleId
          roleName
          isActive
          assignedAt
        }
      }
    }`;

    return this.apiService.graphql<{ users: User[] }>(query).pipe(
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
        console.error('Error fetching users:', error);
        return of([]);
      })
    );
  }

  /**
   * Get user by ID with hierarchy
   */
  getUserById(id: string): Observable<User | null> {
    const query = `query GetUser($id: Int!) {
      user(id: $id) {
        id
        name
        email
        managerId
        manager {
          id
          name
          email
        }
        directReports {
          id
          name
          email
        }
        roles {
          roleId
          roleName
          isActive
          assignedAt
        }
      }
    }`;

    return this.apiService.graphql<{ user: User }>(query, { id: parseInt(id) }).pipe(
      map(response => response.data?.user || null),
      catchError(error => {
        console.error('Error fetching user:', error);
        return of(null);
      })
    );
  }

  /**
   * Get all direct reports for a manager
   */
  getDirectReports(managerId: string): Observable<User[]> {
    const query = `query GetDirectReports($managerId: Int!) {
      users(where: { managerId: { eq: $managerId } }) {
        id
        name
        email
        roles {
          roleId
          roleName
          isActive
          assignedAt
        }
      }
    }`;

    return this.apiService.graphql<{ users: User[] }>(query, { managerId: parseInt(managerId) }).pipe(
      map(response => response.data?.users || []),
      catchError(error => {
        console.error('Error fetching direct reports:', error);
        return of([]);
      })
    );
  }

  // ==================== TEAM MUTATIONS ====================

  /**
   * Create a new team
   */
  createTeam(input: {
    name: string;
    description?: string;
    type: string;
    managerUserId?: number;
    companyId: number;
  }): Observable<Team | null> {
    const mutation = `mutation CreateTeam($input: CreateTeamInput!) {
      createTeam(input: $input) {
        id
        name
        description
        type
        managerUserId
        manager {
          id
          name
          email
        }
        companyId
      }
    }`;

    return this.apiService.graphql<{ createTeam: Team }>(mutation, { input }).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return null;
        }
        const team = response.data?.createTeam;
        if (team) {
          // Refresh teams list
          this.getTeams().subscribe();
        }
        return team || null;
      }),
      catchError(error => {
        console.error('Error creating team:', error);
        return of(null);
      })
    );
  }

  /**
   * Update team details
   */
  updateTeam(id: string, input: {
    name?: string;
    description?: string;
    type?: string;
    managerUserId?: number;
  }): Observable<Team | null> {
    const mutation = `mutation UpdateTeam($id: Int!, $input: UpdateTeamInput!) {
      updateTeam(id: $id, input: $input) {
        id
        name
        description
        type
        managerUserId
        manager {
          id
          name
          email
        }
      }
    }`;

    return this.apiService.graphql<{ updateTeam: Team }>(mutation, { id: parseInt(id), input }).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return null;
        }
        const team = response.data?.updateTeam;
        if (team) {
          this.getTeams().subscribe();
        }
        return team || null;
      }),
      catchError(error => {
        console.error('Error updating team:', error);
        return of(null);
      })
    );
  }

  /**
   * Delete team
   */
  deleteTeam(id: string): Observable<boolean> {
    const mutation = `mutation DeleteTeam($id: Int!) {
      deleteTeam(id: $id)
    }`;

    return this.apiService.graphql<{ deleteTeam: boolean }>(mutation, { id: parseInt(id) }).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return false;
        }
        const success = response.data?.deleteTeam || false;
        if (success) {
          this.getTeams().subscribe();
        }
        return success;
      }),
      catchError(error => {
        console.error('Error deleting team:', error);
        return of(false);
      })
    );
  }

  // ==================== TEAM MEMBER MUTATIONS ====================

  /**
   * Add member to team
   */
  addTeamMember(input: {
    teamId: number;
    userId: number;
    role: string;
  }): Observable<TeamMember | null> {
    const mutation = `mutation AddTeamMember($input: AddTeamMemberInput!) {
      addTeamMember(input: $input) {
        id
        userId
        teamId
        role
        userName
        userEmail
      }
    }`;

    return this.apiService.graphql<{ addTeamMember: TeamMember }>(mutation, { input }).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return null;
        }
        return response.data?.addTeamMember || null;
      }),
      catchError(error => {
        console.error('Error adding team member:', error);
        return of(null);
      })
    );
  }

  /**
   * Remove member from team
   */
  removeTeamMember(teamId: string, userId: string): Observable<boolean> {
    const mutation = `mutation RemoveTeamMember($teamId: Int!, $userId: Int!) {
      removeTeamMember(teamId: $teamId, userId: $userId)
    }`;

    return this.apiService.graphql<{ removeTeamMember: boolean }>(
      mutation, 
      { teamId: parseInt(teamId), userId: parseInt(userId) }
    ).pipe(
      map(response => response.data?.removeTeamMember || false),
      catchError(error => {
        console.error('Error removing team member:', error);
        return of(false);
      })
    );
  }

  /**
   * Update team member role
   */
  updateTeamMemberRole(teamId: string, userId: string, role: string): Observable<TeamMember | null> {
    const mutation = `mutation UpdateTeamMemberRole($teamId: Int!, $userId: Int!, $role: TeamMemberRole!) {
      updateTeamMemberRole(teamId: $teamId, userId: $userId, role: $role) {
        id
        userId
        teamId
        role
      }
    }`;

    return this.apiService.graphql<{ updateTeamMemberRole: TeamMember }>(
      mutation,
      { teamId: parseInt(teamId), userId: parseInt(userId), role }
    ).pipe(
      map(response => response.data?.updateTeamMemberRole || null),
      catchError(error => {
        console.error('Error updating team member role:', error);
        return of(null);
      })
    );
  }

  // ==================== USER HIERARCHY MUTATIONS ====================

  /**
   * Update user's manager (set reporting hierarchy)
   */
  updateUserManager(userId: string, managerId: string | null): Observable<User | null> {
    const mutation = `mutation UpdateUserManager($userId: Int!, $managerId: Int) {
      updateUser(input: { id: $userId, managerId: $managerId }) {
        id
        name
        email
        managerId
        manager {
          id
          name
          email
        }
      }
    }`;

    return this.apiService.graphql<{ updateUser: User }>(
      mutation,
      { userId: parseInt(userId), managerId: managerId ? parseInt(managerId) : null }
    ).pipe(
      map(response => {
        if (response.errors) {
          console.error('GraphQL errors:', response.errors);
          return null;
        }
        const user = response.data?.updateUser;
        if (user) {
          this.getUsers().subscribe();
        }
        return user || null;
      }),
      catchError(error => {
        console.error('Error updating user manager:', error);
        return of(null);
      })
    );
  }

  // ==================== CUSTOMER ASSIGNMENT QUERIES ====================

  /**
   * Get customers assigned to a team
   */
  getCustomersByTeam(teamId: string): Observable<any[]> {
    const query = `query GetCustomersByTeam($teamId: Int!) {
      customers(where: { assignedToTeamId: { eq: $teamId } }) {
        id
        name
        assignedToUserId
        assignedToUser {
          id
          name
          email
        }
      }
    }`;

    return this.apiService.graphql<{ customers: any[] }>(query, { teamId: parseInt(teamId) }).pipe(
      map(response => response.data?.customers || []),
      catchError(error => {
        console.error('Error fetching customers by team:', error);
        return of([]);
      })
    );
  }

  /**
   * Get customers assigned to a user
   */
  getCustomersByUser(userId: string): Observable<any[]> {
    const query = `query GetCustomersByUser($userId: Int!) {
      customers(where: { assignedToUserId: { eq: $userId } }) {
        id
        name
        assignedToTeamId
        assignedTeam {
          id
          name
        }
      }
    }`;

    return this.apiService.graphql<{ customers: any[] }>(query, { userId: parseInt(userId) }).pipe(
      map(response => response.data?.customers || []),
      catchError(error => {
        console.error('Error fetching customers by user:', error);
        return of([]);
      })
    );
  }

  // ==================== HELPER METHODS ====================

  /**
   * Get all users under a manager (recursive)
   */
  getAllReportsUnder(managerId: string, allUsers: User[]): User[] {
    const directReports = allUsers.filter(u => u.managerId === managerId);
    const allReports = [...directReports];

    directReports.forEach(report => {
      const subReports = this.getAllReportsUnder(report.id, allUsers);
      allReports.push(...subReports);
    });

    return allReports;
  }

  /**
   * Build user hierarchy tree
   */
  buildHierarchyTree(users: User[]): User[] {
    const topLevelUsers = users.filter(u => !u.managerId);
    return topLevelUsers;
  }
}
