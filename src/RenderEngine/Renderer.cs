// Filename: Renderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RenderEngine {

  /// <remarks> ... </remarks>
  public class Renderer : IDisposable {
    // --
    private double _aspectRatio;

    /// <remarks> Container for Shaders. </remarks>
    // -- HERE -- //

    /// <summary> Consistent references to OpenGL render resources. </summary>
    private int vertexBufferObject;
    private int vertexArrayObject;
    private int elementBufferObject;

    // DEBUG: Texture work.
    private Shader exampleShader;
    private Texture exampleTexture;

    // DEBUG: More texture work.
    //private Shader exampleShader1;
    //private Texture exampleTexture1;

    // -- //
    float[] vertices = {
      //Position                Texture coordinates
         0.0f,  0.5f, 0.0f,     1.0f, 1.0f, // top right
         0.0f, -0.5f, 0.0f,     1.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f,     0.0f, 0.0f, // bottom left
        -0.5f,  0.5f, 0.0f,     0.0f, 1.0f  // top left
    };

    uint[] indices = {
      0, 1, 3,
      1, 2, 3
    };

    /// <remarks> Maybe, a later 'load assets' function calling this. </remarks>
    private bool LoadShaders(){
      /// <remarks> At some point, there should be a JSON list describing these assets.</remarks>
      try {
        this.exampleTexture = new Texture("res/sandstone-2.png");
        this.exampleShader = new Shader("res/textureshader.vert", "res/textureshader.frag");

      } catch(GraphicsException){
        return false;
      }
      return true;
    }

    /// <remarks> ... </remarks>
    private bool Initialise(){
      GL.ClearColor(0.1f,0.1f,0.1f,1.0f);
      // Load resources, like Shaders and Textures.
      if(!this.LoadShaders()) { return false; }

      // Build the VAO (Contains VBO refs).
      this.vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this.vertexArrayObject);

      // Build the Vertex Buffer (VBO).
      this.vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferObject);

      GL.BufferData(BufferTarget.ArrayBuffer,
        this.vertices.Length * sizeof(float),
        this.vertices,
        BufferUsageHint.StaticDraw);

      // Build the EBO.
      this.elementBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);

      GL.BufferData(BufferTarget.ElementArrayBuffer,
        this.indices.Length * sizeof(uint),
        this.indices,
        BufferUsageHint.StaticDraw);

      // --
      this.exampleShader.Use();
      int vertexLocation = this.exampleShader.GetAttribLocation("aPosition");
      int texCoordLocation = this.exampleShader.GetAttribLocation("aTexCoord");

      /// <remarks> Inform OpenTK how to render this textured vertex data. </remarks>
      GL.EnableVertexAttribArray(vertexLocation);
      GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 0);
      GL.EnableVertexAttribArray(texCoordLocation);
      GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 3 * sizeof(float));
      // -- //

      GL.Enable(EnableCap.DepthTest); // z-buffer.

      return true; // Notice of intention, right now.
    }

    /// <remarks> Clean up resources, track the state of that. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
      if(!this._disposed){

        // Null all references.
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.UseProgram(0);
        // Delete all resources
        GL.DeleteBuffer(this.vertexBufferObject);
        GL.DeleteVertexArray(this.vertexArrayObject);
        GL.DeleteBuffer(this.elementBufferObject);
        // Delete Shader assets.
        GL.DeleteProgram(this.exampleShader.GetHandle());

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

    /// <summary> Renders a frame worth of graphics. </summary>
    /// <remarks> Should take some game state, and use OpenGL calls to represent that. </remarks>
    public void Render(){
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Color and z-buffer.
      //GL.Clear(ClearBufferMask.ColorBufferBit);

      // --
      Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
      Vector3 cameraTarget;
      Vector3 cameraDirection;


      // Rotate the object downwards.
      //Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0.0f));

      Matrix4 model = Matrix4.CreateRotationX(
        MathHelper.DegreesToRadians((float)(DateTime.Now.TimeOfDay.TotalMilliseconds/100)));

      // Moving camera forward? Translate object towards us.
      Matrix4 view = Matrix4.CreateTranslation(0.0f, 0.0f, -5.0f);
      // -- // -- // -- // -- // -- // -- // -- // -- //
      int Width = 1366; int Height = 768;
      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(45.0f),
        Width / Height, 0.1f, 100.0f);
      this.exampleShader.SetMatrix4("model", model);
      this.exampleShader.SetMatrix4("view", view);
      this.exampleShader.SetMatrix4("projection", projection);


      // Activate GPU resources, draw with them.
      GL.BindVertexArray(this.vertexArrayObject);
      this.exampleTexture.Use();
      this.exampleShader.Use();

      // -- EBO --
      //GL.DrawElements(PrimitiveType.Triangles, this.indices.Length, DrawElementsType.UnsignedInt, 0);

      float[] newVertices = {
          -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
           0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
           0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
           0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
          -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
          -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

          -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
           0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
           0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
           0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
          -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
          -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

          -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
          -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
          -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
          -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
          -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
          -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

           0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
           0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
           0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
           0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
           0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
           0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

          -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
           0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
           0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
           0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
          -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
          -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

          -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
           0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
           0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
           0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
          -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
          -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
      };
      // --//
      GL.BufferData(BufferTarget.ArrayBuffer,
        newVertices.Length * sizeof(float),
        newVertices,
        BufferUsageHint.StaticDraw);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

      // Swap buffers: render with the window.
      RenderWindow? window = RenderEngine.RenderWindow.GetInstance();
      if(window != null) { window.SwapBuffers(); }
    }

    /// <remarks> ... </remarks>
    public Renderer(int width, int height) : this(height/width){
      if(height == 0){ throw new DivideByZeroException(); }
      //if(width <= 0 || height <= 0){ throw new IllegalArgumentException("The height or width can't be less than 1."); }
    }

    /// <remarks> Initialise before a Renderer reference can be used. </remarks>
    public Renderer(double aspectRatio){
      this._aspectRatio = aspectRatio;
      this.Initialise();
    }

  }
}
