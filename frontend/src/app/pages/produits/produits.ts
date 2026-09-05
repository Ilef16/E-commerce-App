import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { ProduitService } from '../../services/produit.service';
import { ProduitDto, ProduitWriteDto } from '../../dtos/produit.dto';

@Component({
  selector: 'app-produits',
  imports: [CommonModule, FormsModule, PageHeader],
  templateUrl: './produits.html',
  styleUrl: './produits.scss',
})
export class Produits implements OnInit {
  produits = signal<ProduitDto[]>([]); loading = signal(false); error = signal('');
  editing = signal<ProduitDto | null>(null); details = signal<ProduitDto | null>(null); deleting = signal<ProduitDto | null>(null); creating = signal(false);
  draft = signal<ProduitWriteDto>(this.emptyDraft());
  constructor(private readonly produitService: ProduitService) {}
  ngOnInit(): void { this.load(); }
  load(): void { this.loading.set(true); this.produitService.getAll().subscribe({ next: p => { this.produits.set(p); this.loading.set(false); }, error: () => { this.error.set('Impossible de charger les produits.'); this.loading.set(false); } }); }
  openCreate(): void { this.editing.set(null); this.creating.set(true); this.draft.set(this.emptyDraft()); }
  openEdit(p: ProduitDto): void { this.creating.set(false); this.editing.set(p); this.draft.set({ ...p }); }
  openDetails(p: ProduitDto): void { this.details.set(p); }
  askDelete(p: ProduitDto): void { this.deleting.set(p); }
  closeModals(): void { this.creating.set(false); this.editing.set(null); this.details.set(null); this.deleting.set(null); }
  save(): void { const request = this.editing() ? this.produitService.update(this.editing()!.id, this.draft()) : this.produitService.create(this.draft()); request.subscribe({ next: () => { this.closeModals(); this.load(); }, error: (e: HttpErrorResponse) => this.error.set(e.error?.detail ?? 'Impossible d’enregistrer le produit.') }); }
  confirmDelete(): void { const p = this.deleting(); if (!p) return; this.produitService.delete(p.id).subscribe({ next: () => { this.closeModals(); this.load(); }, error: (e: HttpErrorResponse) => { this.closeModals(); this.error.set(e.error?.detail ?? 'Impossible de supprimer le produit.'); } }); }
  private emptyDraft(): ProduitWriteDto { return { reference: '', libelle: '', description: null, prixUnitaire: 0, stock: 0 }; }
}
