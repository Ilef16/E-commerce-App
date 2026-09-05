import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ProduitDto, ProduitWriteDto } from '../dtos/produit.dto';

@Injectable({ providedIn: 'root' })
export class ProduitService {
  constructor(private readonly api: ApiService) {}

  getAll(): Observable<ProduitDto[]> {
    return this.api.get<ProduitDto[]>('/api/products');
  }

  getById(id: number): Observable<ProduitDto> {
    return this.api.get<ProduitDto>(`/api/products/${id}`);
  }

  create(produit: ProduitWriteDto): Observable<ProduitDto> {
    return this.api.post<ProduitDto>('/api/products', produit);
  }

  update(id: number, produit: ProduitWriteDto): Observable<ProduitDto> {
    return this.api.put<ProduitDto>(`/api/products/${id}`, produit);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`/api/products/${id}`);
  }
}
