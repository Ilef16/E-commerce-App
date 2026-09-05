import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { ProduitService } from '../../services/produit.service';
import { ProduitDto, ProduitWriteDto } from '../../dtos/produit.dto';
import { environment } from '../../../environments/environment';
import { DEFAULT_PAGE_SIZE } from '../../shared/constants/business.constants';
import { apiErrorMessage } from '../../shared/utils/http-error';

@Component({
  selector: 'app-produits',
  imports: [CommonModule, FormsModule, PageHeader],
  templateUrl: './produits.html',
  styleUrl: './produits.scss',
})
export class Produits implements OnInit {
  produits = signal<ProduitDto[]>([]);
  totalCount = signal(0);
  page = signal(1);
  readonly pageSize = DEFAULT_PAGE_SIZE;
  loading = signal(false);
  error = signal('');

  creating = signal(false);
  editing = signal<ProduitDto | null>(null);
  details = signal<ProduitDto | null>(null);
  deleting = signal<ProduitDto | null>(null);
  actionMenuId = signal<number | null>(null);

  draft = signal<ProduitWriteDto>(this.emptyDraft());
  selectedPhoto = signal<File | null>(null);
  photoPreview = signal<string | null>(null);
  removePhoto = signal(false);
  formError = signal('');
  submitted = signal(false);

  constructor(private readonly produitService: ProduitService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.produitService.getAll(this.page(), this.pageSize).subscribe({
      next: (result) => {
        this.produits.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Impossible de charger les produits.');
        this.loading.set(false);
      },
    });
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.page.set(page);
    this.load();
  }

  photoUrl(url: string | null): string | null {
    if (!url) return null;
    return url.startsWith('http') ? url : `${environment.assetBaseUrl}${url}`;
  }

  openCreate(): void {
    this.closeModals();
    this.draft.set(this.emptyDraft());
    this.creating.set(true);
  }

  openEdit(p: ProduitDto): void {
    this.closeModals();
    this.draft.set({
      reference: p.reference,
      libelle: p.libelle,
      description: p.description,
      prixUnitaire: p.prixUnitaire,
      stock: p.stock,
    });
    this.photoPreview.set(p.photoUrl);
    this.editing.set(p);
  }

  save(): void {
    const draft = this.draft();
    this.submitted.set(true);
    if (!draft.libelle.trim() || draft.prixUnitaire <= 0) return;

    const isEdit = !!this.editing();
    const request = isEdit
      ? this.produitService.update(this.editing()!.id, { ...draft, removePhoto: this.removePhoto() }, this.selectedPhoto())
      : this.produitService.create(draft, this.selectedPhoto());

    request.subscribe({
      next: () => {
        this.closeModals();
        this.load();
      },
      error: (e: HttpErrorResponse) => {
        this.formError.set(apiErrorMessage(e, 'Le produit n\'a pas pu être enregistré.'));
      },
    });
  }

  selectPhoto(event: Event): void {
    const input = event.target as HTMLInputElement;
    const photo = input.files?.[0] ?? null;
    this.selectedPhoto.set(photo);
    this.removePhoto.set(false);
    this.photoPreview.set(photo ? URL.createObjectURL(photo) : null);
  }

  deletePhoto(): void {
    this.selectedPhoto.set(null);
    this.photoPreview.set(null);
    this.removePhoto.set(true);
  }

  openDetails(p: ProduitDto): void {
    this.closeModals();
    this.details.set(p);
  }

  askDelete(p: ProduitDto): void {
    this.closeModals();
    this.deleting.set(p);
  }

  confirmDelete(): void {
    const p = this.deleting();
    if (!p) return;
    this.produitService.delete(p.id).subscribe({
      next: () => {
        this.closeModals();
        this.load();
      },
      error: (e: HttpErrorResponse) => {
        this.closeModals();
        this.error.set(apiErrorMessage(e, 'Impossible de supprimer le produit.'));
      },
    });
  }

  toggleActionMenu(id: number): void {
    this.actionMenuId.update((current) => (current === id ? null : id));
  }

  closeModals(): void {
    this.actionMenuId.set(null);
    this.creating.set(false);
    this.editing.set(null);
    this.details.set(null);
    this.deleting.set(null);
    this.formError.set('');
    this.submitted.set(false);
    this.selectedPhoto.set(null);
    this.photoPreview.set(null);
    this.removePhoto.set(false);
  }

  private emptyDraft(): ProduitWriteDto {
    return {
      reference: `PROD-${Date.now().toString().slice(-8)}`,
      libelle: '',
      description: null,
      prixUnitaire: 0,
      stock: 0,
    };
  }
}
