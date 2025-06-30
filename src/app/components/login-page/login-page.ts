import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  standalone: true,
  selector: 'app-login-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './login-page.html',
  styleUrls: ['./login-page.css']
})
export class LoginPageComponent {
  username = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}
  ngOnInit() {
  if (this.auth.isLoggedIn()) {
    this.router.navigate(['/upload']);
  }
}

  async handleLogin() {
    
    this.loading = true;
    this.error = '';

    const success = await this.auth.login(this.username, this.password);
    this.loading = false;

    if (success) {
      this.router.navigate(['/upload']);
    } else {
      this.error = 'Invalid username or password';
    }
  }
}
