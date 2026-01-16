import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from '../../components/header/header.component'; // Aynı header'ı kullanıyoruz

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, HeaderComponent], 
  template: `
    <app-header></app-header>
    
    <div class="container-fluid mt-4"> <div class="alert alert-danger" role="alert">
          ADMIN PANEL MODU
       </div>
      <router-outlet></router-outlet>
    </div>
  `
})
export class AdminLayoutComponent {}