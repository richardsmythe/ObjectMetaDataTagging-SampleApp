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
  @ViewChild('topBarWrapperRef') topBarWrapperRef!: ElementRef;
  
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


  // updatePoints() {
  //   // Calculate the starting and ending positions based on the frame's position and size
  //   const startingX = this.position.x + this.size.w / 2;
  //   const startingY = this.position.y + this.size.h / 2;
  //   const endingX = this.points.endingPosition.x;
  //   const endingY = this.points.endingPosition.y;
  
  //   // Update the points property with the new positions
  //   this.points = { startingPosition: { x: startingX, y: startingY }, endingPosition: { x: endingX, y: endingY } };
  //   console.log(this.points);
  // }

  
  getPoints(): { startingPosition: { x: number; y: number }; endingPosition: { x: number; y: number } } {
    // Todo: account for tag frames too so that their position can be updated for the lines
    let startingPosition: { x: number; y: number } | undefined = undefined;
    let endingPosition: { x: number; y: number } | undefined = undefined;

  if (this.frame && this.frame.frameType === 'Object' && this.frame.objectData) {
    const objectData = this.frame.objectData;
    if (objectData && objectData[0].relatedFrames) {
      const startingFrameId = this.frame.id;
      const endingFrameIds = this.frameService.getAssociatedTagFrameIds(objectData[0].id);

      startingPosition = this.frameService.getFramePosition(startingFrameId);
      if (endingFrameIds.length > 0) {
        const firstEndingFrameId = endingFrameIds[0];
        endingPosition = this.frameService.getFramePosition(firstEndingFrameId);
      }      
    }
  }
  return {
    startingPosition: startingPosition || { x: 0, y: 0 },
    endingPosition: endingPosition || { x: 0, y: 0 }
  };
}

updateFramePosition(newPosition: { x: number; y: number }): void {
  this.position = { x: newPosition.x, y: newPosition.y };
}

drag(event: MouseEvent): void {
  event.preventDefault();
  const mouseX = event.clientX;
  const mouseY = event.clientY;

  const { x: positionX, y: positionY } = this.position;
  let dx: number;
  let dy: number;

  const duringDrag = (e: MouseEvent) => {
    dx = e.clientX - mouseX;
    dy = e.clientY - mouseY;
    this.position = { x: positionX + dx, y: positionY + dy };
    this.lastPosition = { ...this.position };
  };

  const finishDrag = () => {
    this.document.removeEventListener('mousemove', duringDrag);
    this.document.removeEventListener('mouseup', finishDrag);
    this.updateFramePosition(this.position);  
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

  deleteFrame(frameId: number | undefined): void {
    if (frameId !== undefined) {
      console.log(frameId);
      this.frameService.destroyFrame(frameId);
    }
  }
}