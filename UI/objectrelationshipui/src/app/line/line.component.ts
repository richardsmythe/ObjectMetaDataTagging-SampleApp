import { ChangeDetectorRef, Component, Input, OnChanges } from '@angular/core';
import { LineModel } from '../models/LineModel';

@Component({
  selector: 'app-line',
  templateUrl: './line.component.html',
  styleUrls: ['./line.component.css'],
})
export class LineComponent implements OnChanges {
  @Input() linesArray: LineModel[] = [];
  @Input() parentId: number | undefined; // Assuming parentId is of type number

  @Input() width: number = 0;
  @Input() height: number = 0;

  constructor(private cdRef: ChangeDetectorRef) {}
  
  ngOnChanges(): void {
    // Filter the linesArray based on the parentId to get the right lines for each instance
    if (this.parentId && this.linesArray.length > 0) {
      this.linesArray = this.linesArray.filter((line) => line.parentId === this.parentId);
      this.cdRef.detectChanges();
    }
  }
}
