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
import { DEFAULT_PAGE_SIZE, Etattaxe, LOOKUP_PAGE_SIZE, ORDER_STATUS_LABELS, OrderStatus, TVA_RATE } from '../../shared/constants/business.constants';
import { apiErrorMessage } from '../../shared/utils/http-error';

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
  orders = signal<CommandeDto[]>([]);
  totalCount = signal(0);
  page = signal(1);
  readonly pageSize = DEFAULT_PAGE_SIZE;
  loading = signal(false);
  error = signal('');
  Remise = signal<CommandeDto | null>(null);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  details = signal<CommandeDto | null>(null);
  pendingCancel = signal<CommandeDto | null>(null);
  creating = signal(false);
  clients = signal<ClientDto[]>([]);
  produits = signal<ProduitDto[]>([]);
  selectedClientId = signal<number | null>(null);
  lignes = signal<LigneDraft[]>([{ produit: null, quantite: 1 }]);
  createError = signal('');
  readonly TVA_RATE = TVA_RATE;

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

  
 /* if(draftTotalTtc > '500' )
  {
    Remise = draftTotalTtc * 0.2 * ;
  }
    else
      if(draftTotalTtc > 100)
      { Remise = draftTotalTtc * 0.05;
    }
  }
  /* Remise selon pourcentage */ 
  dfRemise = computed(()=>

   Math.round((this.draftTotalTtc() * 0.2 *100))
  )
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

  statusLabel(status: number | string): string {
    return ORDER_STATUS_LABELS[this.toStatus(status)] ?? 'Inconnu';
  }

  /*EtatLabel(Etat: boolean ): void {
    return Etat[this.toEtattaxe(Etat)] ?? 'Inconnu';
  }
   EtatClass(Etat: boolean): void {
    return ['Inactive', 'Active'][this.toEtattaxe(Etat)] ?? '';
  }*/

  statusClass(status: number): string {
    return ['badge-draft', 'badge-validated', 'badge-delivered', 'badge-cancelled'][this.toStatus(status)] ?? '';
  }

  canValidate(status: number | string): boolean {
    return this.toStatus(status) === OrderStatus.Brouillon;
  }

  canCancel(status: number | string): boolean {
    const value = this.toStatus(status);
    return value === OrderStatus.Brouillon || value === OrderStatus.Validee;
  }

  willRestoreStock(status: number | string): boolean {
    return this.toStatus(status) === OrderStatus.Validee;
  }

  openCreate(): void {
    this.creating.set(true);
    this.createError.set('');
    this.selectedClientId.set(null);
    this.lignes.set([{ produit: null, quantite: 1 }]);

    this.clientService.getAll({ page: 1, pageSize: LOOKUP_PAGE_SIZE }).subscribe({
      next: (r) => this.clients.set(r.items),
    });
    this.produitService.getAll(1, LOOKUP_PAGE_SIZE).subscribe({
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
      error: (e: HttpErrorResponse) => this.createError.set(apiErrorMessage(e, 'Impossible de créer la commande.')),
    });

  } 

  validate(order: CommandeDto): void {
    this.commandeService.validate(order.id).subscribe({
      next: () => this.load(),
      error: (e: HttpErrorResponse) => this.error.set(apiErrorMessage(e, 'Impossible de valider la commande.')),
    });
  }

  requestCancel(order: CommandeDto): void {
    this.pendingCancel.set(order);
  }

  dismissCancel(): void {
    this.pendingCancel.set(null);
  }

  confirmCancel(): void {
    const order = this.pendingCancel();
    if (!order) return;
    this.commandeService.cancel(order.id).subscribe({
      next: () => {
        this.dismissCancel();
        this.details.set(null);
        this.error.set('');
        this.load();
      },
      error: (e: HttpErrorResponse) => {
        this.dismissCancel();
        this.error.set(apiErrorMessage(e, 'Impossible d\'annuler la commande.'));
      },
    });
  }

  private toStatus(status: number | string): number {
    if (typeof status === 'number') return status;
    const named: Record<string, number> = {
      Brouillon: OrderStatus.Brouillon,
      Validee: OrderStatus.Validee,
      Livree: OrderStatus.Livree,
      Annulee: OrderStatus.Annulee,
    };
    return named[status] ?? Number(status);
  }

  
  private toEtattaxe (Etat : boolean ) : boolean{
  if(typeof Etat ==='boolean') return Etat;
  const named: Record< any , any> = {
    inactive : Etattaxe.inactive,
    active : Etattaxe.active,
  };
  return named[Etat] ?? Boolean(Etat);
  }
}
