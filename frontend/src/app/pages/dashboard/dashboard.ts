import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardDto } from '../../dtos/dashboard.dto';

type ApiState = 'checking' | 'online' | 'offline';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, PageHeader],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {

  private readonly dashboardService = inject(DashboardService);

  readonly apiState = signal<ApiState>('checking');
  readonly stats = signal<DashboardDto | null>(null);
  readonly statsError = signal(false);

  ngOnInit(): void {
    
  }

  private loadStats(): void {
    this.dashboardService.getStats().subscribe({
      next: (s) => this.stats.set(s),
      error: () => this.statsError.set(true),
    });
  }
}
