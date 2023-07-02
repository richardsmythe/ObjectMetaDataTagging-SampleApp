import { Component } from '@angular/core';
import { FrameService } from './services/frame.service';
import { Frame } from './models/Frame';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  frames: Frame[] = [];

  constructor(private frameService: FrameService) {}

  createNewFrame(): void {
    const position = { x: 200, y: 200 };
    const size = { w: 200, h: 200 };
    const frame = this.frameService.createNewFrame(position, size);
    this.frames.push(frame);
  }
}