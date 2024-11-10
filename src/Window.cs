// Filename: Window.cs
using System;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RenderEngine {

  public class Window : GameWindow {

    /// --
    protected override void OnLoad(){
      base.OnLoad();
      GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
      // --
    }

    /// --
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit);
      SwapBuffers();
      // --
      if(KeyboardState.IsKeyDown(Keys.Escape)){
				this.Close();
			}
    }

    /// --
    public Window(NativeWindowSettings nws, double framerate) : base (
      GameWindowSettings.Default,
      nws){
        // --
        base.UpdateFrequency = framerate;
    }

  }
}
