// Filename: Shader.cs
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine {

  /// <remarks>
  /// The Shader class is designed to instantiate objects in the GPU,
  /// hold their reference, and delete them when they're not needed, to avoid memory leaks.
  /// </remarks>
  public class Shader : IDisposable {
    private bool Disposed = false;
    private int Handle = 0; // OpenGL 'handle': references GPU memory.

    /// <summary> Runs the Shader 'program' in the GPU. </summary>
    public void Use(){
      if(this.Handle != -1){ GL.UseProgram(Handle); }
    }

    /// <remarks> Frees GPU memory. </remarks>
    protected virtual void Dispose(bool disposing){
        if (!this.Disposed){
            GL.DeleteProgram(Handle);
            this.Disposed = true;
        }
    }

    /// <remarks> Keeps track of resource leaks. </remarks>
    ~Shader(){
        if(this.Disposed == false){
            File.AppendAllText("logfile",
              "GPU resource leak from Shader: "+this.Handle+"at "+DateTime.Now+"\n"); // DEBUG: Logger
        }
    }

    /// <remarks> Free GPU resources - inherited from IDisposable. </remarks>
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

      // Compile Shaders.
      int success;

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
      this.Handle = GL.CreateProgram();
      GL.AttachShader(this.Handle, vertexShader); GL.AttachShader(this.Handle, fragmentShader);
      GL.LinkProgram(this.Handle);
      GL.GetProgram(this.Handle, GetProgramParameterName.LinkStatus, out success);
      if(success == 0){
        string infoLog = GL.GetProgramInfoLog(this.Handle);
        File.AppendAllText("logfile", "GPU program creation: "+infoLog+"\n"); // DEBUG: Logger
        throw new GraphicsException(infoLog);
      }

      // Cleanup, now that the Shader is loaded into GPU memory.
      GL.DetachShader(this.Handle, vertexShader);
      GL.DetachShader(this.Handle, fragmentShader);
      GL.DeleteShader(vertexShader);
      GL.DeleteShader(fragmentShader);
    }

  }
}
