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

    // DEBUG: A little cleaner.
    private Shader exampleShader;
    private int vertexBufferObject;
    private int vertexArrayObject;
    private float[] exampleVertices = {
        -0.5f, -0.5f, 0.0f, //Bottom-left vertex
         0.5f, -0.5f, 0.0f, //Bottom-right vertex
         0.0f,  0.5f, 0.0f  //Top vertex
    };


    /// <remarks> Called when the Window is first instantiated. </remarks>
    protected override void OnLoad(){
      base.OnLoad();
      GL.ClearColor(0.2f,0.2f,0.2f,1.0f);

      //DEBUG: A little cleaner.
      this.exampleShader = new Shader("res/exampleshader.vert", "res/exampleshader.frag");
      this.vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);

      GL.BufferData(
        BufferTarget.ArrayBuffer,
        exampleVertices.Length * sizeof(float),
        exampleVertices,
        BufferUsageHint.DynamicDraw);

      this.vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this.vertexArrayObject);

      // 1: location (manually specified for now), 2: size, 3: data type,
      // 4: normalise?, 5: exact sizeof, 6: 'offset'.
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      this.exampleShader.Use(); // Activate the Shader in memory.
    }

    /// <remarks> OpenGL Buffer Cleanup. </remarks>
    protected override void OnUnload(){
      base.OnUnload();

      //DEBUG: Shader cleanup (GPU Leak)
      // Null all references.
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GL.UseProgram(0);

      // Delete all resources.
      GL.DeleteBuffer(this.vertexBufferObject);
      GL.DeleteVertexArray(this.vertexArrayObject);
      GL.DeleteProgram(this.exampleShader.GetHandle());
    }

    /// <summary>
    /// Render graphics from the game engine's state.
    /// </summary>
    protected override void OnUpdateFrame(FrameEventArgs e){
      base.OnUpdateFrame(e); // Necessary.
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit);

      // DEBUG: A little cleaner. Wiggles back and forth!
      this.exampleShader.Use(); // Activate the Shader in memory.
      this.exampleVertices[6] = (float)(0.5 * Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds/750));
      // Copies data into the buffer, so this needs to be here when stuff changes.
      GL.BufferData(
        BufferTarget.ArrayBuffer,
        this.exampleVertices.Length * sizeof(float),
        this.exampleVertices,
        BufferUsageHint.DynamicDraw);

      GL.BindVertexArray(this.vertexArrayObject);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

      SwapBuffers(); // -- //
    }

    /// <remarks> Self-explanatory. </remarks>
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e){
      base.OnFramebufferResize(e);
      GL.Viewport(0, 0, e.Width, e.Height);
    }

    /// <summary> Finalise Window: Notify objects that are listed. </summary>
    public override void Close(){
      //this.SendToAllObservers("WINDOWCLOSE"); // Send signal to GameRunner.
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
