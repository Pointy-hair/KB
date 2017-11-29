import { Component, OnInit } from '@angular/core';
import { AuthService } from 'app/auth.service';
import { Router } from '@angular/router';
import { ProjectService } from './services/project.service';
import { UserInfoService } from './services/user-info.service';
import { User } from 'oidc-client';
import { LocationStrategy } from '@angular/common';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
    title = 'app works!';
    loggedInUser: User;

    constructor(
        private authService: AuthService,
        public userInfoService: UserInfoService,
        public projectService: ProjectService,
        public router: Router,
        private location: LocationStrategy
    ) { }

    ngOnInit() {
        (<any>window).baseUrl = this.location.getBaseHref();
        this.authService.userLoadedEvent.subscribe((user: User) => {
            this.loggedInUser = user;
            if (this.authService.isAuthenticated) {
                this.userInfoService.getRoles().subscribe();
            }
        });
    }

    logout() {
        this.authService.startSignoutMainWindow();
    }

    get currentYear() {
      return new Date().getFullYear();
    }
}
