import { NgModule, ModuleWithProviders } from '@angular/core';
import { TinyMceComponent } from './tiny-mce.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TinyMceOptions } from './tiny-mce-options';
import { TinyMceDefaultOptions } from './tiny-mce-default-options';

declare var tinymce: any;

@NgModule({
  declarations: [
    TinyMceComponent
  ],
  imports: [
    FormsModule,
    ReactiveFormsModule
  ],
  exports: [
    TinyMceComponent
  ],
  providers: [
    { provide: 'TINYMCE_CONFIG', useClass: TinyMceDefaultOptions }
  ]
})
export class TinyMceModule {
  static withConfig(userConfig: TinyMceOptions = {}): ModuleWithProviders {
    return {
      ngModule: TinyMceModule,
      providers: [
        { provide: 'TINYMCE_CONFIG', useValue: userConfig }
      ]
    }
  }
}
export { tinymce }
