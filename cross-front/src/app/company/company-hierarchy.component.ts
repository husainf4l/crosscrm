import { Component, OnInit, signal, computed, ViewChild, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  NgxInteractiveOrgChart, 
  OrgChartNode, 
  NgxInteractiveOrgChartTheme,
  moveNode 
} from 'ngx-interactive-org-chart';
import { CompanyService } from '../services/company.service';
import { TeamService } from '../services/team.service';
import { AuthService } from '../services/auth.service';

interface TeamMember {
  id: string;
  name: string;
  email: string;
  position?: string;
  avatar?: string;
  isTeamLead?: boolean;
}

interface Team {
  id: string;
  name: string;
  description?: string;
  members: TeamMember[];
  children?: Team[];
}

interface CompanyData {
  name: string;
  teams: Team[];
  allUsers: any[];
}

@Component({
  selector: 'app-company-hierarchy',
  standalone: true,
  imports: [CommonModule, NgxInteractiveOrgChart],
  template: `
    <div class="min-h-screen" style="background: white;">
      <!-- Header -->
      <div style="background: rgba(255, 255, 255, 0.8); backdrop-filter: blur(20px); border-bottom: 1px solid rgba(0, 0, 0, 0.04);" class="px-8 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 style="color: #1d1d1f; font-weight: 600; font-size: 32px; letter-spacing: -0.02em; margin: 0;">Organization</h1>
            <p style="color: #86868b; margin: 6px 0 0 0; font-size: 18px;">Interactive company hierarchy</p>
          </div>
          
          <div class="flex items-center space-x-4">
            <!-- Controls -->
            <div class="flex items-center space-x-3">
              <button 
                (click)="resetView()"
                style="background: #007AFF; color: white; padding: 10px 20px; border-radius: 10px; font-weight: 500; font-size: 15px; border: none; cursor: pointer; transition: all 0.2s ease;">
                Reset View
              </button>
              
              <button 
                (click)="toggleMiniMap()"
                style="background: rgba(0, 0, 0, 0.04); color: #1d1d1f; padding: 10px 20px; border-radius: 10px; font-weight: 500; font-size: 15px; border: none; cursor: pointer; transition: all 0.2s ease;">
                {{ showMiniMap() ? 'Hide' : 'Show' }} Mini Map
              </button>
              
              <button 
                (click)="toggleCollapse()"
                style="background: rgba(0, 0, 0, 0.04); color: #1d1d1f; padding: 10px 20px; border-radius: 10px; font-weight: 500; font-size: 15px; border: none; cursor: pointer; transition: all 0.2s ease;">
                {{ allCollapsed() ? 'Expand All' : 'Collapse All' }}
              </button>
            </div>
            
            <!-- Search -->
            <div class="relative">
              <input
                #searchInput
                (input)="searchNode($event)"
                placeholder="Search members..."
                style="background: rgba(255, 255, 255, 0.8); border: 1px solid rgba(0, 0, 0, 0.1); border-radius: 12px; padding: 10px 14px 10px 42px; font-size: 15px; width: 280px; transition: all 0.2s ease;"
              >
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5" style="color: #86868b;" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                </svg>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Organization Chart -->
      <div class="flex-1 p-8">
        @if (isLoading()) {
          <div class="flex items-center justify-center h-96">
            <div class="flex flex-col items-center">
              <div style="width: 40px; height: 40px; border: 3px solid rgba(0, 122, 255, 0.2); border-top-color: #007AFF; border-radius: 50%; animation: spin 1s linear infinite;"></div>
              <p style="margin-top: 20px; color: #86868b; font-size: 17px;">Loading organization chart...</p>
            </div>
          </div>
        } @else if (error()) {
          <div class="flex items-center justify-center h-96">
            <div class="text-center">
              <div style="background: rgba(255, 59, 48, 0.1); border: 1px solid rgba(255, 59, 48, 0.2); color: #FF3B30; padding: 20px; border-radius: 16px; font-size: 15px;">
                {{ error() }}
              </div>
              <button 
                (click)="loadData()"
                style="margin-top: 20px; background: #007AFF; color: white; padding: 10px 20px; border-radius: 10px; font-weight: 500; border: none; cursor: pointer;">
                Retry
              </button>
            </div>
          </div>
        } @else if (orgChartData()) {
          <div style="background: rgba(255, 255, 255, 0.6); backdrop-filter: blur(20px); border-radius: 20px; box-shadow: 0 8px 40px rgba(0, 0, 0, 0.06); border: 1px solid rgba(0, 0, 0, 0.04); height: 850px; overflow: hidden;">
            <ngx-interactive-org-chart
              #orgChart
              [data]="orgChartData()!"
              [themeOptions]="themeOptions"
              [showMiniMap]="showMiniMap()"
              [miniMapPosition]="'bottom-right'"
              [miniMapWidth]="250"
              [miniMapHeight]="180"
              [collapsible]="true"
              [draggable]="true"
              [layout]="'vertical'"
              [initialZoom]="0.8"
              [minZoom]="0.2"
              [maxZoom]="3"
              [zoomSpeed]="1.2"
              [displayChildrenCount]="true"
              [highlightZoomNodeWidthRatio]="0.4"
              [highlightZoomNodeHeightRatio]="0.3"
              [highlightZoomMinimum]="1.0"
              [canDragNode]="canDragNode"
              [canDropNode]="canDropNode"
              (nodeDrop)="onNodeDrop($event)"
              (nodeClick)="onNodeClick($event)"
            >
              <!-- Custom Node Template -->
              <ng-template #nodeTemplate let-node="node">
                <div class="custom-node" [ngClass]="node.data?.type + '-node'">
                  @switch (node.data?.type) {
                    @case ('company') {
                      <div class="company-node-content">
                        <div class="flex items-center space-x-4">
                          <div class="company-icon">
                            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-4m-5 0H9m0 0H5m5 0v-4a1 1 0 011-1h2a1 1 0 011 1v4M7 7h10M7 11h10M7 15h10"></path>
                            </svg>
                          </div>
                          <div>
                            <h3 style="font-weight: 600; font-size: 20px; margin: 0; color: white;">{{ node.name }}</h3>
                            <p style="font-size: 15px; color: rgba(255, 255, 255, 0.8); margin: 4px 0 10px 0;">{{ node.data?.description || 'Company Overview' }}</p>
                            <div class="flex items-center space-x-6">
                              <span style="font-size: 13px; color: rgba(255, 255, 255, 0.9); font-weight: 500;">{{ node.data?.memberCount }} Members</span>
                              <span style="font-size: 13px; color: rgba(255, 255, 255, 0.9); font-weight: 500;">{{ node.data?.teamCount }} Teams</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    }
                    @case ('team') {
                      <div class="team-node-content">
                        <div class="flex items-center space-x-4">
                          <div class="team-icon">
                            <svg class="w-6 h-6" style="color: #007AFF;" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
                            </svg>
                          </div>
                          <div>
                            <h4 style="font-weight: 600; font-size: 17px; margin: 0; color: #1d1d1f;">{{ node.name }}</h4>
                            <p style="font-size: 15px; color: #86868b; margin: 3px 0 6px 0;">{{ node.data?.description || 'Team' }}</p>
                            <span style="font-size: 13px; color: #007AFF; font-weight: 500;">{{ node.data?.memberCount || 0 }} members</span>
                          </div>
                        </div>
                      </div>
                    }
                    @case ('member') {
                      <div class="member-node-content">
                        <div class="flex items-center space-x-4">
                          @if (node.data?.avatar) {
                            <img 
                              [src]="node.data.avatar" 
                              [alt]="node.name" 
                              style="width: 44px; height: 44px; border-radius: 22px; object-fit: cover; border: 2px solid rgba(0, 122, 255, 0.1);"
                            >
                          } @else {
                            <div style="width: 44px; height: 44px; border-radius: 22px; background: linear-gradient(135deg, #007AFF, #5856D6); display: flex; align-items: center; justify-content: center; color: white; font-weight: 600; font-size: 16px; border: 2px solid rgba(0, 122, 255, 0.1);">
                              {{ getInitials(node.name || '') }}
                            </div>
                          }
                          <div>
                            <h5 style="font-weight: 600; font-size: 16px; margin: 0; color: #1d1d1f;">{{ node.name }}</h5>
                            <p style="font-size: 14px; color: #86868b; margin: 3px 0;">{{ node.data?.position || 'Team Member' }}</p>
                            @if (node.data?.isTeamLead) {
                              <span style="font-size: 12px; background: rgba(52, 199, 89, 0.1); color: #34C759; padding: 4px 10px; border-radius: 14px; font-weight: 500;">Team Lead</span>
                            }
                          </div>
                        </div>
                      </div>
                    }
                  }
                </div>
              </ng-template>
            </ngx-interactive-org-chart>
          </div>
        } @else {
          <div class="flex items-center justify-center h-96">
            <div class="text-center">
              <svg class="w-24 h-24 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-4m-5 0H9m0 0H5m5 0v-4a1 1 0 011-1h2a1 1 0 011 1v4M7 7h10M7 11h10M7 15h10"></path>
              </svg>
              <h3 class="text-lg font-medium text-gray-900 mb-2">No Organization Data</h3>
              <p class="text-gray-600">Set up your company structure to see the hierarchy chart.</p>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100vh;
      background: white;
    }

    .custom-node {
      min-width: 220px;
      max-width: 320px;
      transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
    }

    .company-node-content {
      background: linear-gradient(135deg, #007AFF 0%, #5856D6 100%);
      color: white;
      padding: 24px;
      border-radius: 18px;
      box-shadow: 0 12px 40px rgba(0, 122, 255, 0.25);
      border: 1px solid rgba(255, 255, 255, 0.1);
      backdrop-filter: blur(20px);
    }

    .team-node-content {
      background: rgba(255, 255, 255, 0.8);
      backdrop-filter: blur(20px);
      color: #1d1d1f;
      padding: 20px;
      border-radius: 14px;
      box-shadow: 0 6px 24px rgba(0, 0, 0, 0.08);
      border: 1px solid rgba(0, 0, 0, 0.04);
    }

    .member-node-content {
      background: rgba(255, 255, 255, 0.95);
      backdrop-filter: blur(20px);
      padding: 16px;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.06);
      border: 1px solid rgba(0, 0, 0, 0.03);
      transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
    }

    .member-node-content:hover {
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
      border-color: rgba(0, 122, 255, 0.2);
      transform: translateY(-2px) scale(1.02);
    }

    .company-icon, .team-icon {
      background: rgba(255, 255, 255, 0.15);
      backdrop-filter: blur(10px);
      padding: 8px;
      border-radius: 8px;
    }

    /* Override default theme variables */
    :host ::ng-deep ngx-interactive-org-chart {
      --node-background: transparent;
      --node-color: inherit;
      --connector-color: rgba(0, 0, 0, 0.1);
      --connector-active-color: #007AFF;
      --connector-width: 1px;
      --node-outline-color: transparent;
      --node-active-outline-color: #007AFF;
    }

    /* Custom animations with Apple-like timing */
    @keyframes fadeInScale {
      from {
        opacity: 0;
        transform: scale(0.9) translateY(10px);
      }
      to {
        opacity: 1;
        transform: scale(1) translateY(0);
      }
    }

    @keyframes spin {
      from {
        transform: rotate(0deg);
      }
      to {
        transform: rotate(360deg);
      }
    }

    .custom-node {
      animation: fadeInScale 0.5s cubic-bezier(0.4, 0.0, 0.2, 1);
    }

    /* Apple-style typography */
    h1, h3, h4, h5 {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-weight: 600;
      letter-spacing: -0.01em;
    }

    p, span {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-weight: 400;
    }

    /* Button styling */
    button {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-weight: 500;
      border: none;
      border-radius: 8px;
      transition: all 0.2s cubic-bezier(0.4, 0.0, 0.2, 1);
    }

    button:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    }

    button:active {
      transform: translateY(0);
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }
  `]
})
export class CompanyHierarchyComponent implements OnInit {
  @ViewChild('orgChart') orgChart!: NgxInteractiveOrgChart<any>;

