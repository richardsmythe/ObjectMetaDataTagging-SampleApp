import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FrameService } from './services/frame.service';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FrameComponent } from './frame/frame.component';

@NgModule({
  declarations: [
    AppComponent,
    FrameComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule
  ],
  providers: [FrameService],
  bootstrap: [AppComponent]
})
export class AppModule { }
