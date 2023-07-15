export interface LineModel {
    parentId: number;
    childId: number[];
    startingPosition: { x: number; y: number };
    endingPosition: { x: number; y: number };
  }