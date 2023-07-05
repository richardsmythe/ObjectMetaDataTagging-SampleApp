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
             console.log('Object Data:', frameData.objectData);
            const objectFrameData: Frame[] = frameData.objectData.map((object: { objectName: string; }) => {
              return this.createNewFrame(object.objectName, 'Object');
            });
            frames.push(...objectFrameData); // Add frames for objectData
          }

          // Iterate over the associated tags and create frames for each tag
          if (frameData.tagData) {
            const tagFrameData: Frame[] = frameData.tagData.map((tag: { tagName: string; }) => {
              return this.createNewFrame(tag.tagName, 'Tag');
            });
            frames.push(...tagFrameData); // Add frames for tagData
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

  createNewFrame(frameName: string, frameType: string): Frame {
    const frame: Frame = {
      id: 0,
      position: { x: 100, y: 100 },
      size: { w: 400, h: 250 },
      frameName,
      objectData: [],
      tagData: []
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
