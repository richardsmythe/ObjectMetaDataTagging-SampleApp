import { OnInit, ViewChildren, QueryList, Component, ElementRef, Inject, Input, ViewChild, ChangeDetectorRef, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
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
  @Input() frames: Frame[] = [];

  constructor(@Inject(DOCUMENT) private document: Document,
    private frameService: FrameService,
    private cdRef: ChangeDetectorRef) {
    this.lastPosition = undefined;
    this.lastSize = undefined;
  }


  ngOnInit(): void {

    // subscription to update the component's state whenever frames data changes
    this.subs.add(
      this.frameService.frames.subscribe((frames) => {
        this.frames = frames;
        const latestFrame = frames.find((f) => f.id === this.frame?.id);
        if (latestFrame) { 
          this.size = { w: latestFrame.size.w, h: latestFrame.size.h };
          this.position = { x: latestFrame.position.x, y: latestFrame.position.y };
          this.frame = latestFrame;
        }
      })
    );

    this.frameService.frameInitialised();

    // subscription to fetch the lines once all frames are initialised
    this.subs.add(
      this.frameService.allFramesInitialised.pipe(
        take(1),
        switchMap(initialised => {
          if (initialised) {
            return this.frameService.getLines();
          } else {
            return of([]);  // don't fetch lines if frames are not initialised.
          }
        })
      ).subscribe(lines => {
        this.lines = lines;
      })
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['frames']) {
      console.log('Frame data changed:', changes['frames'].currentValue);
    }
  }


  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }


  drag(event: MouseEvent, frame: any, frameId: number | undefined): void {
    event.preventDefault();
    const mouseX = event.clientX;
    const mouseY = event.clientY;
    const { x: positionX, y: positionY } = frame.position;
  
    const duringDrag = (e: MouseEvent) => {
      const dx = e.clientX - mouseX;
      const dy = e.clientY - mouseY;
      frame.position = { x: positionX + dx, y: positionY + dy };
  
      // Assuming this.frameService.updateFramePosition accepts the frame itself
      this.frameService.updateFramePosition(frame.position, frameId);  
  
      this.frameService.updateLinePositions();
    };
  
    const finishDrag = () => {
      this.document.removeEventListener('mousemove', duringDrag);
      this.document.removeEventListener('mouseup', finishDrag);
    };
  
    this.document.addEventListener('mousemove', duringDrag);
    this.document.addEventListener('mouseup', finishDrag);
  }

  resize(event: MouseEvent, anchors: ResizeAnchorType[], direction: ResizeDirectionType, frame: Frame): void {
    event.preventDefault();
  
    const mouseX = event.clientX;
    const mouseY = event.clientY;
    const originalX = frame.position.x;
    const originalY = frame.position.y;
    const originalWidth = frame.size.w;
    const originalHeight = frame.size.h;
  
    const duringResize = (e: MouseEvent) => {
      const deltaX = e.clientX - mouseX;
      const deltaY = e.clientY - mouseY;
      let newWidth = originalWidth;
      let newHeight = originalHeight;
      let newX = originalX;
      let newY = originalY;
  
      if (anchors.includes('left')) {
        newWidth -= deltaX;
        newX += deltaX;
      }
  
      if (anchors.includes('top')) {
        newHeight -= deltaY;
        newY += deltaY;
      }
  
      if (anchors.includes('right')) {
        newWidth += deltaX;
      }
  
      if (anchors.includes('bottom')) {
        newHeight += deltaY;
      }
  
      // Ensure that the frame size doesn't go below the minimum size
      newWidth = Math.max(newWidth, this.minSize.w);
      newHeight = Math.max(newHeight, this.minSize.h);
  
    
      if (frame.id !== undefined) {
        this.frameService.updateFrameSize({ w: newWidth, h: newHeight }, frame.id);
        this.frameService.updateFramePosition({ x: newX, y: newY }, frame.id);
        this.frameService.updateLinePositions();
      }
  

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
      this.frameService.destroyFrame(frameId);
      // Remove the deleted frame from the local list
      this.frames = this.frames.filter((frame) => frame.id !== frameId);
      console.log("number of frames after deleting:", this.frames.length)
      this.cdRef.markForCheck();
    }
  }
}