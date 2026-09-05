import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HealthDto } from '../dtos/health.dto';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class HealthService {
  constructor(private readonly api: ApiService) {}

  getStatus(): Observable<HealthDto> {
    return this.api.get<HealthDto>('/api/health');
  }
}
