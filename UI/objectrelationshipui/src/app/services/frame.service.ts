import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, of, switchMap } from 'rxjs';
import { Frame } from '../models/FrameModel';
import { ObjectModel } from '../models/ObjectModel';
import { TagModel } from '../models/TagModel';
import { LineModel } from '../models/LineModel';

@Injectable({
  providedIn: 'root'
})
export class FrameService {
  private initialisedFramesCounter = 0;
  private frames: BehaviorSubject<Frame[]> = new BehaviorSubject<Frame[]>([]);
  public lines: BehaviorSubject<LineModel[]> = new BehaviorSubject<LineModel[]>([]);
  private frameIdCounter: number = 1;
  public allFramesInitialised: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) { }

  public frameInitialised(): void {
    this.initialisedFramesCounter++;
    const frames = this.frames.getValue();
    if (this.initialisedFramesCounter === frames.length) {
        console.log(this.initialisedFramesCounter, 'frames initialised');
        this.updateLinePositions(); 
        this.allFramesInitialised.next(true);
    }
}

getLines(): Observable<LineModel[]> {
    return this.lines.asObservable();
  }
  getFrames(): Frame[] {
    return this.frames.getValue();
  }
  getFrameData(): Observable<Frame[]> {
    return this.http.get<any[]>('https://localhost:7170/api/Tag').pipe(
      switchMap(response => {
        const frames: Frame[] = [];

        response.forEach(frameData => {
          // Check if there are objects
          if (frameData.objectData) {
            frameData.objectData.forEach((object: ObjectModel) => {
              // Create an object frame
              const objectFrame = this.createNewFrame([object], [], 'Object', frameData.origin);
              frames.push(objectFrame);
              
              // Iterate over the tags associated with the current object
              frameData.tagData.forEach((tag: any) => {
                // Check if the tag is associated with the current object
                if(tag.associatedObjectId === object.id) {
                  // Create a tag frame for each individual tag
                  const tagFrame = this.createNewFrame([], [tag], 'Tag', frameData.origin);
                  frames.push(tagFrame);
                }
              });
            });
          }
        });

        this.frames.next(frames);
        return of(frames);
      }),
      catchError(error => {
        console.error('Error fetching frame data:', error);
        return of([]);
      })
    );
}

  createNewFrame(objectData: ObjectModel[], tagData: TagModel[], frameType: string, origin: string): Frame {
    const frame: Frame = {
      id: this.frameIdCounter++,
      size: this.calculateFrameSize(frameType === 'Tag' ? tagData : objectData),
      position: this.calculateFramePosition(),
      frameType,
      origin,
      objectData,
      tagData
    };

    // Populate relatedFrameIds for object frames
    if (frameType === 'Object') {
      frame.objectData?.forEach(obj => {
        obj.relatedFrames = this.getAssociatedTagFrameIds(obj.id);
      });
    }

    const currentFrames = this.frames.value.slice();
    currentFrames.push(frame);
    this.frames.next(currentFrames);

    return frame;
  }

  calculateFramePosition(): { x: number, y: number } {
    const margin = 20;
    const containerWidth = window.innerWidth;
    const containerHeight = window.innerHeight;
    const frames = this.frames.value;

    let posX = 10;
    let posY = 10;
    let overlapping = true;
    let maxWidth = 0;
    let maxHeight = 0;

    while (overlapping) {
      overlapping = false;

      for (const frame of frames) {
        const currentFrameSize = this.getFrameSize(frame);
        const currentFrameWidth = currentFrameSize?.width || 0;
        const currentFrameHeight = currentFrameSize?.height || 0;

        maxWidth = Math.max(maxWidth, currentFrameWidth);
        maxHeight = Math.max(maxHeight, currentFrameHeight);

        if (
          posX < frame.position.x + currentFrameWidth + margin &&
          posX + maxWidth + margin > frame.position.x &&
          posY < frame.position.y + currentFrameHeight + margin &&
          posY + maxHeight + margin > frame.position.y
        ) {
          overlapping = true;
          break;
        }
      }

      if (overlapping) {
        // Move to the next row if there is not enough space in the current row
        if (posX + maxWidth + margin > containerWidth) {
          posX = 10;
          posY += maxHeight + margin;
          maxHeight = 0;
        } else if (posY + maxHeight + margin > containerHeight) {
          // Stop generating positions if it exceeds the container height
          break;
        } else {
          // Shift the position horizontally
          posX += maxWidth + margin;
        }
      }
    }

    return { x: posX, y: posY };
  }


  calculateFrameSize(data: ObjectModel[] | TagModel[]): { w: number, h: number } {
    let width = 0;
    let height = 0;

    if (Array.isArray(data)) {
      for (const item of data) {
        if (Array.isArray(item)) {
          for (const tag of item as TagModel[]) {
            const tagNameLength = tag?.tagName?.length || 0;
            width = Math.max(width, tagNameLength > 40 ? 300 : 100);
            height = tagNameLength > 40 ? 100 : 0;
          }
        } else {
          const object = item as ObjectModel;
          const objectNameLength = object?.objectName?.length || 0;
          width = Math.max(width, objectNameLength > 40 ? 450 : 250);
          height = objectNameLength > 40 ? 150 : 200;
        }
      }
    }

    return { w: width, h: height };
  }

  getFrameById(frameId: number): Frame | undefined {
    const frames = this.frames.value;
    const frame = frames.find(fr => fr.id === frameId);
    return frame !== undefined ? frame : undefined;
  }

  updateFramePosition(position: { x: number; y: number }, frameId: number | undefined): void {
    const frames = this.frames.getValue();
    const frameIndex = frames.findIndex((frame) => frame.id === frameId);
    if (frameIndex !== -1) {
      frames[frameIndex].position = { ...position };
      this.frames.next(frames);
      //console.log("Frame:",frames[frameIndex].id," New Position:",frames[frameIndex].position);
    }
  }

   getFramePosition(frameId: number): { x: number; y: number } | undefined {
    const frame = this.getFrameById(frameId);
    if (frame && frame.position) {
      //console.log("CURRENT frameId:",frameId, "framePosition: ",frame.position);
      return { x: frame.position.x, y: frame.position.y };
    }
    return undefined; 
  }

  getFrameSize(frame: Frame): { width: number, height: number } | undefined {
      // NB: this only gets the stored size, not current
    if (frame && frame.size) {
      return { width: frame.size.w, height: frame.size.h };
    }
    return undefined;
  }

  getAssociatedTagFrameIds(objectId: number): number[] {
    const tagFrames = this.frames.getValue().filter(frame => frame.frameType === 'Tag' && frame.tagData?.some(tag => tag.associatedObjectId === objectId));
    
    const associatedFrames = tagFrames.map(frame => frame.id);
    
    console.log("Associated Tag Frame IDs for Object ID", objectId, ":", associatedFrames);
    return associatedFrames;
  }
  destroyFrame(frameId: number): void {
    const currentFrames = this.frames.value.slice();
    const index = currentFrames.findIndex(frame => frame.id === frameId);
    if (index !== -1) {
      currentFrames.splice(index, 1);
      this.frames.next(currentFrames);
    }
  }


  updateLinePositions(): void {
    const frames = this.frames.getValue();
    const lines: LineModel[] = [];
  
    for (const frame of frames) {
        if (frame.frameType === 'Object') {
            const startingPosition = frame.position;
            const childIds = frame.objectData ? this.getAssociatedTagFrameIds(frame.objectData[0].id) : [];

            childIds.forEach(childId => {
                const associatedTagFrame = frames.find(f => f.id === childId && f.frameType === 'Tag');
                if (associatedTagFrame && associatedTagFrame.tagData[0].associatedObjectId === frame.objectData?.[0]?.id) {
                  const endingPosition = this.getFramePosition(associatedTagFrame.id) || { x: 0, y: 0 };
                  lines.push({ parentId: frame.id, childId: [associatedTagFrame.id], startingPosition, endingPosition });
              }
            });
        }
    }
  
    this.lines.next(lines);
}
}
