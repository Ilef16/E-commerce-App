import { Component, inject, OnInit, signal } from '@angular/core';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { HealthService } from '../../services/health.service';

type ApiState = 'checking' | 'online' | 'offline';

@Component({
  selector: 'app-dashboard',
  imports: [PageHeader],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly health = inject(HealthService);

  readonly apiState = signal<ApiState>('checking');
  readonly cards = [
    { label: 'Clients', value: '—' },
    { label: 'Produits', value: '—' },
    { label: 'Commandes', value: '—' },
    { label: 'Factures', value: '—' },
  ];

  ngOnInit(): void {
    this.health.getStatus().subscribe({
      next: () => this.apiState.set('online'),
      error: () => this.apiState.set('offline'),
    });
  }
}
