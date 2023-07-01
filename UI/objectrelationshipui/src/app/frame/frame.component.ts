import { Component, ElementRef, Inject, ViewChild } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export type ResizeAnchorType = 'top' | 'left' | 'bottom' | 'right';
export type ResizeDirectionType = 'x' | 'y' | 'xy';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.css']
})
export class FrameComponent {
  @ViewChild('wrapper') wrapperRef!: ElementRef;
  @ViewChild('topBar') topBarRef!: ElementRef;
  @ViewChild('resizeCorner') resizeCornerRef!: ElementRef;

  position = { x: 200, y: 200 };
  size = { w: 200, h: 200 };
  lastPosition: { x: number, y: number } | undefined;
  lastSize: { w: number, h: number } | undefined;
  minSize = { w: 200, h: 200 };
  _document: any;

  constructor(@Inject(DOCUMENT) private document: Document, private elementRef: ElementRef) {
    this.lastPosition = undefined;
    this.lastSize = undefined;
  }
  startDrag(event: MouseEvent): void {
    event.preventDefault();
    const mouseX = event.clientX;
    const mouseY = event.clientY;

    const { x: positionX, y: positionY } = this.position;

    const duringDrag = (e: MouseEvent) => {
      const dx = e.clientX - mouseX;
      const dy = e.clientY - mouseY;
      this.position = { x: positionX + dx, y: positionY + dy };
      this.lastPosition = { ...this.position };
    };

    const finishDrag = () => {
      this.document.removeEventListener('mousemove', duringDrag);
      this.document.removeEventListener('mouseup', finishDrag);
    };

    this.document.addEventListener('mousemove', duringDrag);
    this.document.addEventListener('mouseup', finishDrag);
  }

  startResize(event: MouseEvent, anchors: ResizeAnchorType[], direction: ResizeDirectionType): void {
  event.preventDefault();
  const mouseX = event.clientX;
  const mouseY = event.clientY;
  const lastX = this.position.x;
  const lastY = this.position.y;
  const dimensionWidth = this.size.w;
  const dimensionHeight = this.size.h;

  const duringResize = (e: MouseEvent) => {
    let dw = dimensionWidth;
    let dh = dimensionHeight;
  
    if (direction === 'x' || direction === 'xy') {
      if (anchors.includes('left')) {
        const offsetX = e.clientX - mouseX;
        dw -= offsetX;
        this.position.x = lastX + offsetX;
      } else if (anchors.includes('right')) {
        dw += (e.clientX - mouseX);
      }
    }
  
    if (direction === 'y' || direction === 'xy') {
      if (anchors.includes('top')) {
        const offsetY = e.clientY - mouseY;
        dh -= offsetY;
        this.position.y = lastY + offsetY;
      } else if (anchors.includes('bottom')) {
        dh += (e.clientY - mouseY);
      }
    }

      // stop resizing beyond the original size of 200x200
    if (anchors.includes('left') || anchors.includes('top') || anchors.includes('bottom') || anchors.includes('right')){
      dw = Math.max(dw, this.minSize.w);
      dh = Math.max(dh, this.minSize.h);
    }
  
    this.size.w = dw;
    this.size.h = dh;
    this.lastSize = { ...this.size };
  };

  const finishResize = () => {
    this.document.removeEventListener('mousemove', duringResize);
    this.document.removeEventListener('mouseup', finishResize);
  };

  this.document.addEventListener('mousemove', duringResize);
  this.document.addEventListener('mouseup', finishResize);
}
}