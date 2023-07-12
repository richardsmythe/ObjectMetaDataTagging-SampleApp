import { Component, ElementRef, Inject, Input, ViewChild } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { Frame } from '../models/Frame';
import { FrameService } from '../services/frame.service';

export type ResizeAnchorType = 'top' | 'left' | 'bottom' | 'right';
export type ResizeDirectionType = 'x' | 'y' | 'xy';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.css']
})
export class FrameComponent {
  @Input() frame: Frame | undefined;
  @ViewChild('wrapper') wrapperRef!: ElementRef;
  @ViewChild('topBar') topBarRef!: ElementRef;
  @ViewChild('resizeCorner') resizeCornerRef!: ElementRef;

  points: {
    startingPosition: { x: number; y: number };
    endingPosition: { x: number; y: number };
  } = {
      startingPosition: { x: 0, y: 0 },
      endingPosition: { x: 0, y: 0 }
    };
  centerPosition: { x: number; y: number } = { x: 0, y: 0 };
  position: { x: number, y: number } = { x: 0, y: 0 };
  size = { w: 0, h: 0 };
  lastPosition: { x: number, y: number } | undefined;
  lastSize: { w: number, h: number } | undefined;
  minSize = { w: 0, h: 0 };
  _document: any;

  constructor(@Inject(DOCUMENT) private document: Document, private elementRef: ElementRef, private frameService: FrameService) {
    this.lastPosition = undefined;
    this.lastSize = undefined;
  }

  ngOnInit(): void {

    if (this.frame) {
      const frameSize = this.frameService.getFrameSize(this.frame);
      if (frameSize) {
        this.size = { w: frameSize.width, h: frameSize.height };
      }

      // Get frame position if available
      if (this.frame.position) {
        this.position = { x: this.frame.position.x, y: this.frame.position.y };
      }

      const objectIds = this.frame.objectData?.map(obj => obj.id);
      if (objectIds) {
        for (const objectId of objectIds) {
          this.frameService.getAssociatedTagFrameIds(objectId);
        }
        this.points = this.getPoints();
      }
    }
  }



updateCenterPosition(): void {
  this.centerPosition.x = this.position.x + this.size.w / 2;
  this.centerPosition.y = this.position.y + this.size.h / 2;
}
  
  getPoints(): { startingPosition: { x: number; y: number }; endingPosition: { x: number; y: number } } {

    let startingPosition: { x: number; y: number } | undefined = undefined;
    let endingPosition: { x: number; y: number } | undefined = undefined;

    if (this.frame && this.frame.frameType === 'Object' && this.frame.objectData) {
      const objectData = this.frame.objectData;
      if (objectData && objectData[0].relatedFrames) {

        const startingFrameId = this.frame.id;
        const endingFrameId = this.frameService.getAssociatedTagFrameIds(objectData[0].id);

        startingPosition = this.frameService.getFramePosition(startingFrameId);
        endingPosition = this.frameService.getFramePosition(endingFrameId[0]);

        console.log("Starting position:", startingPosition);
        console.log("Ending position:", endingPosition);
      }
    }
    return {
      startingPosition: startingPosition || { x: 0, y: 0 },
      endingPosition: endingPosition || { x: 0, y: 0 }
    };
  }
  
  drag(event: MouseEvent): void {
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

  resize(event: MouseEvent, anchors: ResizeAnchorType[], direction: ResizeDirectionType): void {
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
          if (dw >= this.minSize.w) {
            this.position.x = lastX + offsetX;
          } else {
            dw = this.minSize.w;
            this.position.x = lastX + dimensionWidth - this.minSize.w;
          }
        } else if (anchors.includes('right')) {
          dw += (e.clientX - mouseX);
        }
      }

      if (direction === 'y' || direction === 'xy') {
        if (anchors.includes('top')) {
          const offsetY = e.clientY - mouseY;
          dh -= offsetY;
          if (dh >= this.minSize.h) {
            this.position.y = lastY + offsetY;
          } else {
            dh = this.minSize.h;
            this.position.y = lastY + dimensionHeight - this.minSize.h;
          }
        } else if (anchors.includes('bottom')) {
          dh += (e.clientY - mouseY);
        }
      }

      if (anchors.includes('left') || anchors.includes('top') || anchors.includes('bottom') || anchors.includes('right')) {
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

  // getFramePosition(frameId: number): { x: number; y: number } | undefined {
  //   const frame = this.frameService.getFrameById(frameId);
  //   if (frame && frame.position) {
  //     console.log("frameId:",frameId, "framePosition: ",frame.position);
  //     return { x: frame.position.x, y: frame.position.y };
  //   }
  //   return undefined;
  // }


  deleteFrame(frameId: number | undefined): void {
    if (frameId !== undefined) {
      console.log(frameId);
      this.frameService.destroyFrame(frameId);
    }
  }
}