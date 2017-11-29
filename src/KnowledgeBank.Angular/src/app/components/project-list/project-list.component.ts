import { Component, OnInit } from '@angular/core';
import { UserInfoService } from 'app/services/user-info.service';
import { Observable } from 'rxjs/Observable';
import { ProjectStats } from '../../models/project-stats';
import { ProjectService } from '../../services/project.service';

@Component({
  templateUrl: './project-list.component.html'
})
export class ProjectListComponent implements OnInit {
  projects: ProjectStats[] = [];

  constructor(private infoService: UserInfoService, private projectService: ProjectService) {
  }

  ngOnInit() {
    this.infoService.getUserProjects().subscribe(projects => {
      if (projects.length) {
        this.projectService.selectedProject = projects[0].id;
      }
      const observables = projects.map(project => this.infoService.getStatsForProject(project));
      Observable.forkJoin(observables).subscribe(stats => this.projects = stats);
    });
  }

}
