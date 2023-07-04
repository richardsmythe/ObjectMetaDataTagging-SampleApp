import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Frame } from '../models/Frame';

@Injectable({
  providedIn: 'root'
})
export class FrameService {
  frames: Frame[] = [];

  constructor(private http: HttpClient) { }

  getFrameData(): void {
    this.http.get<Frame[]>('https://localhost:7170/api/Tag').subscribe(
      (response: Frame[]) => {
        this.frames = response.map(frameData => this.createNewFrame(frameData));
      },
      (error) => {
        console.error('Error fetching frame data:', error);
      }
    );
  }

  createNewFrame(frameData: Frame): Frame {
    const frame: Frame = {
      id: frameData.id,
      position: {x: 100, y: 100},
      size: {w: 200, h: 200},
      title: frameData.title,
      object: frameData.object,
      tags: frameData.tags
    };
    this.frames.push(frame);
    return frame;
  }
 

  
  destroyFrame(frameId: number): void {
    console.log(frameId);
    const index = this.frames.findIndex(frame => frame.id === frameId);
    console.log(index)
    if (index !== -1) {
      this.frames.splice(index, 1); 
    }
  }

}
