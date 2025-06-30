import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-upload-form',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './upload-form.html',
  styleUrls: ['./upload-form.css']
})
export class UploadFormComponent {
  empFileName = 'No file chosen';
  tlistFileName = 'No file(s) chosen';

  empFile: File | null = null;
  tlistFiles: File[] = [];
  errors: string[] = [];
  loading = false;

  constructor(private http: HttpClient, private router: Router) {}

  onEmpFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    this.empFile = input.files?.[0] || null;
    this.empFileName = this.empFile ? this.empFile.name : 'No file chosen';
  }

  onTlistFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files || []);

    this.errors = [];

    if (files.length > 3) {
      this.errors.push('You can upload a maximum of 3 TList files.');
      input.value = '';
      this.tlistFiles = [];
      this.tlistFileName = 'No file(s) chosen';
      return;
    }

    this.tlistFiles = files;

    if (files.length === 0) {
      this.tlistFileName = 'No file(s) chosen';
    } else if (files.length === 1) {
      this.tlistFileName = files[0].name;
    } else {
      this.tlistFileName = `${files.length} files selected`;
    }
  }

  onSubmit() {
    this.errors = [];

    if (!this.empFile) {
      this.errors.push('Employee CSV is required.');
      return;
    }

    if (this.tlistFiles.length === 0) {
      this.errors.push('At least one TList file is required.');
      return;
    }

    const formData = new FormData();
    formData.append('empFile', this.empFile);
    this.tlistFiles.forEach(file => formData.append('tlistFiles', file));

    this.loading = true;

    this.http.post<any>(
      'https://localhost:5001/api/process/match', // You can replace with relative if using proxy
      formData,
      { withCredentials: true } // ✅ Send cookies to backend
    ).subscribe({
      next: (res) => {
        this.loading = false;
        if (res?.result?.id) {
          this.router.navigate(['/result', res.result.id], {
            state: { result: res.result }
          });
        } else {
          this.errors.push('Unexpected server response.');
        }
      },
      error: (err) => {
        console.error('Upload failed:', err);
        this.loading = false;
        this.errors.push('Failed to upload files. Please try again.');
      }
    });
  }
}
