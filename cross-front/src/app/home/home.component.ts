import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../components/layouts/navbar/navbar.component';
import { HeroComponent } from './hero.component';
import { ProblemSolutionComponent } from './problem-solution.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, ProblemSolutionComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {

}