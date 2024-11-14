// Filename: Renderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RenderEngine {

  /// <remarks> ... </remarks>
  public class Renderer : IDisposable {
    /// <remarks> Container for Shaders. </remarks>
    // -- HERE -- //

    // DEBUG: A little cleaner.
    private Shader exampleShader;
    private Shader exampleShader1;
    private int vertexBufferObject;
    private int vertexArrayObject;
    private float[] exampleVertices = {
        -0.5f, -0.5f, 0.0f, //Bottom-left vertex
         0.5f, -0.5f, 0.0f, //Bottom-right vertex
         0.0f,  0.5f, 0.0f  //Top vertex
    };

    /// <remarks> ... </remarks>
    private bool LoadShaders(){
      /// <remarks> At some point, there should be a JSON list describing these assets.</remarks>
      this.exampleShader = new Shader("res/exampleshader.vert", "res/exampleshader.frag");
      this.exampleShader1 = new Shader("res/exampleshader0.vert", "res/exampleshader0.frag");
      return true;
    }

    /// <remarks> ... </remarks>
    private bool Initialise(){
      GL.ClearColor(0.1f,0.1f,0.1f,1.0f);

      //DEBUG: A little cleaner.
      this.LoadShaders();
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

      GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(1);

      return true; // Notice of intention, right now.
    }

    /// <remarks> Clean up resources, track the state of that. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
      if(!this._disposed){
        // Loop here, to dispose of Shaders and any other IDisposables.
        // -- //
        // Null all references.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        // Delete all resources.
        GL.DeleteBuffer(this.vertexBufferObject);
        GL.DeleteVertexArray(this.vertexArrayObject);
        GL.DeleteProgram(this.exampleShader.GetHandle());
        GL.DeleteProgram(this.exampleShader1.GetHandle());

        /// Flag prevents calls made between disposition and deconstruction.
        this._disposed = true;
      }
    }
    /// <remarks> Publically accessible disposition. </remarks>
    public void Dispose(){
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }
    /// <remarks> Just here to report deconstruction without disposition. </remarks>
    ~Renderer(){
      if(!this._disposed){
        File.AppendAllText("logfile",
          "GPU resource leak from Renderer "+"at "+DateTime.Now+"\n"); // DEBUG: Logger
      }
    }

    /// <remarks> Should take some game state, and use OpenGL calls to represent that. </remarks>
    public void Render(){
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
      GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // Draw green triangle.

      // DEBUG

      float[] exampleVertices1 = {
          -0.5f, -0.5f, 0.0f, //Bottom-left vertex
           0.5f, -0.5f, 0.0f, //Bottom-right vertex
           0.0f,  -0.25f, 0.0f  //Top vertex
      };
      exampleVertices1[7] = -0.25f + (float)(0.25f * Math.Sin(Math.PI+DateTime.Now.TimeOfDay.TotalMilliseconds/750));
      this.exampleShader1.Use();
      GL.BufferData(
        BufferTarget.ArrayBuffer,
        exampleVertices1.Length * sizeof(float),
        exampleVertices1,
        BufferUsageHint.DynamicDraw);
      GL.BindVertexArray(this.vertexArrayObject);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // Draw white triangle.

      // OpenGL is quite global, but the window must be notified.
      RenderEngine.RenderWindow.GetInstance().SwapBuffers();
    }

    /// <remarks> Initialise before a Renderer reference can be used. </remarks>
    public Renderer(){
      this.Initialise();
    }

  }
}
