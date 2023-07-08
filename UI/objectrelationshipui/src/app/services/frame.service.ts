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
    
    let posX = 100;
    let posY = 100;
    let overlapping = true;
  
    while (overlapping) {
      overlapping = false;
      
      let maxWidth = 0;
      let maxHeight = 0;
      
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
        // Shift the position horizontally
        posX += maxWidth + margin;
    
        // If it reaches the right edge of the container, move to the next row
        if (posX + maxWidth + margin > containerWidth) {
          posX = 100;
          posY += maxHeight + margin;
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
            width = Math.max(width, tagNameLength > 40 ? 300 : 200);
            height = tagNameLength > 40 ? 100 : 0;
          }
        } else {
          const object = item as ObjectModel;
          const objectNameLength = object?.objectName?.length || 0;
          width = Math.max(width, objectNameLength > 40 ? 400 : 250);
          height = objectNameLength > 40 ? 150 : 200;
        }
      }
    }
  
    return { w: width, h: height };
  }

  getFrameSize(frame: Frame): { width: number, height: number } | undefined {
    if (frame && frame.size) {
      return { width: frame.size.w, height: frame.size.h };
    }
    return undefined;
  }

  getAssociatedTagFrames(objectId: number): Frame[] {
    const tags = this.frames.value.filter(frame => frame.frameType === 'Tag' && 
      frame.tagData?.some(tag => tag.associatedObjectId === objectId));
    console.log(tags);
    return tags;
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
