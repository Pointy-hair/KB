import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from 'app/auth.service';

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(public authService: AuthService, public router: Router) {
    }

    canActivate(): Promise<boolean> {
        return this.authService.initUserPromise.then(() => {
            if (this.authService.isAuthenticated) {
                return true;
            }

            this.authService.startSigninMainWindow();
            return false;
        });
    }
}
