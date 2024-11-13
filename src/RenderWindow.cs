// Filename: RenderWindow.cs
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



    //DEBUG
    private int VertexBufferObject;
    private int VertexArrayObject;
    private Shader shader;


    /// <remarks> Called when the Window is first instantiated. </remarks>
    protected override void OnLoad(){
      base.OnLoad();
      GL.ClearColor(0.2f,0.2f,0.2f,1.0f);

      //DEBUG ???
      this.VertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

      
      float[] vertices = {
          -0.5f, -0.5f, 0.0f, //Bottom-left vertex
           0.5f, -0.5f, 0.0f, //Bottom-right vertex
           0.0f,  0.5f, 0.0f  //Top vertex
      };

      GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
      // DEBUG: Shader.
      this.shader = new Shader("res/exampleshader.vert", "res/exampleshader.frag");
      // DEBUG:
      this.VertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this.VertexArrayObject);
      // DEBUG: 0 - Shader index, 3 - Size, false: normalisation flag, last val (0): offset.
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

      // DEBUG
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
      GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);

      //shader.Use();


    }

    /// <remarks> Nothing happening here yet. </remarks>
    protected override void OnUnload(){
      base.OnUnload();

      //DEBUG: Shader cleanup (GPU Leak)
      // If we're using some kind of resource loader, dispose of that here.
      this.shader.Dispose();
      File.AppendAllText("logfile", "unload at: "+DateTime.Now+"\n"); // DEBUG: Logger
      //DEBUG
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.DeleteBuffer(this.VertexBufferObject);
    }

    /// <remarks> Self-explanatory. </remarks>
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e){
      base.OnFramebufferResize(e);
      GL.Viewport(0,0,e.Width,e.Height);
    }


    /// <summary>
    /// Render graphics from the game engine's state.
    /// </summary>
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e); // Necessary.
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit);

      //DEBUG
      shader.Use();
      GL.BindVertexArray(VertexArrayObject);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


      // MORE REQUIRED HERE.
      SwapBuffers();
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
