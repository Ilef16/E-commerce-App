import { Component, HostListener, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ClientService } from '../../services/client.service';
import { ClientDto, ClientWriteDto } from '../../dtos/client.dto';
import { PageHeader } from '../../shared/components/page-header/page-header';
import { TUNISIA_WILAYAS } from '../../shared/constants/tunisia.constants';

@Component({
  selector: 'app-clients',
  imports: [CommonModule, FormsModule, PageHeader],
  templateUrl: './clients.html',
  styleUrl: './clients.scss',
})
export class Clients implements OnInit {
  readonly wilayasTunisie = TUNISIA_WILAYAS;

  clients = signal<ClientDto[]>([]);
  totalCount = signal(0);
  page = signal(1);
  pageSize = signal(10);
  search = signal('');
  loading = signal(false);
  error = signal<string | null>(null);
  detailsClientId = signal<number | null>(null);
  detailsClient = signal<ClientDto | null>(null);
  editingClientId = signal<number | null>(null);
  editDraft = signal<ClientWriteDto | null>(null);
  pendingDeleteClient = signal<ClientDto | null>(null);
  addingClient = signal(false);
  createDraft = signal<ClientWriteDto>(Clients.emptyClient());
  createSubmitted = signal(false);
  editSubmitted = signal(false);
  createFieldErrors = signal({ identifiant: '', nom: '', email: '' });
  editFieldErrors = signal({ identifiant: '', nom: '', email: '' });
  createFormError = signal('');
  editFormError = signal('');
  actionMenuClientId = signal<number | null>(null);

  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  constructor(private readonly clientService: ClientService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.clientService
      .getAll({ page: this.page(), pageSize: this.pageSize(), q: this.search() })
      .subscribe({
        next: (result) => {
          this.clients.set(result.items);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les clients. Vérifiez que le serveur est démarré.');
          this.loading.set(false);
        },
      });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.page.set(1);
    this.load();
  }

  goToPage(n: number): void {
    if (n < 1 || n > this.totalPages()) return;
    this.page.set(n);
    this.load();
  }

  openDetails(client: ClientDto): void {
    this.cancelOtherModals();
    this.detailsClientId.set(client.id);
    this.detailsClient.set(client);
  }

  toggleActionMenu(id: number): void {
    this.actionMenuClientId.update((currentId) => (currentId === id ? null : id));
  }

  startCreate(): void {
    this.addingClient.set(true);
    this.createDraft.set(Clients.emptyClient());
    this.createSubmitted.set(false);
    this.createFieldErrors.set({ identifiant: '', nom: '', email: '' });
    this.createFormError.set('');
  }

  cancelCreate(): void {
    this.addingClient.set(false);
    this.createDraft.set(Clients.emptyClient());
    this.createSubmitted.set(false);
    this.createFieldErrors.set({ identifiant: '', nom: '', email: '' });
    this.createFormError.set('');
  }

  @HostListener('document:keydown.escape')
  closeCreateWithEscape(): void {
    if (this.addingClient()) this.cancelCreate();
  }

  saveCreate(): void {
    const draft = this.createDraft();
    this.createSubmitted.set(true);
    const errors = this.validateClient(draft);
    this.createFieldErrors.set(errors);
    this.createFormError.set('');
    if (errors.identifiant || errors.nom || errors.email) {
      return;
    }

    this.clientService.create(draft).subscribe({
      next: () => {
        this.cancelCreate();
        this.page.set(1);
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.createFieldErrors.set(this.fieldErrorsFromApi(error));
        if (!Object.values(this.createFieldErrors()).some(Boolean)) {
          this.createFormError.set(this.apiError(error, 'Impossible de créer ce client.'));
        }
      },
    });
  }

  startEdit(client: ClientDto): void {
    this.cancelOtherModals();
    this.editSubmitted.set(false);
    this.editFieldErrors.set({ identifiant: '', nom: '', email: '' });
    this.editFormError.set('');
    this.editingClientId.set(client.id);
    this.editDraft.set({
      identifiant: client.identifiant,
      nom: client.nom,
      prenom: client.prenom,
      email: client.email,
      telephone: client.telephone,
      adresse: client.adresse,
      ville: client.ville,
      codePostal: client.codePostal,
    });
  }

  cancelEdit(): void {
    this.editingClientId.set(null);
    this.editDraft.set(null);
    this.editSubmitted.set(false);
    this.editFieldErrors.set({ identifiant: '', nom: '', email: '' });
    this.editFormError.set('');
  }

  requestDelete(client: ClientDto): void {
    this.cancelOtherModals();
    this.pendingDeleteClient.set(client);
  }

  cancelDelete(): void {
    this.pendingDeleteClient.set(null);
  }

  confirmDelete(): void {
    const client = this.pendingDeleteClient();
    if (!client) return;

    this.clientService.delete(client.id).subscribe({
      next: () => {
        this.cancelDelete();
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.cancelDelete();
        this.error.set(this.apiError(error, 'Impossible de supprimer ce client.'));
      },
    });
  }

  saveEdit(id: number): void {
    const draft = this.editDraft();
    if (!draft) return;

    this.editSubmitted.set(true);
    const errors = this.validateClient(draft);
    this.editFieldErrors.set(errors);
    this.editFormError.set('');
    if (errors.identifiant || errors.nom || errors.email) return;

    this.clientService.update(id, draft).subscribe({
      next: () => {
        this.cancelEdit();
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.editFieldErrors.set(this.fieldErrorsFromApi(error));
        if (!Object.values(this.editFieldErrors()).some(Boolean)) {
          this.editFormError.set(this.apiError(error, 'Impossible de modifier ce client.'));
        }
      },
    });
  }

  private cancelOtherModals(): void {
    this.actionMenuClientId.set(null);
    this.addingClient.set(false);
    this.detailsClientId.set(null);
    this.detailsClient.set(null);
    this.editingClientId.set(null);
    this.editDraft.set(null);
    this.editSubmitted.set(false);
    this.editFieldErrors.set({ identifiant: '', nom: '', email: '' });
    this.editFormError.set('');
    this.pendingDeleteClient.set(null);
  }

  pages(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  private static emptyClient(): ClientWriteDto {
    return {
      identifiant: `CLI-${Date.now().toString().slice(-8)}`,
      nom: '',
      prenom: null,
      email: '',
      telephone: null,
      adresse: null,
      ville: null,
      codePostal: null,
    };
  }

  private apiError(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 405) {
      return 'Le backend doit être redémarré pour activer la création des clients.';
    }

    return error.error?.detail ?? error.error?.title ?? fallback;
  }

  private validateClient(draft: ClientWriteDto): { identifiant: string; nom: string; email: string } {
    return {
      identifiant: draft.identifiant.trim() ? '' : 'L’identifiant est obligatoire.',
      nom: draft.nom.trim() ? '' : 'Le nom est obligatoire.',
      email: !draft.email.trim()
        ? 'L’email est obligatoire.'
        : /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(draft.email.trim())
          ? ''
          : 'L’email n’est pas valide.',
    };
  }

  private fieldErrorsFromApi(error: HttpErrorResponse): { identifiant: string; nom: string; email: string } {
    const detail = error.error?.detail ?? '';
    return {
      identifiant: detail.toLowerCase().includes('identifiant') ? detail : '',
      nom: detail.toLowerCase().includes('nom') ? detail : '',
      email: detail.toLowerCase().includes('email') ? detail : '',
    };
  }
}
