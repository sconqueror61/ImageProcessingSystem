import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../../components/header/header.component'; 

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent], // HeaderComponent ekli olmalı!
  template: `
    <app-header></app-header> 
    <div class="container-fluid mt-3">
        <router-outlet></router-outlet>
    </div>
  `
})
export class UserLayoutComponent {}