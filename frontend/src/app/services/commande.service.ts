import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { CommandeDto } from '../dtos/commande.dto';

@Injectable({ providedIn: 'root' })
export class CommandeService {
  constructor(private readonly api: ApiService) {}
  getAll(): Observable<CommandeDto[]> { return this.api.get<CommandeDto[]>('/api/orders'); }
  getById(id: number): Observable<CommandeDto> { return this.api.get<CommandeDto>(`/api/orders/${id}`); }
  validate(id: number): Observable<CommandeDto> { return this.api.post<CommandeDto>(`/api/orders/${id}/validate`, {}); }
  delete(id: number): Observable<void> { return this.api.delete<void>(`/api/orders/${id}`); }
}