  // Signals for reactive state management
  isLoading = signal(true);
  error = signal<string | null>(null);
  showMiniMap = signal(true);
  allCollapsed = signal(false);

  // Data signals
  companyData = signal<CompanyData | null>(null);
  orgChartData = signal<OrgChartNode | null>(null);

  // Theme configuration with Apple-style design
  themeOptions: NgxInteractiveOrgChartTheme = {
    node: {
      background: 'transparent',
      color: 'inherit',
      shadow: '0 4px 20px rgba(0, 0, 0, 0.06)',
      outlineColor: 'transparent',
      outlineWidth: '0px',
      activeOutlineColor: '#007AFF',
      highlightShadowColor: '#007AFF',
      padding: '0px',
      borderRadius: '0px',
      maxWidth: '340px',
      minWidth: '240px'
    },
    connector: {
      color: 'rgba(0, 0, 0, 0.08)',
      activeColor: '#007AFF',
      borderRadius: '2px',
      width: '2px'
    },
    collapseButton: {
      size: '32px',
      borderColor: '#007AFF',
      borderRadius: '50%',
      color: '#007AFF',
      background: 'rgba(255, 255, 255, 0.9)',
      hoverColor: 'white',
      hoverBackground: '#007AFF',
      hoverShadow: '0 6px 24px rgba(0, 122, 255, 0.3)',
      hoverTransformScale: '1.15',
      focusOutline: '2px solid rgba(0, 122, 255, 0.3)',
      countFontSize: '13px'
    },
    container: {
      background: 'white',
      border: 'none'
    },
    miniMap: {
      background: 'rgba(255, 255, 255, 0.9)',
      borderColor: 'rgba(0, 0, 0, 0.08)',
      borderRadius: '18px',
      shadow: '0 12px 40px rgba(0, 0, 0, 0.08)',
      nodeColor: 'rgba(0, 122, 255, 0.6)',
      viewportBackground: 'rgba(0, 122, 255, 0.1)',
      viewportBorderColor: '#007AFF',
      viewportBorderWidth: '2px'
    }
  };

