import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserInfoService } from 'app/services/user-info.service';
import { Project } from '../../models/project';
import { User } from 'oidc-client';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.css'],

})

export class LandingPageComponent implements OnInit {
  projects: Project[];
  selectedProject: number;
  searchText = '';
  loggedInUser: User;

  constructor(
    private router: Router,
    private userInfoService: UserInfoService,
    private authService: AuthService
  ) {
  }

  ngOnInit(): void {
    this.userInfoService.getUserProjects().subscribe(result => {
      this.projects = result;
      if (this.projects.length > 0) {
        this.selectedProject = this.projects[0].id;
      }
    });

    this.authService.userLoadedEvent.subscribe((user: User) => {
      this.loggedInUser = user;
      if (this.authService.isAuthenticated) {
        this.userInfoService.getRoles().subscribe();
      }
    });
  }

  search() {
    this.router.navigate(['/project/', this.selectedProject, { search: this.searchText }]);
  }

  logout() {
    this.authService.startSignoutMainWindow();
  }
}

