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

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'features', component: FeaturesComponent },
  { path: 'pricing', component: PricingComponent },
  { path: 'about', component: AboutComponent },
  { path: 'signin', component: SignInComponent },
  { path: 'register', component: RegisterComponent },
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
  { path: 'company-setup', component: CompanySetupComponent },
  { path: '**', redirectTo: '' }
];