  // Drag & Drop constraints
  canDragNode = (node: OrgChartNode) => {
    // Allow dragging team members, but not company or team nodes
    return (node.data as any)?.type === 'member';
  };

  canDropNode = (draggedNode: OrgChartNode, targetNode: OrgChartNode) => {
    // Members can only be dropped on teams
    return (draggedNode.data as any)?.type === 'member' && (targetNode.data as any)?.type === 'team';
  };

  constructor(
    private companyService: CompanyService,
    private teamService: TeamService
    ,private authService: AuthService
  ) {
    // Auto-update org chart when company data changes
    effect(() => {
      const data = this.companyData();
      if (data) {
        this.orgChartData.set(this.transformToOrgChart(data));
      }
    });
  }

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    try {
      this.isLoading.set(true);
      this.error.set(null);

      const currentUser = this.authService.getCurrentUser();
      const companyId = currentUser?.companyId;

      if (!companyId) {
        throw new Error('No company ID found for current user');
      }

      const [companyProfile, companyUsers, teams] = await Promise.all([
        this.companyService.getCompanyProfile(companyId).toPromise(),
        this.companyService.getCompanyUsers().toPromise(),
        this.teamService.getTeams().toPromise()
      ]);

      if (companyProfile && companyUsers) {
        // Create user lookup map
        const userMap = new Map(companyUsers.map(user => [user.id, user]));
        
        let processedTeams: Team[] = [];

        if (teams && teams.length > 0) {
          // Use actual teams from API
          processedTeams = teams.map(team => ({
            id: team.id.toString(),
            name: team.name,
            description: team.description || `${team.type} Team`,
            members: team.members?.map((member: any) => {
              const user = userMap.get(member.userId) || {
                id: member.userId,
                name: member.userName,
                email: member.userEmail,
                avatar: null,
                role: member.role
              };
              
              return {
                id: user.id.toString(),
                name: user.name || member.userName,
                email: user.email || member.userEmail,
                position: member.role || user.role || 'Team Member',
                avatar: user.avatar || undefined,
                isTeamLead: member.role === 'MANAGER' || member.role === 'LEAD' || team.managerUserId === user.id
              };
            }) || []
          }));

          // Add team manager to members if not already present
          processedTeams.forEach(team => {
            const originalTeam = teams.find(t => t.id.toString() === team.id);
            if (originalTeam?.manager && originalTeam.managerUserId) {
              const managerExists = team.members.some(member => 
                member.id === originalTeam.managerUserId!.toString()
              );
              
              if (!managerExists) {
                team.members.unshift({
                  id: originalTeam.manager.id.toString(),
                  name: originalTeam.manager.name,
                  email: originalTeam.manager.email,
                  position: 'Team Manager',
                  avatar: userMap.get(originalTeam.manager.id)?.avatar || undefined,
                  isTeamLead: true
                });
              }
            }
          });

          // Find users not in any team
          const usersInTeams = new Set(
            processedTeams.flatMap(team => 
              team.members.map(member => parseInt(member.id))
            )
          );
          
          const unassignedUsers = companyUsers.filter(user => 
            !usersInTeams.has(parseInt(user.id.toString()))
          );

          if (unassignedUsers.length > 0) {
            processedTeams.push({
              id: 'unassigned',
              name: 'Unassigned Members',
              description: 'Members not assigned to any team',
              members: unassignedUsers.map(user => ({
                id: user.id.toString(),
                name: user.name,
                email: user.email,
                position: user.role || 'Team Member',
                avatar: user.avatar,
                isTeamLead: user.role === 'Admin' || user.role === 'Manager'
              }))
            });
          }
        } else {
          // Fallback: Create a default team with all users if no teams exist
          processedTeams.push({
            id: 'all-members',
            name: 'All Members',
            description: 'All company members',
            members: companyUsers.map(user => ({
              id: user.id.toString(),
              name: user.name,
              email: user.email,
              position: user.role || 'Team Member',
              avatar: user.avatar,
              isTeamLead: user.role === 'Admin' || user.role === 'Manager'
            }))
          });
        }

        const companyData: CompanyData = {
          name: companyProfile.name || 'Company',
          teams: processedTeams,
          allUsers: companyUsers
        };

        this.companyData.set(companyData);
      }
    } catch (error) {
      console.error('Failed to load organization data:', error);
      this.error.set('Failed to load organization data. Please try again.');
    } finally {
      this.isLoading.set(false);
    }
  }

  private transformToOrgChart(data: CompanyData): OrgChartNode {
    const teamNodes: OrgChartNode[] = data.teams.map(team => {
      const memberNodes: OrgChartNode[] = team.members.map(member => ({
        id: `member-${member.id}`,
        name: member.name,
        data: {
          type: 'member',
          email: member.email,
          position: member.position,
          avatar: member.avatar,
          isTeamLead: member.isTeamLead,
          originalId: member.id
        }
      }));

      return {
        id: `team-${team.id}`,
        name: team.name,
        data: {
          type: 'team',
          description: team.description,
          memberCount: team.members.length,
          originalId: team.id
        },
        children: memberNodes,
        collapsed: team.members.length > 5 // Auto-collapse large teams
      };
    });

    return {
      id: 'company',
      name: data.name,
      data: {
        type: 'company',
        description: `${data.teams.length} teams, ${data.allUsers.length} total members`,
        teamCount: data.teams.length,
        memberCount: data.allUsers.length
      },
      children: teamNodes
    };
  }

  // Event handlers
  onNodeDrop(event: { draggedNode: OrgChartNode; targetNode: OrgChartNode }) {
    const currentData = this.orgChartData();
    if (!currentData) return;

    // Move the node in the data structure
    const updatedData = moveNode(
      currentData,
      event.draggedNode.id!,
      event.targetNode.id!
    );

    if (updatedData) {
      this.orgChartData.set(updatedData);
      
      // Here you would sync with your backend
      // this.updateMemberTeam(event.draggedNode.data?.originalId, event.targetNode.data?.originalId);
      console.log('Member moved:', event.draggedNode.name, 'to team:', event.targetNode.name);
    }
  }

  onNodeClick(event: any) {
    const node = event as OrgChartNode;
    console.log('Node clicked:', node);
    
    if ((node.data as any)?.type === 'member') {
      // Show member details modal or navigate to profile
      console.log('Show member profile:', node.data);
    } else if ((node.data as any)?.type === 'team') {
      // Show team details or manage team
      console.log('Manage team:', node.data);
    }
  }

  // Control methods
  resetView() {
    if (this.orgChart) {
      this.orgChart.resetPanAndZoom(50);
    }
  }

  toggleMiniMap() {
    this.showMiniMap.update(show => !show);
  }

  toggleCollapse() {
    if (this.orgChart) {
      const collapsed = this.allCollapsed();
      this.orgChart.toggleCollapseAll(!collapsed);
      this.allCollapsed.set(!collapsed);
    }
  }

  searchNode(event: any) {
    const searchTerm = event.target.value.toLowerCase().trim();
    
    if (searchTerm && this.orgChart) {
      // Find nodes that match the search term
      const allNodes = this.flattenNodes(this.orgChartData());
      const matchedNode = allNodes.find(node => 
        node.name?.toLowerCase().includes(searchTerm) ||
        ((node.data as any)?.email?.toLowerCase().includes(searchTerm))
      );

      if (matchedNode && matchedNode.id) {
        this.orgChart.highlightNode(matchedNode.id);
      }
    }
  }

  private flattenNodes(node: OrgChartNode | null): OrgChartNode[] {
    if (!node) return [];
    
    const nodes = [node];
    if (node.children) {
      for (const child of node.children) {
        nodes.push(...this.flattenNodes(child));
      }
    }
    return nodes;
  }

  // Utility methods
  getInitials(name: string): string {
    return name
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}