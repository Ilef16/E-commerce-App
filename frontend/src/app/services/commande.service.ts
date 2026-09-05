import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CommandeDto, CommandeWriteDto } from '../dtos/commande.dto';
import { PagedResult } from '../dtos/paged-result.dto';

export interface CommandeListParams {
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class CommandeService {
  constructor(private readonly api: ApiService) {}

  getAll(params: CommandeListParams = {}): Observable<PagedResult<CommandeDto>> {
    const httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 10);
    return this.api.get<PagedResult<CommandeDto>>('/api/orders', httpParams);
  }

  getById(id: number): Observable<CommandeDto> {
    return this.api.get<CommandeDto>(`/api/orders/${id}`);
  }

  create(order: CommandeWriteDto): Observable<CommandeDto> {
    return this.api.post<CommandeDto>('/api/orders', order);
  }

  validate(id: number): Observable<CommandeDto> {
    return this.api.post<CommandeDto>(`/api/orders/${id}/validate`, {});
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`/api/orders/${id}`);
  }
}
