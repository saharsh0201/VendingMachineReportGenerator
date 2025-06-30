import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:5001/api/auth';

  constructor(private http: HttpClient) {}

  async login(username: string, password: string): Promise<boolean> {
    try {
      const response: any = await this.http
        .post(`${this.apiUrl}/login`, { username, password }, { withCredentials: true }) // ✅ Send cookie
        .toPromise();

      if (response?.success) {
        sessionStorage.setItem('loggedIn', 'true');
        return true;
      }
      return false;
    } catch (error) {
      return false;
    }
  }

  logout(): Promise<void> {
    sessionStorage.removeItem('loggedIn');
    return this.http
      .post(`${this.apiUrl}/logout`, {}, { withCredentials: true }) // ✅ Ensure logout sends cookie too
      .toPromise()
      .then(() => {});
  }

  isLoggedIn(): boolean {
    return sessionStorage.getItem('loggedIn') === 'true';
  }
}
