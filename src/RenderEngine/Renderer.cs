// Filename: Renderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Windowing.Common.Input; // DEBUG: Mouse Cursor.

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

    /// DEBUG: Move me elsewhere later.
    private Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 5.0f);
    private Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 cameraUp = new Vector3(0.0f, 1.0f,  0.0f);
    private float Yaw = 0.0f; private float Pitch = 0.0f;
    Vector2 lastMousePos = new Vector2(0,0);




    // DEBUG: Texture work.
    private Shader exampleShader;
    private Texture exampleTexture;

    // DEBUG: More texture work.
    private Texture exampleTexture1;

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
        this.exampleTexture1 = new Texture("res/sandstone-1.jpg");
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

      GL.Enable(EnableCap.DepthTest); // Enable z-buffer.

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
    public void Render(FrameEventArgs e){
      // Run Graphics.
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Color and z-buffer.
      RenderWindow? window = RenderEngine.RenderWindow.GetInstance();
      if(window == null){ throw new InvalidOperationException("No window reference in Renderer!"); }

      // Local Space -> World Space translation.
      Matrix4 model = Matrix4.CreateTranslation(5.5f,-1.25f,5.5f);

      // -- //
      KeyboardState input = window.KeyboardState; float speed = 0.25f;
      if(input.IsKeyDown(Keys.W)){
        this.cameraPosition += this.cameraFront * speed; // * (float)e.Time
      }
      if(input.IsKeyDown(Keys.S)){
        this.cameraPosition -= this.cameraFront * speed; // * (float)e.Time
      }
      if(input.IsKeyDown(Keys.A)){
        this.cameraPosition -= Vector3.Normalize(Vector3.Cross(this.cameraFront, this.cameraUp)) * speed; // * (float)e.Time
      }
      if(input.IsKeyDown(Keys.D)){
        this.cameraPosition += Vector3.Normalize(Vector3.Cross(this.cameraFront, this.cameraUp)) * speed; // * (float)e.Time
      }

      // Camera Look-Around //
      Vector2 mouse = window.MousePosition;
      float deltaX = mouse.X - this.lastMousePos.X;
      float deltaY = mouse.Y - this.lastMousePos.Y;
      this.lastMousePos = new Vector2(mouse.X, mouse.Y);
      float sensitivity = 0.02f;
      this.Yaw += (deltaX * sensitivity);
      //this.Pitch -= (deltaY * sensitivity);
      // Lock pitch, prevent flipping.
      if(this.Pitch > 89.9f){
          this.Pitch = 89.9f;
      } else if(this.Pitch < -89.9f){
          this.Pitch = -89.9f;
      } else{
          this.Pitch -= deltaY * sensitivity;
      }


      this.cameraFront.X =
        (float)Math.Cos(MathHelper.DegreesToRadians(this.Pitch)) *
        (float)Math.Cos(MathHelper.DegreesToRadians(this.Yaw));
      this.cameraFront.Y =
        (float)Math.Sin(MathHelper.DegreesToRadians(this.Pitch));
      this.cameraFront.Z =
        (float)Math.Cos(MathHelper.DegreesToRadians(this.Pitch)) *
        (float)Math.Sin(MathHelper.DegreesToRadians(this.Yaw));
      this.cameraFront = Vector3.Normalize(this.cameraFront);

      // World Space -> Camera Space translation.
      Matrix4 view = Matrix4.LookAt(
        this.cameraPosition,
        this.cameraPosition+this.cameraFront,
        this.cameraUp
      );

      // Camera Space -> Clip Space culling.
      int Width = 1366; int Height = 768;
      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(45.0f),
        Width / Height, 0.01f, 100.0f);
      this.exampleShader.SetMatrix4("model", model);
      this.exampleShader.SetMatrix4("view", view);
      this.exampleShader.SetMatrix4("projection", projection);

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
      // -- //
      // Activate GPU resources, draw with them.
      GL.BindVertexArray(this.vertexArrayObject);
      this.exampleTexture.Use();
      this.exampleShader.Use();
      GL.BufferData(BufferTarget.ArrayBuffer,
        newVertices.Length * sizeof(float),
        newVertices,
        BufferUsageHint.StaticDraw);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

      // -- // Two cubes? // -- //
      // --
      model = Matrix4.CreateTranslation(7.5f,-6.25f,7.5f); // World-space.
      model *= Matrix4.CreateTranslation(
        0.0f,
        (float)Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds/1000),
        0.0f);
      this.exampleShader.SetMatrix4("model", model);
      this.exampleShader.SetMatrix4("view", view);
      this.exampleShader.SetMatrix4("projection", projection);
      this.exampleTexture1.Use();
      this.exampleShader.Use();
      GL.BufferData(BufferTarget.ArrayBuffer,
        newVertices.Length * sizeof(float),
        newVertices,
        BufferUsageHint.StaticDraw);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

      // Swap buffers: render with the window.
      window.SwapBuffers();
    }

    /// <remarks> ... </remarks>
    public Renderer(int width, int height) : this((double)(height / width)){
      if(height == 0){ throw new DivideByZeroException(); }
      if(width <= 0 || height <= 0){ throw new ArgumentException("The height or width can't be less than 1."); }
    }

    /// <remarks> Initialise before a Renderer reference can be used. </remarks>
    public Renderer(double aspectRatio){
      this._aspectRatio = aspectRatio;
      this.Initialise();
    }

  }
}
