export interface TagModel {
  tagId:string;
  associatedObjectId: string;
  associatedObject: string;
  tagName: string
  description: string;
  childTags?: TagModel[];
  relatedFrames?: string[];
  }