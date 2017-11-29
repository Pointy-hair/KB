import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';

@Injectable()
export class ProjectService {
    private _selectedProject: number;

    constructor(private route: ActivatedRoute, private router: Router) {
      this.router.events.subscribe(_ => {
        this.route.params.subscribe(params => {
          this._selectedProject = +params['projectId'];
        });
      });
      this.router.events
       .filter(e => e instanceof NavigationEnd)
       .forEach(e => {
         let firstChild = router.routerState.snapshot.root.firstChild;
         while (firstChild != null && !firstChild.params['projectId']) {
           firstChild = firstChild.firstChild;
         }
         if (firstChild) {
           this._selectedProject = +firstChild.params['projectId'];
         }
       });
    }

   get selectedProject() {
       return this._selectedProject;
    }

    set selectedProject(newValue: number) {
        this._selectedProject = newValue;
    }

}