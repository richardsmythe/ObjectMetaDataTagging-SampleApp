import { ChangeDetectorRef, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FrameService } from '../services/frame.service'; // Ensure you have the correct path here

@Component({
  selector: 'app-line',
  templateUrl: './line.component.html',
  styleUrls: ['./line.component.css'],
})
export class LineComponent implements OnChanges {
  @Input() parentId: number | undefined;
  @Input() childId: number | undefined;
  @Input() width: number = 0;
  @Input() height: number = 0;

  svgLeft: number | undefined;
  svgTop: number | undefined;
  lineX1: number | undefined;
  lineY1: number | undefined;
  lineX2: number | undefined;
  lineY2: number | undefined;

  constructor(private cdRef: ChangeDetectorRef, private frameService: FrameService) { // Inject FrameService here
  }

  ngOnInit() {
    this.updateLineData();
  }

  updateLineData() {

    if (this.parentId !== undefined && this.childId !== undefined) {
      const startingPosition = this.frameService.getCenterOfFrame(this.parentId);
      const endingPosition = this.frameService.getCenterOfFrame(this.childId);

      if (endingPosition && startingPosition) {
        const startX = startingPosition.x;
        const startY = startingPosition.y;

        const endX = endingPosition.x;
        const endY = endingPosition.y;

        this.svgLeft = Math.min(startX, endX) - this.width;
        this.svgTop = Math.min(startY, endY) - this.height;

        this.lineX1 = startX - this.svgLeft;
        this.lineY1 = startY - this.svgTop;
        this.lineX2 = endX - this.svgLeft;
        this.lineY2 = endY - this.svgTop;

        this.width = Math.abs(endX - startX) + this.width;
        this.height = Math.abs(endY - startY) + this.height;
      }
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    this.cdRef.detectChanges();
  }
}
