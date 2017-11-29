import { Component, OnDestroy, AfterViewInit, Input, forwardRef, NgZone, Inject, OnInit } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { TinyMceOptions } from './tiny-mce-options';
import { TinyMceDefaultOptions } from './tiny-mce-default-options';

declare var tinymce: any;

const noop = () => {
};

@Component({
  selector: 'tiny-mce',
  templateUrl: './tiny-mce.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TinyMceComponent),
      multi: true
    }
  ]
})
export class TinyMceComponent implements OnInit, AfterViewInit, OnDestroy {

  @Input() elementId: string;

  globalConfig: TinyMceOptions;
  @Input() options: TinyMceOptions;

  private onTouchedCallback: () => void = noop;
  private onChangeCallback: (_: any) => void = noop;

  editor: any;
  private innerValue: string;

  constructor(
    private zone: NgZone,
    @Inject('TINYMCE_CONFIG') private config: TinyMceOptions
  ) {
    this.globalConfig = Object.assign(new TinyMceDefaultOptions(), this.config);
    this.globalConfig.setup = (editor: any) => {
      this.editor = editor;
      editor.on('change keyup', () => {
        this.value = editor.getContent();
      });
      if (typeof this.config.setup === 'function') {
        this.config.setup(editor);
      }
    };
    this.globalConfig.init_instance_callback = (editor: any) => {
      editor && this.value && editor.setContent(this.value);
      if (typeof this.config.init_instance_callback === 'function') {
        this.config.init_instance_callback(editor);
      }
    };

  }

  ngOnInit() {
    if (!this.elementId) {
      this.elementId = 'tiny-'+Math.random().toString(36).substring(2);
    }
  }

  ngAfterViewInit() {
    // merge global config with config provided via @Input
    this.options = Object.assign(this.options || new TinyMceDefaultOptions(), this.globalConfig);

    if (this.options.baseURL) {
      tinymce.baseURL = this.options.baseURL;
    }
    this.options.selector = '#' + this.elementId;
    if (this.options.auto_focus) {
      this.options.auto_focus = this.elementId;
    }

    tinymce.init(this.options);
  }

  ngOnDestroy() {
    tinymce.remove(this.editor);
  }

  // get accessor
  get value(): any {
    return this.innerValue;
  };

  // set accessor including call the onchange callback
  set value(v: any) {
    if (v !== this.innerValue) {
      this.innerValue = v;
      this.zone.run(() => {
        this.onChangeCallback(v);
      });

    }
  }
  // From ControlValueAccessor interface
  writeValue(value: any) {
    if (value && value !== this.innerValue) {
      this.innerValue = value;
      this.editor && this.editor.setContent(value);
    }
  }

  registerOnChange(fn: any) {
    this.onChangeCallback = fn;
  }

  registerOnTouched(fn: any) {
    this.onTouchedCallback = fn;
  }
}
