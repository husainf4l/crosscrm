import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CustomerService, Customer, CustomerFilters, CustomerContact, CustomerAddress } from '../services/customer.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './customers.component.html',
  styleUrls: ['./customers.component.scss']
})
export class CustomersComponent implements OnInit, OnDestroy {
  customers: Customer[] = [];
  loading = false;
  error: string | null = null;
  searchTerm = '';
  totalCount = 0;
  hasNextPage = false;
  currentPage = 1;
  pageSize = 20;

  // Filters
  selectedCustomerTypes: string[] = [];
  selectedStatuses: string[] = [];
  selectedPriorities: string[] = [];
  selectedIndustries: string[] = [];

  // Single-select dropdown filters
  selectedTypeFilter = '';
  selectedStatusFilter = '';
  selectedPriorityFilter = '';

  // Filter options
  customerTypes = [
    { value: 'lead', label: 'Lead (Initial Contact)', color: 'bg-blue-100 text-blue-800' },
    { value: 'prospect', label: 'Prospect (Qualified)', color: 'bg-yellow-100 text-yellow-800' },
    { value: 'customer', label: 'Active Customer (Paying)', color: 'bg-green-100 text-green-800' },
    { value: 'client', label: 'Established Client', color: 'bg-emerald-100 text-emerald-800' },
    { value: 'enterprise', label: 'Enterprise Client', color: 'bg-purple-100 text-purple-800' },
    { value: 'partner', label: 'Business Partner', color: 'bg-indigo-100 text-indigo-800' },
    { value: 'dormant', label: 'Dormant (Previous)', color: 'bg-gray-100 text-gray-800' }
  ];

  statuses = [
    { value: 'active', label: 'Active (Engaged)', color: 'bg-green-100 text-green-800' },
    { value: 'purchasing', label: 'Currently Purchasing', color: 'bg-emerald-100 text-emerald-800' },
    { value: 'inactive', label: 'Inactive (Not Engaged)', color: 'bg-gray-100 text-gray-800' },
    { value: 'lost', label: 'Lost Customer', color: 'bg-orange-100 text-orange-800' },
    { value: 'blacklisted', label: 'Blacklisted', color: 'bg-red-100 text-red-800' }
  ];

  priorities = [
    { value: 'critical', label: 'Critical', color: 'bg-red-100 text-red-800' },
    { value: 'high', label: 'High', color: 'bg-orange-100 text-orange-800' },
    { value: 'medium', label: 'Medium', color: 'bg-yellow-100 text-yellow-800' },
    { value: 'low', label: 'Low', color: 'bg-gray-100 text-gray-800' }
  ];

  private subscriptions = new Subscription();

  constructor(
    private customerService: CustomerService,
    private router: Router
  ) {}

