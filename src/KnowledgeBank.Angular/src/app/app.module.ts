import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '@angular/material';

import { AppComponent } from './app.component';
import { AuthGuard } from './auth.guard';
import { RouterModule } from '@angular/router';
import { AuthService } from './auth.service';
import { ProjectService } from './services/project.service';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ProjectComponent } from './components/project/project.component';
import { CaseService } from './services/case.service';
import { CaseCreateComponent } from './components/case-create/case-create.component';
import { TagService } from './services/tag.service';
import { UserInfoService } from './services/user-info.service';
import { CaseViewComponent } from './components/case-view/case-view.component';
import { CaseEditComponent } from './components/case-edit/case-edit.component';
import { NotFoundComponent } from './components/errors/not-found/not-found.component';
import { ExceptionPageComponent } from './components/errors/exception-page/exception-page.component';
import { ClipboardService } from './services/clipboard.service';
import { ErrorHandlingService } from './services/error-handling.service';
import { FileSizePipe } from './pipes/file-size.pipe';
import { AttachmentService } from './services/attachment.service';
import { ConfirmModalComponent } from './components/confirm-modal/confirm-modal.component';
import { LandingPageComponent } from './components/landing-page/landing-page.component';
import { TinyMceModule } from './components/tiny-mce/tiny-mce..module';
import 'rxjs';


@NgModule({
  declarations: [
    AppComponent,
    ProjectListComponent,
    ProjectComponent,
    CaseCreateComponent,
    CaseViewComponent,
    CaseEditComponent,
    NotFoundComponent,
    ExceptionPageComponent,
    FileSizePipe,
    ConfirmModalComponent,
    LandingPageComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    BrowserAnimationsModule,
    TinyMceModule,
    MaterialModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'landing-page', pathMatch: 'full' },
      { path: 'landing-page', component: LandingPageComponent, canActivate: [AuthGuard] },
      { path: 'projects', component: ProjectListComponent, canActivate: [AuthGuard] },
      { path: 'project/:projectId', component: ProjectComponent, canActivate: [AuthGuard] },
      { path: 'project/:projectId/new-case/:category', component: CaseCreateComponent, canActivate: [AuthGuard] },
      { path: 'project/:projectId/edit-case/:caseId', component: CaseEditComponent, canActivate: [AuthGuard] },
      { path: 'project/:projectId/new-case', component: CaseCreateComponent, canActivate: [AuthGuard] },
      { path: 'project/:projectId/case/:caseId', component: CaseViewComponent, canActivate: [AuthGuard] },
      { path: 'exception-page', component: ExceptionPageComponent },
      { path: '**', component: NotFoundComponent }
    ])
  ],
  providers: [
    AuthService,
    AuthGuard,
    AttachmentService,
    ClipboardService,
    ProjectService,
    UserInfoService,
    TagService,
    CaseService,
    ErrorHandlingService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
