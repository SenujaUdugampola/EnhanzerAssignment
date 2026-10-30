import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LocationDetail {
  id: number;
  locationCode: string;
  locationName: string;
}

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);

  getLocations(): Observable<LocationDetail[]> {
    return this.http.get<LocationDetail[]>(`${environment.apiUrl}/locations`);
  }
}
