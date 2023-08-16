import { ChangeDetectorRef, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { LineModel } from '../models/LineModel';

@Component({
  selector: 'app-line',
  templateUrl: './line.component.html',
  styleUrls: ['./line.component.css'],
})
export class LineComponent implements OnChanges {
  @Input() linesArray: LineModel[] = [];
  @Input() parentId: number | undefined; 
  @Input() startingPosition: { x: number; y: number; } | undefined;
  @Input() endingPosition: { x: number; y: number; } | undefined;
  @Input() width: number = 0;
  @Input() height: number = 0;
  svgLeft: number | undefined;
  svgTop: number| undefined;
  lineX1: number| undefined;
  lineY1: number| undefined;
  lineX2: number| undefined;
  lineY2: number| undefined;
  constructor(private cdRef: ChangeDetectorRef) {
  
  }

 ngOnInit() {

  if (this.endingPosition && this.startingPosition) {
    // Calculate the midpoints of the starting and ending boxes
    const startX = this.startingPosition.x + this.width / 2;
    const startY = this.startingPosition.y + this.height / 2;
    
    const endX = this.endingPosition.x + this.width / 2;
    const endY = this.endingPosition.y + this.height / 2;
  
    // The SVG's top-left corner should be at the minimum x,y 
    this.svgLeft = Math.min(startX, endX) - this.width / 2;
    this.svgTop = Math.min(startY, endY) - this.height / 2;
  
    // Line coordinates relative to the SVG's top-left corner
    this.lineX1 = startX - this.svgLeft;
    this.lineY1 = startY - this.svgTop;
    this.lineX2 = endX - this.svgLeft;
    this.lineY2 = endY - this.svgTop;
  
    // SVG's dimensions should be the distance between the two midpoints + width and height
    this.width = Math.abs(endX - startX) + this.width;
    this.height = Math.abs(endY - startY) + this.height;
  }
}
  
  ngOnChanges(changes: SimpleChanges){  

      this.cdRef.detectChanges();
 
  
  
    // Filter the linesArray based on the parentId to get the right lines for each instance
    // console.log("LINESARRAY:",this.linesArray);
    // console.log("PARENT ID:", this.parentId);
    // if (this.parentId && this.linesArray.length > 0) {
    //   this.linesArray = this.linesArray.filter((line) => line.parentId === this.parentId);
    //   this.cdRef.detectChanges();
    // }
  }
}

