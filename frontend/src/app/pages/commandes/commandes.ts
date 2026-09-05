import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { CommandeService } from '../../services/commande.service';
import { ClientService } from '../../services/client.service';
import { ProduitService } from '../../services/produit.service';
import { CommandeDto, CommandeLigneWriteDto, CommandeWriteDto } from '../../dtos/commande.dto';
import { ClientDto } from '../../dtos/client.dto';
import { ProduitDto } from '../../dtos/produit.dto';
import { ORDER_STATUS_LABELS } from '../../shared/constants/tunisia.constants';

interface LigneDraft {
  produit: ProduitDto | null;
  quantite: number;
}

@Component({
  selector: 'app-commandes',
  imports: [CommonModule, FormsModule, PageHeader],
  templateUrl: './commandes.html',
  styleUrl: './commandes.scss',
})
export class Commandes implements OnInit {
  // ── List state ─────────────────────────────────
  orders = signal<CommandeDto[]>([]);
  totalCount = signal(0);
  page = signal(1);
  readonly pageSize = 10;
  loading = signal(false);
  error = signal('');

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  // ── Detail modal ───────────────────────────────
  details = signal<CommandeDto | null>(null);

  // ── Delete confirm ─────────────────────────────
  pendingDelete = signal<CommandeDto | null>(null);

  // ── Create modal ───────────────────────────────
  creating = signal(false);
  clients = signal<ClientDto[]>([]);
  produits = signal<ProduitDto[]>([]);
  selectedClientId = signal<number | null>(null);
  lignes = signal<LigneDraft[]>([{ produit: null, quantite: 1 }]);
  createError = signal('');
  createSubmitted = signal(false);

  // ── Computed totals (create form preview) ─────
  readonly TVA_RATE = 0.19;

  draftTotalHt = computed(() =>
    this.lignes().reduce((sum, l) =>
      sum + (l.produit ? l.produit.prixUnitaire * l.quantite : 0), 0)
  );

  draftTva = computed(() =>
    Math.round(this.draftTotalHt() * this.TVA_RATE * 100) / 100
  );

  draftTotalTtc = computed(() =>
    Math.round((this.draftTotalHt() + this.draftTva()) * 100) / 100
  );

  readonly statusLabels = ORDER_STATUS_LABELS;

  constructor(
    private readonly commandeService: CommandeService,
    private readonly clientService: ClientService,
    private readonly produitService: ProduitService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.commandeService.getAll({ page: this.page(), pageSize: this.pageSize }).subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => { this.error.set('Impossible de charger les commandes.'); this.loading.set(false); },
    });
  }

  goToPage(n: number): void {
    if (n < 1 || n > this.totalPages()) return;
    this.page.set(n);
    this.load();
  }

  pages(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  statusLabel(status: number): string {
    return ORDER_STATUS_LABELS[status] ?? 'Inconnu';
  }

  statusClass(status: number): string {
    return ['badge-draft', 'badge-validated', 'badge-delivered', 'badge-cancelled'][status] ?? '';
  }

  // ── Create ─────────────────────────────────────
  openCreate(): void {
    this.creating.set(true);
    this.createError.set('');
    this.createSubmitted.set(false);
    this.selectedClientId.set(null);
    this.lignes.set([{ produit: null, quantite: 1 }]);

    this.clientService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (r) => this.clients.set(r.items),
    });
    this.produitService.getAll(1, 100).subscribe({
      next: (r) => this.produits.set(r.items),
    });
  }

  closeCreate(): void {
    this.creating.set(false);
  }

  addLigne(): void {
    this.lignes.update((lines) => [...lines, { produit: null, quantite: 1 }]);
  }

  removeLigne(index: number): void {
    this.lignes.update((lines) => lines.filter((_, i) => i !== index));
  }

  setLigneProduit(index: number, produitId: string): void {
    const produit = this.produits().find((p) => p.id === +produitId) ?? null;
    this.lignes.update((lines) =>
      lines.map((l, i) => (i === index ? { ...l, produit } : l))
    );
  }

  setLigneQuantite(index: number, qty: number): void {
    this.lignes.update((lines) =>
      lines.map((l, i) => (i === index ? { ...l, quantite: qty } : l))
    );
  }

  submitCreate(): void {
    this.createSubmitted.set(true);
    this.createError.set('');

    const clientId = this.selectedClientId();
    const lignes = this.lignes();

    if (!clientId) { this.createError.set('Veuillez sélectionner un client.'); return; }
    if (lignes.some((l) => !l.produit)) { this.createError.set('Chaque ligne doit avoir un produit sélectionné.'); return; }
    if (lignes.some((l) => l.quantite < 1)) { this.createError.set('La quantité doit être au minimum 1.'); return; }
    if (lignes.some((l) => l.produit && l.quantite > l.produit.stock)) {
      this.createError.set('La quantité demandée dépasse le stock disponible pour un ou plusieurs produits.');
      return;
    }

    const payload: CommandeWriteDto = {
      clientId,
      lignes: lignes.map((l) => ({ produitId: l.produit!.id, quantite: l.quantite } as CommandeLigneWriteDto)),
    };

    this.commandeService.create(payload).subscribe({
      next: () => { this.closeCreate(); this.load(); },
      error: (e: HttpErrorResponse) => this.createError.set(e.error?.detail ?? 'Impossible de créer la commande.'),
    });
  }

  // ── Validate ───────────────────────────────────
  validate(order: CommandeDto): void {
    this.commandeService.validate(order.id).subscribe({
      next: () => this.load(),
      error: (e: HttpErrorResponse) => this.error.set(e.error?.detail ?? 'Impossible de valider la commande.'),
    });
  }

  // ── Delete ─────────────────────────────────────
  requestDelete(order: CommandeDto): void {
    this.pendingDelete.set(order);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const order = this.pendingDelete();
    if (!order) return;
    this.commandeService.delete(order.id).subscribe({
      next: () => { this.cancelDelete(); this.load(); },
      error: (e: HttpErrorResponse) => {
        this.cancelDelete();
        this.error.set(e.error?.detail ?? 'Impossible de supprimer la commande.');
      },
    });
  }
}
