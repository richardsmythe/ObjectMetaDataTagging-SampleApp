import { Component } from '@angular/core';
import { FrameService } from './services/frame.service';
import { Frame } from './models/FrameModel';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  frames: Frame[] = [];

  constructor(private frameService: FrameService) {}

  ngOnInit(): void {
    this.frameService.getFrameData().subscribe((data: Frame[]) => {
      this.frames = data;
    });
  }
}