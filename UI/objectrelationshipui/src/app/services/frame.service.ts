import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FrameService {
  frames: { position: { x: number; y: number }; size: { w: number; h: number } }[] = [];

  constructor() { }


  createNewFrame(position: { x: number; y: number }, size: { w: number; h: number }): void {
    this.frames.push({ position, size });
  }


}
