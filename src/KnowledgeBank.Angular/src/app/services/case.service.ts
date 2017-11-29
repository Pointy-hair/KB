import { Injectable } from '@angular/core';
import { RequestOptions, URLSearchParams } from '@angular/http';
import { Category } from '../models/category';
import { Observable } from 'rxjs/Observable';
import { AuthService } from '../auth.service';
import { Case } from '../models/case';
import { ProjectService } from './project.service';

@Injectable()
export class CaseService {

    private address = 'api/cases';

    getById(id: number): Observable<Case> {
        return this.http.AuthGet(`${this.address}/${id}`)
            .map(c => c.json() as Case)
            .do(c => {
            c.createdAt = new Date(c.createdAt);
            c.lastModifiedAt = new Date(c.lastModifiedAt);
        });
    }
    constructor(
        private http: AuthService,
        private  projectService: ProjectService,
    ) { }

    get(query?: string): Observable<Category[]> {
        const urlSearchParams = new URLSearchParams();
        if (query !== undefined && query !== '') {
            urlSearchParams.set('query', query);
        }

        return this.http.AuthGet(this.address, new RequestOptions({ search: urlSearchParams }))
            .map(x => x.json());
    }

    create(newCase: Case, files: File[], isUpdate: boolean) {
        return this.http.AuthPostWithFile(this.address, newCase, files, this.projectService.selectedProject, isUpdate);
    }


    delete(caseId: number) {
      return this.http.AuthDelete(`${this.address}/${caseId}`);
    }

    getCategories(): Observable<string[]> {
        return this.http.AuthGet(`${this.address}/categories`)
            .map(x => x.json());
    }
}
