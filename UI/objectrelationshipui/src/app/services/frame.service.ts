import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, of, switchMap } from 'rxjs';
import { Frame } from '../models/FrameModel';
import { ObjectModel } from '../models/ObjectModel';
import { TagModel } from '../models/TagModel';
import { LineModel } from '../models/LineModel';
import { response } from 'express';

@Injectable({
  providedIn: 'root'
})
export class FrameService {
  private initialisedFramesCounter = 0;
  public frames: BehaviorSubject<Frame[]> = new BehaviorSubject<Frame[]>([]);
  public lines: BehaviorSubject<LineModel[]> = new BehaviorSubject<LineModel[]>([]);
  private frameIdCounter: number = 1;
  public allFramesInitialised: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor(private http: HttpClient) { }

  public frameInitialised(): void {
    this.initialisedFramesCounter++;
    const frames = this.frames.getValue();

    if (this.initialisedFramesCounter === frames.length) {
      console.log(this.initialisedFramesCounter, 'frames initialised');
      this.updateLinePositions();
      this.allFramesInitialised.next(true);
    }
  }

  getFrameData(): Observable<Frame[]> {
    return this.http.get<any[]>('https://localhost:7170/api/Tag').pipe(
      switchMap(response => {
        const frames = this.processFrameData(response);
        this.frames.next(frames);
        return of(frames);
      }),
      catchError(error => {
        console.error('Error fetching frame data:', error);
        return of([]);
      })
    );
  }

  getParentFramesByTagId(tagFrame: Frame): Frame[] {
    const parentFrames: Frame[] = [];

    if (tagFrame && tagFrame.objectData && tagFrame.tagData) {
      const associatedObjectIds = tagFrame.tagData.map((tag) => tag?.associatedObjectId).filter(Boolean);

      if (associatedObjectIds.length > 0) {
        const frames = this.frames.getValue();

        for (const associatedObjectId of associatedObjectIds) {
          const associatedObjectFrames = frames.filter(
            (f) => f.frameType === 'Object' && f.objectData?.some((obj) => obj.id === associatedObjectId)
          );
          parentFrames.push(...associatedObjectFrames);
        }
      }
    }
    return parentFrames;
  }

  private processFrameData(response: any[]): Frame[] {
    const frames: Frame[] = [];

    response.forEach(frameData => {
      if (frameData.objectData) {
        frameData.objectData.forEach((object: ObjectModel) => {
          const objectFrame = this.createNewFrame([object], [], 'Object', frameData.origin);
          frames.push(objectFrame);

          frameData.tagData.forEach((tag: any) => {
            if (tag.associatedObjectId === object.id) {
              const tagFrame = this.createNewFrame([], [tag], 'Tag', frameData.origin);
              frames.push(tagFrame);
            }
            if (tag.childTags) {
              tag.childTags.forEach((childTag: any) => {

                // create childTagModel
                const childTagModel: TagModel = {
                  tagId: childTag.id,
                  tagName: childTag.name,
                  associatedObject: childTag.associatedParentObjectName,
                  associatedObjectId: childTag.associatedParentObjectId,
                  description: childTag.description,
                };

                console.log("Child Tag Model:", childTagModel);
                const childTagFrame = this.createNewFrame([], [childTagModel], 'Tag', frameData.origin);
                frames.push(childTagFrame);
              });
            }
          });
        });
      }
    });
    console.log("RESULT:", frames);
    return frames;
  }

  createNewFrame(objectData: ObjectModel[], tagData: TagModel[], frameType: string, origin: string): Frame {
    const frame: Frame = {
      counter: this.frameIdCounter++,
      id: frameType === 'Tag' ? tagData[0].tagId : objectData[0].id,
      size: this.calculateFrameSize(frameType === 'Tag' ? [tagData[0]] : [objectData[0]], frameType === 'Tag'),
      position: this.calculateFramePosition(),
      frameType,
      origin,
      objectData,
      tagData
    };

    if(frameType==="Object"){
  
      frame.objectData?.forEach(obj => {
        obj.relatedFrames = this.getAssociatedTagIds(obj.id);
        console.log(obj.relatedFrames);
      });
    }
    if(frameType==="Tag"){
      frame.tagData?.forEach(tag => {
        tag.relatedFrames = this.getAssociatedTagIds(tag.tagId);
        console.log(tag.relatedFrames);
      });
    }


    const currentFrames = this.frames.value.slice();
    currentFrames.push(frame);

    this.frames.next(currentFrames);

    return frame;
  }

  destroyFrame(frameId: string): void {
    const currentFrames = this.frames.value.slice().filter((f) => f.id !== frameId);
    const currentFrame = this.getFrameById(frameId);
    const tagId = currentFrame?.tagData[0]?.tagId;

    if (!tagId) {
      return;
    }

    this.http.delete<any[]>(`https://localhost:7170/api/Tag/?tagId=${tagId}`).subscribe({
      next: (response) => {
        this.frames.next(currentFrames);
        this.updateLinePositions();
      },
      error: (error) => {
        console.error('HTTP Delete Error:', error);
      },
    });
  }


  getLines(): Observable<LineModel[]> {
    return this.lines.asObservable();
  }

  getFrames(): Observable<Frame[]> {
    return this.frames.asObservable();
  }

