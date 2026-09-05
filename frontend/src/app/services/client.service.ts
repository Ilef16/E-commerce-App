import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ClientDto, ClientWriteDto } from '../dtos/client.dto';
import { PagedResult } from '../dtos/paged-result.dto';

export interface ClientListParams {
  page?: number;
  pageSize?: number;
  q?: string;
}

@Injectable({ providedIn: 'root' })
export class ClientService {
  constructor(private readonly api: ApiService) {}

  getAll(params: ClientListParams = {}): Observable<PagedResult<ClientDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);

    if (params.q?.trim()) {
      httpParams = httpParams.set('q', params.q.trim());
    }

    return this.api.get<PagedResult<ClientDto>>('/api/clients', httpParams);
  }

  getById(id: number): Observable<ClientDto> {
    return this.api.get<ClientDto>(`/api/clients/${id}`);
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
