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

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'features', component: FeaturesComponent },
  { path: 'pricing', component: PricingComponent },
  { path: 'about', component: AboutComponent },
  { path: 'signin', component: SignInComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'company-setup', component: CompanySetupComponent },
  { path: 'customers', component: CustomersComponent },
  { path: 'customers/create', component: CustomerCreateComponent },
  { path: 'customers/:id', component: CustomerDetailComponent },
  { path: '**', redirectTo: '' }
];
