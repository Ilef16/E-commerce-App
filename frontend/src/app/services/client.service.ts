import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ClientDto, ClientWriteDto } from '../dtos/client.dto';
import { PagedResult } from '../dtos/paged-result.dto';
import { DEFAULT_PAGE_SIZE } from '../shared/constants/business.constants';

export interface ClientListParams {
  page?: number;
  pageSize?: number;
  search?: string;
}

@Injectable({ providedIn: 'root' })
export class ClientService {
  constructor(private readonly api: ApiService) {}

  getAll(params: ClientListParams = {}): Observable<PagedResult<ClientDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? DEFAULT_PAGE_SIZE);

    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }

    return this.api.get<PagedResult<ClientDto>>('/api/clients', httpParams);
  }

  create(client: ClientWriteDto): Observable<ClientDto> {
    return this.api.post<ClientDto>('/api/clients', client);
  }

  update(id: number, client: ClientWriteDto): Observable<ClientDto> {
    return this.api.put<ClientDto>(`/api/clients/${id}`, client);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`/api/clients/${id}`);
  }
}
