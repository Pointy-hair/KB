import { Component, OnInit } from '@angular/core';
import { CaseService } from '../../services/case.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Case } from '../../models/case';
import { ErrorHandlingService } from '../../services/error-handling.service';
import { AttachmentService } from '../../services/attachment.service';

import { ClipboardService } from '../../services/clipboard.service';

@Component({
    templateUrl: './case-view.component.html',
    styleUrls: ['case-view.component.css']
})
export class CaseViewComponent implements OnInit {
    projectId: number;
    caseId: number;
    caseDetails: Case = new Case();
    error: string;

    constructor(
        private caseService: CaseService,
        private attachmentService: AttachmentService,
        private errorService: ErrorHandlingService,
        private route: ActivatedRoute,
        //private router: Router,
        public clipboard: ClipboardService) { }

    ngOnInit(): void {
        this.route.params.subscribe(params => {
            this.projectId = +params['projectId'];
            this.caseId = +params['caseId'];

            this.caseService.getById(this.caseId).subscribe(
                c => this.caseDetails = c,
                async (err: Response) => await this.errorService.handleError(err, 'Something went wrong while loading case details.')
            );
        });
    }

  downloadFile(attachmentId: number) {
    this.attachmentService.download(attachmentId);
  }
}
