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
  frameCount: number = 0;

  constructor(private frameService: FrameService) {}

  
    ngOnInit(): void {
      this.frameService.getFrameData();
    } 
}