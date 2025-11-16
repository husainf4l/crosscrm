import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { TeamService, Team, User, TeamMember } from '../services/team.service';

@Component({
  selector: 'app-team-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="w-full py-6 px-4 sm:px-6 lg:px-8">
      <!-- Header -->
      <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Team Management</h1>
        <p class="mt-1 text-sm text-gray-600">Manage teams, users, and organizational hierarchy</p>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200 mb-6">
        <nav class="-mb-px flex space-x-6">
          <button
            *ngFor="let tab of tabs"
            (click)="activeTab = tab.id"
            [class.border-blue-600]="activeTab === tab.id"
            [class.text-blue-600]="activeTab === tab.id"
            [class.border-transparent]="activeTab !== tab.id"
            [class.text-gray-600]="activeTab !== tab.id"
            class="whitespace-nowrap py-3 px-1 border-b-2 font-medium text-sm transition-colors">
            {{ tab.label }}
          </button>
        </nav>
      </div>

      <!-- Teams Tab -->
      <div *ngIf="activeTab === 'teams'" class="space-y-6">
        <!-- Create Team Button -->
        <div class="flex justify-end">
          <button
            (click)="showCreateTeamModal = true"
            class="inline-flex items-center px-4 py-2 text-sm font-medium rounded-xl text-white bg-blue-600 hover:bg-blue-700 transition-colors">
            <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
            </svg>
            Create Team
          </button>
        </div>

        <!-- Teams Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div
            *ngFor="let team of teams"
            class="bg-white border border-gray-200 rounded-xl p-4 hover:shadow-md transition-shadow">
            <div class="flex items-start justify-between mb-3">
              <div class="flex-1">
                <h3 class="text-sm font-semibold text-gray-900">{{ team.name }}</h3>
                <p class="text-xs text-gray-500 mt-1" *ngIf="team.description">{{ team.description }}</p>
              </div>
              <span
                [class]="getTeamTypeBadgeClass(team.type)"
                class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ml-2">
                {{ team.type }}
              </span>
            </div>

            <div class="space-y-2 text-xs">
              <div class="flex items-center text-gray-600">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                </svg>
                Manager: {{ team.manager?.name || 'None' }}
              </div>
              <div class="flex items-center text-gray-600">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2m0 0a3 3 0 015.356-1.857m0 0a3 3 0 015.356 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
                </svg>
                Members: {{ team.members?.length || 0 }}
              </div>
            </div>

            <div class="flex gap-2 mt-4 pt-3 border-t border-gray-100">
              <button
                (click)="selectTeamForEdit(team)"
                class="flex-1 px-3 py-1.5 text-xs font-medium text-blue-600 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors">
                Edit
              </button>
              <button
                (click)="selectTeamForMembers(team)"
                class="flex-1 px-3 py-1.5 text-xs font-medium text-gray-700 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
                Members
              </button>
              <button
                (click)="deleteTeamConfirm(team)"
                class="px-3 py-1.5 text-xs font-medium text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition-colors">
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Users Tab -->
      <div *ngIf="activeTab === 'users'" class="space-y-6">
        <!-- Users Table -->
        <div class="bg-white border border-gray-200 rounded-xl overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-gray-600 uppercase">Name</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-gray-600 uppercase">Email</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-gray-600 uppercase">Manager</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-gray-600 uppercase">Roles</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-gray-600 uppercase">Direct Reports</th>
                <th class="px-4 py-2.5 text-right text-xs font-semibold text-gray-600 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr *ngFor="let user of users" class="hover:bg-gray-50">
                <td class="px-4 py-2.5 text-xs font-medium text-gray-900">{{ user.name }}</td>
                <td class="px-4 py-2.5 text-xs text-gray-600">{{ user.email }}</td>
                <td class="px-4 py-2.5 text-xs text-gray-600">{{ user.manager?.name || '-' }}</td>
                <td class="px-4 py-2.5">
                  <div class="flex gap-1">
                    <span
                      *ngFor="let role of user.roles"
                      class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-purple-50 text-purple-700">
                      {{ role.roleName }}
                    </span>
                  </div>
                </td>
                <td class="px-4 py-2.5 text-xs text-gray-600">{{ user.directReports?.length || 0 }}</td>
                <td class="px-4 py-2.5 text-right">
                  <button
                    (click)="selectUserForEdit(user)"
                    class="text-xs text-blue-600 hover:text-blue-700 font-medium">
                    Edit Hierarchy
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Hierarchy Tab -->
      <div *ngIf="activeTab === 'hierarchy'" class="space-y-6">
        <div class="bg-white border border-gray-200 rounded-xl p-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Organizational Hierarchy</h3>
          <div class="space-y-3">
            <div *ngFor="let user of getTopLevelUsers()" class="space-y-2">
              <div [ngStyle]="{'margin-left': '0px'}" class="flex items-center gap-2">
                <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center text-white font-semibold text-xs">
                  {{ getUserInitials(user) }}
                </div>
                <div class="flex-1">
                  <div class="text-xs font-medium text-gray-900">{{ user.name }}</div>
                  <div class="text-xs text-gray-500">{{ user.email }}</div>
                </div>
                <span class="text-xs text-gray-500">{{ user.directReports?.length || 0 }} reports</span>
              </div>
              <div *ngFor="let report of user.directReports" [ngStyle]="{'margin-left': '40px'}" class="flex items-center gap-2">
                <div class="w-6 h-6 bg-gray-400 rounded-lg flex items-center justify-center text-white font-semibold text-xs">
                  {{ getUserInitials(report) }}
                </div>
                <div class="flex-1">
                  <div class="text-xs font-medium text-gray-900">{{ report.name }}</div>
                  <div class="text-xs text-gray-500">{{ report.email }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Assignments Tab -->
      <div *ngIf="activeTab === 'assignments'" class="space-y-6">
        <div class="bg-white border border-gray-200 rounded-xl p-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Customer Assignments</h3>
          <p class="text-xs text-gray-600">Manage customer assignments to teams and users from the Customers page.</p>
        </div>
      </div>
    </div>

    <!-- Create/Edit Team Modal -->
    <div
      *ngIf="showCreateTeamModal || showEditTeamModal"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-xl max-w-md w-full p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">
          {{ showCreateTeamModal ? 'Create Team' : 'Edit Team' }}
        </h3>
        <form (ngSubmit)="saveTeam()" class="space-y-4">
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Team Name</label>
            <input
              [(ngModel)]="teamForm.name"
              name="name"
              type="text"
              required
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Description</label>
            <textarea
              [(ngModel)]="teamForm.description"
              name="description"
              rows="3"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"></textarea>
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Type</label>
            <select
              [(ngModel)]="teamForm.type"
              name="type"
              required
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="SALES">Sales</option>
              <option value="SUPPORT">Support</option>
              <option value="MANAGEMENT">Management</option>
              <option value="CROSS_FUNCTIONAL">Cross Functional</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Team Manager</label>
            <select
              [(ngModel)]="teamForm.managerUserId"
              name="managerUserId"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option [value]="null">Select Manager</option>
              <option *ngFor="let user of users" [value]="user.id">{{ user.name }}</option>
            </select>
          </div>
          <div class="flex gap-3 pt-4">
            <button
              type="button"
              (click)="closeTeamModal()"
              class="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
              Cancel
            </button>
            <button
              type="submit"
              [disabled]="saving"
              class="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50">
              {{ saving ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit User Manager Modal -->
    <div
      *ngIf="showEditUserModal"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-xl max-w-md w-full p-6">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">Edit User Hierarchy</h3>
        <form (ngSubmit)="saveUserManager()" class="space-y-4">
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">User</label>
            <input
              [value]="selectedUser?.name"
              disabled
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg bg-gray-50">
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-700 mb-1">Reports To</label>
            <select
              [(ngModel)]="userForm.managerId"
              name="managerId"
              class="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option [value]="null">None (Top Level)</option>
              <option *ngFor="let user of users" [value]="user.id" [disabled]="user.id === selectedUser?.id">
                {{ user.name }}
              </option>
            </select>
          </div>
          <div class="flex gap-3 pt-4">
            <button
              type="button"
              (click)="closeUserModal()"
              class="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
              Cancel
            </button>
            <button
              type="submit"
              [disabled]="saving"
              class="flex-1 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50">
              {{ saving ? 'Saving...' : 'Save' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Team Members Modal -->
    <div
      *ngIf="showMembersModal && selectedTeam"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-xl max-w-2xl w-full p-6 max-h-[80vh] overflow-y-auto">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">
          {{ selectedTeam.name }} - Team Members
        </h3>

        <!-- Add Member Form -->
        <form (ngSubmit)="addMember()" class="mb-6 p-4 bg-gray-50 rounded-lg">
          <div class="flex gap-3">
            <select
              [(ngModel)]="memberForm.userId"
              name="userId"
              required
              class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="">Select User</option>
              <option *ngFor="let user of getAvailableUsers()" [value]="user.id">
                {{ user.name }} ({{ user.email }})
              </option>
            </select>
            <select
              [(ngModel)]="memberForm.role"
              name="role"
              required
              class="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500">
              <option value="MEMBER">Member</option>
              <option value="MANAGER">Manager</option>
              <option value="OBSERVER">Observer</option>
            </select>
            <button
              type="submit"
              [disabled]="!memberForm.userId || saving"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50">
              Add
            </button>
          </div>
        </form>

        <!-- Members List -->
        <div class="space-y-2">
          <div
            *ngFor="let member of selectedTeam.members"
            class="flex items-center justify-between p-3 bg-white border border-gray-200 rounded-lg">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center text-white font-semibold text-xs">
                {{ getUserInitials(member.user!) }}
              </div>
              <div>
                <div class="text-sm font-medium text-gray-900">{{ member.user?.name }}</div>
                <div class="text-xs text-gray-500">{{ member.user?.email }}</div>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <span
                [class]="getMemberRoleBadgeClass(member.role)"
                class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium">
                {{ member.role }}
              </span>
              <button
                (click)="removeMember(member)"
                class="p-1 text-red-600 hover:bg-red-50 rounded-lg transition-colors">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
              </button>
            </div>
          </div>
        </div>

        <div class="flex justify-end mt-6">
          <button
            (click)="closeMembersModal()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
            Close
          </button>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class TeamManagementComponent implements OnInit, OnDestroy {
  activeTab: 'teams' | 'users' | 'hierarchy' | 'assignments' = 'teams';
  teams: Team[] = [];
  users: User[] = [];
  loading = false;
  saving = false;
  private subscriptions = new Subscription();

  // Modals
  showCreateTeamModal = false;
  showEditTeamModal = false;
  showEditUserModal = false;
  showMembersModal = false;

  // Selected items
  selectedTeam: Team | null = null;
  selectedUser: User | null = null;

  // Forms
  teamForm: any = {
    name: '',
    description: '',
    type: 'SALES',
    managerUserId: null
  };

  userForm: any = {
    managerId: null
  };

  memberForm: any = {
    userId: '',
    role: 'MEMBER'
  };

  tabs: Array<{ id: 'teams' | 'users' | 'hierarchy' | 'assignments'; label: string }> = [
    { id: 'teams', label: 'Teams' },
    { id: 'users', label: 'Users' },
    { id: 'hierarchy', label: 'Hierarchy' },
    { id: 'assignments', label: 'Assignments' }
  ];

  constructor(private teamService: TeamService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadData(): void {
    this.loading = true;
    this.subscriptions.add(
      this.teamService.getTeams().subscribe(teams => {
        this.teams = teams;
        this.loading = false;
      })
    );

    this.subscriptions.add(
      this.teamService.getUsers().subscribe(users => {
        this.users = users;
      })
    );
  }

  // Team methods
  selectTeamForEdit(team: Team): void {
    this.selectedTeam = team;
    this.teamForm = {
      name: team.name,
      description: team.description,
      type: team.type,
      managerUserId: team.managerUserId
    };
    this.showEditTeamModal = true;
  }

  selectTeamForMembers(team: Team): void {
    this.selectedTeam = team;
    this.showMembersModal = true;
  }

  saveTeam(): void {
    this.saving = true;
    const input = {
      name: this.teamForm.name,
      description: this.teamForm.description,
      type: this.teamForm.type,
      managerUserId: this.teamForm.managerUserId ? parseInt(this.teamForm.managerUserId) : undefined,
      companyId: 1 // TODO: Get from auth service
    };

    const action = this.showCreateTeamModal
      ? this.teamService.createTeam(input)
      : this.teamService.updateTeam(this.selectedTeam!.id, input);

    action.subscribe({
      next: () => {
        this.saving = false;
        this.closeTeamModal();
        this.loadData();
      },
      error: () => {
        this.saving = false;
        alert('Failed to save team');
      }
    });
  }

  deleteTeamConfirm(team: Team): void {
    if (confirm(`Are you sure you want to delete team "${team.name}"?`)) {
      this.teamService.deleteTeam(team.id).subscribe({
        next: () => this.loadData(),
        error: () => alert('Failed to delete team')
      });
    }
  }

  closeTeamModal(): void {
    this.showCreateTeamModal = false;
    this.showEditTeamModal = false;
    this.selectedTeam = null;
    this.teamForm = { name: '', description: '', type: 'SALES', managerUserId: null };
  }

  // User methods
  selectUserForEdit(user: User): void {
    this.selectedUser = user;
    this.userForm = { managerId: user.managerId };
    this.showEditUserModal = true;
  }

  saveUserManager(): void {
    this.saving = true;
    this.teamService.updateUserManager(this.selectedUser!.id, this.userForm.managerId).subscribe({
      next: () => {
        this.saving = false;
        this.closeUserModal();
        this.loadData();
      },
      error: () => {
        this.saving = false;
        alert('Failed to update user hierarchy');
      }
    });
  }

  closeUserModal(): void {
    this.showEditUserModal = false;
    this.selectedUser = null;
    this.userForm = { managerId: null };
  }

  // Member methods
  getAvailableUsers(): User[] {
    const memberUserIds = this.selectedTeam?.members?.map(m => m.userId) || [];
    return this.users.filter(u => !memberUserIds.includes(u.id));
  }

  addMember(): void {
    if (!this.memberForm.userId) return;

    this.saving = true;
    const input = {
      teamId: parseInt(this.selectedTeam!.id),
      userId: parseInt(this.memberForm.userId),
      role: this.memberForm.role
    };

    this.teamService.addTeamMember(input).subscribe({
      next: () => {
        this.saving = false;
        this.memberForm = { userId: '', role: 'MEMBER' };
        this.loadData();
      },
      error: () => {
        this.saving = false;
        alert('Failed to add team member');
      }
    });
  }

  removeMember(member: TeamMember): void {
    if (confirm(`Remove ${member.user?.name} from team?`)) {
      this.teamService.removeTeamMember(member.teamId, member.userId).subscribe({
        next: () => this.loadData(),
        error: () => alert('Failed to remove team member')
      });
    }
  }

  closeMembersModal(): void {
    this.showMembersModal = false;
    this.selectedTeam = null;
    this.memberForm = { userId: '', role: 'MEMBER' };
  }

  // Helper methods
  getTopLevelUsers(): User[] {
    return this.users.filter(u => !u.managerId);
  }

  getUserInitials(user: User): string {
    return user.name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  getTeamTypeBadgeClass(type: string): string {
    const classes: Record<string, string> = {
      SALES: 'bg-blue-50 text-blue-700',
      SUPPORT: 'bg-green-50 text-green-700',
      MANAGEMENT: 'bg-purple-50 text-purple-700',
      CROSS_FUNCTIONAL: 'bg-orange-50 text-orange-700'
    };
    return classes[type] || 'bg-gray-50 text-gray-700';
  }

  getMemberRoleBadgeClass(role: string): string {
    const classes: Record<string, string> = {
      MANAGER: 'bg-purple-50 text-purple-700',
      MEMBER: 'bg-blue-50 text-blue-700',
      OBSERVER: 'bg-gray-50 text-gray-700'
    };
    return classes[role] || 'bg-gray-50 text-gray-700';
  }
}
