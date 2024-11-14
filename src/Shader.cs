// Filename: Shader.cs
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine {

  /// <remarks>
  /// The Shader class is designed to instantiate objects in the GPU,
  /// hold their reference, and delete them when they're not needed, to avoid memory leaks.
  /// </remarks>
  public class Shader : IDisposable {
    private bool _disposed = false;
    private int _handle; // OpenGL 'handle': references GPU memory.
    public int GetHandle(){
      return this._handle;
    }

    /// <summary> Runs the Shader 'program' in the GPU. </summary>
    public void Use(){
      GL.UseProgram(_handle);
    }

    /// <remarks> At runtime, we can find the location reference. </remarks>
    public int GetAttribLocation(String attribName){
      return GL.GetAttribLocation(this._handle, attribName);
    }

    /// <remarks> Frees GPU memory, protected - inherited from IDisposable. </remarks>
    protected virtual void Dispose(bool disposing){
        if (!this._disposed){
            GL.DeleteProgram(_handle);
            this._disposed = true;
        }
    }

    /// <remarks> Keeps track of resource leaks. </remarks>
    ~Shader(){
        if(!this._disposed){
            File.AppendAllText("logfile",
              "GPU resource leak from Shader: "+this._handle+" at "+DateTime.Now+"\n"); // DEBUG: Logger
        }
    }

    /// <remarks> Frees GPU memory, public - inherited from IDisposable. </remarks>
    public void Dispose(){
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary> Loads vertex and fragment shader data and combines them. </summary>
    public Shader(String vertexPath, String fragmentPath){
      if(!File.Exists(vertexPath) || !File.Exists(fragmentPath)){
        throw new FileNotFoundException("Couldn't load "+vertexPath+" or "+fragmentPath+".");
      }
      String vertexSource = File.ReadAllText(vertexPath);
      String fragmentSource = File.ReadAllText(fragmentPath);

      // Generate Shaders.
      int vertexShader = GL.CreateShader(ShaderType.VertexShader);
      GL.ShaderSource(vertexShader, vertexSource);
      int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
      GL.ShaderSource(fragmentShader, fragmentSource);


      int success; // Compile Shaders.

      GL.CompileShader(vertexShader);
      GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
      if(success == 0){ // Vertex.
        string infoLog = GL.GetShaderInfoLog(vertexShader);
        File.AppendAllText("logfile", "Issue with vertex shader compilation: "+infoLog+"\n"); // DEBUG: Logger
        throw new GraphicsException(infoLog);
      }

      GL.CompileShader(fragmentShader);
      GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
      if(success == 0){ // Fragment.
        string infoLog = GL.GetShaderInfoLog(fragmentShader);
        File.AppendAllText("logfile", "Issue with fragment shader compilation: "+infoLog+"\n"); // DEBUG: Logger
        throw new GraphicsException(infoLog);
      }

      // Generate GPU 'program'.
      this._handle = GL.CreateProgram();
      GL.AttachShader(this._handle, vertexShader); GL.AttachShader(this._handle, fragmentShader);
      GL.LinkProgram(this._handle);
      GL.GetProgram(this._handle, GetProgramParameterName.LinkStatus, out success);
      if(success == 0){
        string infoLog = GL.GetProgramInfoLog(this._handle);
        File.AppendAllText("logfile", "GPU program creation: "+infoLog+"\n"); // DEBUG: Logger
        throw new GraphicsException(infoLog);
      }

      // Cleanup, now that the Shader is loaded into GPU memory.
      GL.DetachShader(this._handle, vertexShader);
      GL.DetachShader(this._handle, fragmentShader);
      GL.DeleteShader(vertexShader);
      GL.DeleteShader(fragmentShader);
    }

  }
}
