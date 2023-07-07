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
  
  createNewFrame(objectData: ObjectModel[], tagData: TagModel[], frameType: string,  origin: string): Frame {
    const frame: Frame = {
      id: this.frameIdCounter++,
      position: { x: 100, y: 100 },
      size: this.calculateFrameSize(frameType === 'Tag' ? tagData : objectData),
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