  async ngOnInit() {
    // Load customers
    this.loadCustomers();

    // Subscribe to customers updates
    this.subscriptions.add(
      this.customerService.customers$.subscribe(customers => {
        this.customers = customers;
      })
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadCustomers() {
    this.loading = true;
    this.error = null;

    const filters: CustomerFilters = {
      search: this.searchTerm || undefined,
      customerType: this.selectedCustomerTypes.length > 0 ? this.selectedCustomerTypes : undefined,
      status: this.selectedStatuses.length > 0 ? this.selectedStatuses : undefined,
      priority: this.selectedPriorities.length > 0 ? this.selectedPriorities : undefined,
      industry: this.selectedIndustries.length > 0 ? this.selectedIndustries : undefined
    };

    console.log('📊 Loading customers with filters:', filters);

    this.subscriptions.add(
      this.customerService.getCustomers(this.pageSize, undefined, filters).subscribe({
        next: (response) => {
          console.log('✅ Customers loaded:', {
            count: response.customers.length,
            totalCount: response.totalCount,
            customers: response.customers
          });
          this.customers = response.customers;
          this.totalCount = response.totalCount;
          this.hasNextPage = response.pageInfo.hasNextPage;
          this.loading = false;
        },
        error: (error) => {
          this.error = error.message || 'Failed to load customers';
          this.loading = false;
          console.error('Error loading customers:', error);
        }
      })
    );
  }

  onSearch(event: any) {
    this.searchTerm = event.target.value;
    this.debounceSearch();
  }

  private debounceSearch() {
    // Simple debounce implementation
    setTimeout(() => {
      this.loadCustomers();
    }, 300);
  }

  onTypeFilterChange() {
    this.selectedCustomerTypes = this.selectedTypeFilter ? [this.selectedTypeFilter] : [];
    this.loadCustomers();
  }

  onStatusFilterChange() {
    this.selectedStatuses = this.selectedStatusFilter ? [this.selectedStatusFilter] : [];
    this.loadCustomers();
  }

  onPriorityFilterChange() {
    this.selectedPriorities = this.selectedPriorityFilter ? [this.selectedPriorityFilter] : [];
    this.loadCustomers();
  }

  toggleFilter(filterArray: string[], value: string) {
    const index = filterArray.indexOf(value);
    if (index > -1) {
      filterArray.splice(index, 1);
    } else {
      filterArray.push(value);
    }
    
    console.log('🔄 Filter toggled:', {
      filterType: filterArray === this.selectedCustomerTypes ? 'customerType' :
                  filterArray === this.selectedStatuses ? 'status' :
                  filterArray === this.selectedPriorities ? 'priority' : 'industry',
      value,
      currentFilters: [...filterArray],
      allFilters: {
        customerType: this.selectedCustomerTypes,
        status: this.selectedStatuses,
        priority: this.selectedPriorities,
        industry: this.selectedIndustries
      }
    });
    
    this.loadCustomers();
  }

  clearFilters() {
    this.selectedCustomerTypes = [];
    this.selectedStatuses = [];
    this.selectedPriorities = [];
    this.selectedIndustries = [];
    this.selectedTypeFilter = '';
    this.selectedStatusFilter = '';
    this.selectedPriorityFilter = '';
    this.searchTerm = '';
    this.loadCustomers();
  }

  getCustomerTypeConfig(type: string) {
    return this.customerTypes.find(t => t.value === type) || { value: type, label: type, color: 'bg-gray-100 text-gray-800' };
  }

  getStatusConfig(status: string) {
    return this.statuses.find(s => s.value === status) || { value: status, label: status, color: 'bg-gray-100 text-gray-800' };
  }

  getPriorityConfig(priority: string) {
    return this.priorities.find(p => p.value === priority) || { value: priority, label: priority, color: 'bg-gray-100 text-gray-800' };
  }

  formatCurrency(amount: number | undefined): string {
    if (!amount) return '$0';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  formatDate(dateString: string | undefined): string {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString();
  }

  navigateToCustomer(customer: Customer) {
    this.router.navigate(['/customers', customer.id]);
  }

  navigateToCreateCustomer() {
    this.router.navigate(['/customers/create']);
  }

  getPrimaryContact(contacts?: CustomerContact[]): string {
    if (!contacts || contacts.length === 0) return '';
    const primary = contacts.find(c => c.isPrimary);
    return primary?.value || contacts[0]?.value || '';
  }

  getPrimaryAddress(addresses?: CustomerAddress[]): string {
    if (!addresses || addresses.length === 0) return '';
    const primary = addresses.find(a => a.isPrimary);
    if (primary) {
      return `${primary.city || ''}, ${primary.state || ''}`.replace(/^,\s*|,\s*$/, '');
    }
    const first = addresses[0];
    return `${first?.city || ''}, ${first?.state || ''}`.replace(/^,\s*|,\s*$/, '');
  }

  deleteCustomer(customer: Customer, event: Event) {
    event.stopPropagation();
    
    if (confirm(`Are you sure you want to delete ${customer.companyName}?`)) {
      this.subscriptions.add(
        this.customerService.deleteCustomer(customer.id!).subscribe({
          next: () => {
            console.log('Customer deleted successfully');
          },
          error: (error) => {
            alert('Failed to delete customer: ' + error.message);
          }
        })
      );
    }
  }
}