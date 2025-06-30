import { Component } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class NavbarComponent {
  constructor(private auth: AuthService, private router: Router) {}

  async logout() {
    await this.auth.logout(); // ✅ Ensures cookie gets cleared on server
    this.router.navigate(['/login']);
  }
}
