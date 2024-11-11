// Filename: Window.cs
using System;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RenderEngine {

  public class Window : GameWindow {

    /// <summary> Message observers: communicate to other threads. </summary>
    private List<IWindowObserver> Observers = new List<IWindowObserver>();
    public void AddObserver(IWindowObserver iwo){
      this.Observers.Add(iwo);
    }

    /// <remarks> Called when the Window is first instantiated. </remarks>
    protected override void OnLoad(){
      base.OnLoad();
      GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
    }

    /// <remarks> Nothing happening here yet. </remarks>
    protected override void OnUnload(){
      // --
    }

    /// --

    /// <summary>
    /// Take controls, pass them to an interpeter.
    /// Render graphics from game engine state.
    /// </summary>
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e);
      // Pass controls to interpreter. (SHIFT-ESC for now)
      if((KeyboardState.IsKeyDown(Keys.LeftShift)) && KeyboardState.IsKeyDown(Keys.Escape)){
        this.Close();
      }

      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit);
      SwapBuffers();
      // MORE REQUIRED HERE.
    }

    /// <summary> Finalise Window: Notify objects that are listed. </summary>
    public override void Close(){
      // Send signal to GameRunner. (extensible in future)
      foreach(IWindowObserver iwo in this.Observers){
        iwo.Notify("WINDOWCLOSE");
      }
      // Close Window.
      base.Close();
    }

    /// <remarks>
    /// The key functionality is run by GameWindow,
    /// but we want to pass it the NWS externally from settings.
    /// </remarks>
    public Window(NativeWindowSettings nws, double framerate) : base (
      GameWindowSettings.Default,
      nws){
        base.UpdateFrequency = framerate; // 60.0 hz/fps: adjustable.
    }
  }
}