  calculateFramePosition(): { x: number, y: number } {
    const margin = 20;
    const containerWidth = window.innerWidth;
    const containerHeight = window.innerHeight;
    const frames = this.frames.value;

    let posX = 10;
    let posY = 10;
    let overlapping = true;

    while (overlapping) {
      overlapping = false;

      for (const frame of frames) {
        const currentFrameSize = this.getFrameSize(frame.id);
        const currentFrameWidth = currentFrameSize?.width || 0;
        const currentFrameHeight = currentFrameSize?.height || 0;

        if (
          posX < frame.position.x + currentFrameWidth + margin && posX + margin > frame.position.x
          &&
          posY < frame.position.y + currentFrameHeight + margin && posY + margin > frame.position.y
        ) {
          overlapping = true;
          break;
        }
      }

      if (overlapping) {

        // Move to the next row if there is not enough space in the current row
        if (posX + margin > containerWidth) {
          posX = margin;
          posY += margin;

        } else if (posY + margin > containerHeight) {
          // Stop generating positions if it exceeds the container height
          break;
        } else {
          // Shift the position horizontally
          posX += margin;
        }
      }
    }

    return { x: posX, y: posY };
  }

  calculateFrameSize(data: ObjectModel[] | TagModel[], isTagType: boolean): { w: number, h: number } {
    let width = 0;
    let height = 0;
    if (Array.isArray(data)) {
      for (const item of data) {
        if (isTagType) {
          const tag = item as TagModel;
          width = Math.max(width, tag?.description?.length > 20 ? 320 : 320);;
          height = 200;
        } else {
          const object = item as ObjectModel;
          const objectIdLength = object?.id?.toString().length || 0;
          width = Math.max(width, objectIdLength > 30 ? 350 : 200);
          height = 200;
        }
      }
    }
    return { w: width, h: height };
  }

  getFrameById(frameId: string): Frame | undefined {
    const frames = this.frames.value;
    const frame = frames.find(fr => fr.id === frameId);
    return frame !== undefined ? frame : undefined;
  }

  updateFramePosition(position: { x: number; y: number }, frameId: string | undefined): void {
    const frames = this.frames.getValue();
    const frameIndex = frames.findIndex((frame) => frame.id === frameId);
    if (frameIndex !== -1) {
      frames[frameIndex].position = { ...position };
      this.frames.next(frames);

    }
  }

  updateFrameSize(newSize: { w: number; h: number }, frameId: string | undefined): void {
    const frames = this.frames.getValue();
    const updatedFrames = frames.map(frame =>
      frame.id === frameId ? { ...frame, size: newSize } : frame
    );
    this.frames.next(updatedFrames);
  }

  getFrameSize(frameId: string | undefined): { width: number, height: number } | undefined {
    if (frameId === undefined) {
      return undefined;
    }
    const frames = this.frames.getValue();
    const frameIndex = frames.findIndex((frame) => frame.id === frameId);

    if (frameIndex !== -1) {
      return { width: frames[frameIndex].size.w, height: frames[frameIndex].size.h };
    }
    return undefined;
  }

  getCenterOfFrame(frameId: string): { x: number, y: number } | undefined {
    const frame = this.getFrameById(frameId);
    if (frame) {
      return {
        x: frame.position.x + frame.size.w / 2,
        y: frame.position.y + frame.size.h / 2
      };
    }
    return undefined;
  }

  getFramePosition(frameId: string): { x: number; y: number } | undefined {
    const frame = this.getFrameById(frameId);
    if (frame && frame.position) {
      //console.log("CURRENT frameId:",frameId, "framePosition: ",frame.position);
      return { x: frame.position.x, y: frame.position.y };
    }
    return undefined;
  }

  getAssociatedTagIds(objectId: string): string[] {
    const allFrames = this.frames.getValue();
    let associatedFrames: string[] = [];
  
    // Direct associations
    associatedFrames = allFrames
      .filter(frame => frame.tagData?.some(tag => tag.associatedObjectId === objectId))
      .filter(frame => frame.frameType === 'Tag')
      .map(frame => frame.id);
  
    // If no direct associations, check associations through child tags
    if (associatedFrames.length === 0) {
      
      associatedFrames = allFrames
        .filter(frame => frame.tagData?.some(tag => tag.childTags?.some(childTag => childTag.associatedObjectId === objectId)))
        .filter(frame => frame.frameType === 'Tag')
        .map(frame => frame.id);
    }
  
     console.log("assoc. frames", associatedFrames);
    return associatedFrames;
  }

  updateLinePositions(): void {
    const frames = this.frames.getValue();
    const lines: LineModel[] = [];

    for (const frame of frames) {
      if (frame.frameType === 'Object') {
        const startingPosition = frame.position;
        const childIds = frame.objectData ? this.getAssociatedTagIds(frame.objectData[0].id) : [];

        childIds.forEach(childId => {
          // Check if the associated tag frame still exists
          const associatedTagFrame = frames.find(f => f.id === childId && f.frameType === 'Tag');
          if (associatedTagFrame) {
            const endingPosition = this.getFramePosition(associatedTagFrame.id) || { x: 0, y: 0 };
            lines.push({ parentId: frame.id, childId: [associatedTagFrame.id], startingPosition, endingPosition });
          }
        });
      }
    }
    this.lines.next(lines);
  }

}
