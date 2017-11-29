import { Component, OnInit } from '@angular/core';
import { CaseService } from '../../services/case.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Case } from '../../models/case';
import { ProjectService } from '../../services/project.service';
import { Tag } from '../../models/tag';
import { TagService } from '../../services/tag.service';
import { NgForm } from '@angular/forms';
import { Step } from '../../models/step';
import { Link } from '../../models/link';
import { EmailTemplate } from '../../models/email-template';
import { Attachment } from '../../models/attachment';
import { ConfirmModalComponent } from '../../components/confirm-modal/confirm-modal.component';
import { CaseBase } from '../../components/shared/case-base.component';

@Component({
  templateUrl: './case-create.component.html',
  styleUrls: ['case-create.component.css']
})
export class CaseCreateComponent extends CaseBase implements OnInit {
  caseDetails: Case = new Case();
  existingCategories: string[] = [];
  isCategoryEditable = false;
  existingTags: Tag[];

  constructor(
     caseService: CaseService,
     projectService: ProjectService,
     tagService: TagService,
     router: Router,
     route: ActivatedRoute,
  ) {
      super(caseService, projectService, tagService, router, route)
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.caseDetails.areaId = +params['projectId'];
      this.caseDetails.category = params['category'];
      if (this.caseDetails.category === undefined) {
        this.caseDetails.category = '';
        this.isCategoryEditable = true;
      }

      this.caseService.getCategories().subscribe(categories => {
        this.existingCategories = categories;
      });
      this.tagService.getAll().subscribe(tags => {
        this.existingTags = tags;
      });
    });
  }
}
