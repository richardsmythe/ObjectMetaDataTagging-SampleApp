import { ObjectModel } from './ObjectModel';
import { TagModel } from './TagModel';

export interface Frame {
  id: number;
  position: { x: number; y: number };
  size: { w: number; h: number };
  origin: string;
  frameType: string;
  objectData?: ObjectModel[] ;
  tagData: TagModel[];
}