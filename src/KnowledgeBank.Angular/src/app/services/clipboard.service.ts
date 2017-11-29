import { Injectable, SecurityContext } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Injectable()
export class ClipboardService {

  constructor(
    private sanitizer: DomSanitizer
  ) { }

  copy(text: string, event: MouseEvent) {
    const textArea = document.createElement('textarea');

    //
    // *** This styling is an extra step which is likely not required. ***
    //
    // Why is it here? To ensure:
    // 1. the element is able to have focus and selection.
    // 2. if element was to flash render it has minimal visual impact.
    // 3. less flakyness with selection and copying which **might** occur if
    //    the textarea element is not visible.
    //
    // The likelihood is the element won't even render, not even a flash,
    // so some of these are just precautions. However in IE the element
    // is visible whilst the popup box asking the user for permission for
    // the web page to copy to the clipboard.
    //

    // Place in top-left corner of screen regardless of scroll position.
    textArea.style.position = 'fixed';
    textArea.style.top = '0';
    textArea.style.left = '0';

    // Ensure it has a small width and height. Setting to 1px / 1em
    // doesn't work as this gives a negative w/h on some browsers.
    textArea.style.width = '2em';
    textArea.style.height = '2em';

    // We don't need padding, reducing the size if it does flash render.
    textArea.style.padding = '0';

    // Clean up any borders.
    textArea.style.border = 'none';
    textArea.style.outline = 'none';
    textArea.style.boxShadow = 'none';

    // Avoid flash of white box if rendered for any reason.
    textArea.style.background = 'transparent';


    textArea.value = text;

    document.body.appendChild(textArea);

    textArea.select();

    try {
      const successful = document.execCommand('copy');
      const msg = successful ? 'successful' : 'unsuccessful';
      console.log('Copying text command was ' + msg);
      this.createAlert(event);
    } catch (err) {
      console.log('Oops, unable to copy');
    } finally {
      document.body.removeChild(textArea);
    }
  }

  copyHtml(html: string, event: MouseEvent) {
    const clipboardDiv = document.createElement('div');
    clipboardDiv.style.fontSize = '12pt'; // Prevent zooming on iOS
// Reset box model
    clipboardDiv.style.border = '0';
    clipboardDiv.style.padding = '0';
    clipboardDiv.style.margin = '0';
// Move element out of screen
    clipboardDiv.style.position = 'fixed';
    clipboardDiv.style['right'] = '-9999px';
    clipboardDiv.style.top = (window.pageYOffset || document.documentElement.scrollTop) + 'px';
// more hiding
    clipboardDiv.setAttribute('readonly', '');
    clipboardDiv.style['opacity'] = '0';
    clipboardDiv.style.pointerEvents = 'none';
    clipboardDiv.style['zIndex'] = '-1';
    clipboardDiv.setAttribute('tabindex', '0'); // so it can be focused
    clipboardDiv.innerHTML = '';
    document.body.appendChild(clipboardDiv);

    let sanitizedHtml = this.sanitizer.sanitize(SecurityContext.HTML, html);
    clipboardDiv.innerHTML = sanitizedHtml || '';

    let focused = document.activeElement as HTMLElement;
    clipboardDiv.focus();

    window.getSelection().removeAllRanges();
    if (!clipboardDiv.hasChildNodes()) {
      return;
    }

    let range = document.createRange();
    range.setStartBefore(clipboardDiv.firstChild || new Node()); // hack for tuparylyi TypeScript
    range.setEndAfter(clipboardDiv.lastChild || new Node());
    window.getSelection().addRange(range);

    try {
      const successful = document.execCommand('copy');
      const msg = successful ? 'successful' : 'unsuccessful';
      console.log('Copying text command was ' + msg);
      this.createAlert(event);
    } catch (err) {
      console.log('Oops, unable to copy');
    } finally {
      focused.focus();
      document.body.removeChild(clipboardDiv);
    }
  }

  copyContents(el: HTMLElement, event: MouseEvent) {
    let focused = document.activeElement as HTMLElement;
    el.focus();

    window.getSelection().removeAllRanges();
    if (!el.hasChildNodes()) {
      return;
    }

    let range = document.createRange();
    range.setStartBefore(el.firstChild || new Node()); // hack for tuparylyi TypeScript
    range.setEndAfter(el.lastChild || new Node());
    window.getSelection().addRange(range);

    try {
      const successful = document.execCommand('copy');
      const msg = successful ? 'successful' : 'unsuccessful';
      console.log('Copying text command was ' + msg);
      this.createAlert(event);
    } catch (err) {
      console.log('Oops, unable to copy');
    } finally {
      window.getSelection().removeAllRanges();
      focused.focus();
    }
  }

  createAlert(event: MouseEvent) {
    const alert = document.createElement('span');
    alert.className = 'alert alert-info';
    alert.textContent = 'Copied';

    alert.style.position = 'fixed';
    alert.style.top = event.clientY + 'px';
    alert.style.left = event.clientX + 'px';
    alert.style.animation = 'fadeOut 250ms linear 500ms forwards';

    document.body.appendChild(alert);
    setTimeout(() => {
      document.body.removeChild(alert);
    }, 1000);
  }

}
