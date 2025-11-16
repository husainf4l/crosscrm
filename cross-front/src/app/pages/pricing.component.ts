import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../components/layouts/navbar/navbar.component';

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [CommonModule, NavbarComponent],
  template: `
    <app-navbar></app-navbar>
    <div class="pt-12">
      <div class="container mx-auto px-4 py-8">
        <h1 class="text-3xl font-light text-gray-900 mb-4">Pricing</h1>
        <p class="text-gray-600">Coming soon...</p>
      </div>
    </div>
  `
})
export class PricingComponent {

}