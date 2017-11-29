import { Injectable } from '@angular/core';
import { Project } from '../models/project';
import { AuthService } from 'app/auth.service';
import { Observable } from 'rxjs/Observable';
import { Role } from '../models/role';
import { ProjectStats } from '../models/project-stats';

@Injectable()
export class UserInfoService {
    roles: string[];
    private address = 'info/userinfo';

    constructor(private http: AuthService) { }

    getUserProjects(): Observable<Project[]> {
        return this.http.AuthGet(`${this.address}/areas`).map(x => x.json());
    }

    getProject(projectId: number): Observable<Project> {
        return this.http.AuthGet(`${this.address}/areas/${projectId}`).map(x => x.json());
    }

    getStatsForProject(project: Project): Observable<ProjectStats> {
        return this.http.AuthGet('api/cases/statsForArea', undefined, project.id)
            .map(x => x.json() as ProjectStats)
            .do(stats => stats.projectName = project.name);
    }

    /**
     * Gets the roles for the current users
     * @returns {Observable<string[]>}
     */
    getRoles(): Observable<string[]> {
        return this.http.AuthGet(`${this.address}/roles`)
            .map(x => {
                this.roles = x.json();
                return x.json();
            });
    }

    isAnyOfRoles(requiredRoles: string[]) {
        if (!this.roles) {
            return false;
        }

        return requiredRoles.some(role => this.roles.includes(role));
    }

    get canCreateCase() {
        const allowedRoles = [Role.AreaAdmin, Role.ReadWriteUser];
        return this.isAnyOfRoles(allowedRoles);
    }


}
