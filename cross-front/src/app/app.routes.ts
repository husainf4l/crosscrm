import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { FeaturesComponent } from './pages/features.component';
import { PricingComponent } from './pages/pricing.component';
import { AboutComponent } from './pages/about.component';
import { SignInComponent } from './auth/signin.component';
import { RegisterComponent } from './auth/register.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { CompanySetupComponent } from './company-setup/company-setup.component';
import { CustomersComponent } from './customers/customers.component';
import { CustomerCreateComponent } from './customers/customer-create.component';
import { CustomerDetailComponent } from './customers/customer-detail.component';
import { TeamManagementComponent } from './admin/team-management.component';
import { CompanyProfileComponent } from './company/company-profile.component';
import { AcceptInvitationComponent } from './company/accept-invitation.component';
import { CompanyHierarchyComponent } from './company/company-hierarchy.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'features', component: FeaturesComponent },
  { path: 'pricing', component: PricingComponent },
  { path: 'about', component: AboutComponent },
  { path: 'signin', component: SignInComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'accept-invitation/:token', component: AcceptInvitationComponent },
  { 
    path: 'dashboard', 
    component: DashboardComponent
  },
  { 
    path: 'customers', 
    component: DashboardComponent,
    children: [
      { path: '', component: CustomersComponent },
      { path: 'create', component: CustomerCreateComponent },
      { path: ':id', component: CustomerDetailComponent }
    ]
  },
  { 
    path: 'team-management', 
    component: DashboardComponent,
    children: [
      { path: '', component: TeamManagementComponent }
    ]
  },
  { 
    path: 'company', 
    component: DashboardComponent,
    children: [
      { path: 'profile', component: CompanyProfileComponent },
      { path: 'hierarchy', component: CompanyHierarchyComponent }
    ]
  },
  { path: 'company-setup', component: CompanySetupComponent },
  { path: '**', redirectTo: '' }
];
