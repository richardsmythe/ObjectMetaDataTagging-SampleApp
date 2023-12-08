export interface LineModel {
  parentId: string;
  childId: string[];
  startingPosition: { x: number; y: number };
  endingPosition: { x: number; y: number };
}