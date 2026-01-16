import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // <-- BU EKLENECEK (ngIf, ngClass için)
import { RouterModule } from '@angular/router'; // <-- BU EKLENECEK (routerLink için)
import { AuthService } from '../../auth.service'; // Yolunu kendine göre ayarla

@Component({
  selector: 'app-header',
  standalone: true,
  // imports dizisine dikkat et!
  imports: [CommonModule, RouterModule], 
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  userRole: string = '';

  constructor(private authService: AuthService) { }

  ngOnInit(): void {
      const role = this.authService.getUserRole();
      if(role) this.userRole = role.toLowerCase();
  }
  
  onLogout() {
      this.authService.logout();
  }
}