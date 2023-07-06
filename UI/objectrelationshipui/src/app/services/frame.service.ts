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
              const objectFrame = this.createNewFrame([object], [], 'Object', frameData.id, frameData.origin);
              frames.push(objectFrame);
  
              // Filter the tags associated with the current object
              const associatedTags = frameData.tagData.filter((tag: { associatedObjectId: number; }) => tag.associatedObjectId === object.id);
              
              // Create a tag frame and populate the grouped tags
              if (associatedTags.length > 0) {
                const tagFrame = this.createNewFrame([], associatedTags, 'Tag', frameData.id, frameData.origin);
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
  
  createNewFrame(objectData: ObjectModel[], tagData: TagModel[], frameType: string, id: number, origin: string): Frame {
    const frame: Frame = {
      id,
      position: { x: 100, y: 100 },
      size: { w: 450, h: 250 },
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







  // getFrameData(): Observable<Frame[]> {
  //   return this.http.get<any[]>('https://localhost:7170/api/Tag').pipe(
  //     switchMap(response => {
  //       const frames: Frame[] = [];
  
  //       response.forEach(frameData => {
  //         // Check if there are objects
  //         if (frameData.objectData) {
  //           frameData.objectData.forEach((object: ObjectModel) => {
  //             // Create an object frame
  //             const objectFrame = this.createNewFrame(object.objectName, 'Object', frameData.id, frameData.origin);
  //             frames.push(objectFrame);
  
  //             // Filter the tags associated with the current object
  //             const associatedTags = frameData.tagData.filter((tag: { associatedObjectId: number; }) => tag.associatedObjectId === object.id);
              
  //             // Create a tag frame and populate the grouped tags
  //             if (associatedTags.length > 0) {
  //               const tagNames = associatedTags.map((tag: { tagName: any; }) => tag.tagName);
  //               const tagFrame = this.createNewFrame(tagNames.join(', '), 'Tag', frameData.id, frameData.origin);
  //               frames.push(tagFrame);
  //             }
  //           });
  //         }
  //       });
  
  //       this.frames.next(frames);
  //       return of(frames);
  //     }),
  //     catchError(error => {
  //       console.error('Error fetching frame data:', error);
  //       return of([]);
  //     })
  //   );
  // }
  
  // createNewFrame(data: string, frameType: string, id: number, origin: string): Frame {
  //   const frame: Frame = {
  //     id,
  //     position: { x: 100, y: 100 },
  //     size: { w: 450, h: 250 },
  //     data,
  //     origin,
  //     frameType,
  //     objectData: [],
  //     tagData: []
  //   };
  
  //   const currentFrames = this.frames.value.slice();
  //   currentFrames.push(frame);
  //   this.frames.next(currentFrames);
  
  //   return frame;
  // }

  getFrameSize(frame: Frame): { width: number, height: number } | undefined {
    if (frame && frame.size) {
      return { width: frame.size.w, height: frame.size.h };
    }
    return undefined;
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
