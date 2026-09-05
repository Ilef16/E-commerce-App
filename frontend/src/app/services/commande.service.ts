import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CommandeDto, CommandeWriteDto } from '../dtos/commande.dto';
import { PagedResult } from '../dtos/paged-result.dto';
import { DEFAULT_PAGE_SIZE } from '../shared/constants/business.constants';

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
      .set('pageSize', params.pageSize ?? DEFAULT_PAGE_SIZE);
    return this.api.get<PagedResult<CommandeDto>>('/api/orders', httpParams);
  }

  create(order: CommandeWriteDto): Observable<CommandeDto> {
    return this.api.post<CommandeDto>('/api/orders', order);
  }

  validate(id: number): Observable<CommandeDto> {
    return this.api.post<CommandeDto>(`/api/orders/${id}/validate`, {});
  }

  cancel(id: number): Observable<CommandeDto> {
    return this.api.post<CommandeDto>(`/api/orders/${id}/cancel`, null);
  }
}
