import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardDto } from '../../dtos/dashboard.dto';
import { LOW_STOCK_THRESHOLD } from '../../shared/constants/business.constants';

type ApiState = 'online' | 'offline' | 'loading';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, PageHeader],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly dashboardService = inject(DashboardService);

  readonly lowStockThreshold = LOW_STOCK_THRESHOLD;
  readonly today = new Date();
  readonly apiState = signal<ApiState>('loading');
  readonly stats = signal<DashboardDto | null>(null);

  readonly hasStockAlerts = computed(() => {
    const current = this.stats();
    return !!current && (current.produitsRupture > 0 || current.produitsStockFaible > 0);
  });

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.apiState.set('online');
      },
      error: () => this.apiState.set('offline'),
    });
  }

  share(count: number): number {
    const total = this.stats()?.totalCommandes ?? 0;
    return total === 0 ? 0 : Math.round((count / total) * 100);
  }
}
