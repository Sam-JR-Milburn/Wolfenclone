// Filename: Window.cs
using System;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RenderEngine {

  public class RenderWindow : GameWindow {

    /// <remarks>
    /// RenderWindow is used like a singleton, it probably should act like one.
    /// </remarks>
    private static RenderWindow? Instance;
    public static RenderWindow? GetInstance(){
      return RenderWindow.Instance;
    }

    /// <remarks>
    /// Not as protected as it should be, but the Window
    /// should have a consistent reference, and be accessible from everywhere.
    /// </remarks>
    public static RenderWindow InitialiseInstance(NativeWindowSettings nws, double framerate){
      if(!(RenderWindow.Instance is RenderWindow)){
        RenderWindow.Instance = new RenderWindow(nws, framerate);
      }
      return RenderWindow.Instance;
    }

    /// <summary>
    /// Message observers: communicate to modules interested in RenderWindow messages.
    /// </summary>
    private List<IWindowObserver> Observers = new List<IWindowObserver>();
    private void SendToAllObservers(String message){
      foreach(IWindowObserver iwo in this.Observers){
        if(iwo != null){ iwo.Notify(message); }
      }
    }
    public void AddObserver(IWindowObserver iwo){
      this.Observers.Add(iwo);
    }

    /// <remarks> Called when the Window is first instantiated. </remarks>
    protected override void OnLoad(){
      base.OnLoad();
      GL.ClearColor(0.2f,0.2f,0.2f,1.0f);
    }

    /// <remarks> Nothing happening here yet. </remarks>
    protected override void OnUnload(){ base.OnUnload(); }

    /// <summary>
    /// Render graphics from the game engine's state.
    /// </summary>
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e); // Necessary.
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit);
      SwapBuffers();
      // MORE REQUIRED HERE.
    }

    /// <summary> Finalise Window: Notify objects that are listed. </summary>
    public override void Close(){
      // Send signal to GameRunner.
      this.SendToAllObservers("WINDOWCLOSE");
      // Close Window.
      base.Close();
    }





    /// <remarks>
    /// We must override Run() such that when the
    /// window close button is clicked, other threads can be notified.
    /// </remarks>
    public override void Run(){
      base.Run();
      if(this.IsExiting){
        // Send signal to GameRunner.
        this.SendToAllObservers("WINDOWCLOSE");

      }
    }

    /// <remarks>
    /// private: Singleton Pattern, controlled at the static level.
    /// Initialises NWS externally.
    /// </remarks>
    private RenderWindow(NativeWindowSettings nws, double framerate) : base (
      GameWindowSettings.Default, nws){
        base.UpdateFrequency = framerate; // 60.0 hz/fps: adjustable.
    }
  }
}
