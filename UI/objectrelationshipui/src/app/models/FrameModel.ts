import { ObjectModel } from './ObjectModel';
import { TagModel } from './TagModel';

export interface Frame {
  counter: number;
  id: string;
  position: { x: number; y: number };
  size: { w: number; h: number };
  origin: string;
  frameType: string;
  objectData?: ObjectModel[] ;
  tagData: TagModel[];
}