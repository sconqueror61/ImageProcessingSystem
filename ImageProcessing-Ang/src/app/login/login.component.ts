import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  mode: 'login' | 'register' | 'createTenant' = 'login';
  tenants: any[] = [];
  
  // DÜZELTME: Backend 'Name' ve 'Surname' istiyor!
  user = { email: '', password: '', name: '', surname: '', tanetId: '', role: 'User' };
  
  tenant = { name: '', address: '' }; 

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.loadTenants();
  }

  loadTenants() {
    this.authService.getTenants().subscribe({
      next: (data) => this.tenants = data,
      error: () => console.error('Failed to load tenants.')
    });
  }

  setMode(targetMode: 'login' | 'register' | 'createTenant') {
    this.mode = targetMode;
  }

  onSubmit() {
    // --- 1. LOGIN ---
    if (this.mode === 'login') {
      this.authService.login({ email: this.user.email, password: this.user.password }).subscribe({
        next: (res) => {
          this.authService.redirectBasedOnRole(); 
        },
        error: (err) => {
          alert('Login Failed: ' + (err.error?.message || 'Invalid credentials.'));
        }
      });
    } 

    // --- 2. REGISTER (Düzeltildi) ---
    else if (this.mode === 'register') { 
      
      if (!this.user.tanetId) {
        alert("Please select a company!");
        return;
      }
      // İsim Soyisim kontrolü
      if (!this.user.name || !this.user.surname) {
         alert("Name and Surname are required!");
         return;
      }

      this.authService.register(this.user).subscribe({
        next: (res) => { 
          alert('Registration Successful! Please login.'); 
          this.setMode('login'); 
        },
        error: (err) => {
          console.error(err);
          // Hatanın detayını göster
          alert('Registration Error: ' + JSON.stringify(err.error?.errors || err.error));
        }
      });
    }

    // --- 3. CREATE TENANT ---
    else if (this.mode === 'createTenant') {
      if(!this.tenant.name || !this.tenant.address) {
          alert("Enter company name and address.");
          return;
      }
      this.authService.createTenant(this.tenant).subscribe({
        next: (res) => {
          alert(`"${this.tenant.name}" created!`);
          this.loadTenants(); 
          if(res && res.id) this.user.tanetId = res.id; 
          this.setMode('register'); 
        },
        error: (err) => {
          alert('Error: ' + (err.error?.message || 'Unknown error'));
        }
      });
    }
  }
}