import { Injectable } from '@angular/core';
import { AuthService } from '../auth.service';
import { ProjectService } from './project.service';
import { DownloadedFile } from '../models/downloaded-file';

@Injectable()
export class AttachmentService {

  constructor(
    private http: AuthService,
    private projectService: ProjectService
  ) { }

  download(attachmentId: number): void {
    this.http.AuthDownload('api/attachments/'+attachmentId, this.projectService.selectedProject)
      .subscribe(this.saveFile);
  }

  saveFile(file: DownloadedFile) {
    let downloadUrl = URL.createObjectURL(file.blob);

    let a = document.createElement("a");
    document.body.appendChild(a);
    a.setAttribute('style', "display: none");
    a.href = downloadUrl;
    a.download = file.filename;
    a.click();
    window.URL.revokeObjectURL(downloadUrl);
    document.body.removeChild(a);
  }
}
