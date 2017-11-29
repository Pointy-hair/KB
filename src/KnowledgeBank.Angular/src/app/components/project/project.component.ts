import { Component, OnInit, ViewChild } from '@angular/core';
import { CaseService } from '../../services/case.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Category } from '../../models/category';
import { ProjectService } from '../../services/project.service';
import { UserInfoService } from 'app/services/user-info.service';
import { ErrorHandlingService } from '../../services/error-handling.service';

import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/operator/first';
import 'rxjs/add/operator/distinctUntilChanged';
import { ConfirmModalComponent } from '../confirm-modal/confirm-modal.component';

@Component({
    templateUrl: './project.component.html'
})
export class ProjectComponent implements OnInit {

    projectId: number;
    projectName: string;
    categories: Observable<Category[]>;
    totalCases: number;
    totalCategories: number;
    searchString: string;

    private searchTerms = new BehaviorSubject<string>('');

    constructor(
        private projectService: ProjectService,
        private userInfoService: UserInfoService,
        private caseService: CaseService,
        private errorService: ErrorHandlingService,
        private route: ActivatedRoute,
        private router: Router
    ) {

      this.searchTerms
        .debounceTime(500)
        .distinctUntilChanged()
        .subscribe(term => {
          if (!!term) {
            this.router.navigate(['project', this.projectId, {search: term}], {replaceUrl: true});
          }
        });

      // here and not in ngOnInit - to avoid errors in app.component
      this.route.params.subscribe(params => {
        this.projectId = +params['projectId'];
        this.projectService.selectedProject = this.projectId;
      });
    }

    ngOnInit() {
        let allCategories = this.caseService.get();
        allCategories.first().subscribe(categories => {
          this.totalCases = categories.reduce((sum, current) => sum + current.cases.length, 0);
          this.totalCategories = categories.length;
        });

        this.route.params.subscribe(params => {

            if (params['search']) {
                this.searchString = params['search'];
            }
            this.categories = this.caseService.get(this.searchString || '');

            this.userInfoService.getProject(this.projectId).subscribe(project => {
                this.projectName = project.name;
            });
        });
    }

    search() {
        this.searchTerms.next(this.searchString);
    }

    @ViewChild(ConfirmModalComponent)
    public readonly confirmPopUp: ConfirmModalComponent;
    deleteCase(caseId: number, e: Event) {
      this.confirmPopUp.show(this.makeDeleteRequest.bind(this, caseId));
      e.preventDefault();
      e.stopPropagation(); // otherwise the case would open
    }

    private makeDeleteRequest(caseId: number) {
      this.caseService.delete(caseId).subscribe(
        this.ngOnInit.bind(this),
        async (err: Response) => await this.errorService.handleError(err, `Something went wrong while deleting case #${caseId}.`)
      );
    }
}
