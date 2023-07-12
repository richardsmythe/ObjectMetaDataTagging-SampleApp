import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { Frame } from '../models/Frame';
import { ObjectModel } from '../models/ObjectModel';
import { TagModel } from '../models/TagModel';

@Injectable({
  providedIn: 'root'
})
export class FrameService {

  private frames: BehaviorSubject<Frame[]> = new BehaviorSubject<Frame[]>([]);
  private frameIdCounter: number = 1;

  constructor(private http: HttpClient) { }

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
  
              // Filter the tags associated with the current object
              const associatedTags = frameData.tagData.filter((tag: { associatedObjectId: number; }) => tag.associatedObjectId === object.id);
              
              // Create a tag frame and populate the grouped tags
              if (associatedTags.length > 0) {
                const tagFrame = this.createNewFrame([], associatedTags, 'Tag', frameData.origin);
                frames.push(tagFrame);
              }
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

   getFramePosition(frameId: number): { x: number; y: number } | undefined {
    const frame = this.getFrameById(frameId);
    if (frame && frame.position) {
      console.log("frameId:",frameId, "framePosition: ",frame.position);
      return { x: frame.position.x, y: frame.position.y };
    }
    return undefined;
  }

  getFrameSize(frame: Frame): { width: number, height: number } | undefined {
    if (frame && frame.size) {
      return { width: frame.size.w, height: frame.size.h };
    }
    return undefined;
  }

  getAssociatedTagFrameIds(objectId: number): number[] {
   
    const tagFrames = this.frames.value.filter(frame => frame.frameType === 'Tag' && frame.tagData?.some(tag => tag.associatedObjectId === objectId));
    const associatedFrames = tagFrames.map(frame => frame.id);

    // console.log("objectId:"+objectId +" related frame id:"+associatedFrames)   

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

}
