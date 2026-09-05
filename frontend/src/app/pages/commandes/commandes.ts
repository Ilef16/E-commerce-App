import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { CommandeService } from '../../services/commande.service';
import { CommandeDto } from '../../dtos/commande.dto';

@Component({
  selector: 'app-commandes',
  imports: [CommonModule, PageHeader],
  template: `
    <app-page-header title="Commandes" subtitle="Suivi des commandes et du pipeline commercial." />
    @if (error()) { <p class="orders-error">{{ error() }}</p> }
    @if (loading()) { <p class="orders-empty">Chargement...</p> }
    @if (!loading()) {
      <div class="orders-table"><table><thead><tr><th>Numéro</th><th>Client</th><th>Date</th><th>Statut</th><th>Total</th><th>Actions</th></tr></thead><tbody>
        @for (order of orders(); track order.id) {
          <tr><td>{{ order.numero }}</td><td>{{ order.clientNom }}</td><td>{{ order.dateCommande | date : 'dd/MM/yyyy' }}</td><td>{{ statusLabel(order.statut) }}</td><td>{{ order.total | number : '1.2-2' }} TND</td><td><button type="button" (click)="details.set(order)">Détails</button>@if (order.statut === 0) { <button type="button" (click)="validate(order)">Valider</button><button class="danger" type="button" (click)="remove(order)">Supprimer</button> }</td></tr>
        } @empty { <tr><td colspan="6" class="orders-empty">Aucune commande.</td></tr> }
      </tbody></table></div>
    }
    @if (details(); as order) { <div class="orders-modal" (click)="details.set(null)"><section (click)="$event.stopPropagation()"><h2>{{ order.numero }}</h2><p>Client : {{ order.clientNom }}</p><p>Statut : {{ statusLabel(order.statut) }}</p>@for (line of order.lignes; track line.id) { <p>{{ line.libelleProduit }} x {{ line.quantite }} : {{ line.totalLigne | number : '1.2-2' }} TND</p> }<button type="button" (click)="details.set(null)">Fermer</button></section></div> }
  `,
})
export class Commandes implements OnInit {
  orders = signal<CommandeDto[]>([]); details = signal<CommandeDto | null>(null); loading = signal(false); error = signal('');
  constructor(private readonly commandeService: CommandeService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading.set(true); this.commandeService.getAll().subscribe({ next: o => { this.orders.set(o); this.loading.set(false); }, error: () => { this.error.set('Impossible de charger les commandes.'); this.loading.set(false); } }); }
  statusLabel(status: number): string { return ['Brouillon', 'Validée', 'Livrée', 'Annulée'][status] ?? 'Inconnu'; }
  validate(order: CommandeDto): void { this.commandeService.validate(order.id).subscribe({ next: () => this.load(), error: (e: HttpErrorResponse) => this.error.set(e.error?.detail ?? 'Impossible de valider la commande.') }); }
  remove(order: CommandeDto): void { this.commandeService.delete(order.id).subscribe({ next: () => this.load(), error: (e: HttpErrorResponse) => this.error.set(e.error?.detail ?? 'Impossible de supprimer la commande.') }); }
}
