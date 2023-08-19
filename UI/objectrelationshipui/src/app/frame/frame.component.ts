import { OnInit, ViewChildren, QueryList, Component, ElementRef, Inject, Input, ViewChild, ChangeDetectorRef, SimpleChanges } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { Frame } from '../models/FrameModel';
import { FrameService } from '../services/frame.service';
import { LineModel } from '../models/LineModel';
import { LineComponent } from '../line/line.component';
import { Subscription, of, switchMap, take } from 'rxjs';

export type ResizeAnchorType = 'top' | 'left' | 'bottom' | 'right';
export type ResizeDirectionType = 'x' | 'y' | 'xy';

@Component({
  selector: 'app-frame',
  templateUrl: './frame.component.html',
  styleUrls: ['./frame.component.css'],

})
export class FrameComponent implements OnInit {

  @ViewChildren(LineComponent) lineComponents!: QueryList<LineComponent>;

  @Input() frame: Frame | undefined;
  @ViewChild('wrapper') wrapperRef!: ElementRef;
  @ViewChild('topBar') topBarRef!: ElementRef;
  @ViewChild('resizeCorner') resizeCornerRef!: ElementRef;
  @ViewChild('topBarWrapperRef') topBarWrapperRef!: ElementRef;
  private subs: Subscription = new Subscription();
  public lines: LineModel[] = [];
  position: { x: number, y: number } = { x: 0, y: 0 };
  size = { w: 0, h: 0 };
  lastPosition: { x: number, y: number } | undefined;
  lastSize: { w: number, h: number } | undefined;
  minSize = { w: 150, h: 150 };

  constructor(@Inject(DOCUMENT) private document: Document,
    private frameService: FrameService,
    private cdRef: ChangeDetectorRef) {

    this.lastPosition = undefined;
    this.lastSize = undefined;
  }


  ngOnInit(): void {
    this.cdRef.detectChanges();

    // Subscription to update the component's state whenever frames data changes
    this.subs.add(
      this.frameService.frames.subscribe(frames => {
        const currentFrame = frames.find(f => f.id === this.frame?.id);
        if (currentFrame) {
          this.size = { w: currentFrame.size.w, h: currentFrame.size.h };
          this.position = { x: currentFrame.position.x, y: currentFrame.position.y };    
        }
      })
    );

    if (this.frame) {
      // if object frame, get associated tags
      const objectIds = this.frame.objectData?.map(obj => obj.id);
      if (objectIds) {
        for (const objectId of objectIds) {
          this.frameService.getAssociatedTagFrameIds(objectId);
        }
      }
    }

    // Inform the service that this frame has been initialised
    this.frameService.frameInitialised();

    // Setup a subscription to fetch the lines once all frames are initialised
    this.frameService.allFramesInitialised.pipe(
      take(1),
      switchMap(initialised => {
        if (initialised) {
          return this.frameService.getLines();
        } else {
          return of([]);  // Don't fetch lines if frames are not initialised.
        }
      })
    ).subscribe(lines => {
      this.lines = lines;
    });
  }


  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }


  drag(event: MouseEvent, frameId: number | undefined): void {
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


      this.frameService.updateFramePosition(this.position, frameId);
      //this.frameService.updateFrameSize(this.size,frameId);
      this.frameService.updateLinePositions();
    };

    const finishDrag = () => {
      this.document.removeEventListener('mousemove', duringDrag);
      this.document.removeEventListener('mouseup', finishDrag);


    };

    this.document.addEventListener('mousemove', duringDrag);
    this.document.addEventListener('mouseup', finishDrag);


  }
  resize(event: MouseEvent,
    anchors: ResizeAnchorType[],
    direction: ResizeDirectionType,
    frameId: number | undefined): void {
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

      const newSize = { w: dw, h: dh };
      this.frameService.updateFramePosition(this.position, frameId);
      this.frameService.updateFrameSize(newSize, frameId);
      console.log("SIZE: ",this.frameService.getFrameSize(frameId))
      console.log("CENTER:", this.frameService.getCenterOfFrame(frameId!));

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