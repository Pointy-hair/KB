import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Injectable()
export class ErrorHandlingService {
    public error: string;

    constructor(private router: Router) {
    }

    async handleError(err: Response, message: string) {
        if (err.status === 404) {
            this.router.navigate(['/not-found'], { skipLocationChange: true });
        } else {
            this.error = environment.production ? message : await err.text();
            this.router.navigate(['/exception-page'], { skipLocationChange: true });
        }
    }
}