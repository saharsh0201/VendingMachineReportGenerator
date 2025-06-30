import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';

interface ProcessingResult {
  id: string;
  startDate: string;
  endDate: string;
  totalTransactions: number;
  totalUsedAmount: number;
  refundedTransactions: number;
  fullyRefundedAmount: number;
  partiallyRefundedTransactions: number;
  partiallyRefundedAmount: number;
  totalRefundedAmount: number;
  resultFilePath: string;
}

@Component({
  selector: 'app-history-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './history-page.html',
  styleUrls: ['./history-page.css']
})
export class HistoryPageComponent implements OnInit {
  private http = inject(HttpClient);

  results: ProcessingResult[] = [];
  fromDate: string = '';
  toDate: string = '';
  loading = false;

  ngOnInit(): void {
    this.fetchData();
  }

  fetchData(): void {
    this.loading = true;
    let url = `/api/process/history`;
    const params: string[] = [];

    if (this.fromDate) params.push(`from=${this.fromDate}`);
    if (this.toDate) params.push(`to=${this.toDate}`);
    if (params.length) url += `?${params.join('&')}`;

    this.http.get<ProcessingResult[]>(url).subscribe({
      next: data => {
        this.results = data;
        this.loading = false;
      },
      error: err => {
        console.error('Error loading history:', err);
        this.loading = false;
      }
    });
  }

  deleteRecord(id: string): void {
    if (!confirm('Are you sure you want to delete this report?')) return;
    this.http.delete(`/api/process/${id}`).subscribe({
      next: () => this.fetchData(),
      error: err => console.error('Delete failed:', err)
    });
  }

  downloadCsv(id: string): void {
    const url = `/api/process/download/${id}`;
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: blob => {
        const a = document.createElement('a');
        a.href = window.URL.createObjectURL(blob);
        a.download = `${id}_matched.csv`;
        a.click();
      },
      error: err => {
        console.error('Download failed:', err);
        alert('Download failed. The file may no longer exist.');
      }
    });
  }
}
