import { TinyMceOptions } from './tiny-mce-options';

export class TinyMceDefaultOptions implements TinyMceOptions {
  plugins = [
    // 'link', - for preventing XSS
    'paste',
    'table',
    'advlist',
    'autoresize',
    'lists',
    'code'
  ];
  skin_url = (<any>window).baseUrl + 'assets/tinymce/skins/lightgray';
  baseURL = (<any>window).baseUrl + 'assets/tinymce';
  auto_focus = true;
  branding = false;
  elementpath = false;
  statusbar = false;
  min_height: 100;
}
