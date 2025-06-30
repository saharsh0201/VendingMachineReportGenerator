import { Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';

export interface ProcessingResult {
  id: string;
  totalTransactions: number;
  totalUsedAmount: number;
  refundedTransactions: number;
  fullyRefundedAmount: number;
  partiallyRefundedTransactions: number;
  partiallyRefundedAmount: number;
  totalRefundedAmount: number;
  machineWiseStats?: { [machineId: string]: MachineStats };
  resultFilePath?: string; 
  startDate?: string;
  endDate?: string;
}

export interface MachineStats {
  totalTransactions: number;
  totalUsedAmount: number;
  refundedTransactions: number;
  fullyRefundedAmount: number;
  partiallyRefundedTransactions: number;
  partiallyRefundedAmount: number;
  totalRefundedAmount: number;
}

@Component({
  selector: 'app-result-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './result-page.html',
  styleUrls: ['./result-page.css'],
  encapsulation: ViewEncapsulation.None
})
export class ResultPageComponent implements OnInit {
  result: ProcessingResult | null = null;
  loading = true;
  error = '';

  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    const navigation = history.state;

    if (navigation && navigation.result) {
      this.result = navigation.result;
      this.loading = false;
      return;
    }

    if (id) {
      this.http.get<ProcessingResult>(`/api/process/${id}`).subscribe({
        next: res => {
          this.result = res;
          this.loading = false;
        },
        error: err => {
          console.error('Failed to load result:', err);
          this.error = 'Failed to load result data.';
          this.loading = false;
        }
      });
    } else {
      this.error = 'Invalid result ID.';
      this.loading = false;
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR'
    }).format(value);
  }

  objectKeys(obj: Record<string, any> | undefined): string[] {
    return obj ? Object.keys(obj) : [];
  }
}
