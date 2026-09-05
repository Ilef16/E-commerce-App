import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ProduitDto, ProduitWriteDto } from '../dtos/produit.dto';
import { PagedResult } from '../dtos/paged-result.dto';
import { DEFAULT_PAGE_SIZE } from '../shared/constants/business.constants';

@Injectable({ providedIn: 'root' })
export class ProduitService {
  constructor(private readonly api: ApiService) {}

  getAll(page = 1, pageSize = DEFAULT_PAGE_SIZE): Observable<PagedResult<ProduitDto>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.api.get<PagedResult<ProduitDto>>('/api/products', params);
  }

  create(produit: ProduitWriteDto, photo: File | null): Observable<ProduitDto> {
    return this.api.post<ProduitDto>('/api/products', this.toFormData(produit, photo));
  }

  update(id: number, produit: ProduitWriteDto, photo: File | null): Observable<ProduitDto> {
    return this.api.put<ProduitDto>(`/api/products/${id}`, this.toFormData(produit, photo));
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`/api/products/${id}`);
  }

  private toFormData(produit: ProduitWriteDto, photo: File | null): FormData {
    const formData = new FormData();
    formData.append('reference', produit.reference);
    formData.append('libelle', produit.libelle);
    formData.append('description', produit.description ?? '');
    formData.append('prixUnitaire', String(produit.prixUnitaire));
    formData.append('stock', String(produit.stock));
    formData.append('removePhoto', String(produit.removePhoto ?? false));
    if (photo) formData.append('photo', photo, photo.name);
    return formData;
  }
}
