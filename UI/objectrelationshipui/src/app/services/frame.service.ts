import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { Frame } from '../models/Frame';

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
  
        // Iterate over the response data
        response.forEach(frameData => {
          // Check if there are objects
          if (frameData.objectData) {
            const objectFrame: Frame = this.createNewFrame(frameData.objectData, 'Object');
            frames.push(objectFrame); // Add frame for the object
          }
  
          // Iterate over the associated tags and create frames for each tag
          if (frameData.tagData && frameData.tagData.length > 0) {
            frameData.tagData.forEach((tag: any) => {
              const tagFrameData: Frame = this.createNewFrame(tag, 'Tag');
              frames.push(tagFrameData); // Add frame for the tag
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

  createNewFrame(frameData: any, frameType: string): Frame {
    const frame: Frame = {
      id: frameData.id,
      position: { x: 100, y: 100 },
      size: { w: 400, h: 250 },
      frameName: frameType === 'Object' ? frameData.objectName : "Associated Tags",
      objectData: frameType === 'Object' ? [frameData] : [],
      tagData: frameType === 'Tag' ? [frameData] : []
    };

    const currentFrames = this.frames.value.slice();
    currentFrames.push(frame);
    this.frames.next(currentFrames);

    return frame;
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
