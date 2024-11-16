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


    /// <summary> Consistent references to OpenGL render resources. </summary>
    private int vertexBufferObject;
    private int vertexArrayObject;
    private int elementBufferObject;

    // DEBUG: A little cleaner.
    private Shader exampleShader;
    private Shader exampleShader1;

    // DEBUG: Element Section.
    // DEBUG: Working on Element Buffers.
    float[] vertices = {
         0.0f,  0.5f, 0.0f,  // top right
         0.0f, -0.5f, 0.0f,  // bottom right
        -0.5f, -0.5f, 0.0f,  // bottom left
        -0.5f,  0.5f, 0.0f   // top left
    };
    uint[] indices = {
      0, 1, 3,
      1, 2, 3
    };

    /// <remarks> Maybe, a later 'load assets' function calling this. </remarks>
    private bool LoadShaders(){
      /// <remarks> At some point, there should be a JSON list describing these assets.</remarks>
      try {
        this.exampleShader = new Shader("res/exampleshader.vert", "res/exampleshader.frag");
        this.exampleShader1 = new Shader("res/exampleshader0.vert", "res/exampleshader0.frag");
      } catch(GraphicsException){
        return false;
      }

      return true;
    }

    /// <remarks> ... </remarks>
    private bool Initialise(){
      GL.ClearColor(0.1f,0.1f,0.1f,1.0f);

      // Load resources, like Shaders and Textures.
      this.LoadShaders();

      // Build the Vertex Buffer (VBO).
      this.vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);

      // Build the VAO.
      this.vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this.vertexArrayObject);

      /// <remarks> Register Shaders with the VAO.</remarks>
      // 1: location (manually specified), 2: size, 3: data type,
      // 4: normalise?, 5: exact sizeof, 6: 'offset'.
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      GL.EnableVertexAttribArray(0);
      //GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      //GL.EnableVertexAttribArray(1);

      // Build the EBO.
      this.elementBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);
      /*
      GL.BufferData(
        BufferTarget.ElementArrayBuffer,
        indices.Length * sizeof(uint),
        indices, BufferUsageHint.StaticDraw);
      */

      return true; // Notice of intention, right now.
    }

    /// <remarks> Clean up resources, track the state of that. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
      if(!this._disposed){
        // Loop here, to dispose of Shaders and any other IDisposables.

        // Null all references.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        // Delete all resources.
        GL.DeleteBuffer(this.vertexBufferObject);
        GL.DeleteVertexArray(this.vertexArrayObject);
        //GL.DeleteElementBuffer(this.elementBufferObject); //What's the correct one here?
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

      //
      this.exampleShader.Use(); // Activate the Shader in memory.

      // top-right
      this.vertices[1] = 0.0f + Math.Abs((float)(0.5 * Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds/2000)));
      // bottom-right
      this.vertices[4] = 0.0f - Math.Abs((float)(0.5 * Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds/2000)));

      // Buffer to VAO
      GL.BufferData(
        BufferTarget.ArrayBuffer,
        vertices.Length * sizeof(float),
        vertices,
        BufferUsageHint.DynamicDraw);

      // Buffer to EBO.
      GL.BufferData(
        BufferTarget.ElementArrayBuffer,
        indices.Length * sizeof(uint),
        indices, BufferUsageHint.DynamicDraw);

      // Draw with EBO.
      GL.DrawElements(
        PrimitiveType.Triangles,
        indices.Length,
        DrawElementsType.UnsignedInt,
        0);


      // OpenGL is 'global', but the changes appear only if our window's buffers are swapped.
      RenderWindow window = RenderEngine.RenderWindow.GetInstance();
      if(window != null) { window.SwapBuffers(); }
      //RenderEngine.RenderWindow.GetInstance().SwapBuffers();
    }

    /// <remarks> Initialise before a Renderer reference can be used. </remarks>
    public Renderer(){
      this.Initialise();
    }

  }
}
