// Filename: Renderer.cs

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTK.Windowing.Common.Input; // DEBUG: Mouse Cursor.
using Misc; // Logger.

namespace RenderEngine {

  /// <remarks> ... </remarks>
  public class Renderer : IDisposable {
    private double _aspectRatio; // -- //

    /// <remarks> Container for Textures + Vertices. </remarks>
    // -- HERE -- //

    /// <summary> Consistent references to OpenGL render resources. </summary>
    /// <remarks> No need for ElementBufferObject any longer. </remarks>
    private int _vertexBufferObject;
    private int _vertexArrayObject;

    /// <summary>
    /// This Shader GPU program takes a model matrix, a view matrix and a projection matrix.
    /// It also takes Textures in an on-line way.
    /// </summary>
    private Shader _textureShader;

    /// DEBUG: Move me elsewhere later.
    private Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 5.0f);
    private Vector3 cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 cameraUp = new Vector3(0.0f, 1.0f,  0.0f);
    private float Yaw = 0.0f; private float Pitch = 0.0f;
    Vector2 lastMousePos = new Vector2(0,0);

    // DEBUG: Get rid of me.
    private Texture exampleTexture;
    private Texture exampleTexture1;

    /// <summary> Load the shader and texture assets from self-describing data. </summary>
    private void LoadAssets(){
      String resDir = "res/"; String textureDir = "textures/";
      // Used for most rendering.
      try {
        this._textureShader = new Shader(
          resDir+"textureshader.vert",
          resDir+"textureshader.frag");
      } catch {
        Logger.LogToFile("Couldn't load the texture shader."); throw;
      }
      // Load from a dictionary.
      this.exampleTexture = new Texture(resDir+textureDir+"sandstone-1.jpg");
      this.exampleTexture1 = new Texture(resDir+textureDir+"sandstone-2.png");
    }

    /// <remarks> ... </remarks>
    private void Initialise(){
      GL.ClearColor(0.1f,0.1f,0.1f,1.0f);

      // Build the VAO (Contains VBO refs).
      this._vertexArrayObject = GL.GenVertexArray();
      GL.BindVertexArray(this._vertexArrayObject);
      // Build the Vertex Buffer (VBO).
      this._vertexBufferObject = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);

      /// <remarks> Inform OpenTK how to render this textured vertex data. </remarks>
      int vertexLocation      = this._textureShader.GetAttribLocation("aPosition");
      int texCoordLocation    = this._textureShader.GetAttribLocation("aTexCoord");

      GL.EnableVertexAttribArray(vertexLocation); // Object vertices [3D].
      GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 0);

      GL.EnableVertexAttribArray(texCoordLocation); // Texture vertices [2D].
      GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
        5 * sizeof(float), 3 * sizeof(float));

      GL.Enable(EnableCap.DepthTest); // Enable z-buffer.
    }

    /// <remarks> Clean up resources, track the state of that. </remarks>
    private bool _disposed = false;
    protected virtual void Dispose(bool disposing){
      if(this._disposed){ return; }
      // Null all references.
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GL.UseProgram(0);
      // Delete all resources
      GL.DeleteBuffer(this._vertexBufferObject);
      GL.DeleteVertexArray(this._vertexArrayObject);
      // Delete the texture shader.
      GL.DeleteProgram(this._textureShader.GetHandle());

      /// Flag prevents calls made between disposition and deconstruction.
      this._disposed = true;
    }
    /// <summary> Publically accessible disposition. </summary>
    public void Dispose(){
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }
    /// <remarks> Just here to report deconstruction without disposition. </remarks>
    ~Renderer(){
      if(!this._disposed){
        Logger.LogToFile("GPU resource leak from Renderer "+"at "+DateTime.Now);
      }
    }

    /// <summary> Renders a frame worth of graphics. </summary>
    /// <remarks> Should take some game state, and use OpenGL calls to represent that. </remarks>
    public void Render(){
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
      this._textureShader.SetMatrix4("model", model);
      this._textureShader.SetMatrix4("view", view);
      this._textureShader.SetMatrix4("projection", projection);

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
      GL.BindVertexArray(this._vertexArrayObject);

      this.exampleTexture.Use();
      this._textureShader.Use();

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
      this._textureShader.SetMatrix4("model", model);
      this._textureShader.SetMatrix4("view", view);
      this._textureShader.SetMatrix4("projection", projection);
      this.exampleTexture1.Use();
      this._textureShader.Use();
      GL.BufferData(BufferTarget.ArrayBuffer,
        newVertices.Length * sizeof(float),
        newVertices,
        BufferUsageHint.StaticDraw);
      GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

      // Swap buffers: render with the window.
      window.SwapBuffers();
    }

    /// <remarks> Pass the aspect ratio through constructor chaining. </remarks>
    public Renderer(int width, int height) : this((double)(height / width)){
      if(height == 0){ throw new DivideByZeroException(); }
      if(width <= 0 || height <= 0){ throw new ArgumentException("The height or width can't be less than 1."); }
    }

    /// <summary> Build and establish the rendering portion of the RenderEngine </summary>
    public Renderer(double aspectRatio){
      this._aspectRatio = aspectRatio;
      try {
        this.LoadAssets();
      } catch {
        Logger.LogToFile("Renderer failed with LoadAssets.");
        throw;
      }
      // This shouldn't happen, but the compiler won't shut up about references on exit.
      if(!(this._textureShader is Shader)){
        throw new NullReferenceException("TextureShader is not available. ");
      }
      this.Initialise(); // Build the OpenGL objects.
    }
  }
}
