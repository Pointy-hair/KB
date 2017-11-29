import { Injectable } from '@angular/core';
import { AuthService } from '../auth.service';
import { Observable } from 'rxjs/Observable';
import { Tag } from '../models/tag';

@Injectable()
export class TagService {

  constructor(private http: AuthService) { }

  getAll(): Observable<Tag[]> {
    const address = 'api/tags';
    return this.http.AuthGet(address)
      .map(x => x.json());
  }
}
