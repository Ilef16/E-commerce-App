import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { DashboardDto } from '../dtos/dashboard.dto';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private readonly api: ApiService) {}

  getStats(): Observable<DashboardDto> {
    return this.api.get<DashboardDto>('/api/dashboard');
  }
}
