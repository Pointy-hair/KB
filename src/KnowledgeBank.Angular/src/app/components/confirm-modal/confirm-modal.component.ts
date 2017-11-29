import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-modal',
    templateUrl: './confirm-modal.component.html',
    styles: ['./confirm-modal.component.css'],
})
export class ConfirmModalComponent {

    visible = false;
    visibleAnimate = false;
    @Input() header: string;
    @Input() body: any;
    @Input() buttonConfirmText: string;
    callback: () => void;

    constructor() { }

    public show(callback: () => void): void {
        this.visible = true;
        setTimeout(() => this.visibleAnimate = true, 100);
        this.callback = callback;
    }

    public hide(): void {
        this.visibleAnimate = false;
        console.log(this.header);
        setTimeout(() => this.visible = false, 300);
    }

    public onContainerClicked(event: MouseEvent): void {
        if ((<HTMLElement>event.target).classList.contains('modal')) {
            this.hide();
        }
    }

    public confirm() {
        this.callback();
        this.hide();
    }
}
