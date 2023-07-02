import { Injectable } from '@angular/core';
import { Frame } from '../models/Frame';

@Injectable({
  providedIn: 'root'
})
export class FrameService {
  frames: Frame[] = [];

  constructor() { }


  createNewFrame(position: { x: number; y: number }, size: { w: number; h: number }): Frame {
    const frame: Frame = {
      id: this.generateUniqueId(),
      position,
      size,
      title: '',
      object: undefined,
      tags: []
    };
    this.frames.push(frame);
    return frame;
  }

  // placeholder for actual id
  private generateUniqueId(): number {
    const min = 0;
    const max = 100;
    const randomId = Math.floor(Math.random() * (max - min + 1)) + min;
    return randomId;
  }

  destroyFrame(frameId: number): void {
    const index = this.frames.findIndex(frame => frame.id === frameId);
    if (index !== -1) {
      this.frames.splice(index, 1);
    }
  }

}
