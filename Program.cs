using System;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Tizen;

namespace GLViewSample
{
    class Program : NUIApplication
    {
        private int glProgram;
        private int width;
        private int height;
        private int numTouched;
        private float clearColor;
        GLView glView;

        protected override void OnCreate()
        {
            base.OnCreate();
            Initialize();
        }

        void Initialize()
        {
            Window.Instance.KeyEvent += OnKeyEvent;
            glView = new GLView(GLView.ColorFormat.RGBA8888);
            glView.WidthResizePolicy = ResizePolicyType.FillToParent;
            glView.HeightResizePolicy = ResizePolicyType.FillToParent;

            glView.SetGraphicsConfig(true, true, 0, GLView.GLESVersion.Version_2_0);
            glView.RenderingMode = Tizen.NUI.GLRenderingMode.Continuous;
            glView.RegisterGLCallback(this.initialize_gl, this.renderFrame_gl, this.terminate_gl);
            glView.SetResizeCallback(this.ResizeCallback);

            glView.TouchEvent += OnTouchEvent;
            Window.Instance.GetDefaultLayer().Add(glView);

            numTouched = 0;
        }

        public void ResizeCallback(int x, int y)
        {
            // Tizen.Log.Error("DKDK", "int Callback REsized before width" + x + " " + y);
            width = x;
            height = y;
        }

        private bool OnTouchEvent(object sender, View.TouchEventArgs e)
        {
            if (e.Touch.GetState(0) == PointStateType.Up)
            {
                numTouched++;
                if (numTouched % 2 == 1)
                {
                    glView.Size = new Size(320, 320);
                }
                else
                {
                    glView.Size = new Size(720, 1280);
                }

                Tizen.Log.Error("DKDK", "callback OnTouchEvnet " + width + " " + height);
            }

            return true;
        }

        int LoadShader(ShaderType type, string shaderSrc)
        {
            int shader;

            // Create the shader object
            shader = GL.CreateShader(type);

            if (shader == 0)
            {
                return 0;
            }

            // Load the shader source
            GL.ShaderSource(shader, 1, new string[] { shaderSrc }, (int[])null);

            // Comoppile the shader
            GL.CompileShader(shader);

            // Check the compile status
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);

            if (compiled == 0)
            {
                int length = 0;

                GL.GetShader(shader, ShaderParameter.InfoLogLength, out length);

                if (length > 0)
                {
                    GL.GetShaderInfoLog(shader, length, out length, out string log);
                }

                GL.DeleteShader(shader);
                throw new InvalidOperationException("Unable to compile shader of type : " + type.ToString());
            }

            return shader;
        }

        public void initialize_gl()
        {
            Tizen.Log.Error("DKDK", "InitCallback");

            //Load the shaders and get a linked program object
            string vShaderStr =
                "attribute vec4 vPosition;  \n" +
                "void main()                              \n" +
                "{                                        \n" +
                "   gl_Position = vPosition;              \n" +
                "}                                        \n";

            string fShaderStr =
                "void main()                                     \n" +
                "{                                               \n" +
                "   gl_FragColor = vec4 ( 1.0, 0.0, 0.0, 1.0 );  \n" +
                "}                                               \n";

            int vertexShader;
            int fragmentShader;
            int programObject;

            // Load the vertex/fragment shaders
            vertexShader = LoadShader(ShaderType.VertexShader, vShaderStr);
            fragmentShader = LoadShader(ShaderType.FragmentShader, fShaderStr);

            // Create the program object
            programObject = GL.CreateProgram();

            if (programObject == 0)
            {
                return;
            }

            GL.AttachShader(programObject, vertexShader);
            GL.AttachShader(programObject, fragmentShader);

            //Link the program
            GL.LinkProgram(programObject);

            // Check the link status
            int linkStatus;
            GL.GetProgram(programObject, GetProgramParameterName.LinkStatus, out linkStatus);

            if (linkStatus == 0)
            {
                int infoLogLength;
                GL.GetProgram(programObject, GetProgramParameterName.InfoLogLength, out infoLogLength);

                if (infoLogLength > 0)
                {
                    String infoLog;
                    GL.GetProgramInfoLog(programObject, infoLogLength, out infoLogLength, out infoLog);
                    Console.WriteLine("GL2", "Couldn't link program: " + infoLog.ToString());
                }

                GL.DeleteProgram(programObject);
                return;
            }

            // Store the program obejct
            glProgram = programObject;
            clearColor = 1.0f;
            GL.ClearColor(clearColor, clearColor, clearColor, 1.0f);
        }

        public int renderFrame_gl()
        {
            GL.ClearColor(clearColor, clearColor, clearColor, 1.0f);
            if (clearColor < 0)
            {
                clearColor = 1.0f;
            }
            else
            {
                clearColor -= 0.01f;
            }

            float[] vVertices = new float[] { 0.0f, 0.5f, 0.0f, -0.5f, -0.5f, 0.0f, 0.5f, -0.5f, 0.0f };

            // SEt the viewport
            GL.Viewport(0, 0, width, height);

            // Clear the color buffer
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Use the program object
            GL.UseProgram(glProgram);

            // Load the vertex data
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, vVertices);

            GL.EnableVertexAttribArray(0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            return 1;
        }

        public void terminate_gl()
        {
        }

        public void OnKeyEvent(object sender, Window.KeyEventArgs e)
        {
            if (e.Key.State == Key.StateType.Down && (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
            {
                Exit();
            }
        }

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run(args);
        }
    }
}
