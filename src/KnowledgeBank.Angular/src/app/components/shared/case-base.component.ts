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

export class CaseBase {
  caseDetails: Case = new Case();
  selectedStep: Step;
  newTag = new Tag();
  newLink = new Link();
  attachments: File[] = [];
  selectedTemplate: EmailTemplate;
  submitError = '';
  showLinkSection = false;
  showAttachmentSection = false;
  caseId: number;


  constructor(
    protected caseService: CaseService,
    protected projectService: ProjectService,
    protected tagService: TagService,
    protected router: Router,
    protected route: ActivatedRoute
  ) {
  }

  createCase() {
      this.caseService.create(this.caseDetails, this.attachments, this.caseId? true:false).subscribe(
      response => this.router.navigate(['/project/' + this.projectService.selectedProject]),
      error => this.submitError = error);
  }

  addTag(tagForm: NgForm) {
    if (!!this.newTag.name && !this.caseDetails.tags.map(t => t.name).includes(this.newTag.name)) {
      this.caseDetails.tags.push(this.newTag);
      this.newTag = new Tag();
    }

    tagForm.resetForm();
  }

  addStep() {
    const newStep = new Step();
    this.caseDetails.steps.push(newStep);
    this.selectedStep = newStep;
  }

  editStep(step: Step) {
    this.selectedStep = step;
  }

  addLink() {
    this.caseDetails.links.push(this.newLink);
    this.newLink = new Link();
  }

  addAttachment(event: any) {
    for (const file of event.target.files) {
      this.attachments.push(file);
      const newAttachment = new Attachment();
      newAttachment.name = file.name;
      newAttachment.size = file.size;
      this.caseDetails.attachments.push(newAttachment);
    }
    event.srcElement.value = null;
  }

  addEmailTemplate() {
    this.selectedTemplate = new EmailTemplate();
    this.caseDetails.emailTemplates.push(this.selectedTemplate);
  }

  editTemplate(emailTemplate: EmailTemplate) {
    this.selectedTemplate = emailTemplate;
  }

  stepUp(array: Step[], index: number) {
    if (index !== 0) {
      const tmp = array[index].description;
      array[index].description = array[index - 1].description;
      array[index - 1].description = tmp;
    }
  }
  stepDown(array: Step[], index: number) {
    if (index !== array.length - 1) {
      const tmp = array[index].description;
      array[index].description = array[index + 1].description;
      array[index + 1].description = tmp;
    }
  }

  addStepSecction(confirmDeleteAllModal: ConfirmModalComponent) {
    if (this.caseDetails.steps.length === 0) {
      this.addStep();
    } else {
      confirmDeleteAllModal.show(function() { this.caseDetails.steps = []; }.bind(this));
    }
  }

  addEmailSecction(confirmDeleteAllModal: ConfirmModalComponent) {
    if (this.caseDetails.emailTemplates.length === 0) {
      this.addEmailTemplate();
    } else {
      confirmDeleteAllModal.show(function() { this.caseDetails.emailTemplates = []; }.bind(this));
    }
  }

  addLinkSecction(confirmDeleteAllModal: ConfirmModalComponent) {
    if (!this.showLinkSection) {
      this.showLinkSection = true;
    } else {
      confirmDeleteAllModal.show(function() {
        this.caseDetails.links = [];
        this.showLinkSection = false;
      }.bind(this));
    }
  }

  addAttachmentSecction(confirmDeleteAllModal: ConfirmModalComponent) {
    if (!this.showAttachmentSection) {
      this.showAttachmentSection = true;
    } else {
      confirmDeleteAllModal.show(function() {
        this.caseDetails.attachments = [];
        this.showAttachmentSection = false;
      }.bind(this));
    }
  }
}
