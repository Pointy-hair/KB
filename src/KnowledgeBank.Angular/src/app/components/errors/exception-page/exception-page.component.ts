import { Component } from '@angular/core';
import { ErrorHandlingService } from '../../../services/error-handling.service';

@Component({
  templateUrl: './exception-page.component.html'
})

export class ExceptionPageComponent {
  error: string;

  constructor(private errorService: ErrorHandlingService) {
    this.error = errorService.error;
  }
}