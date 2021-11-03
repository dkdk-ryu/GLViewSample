using System;
using Tizen.NUI;
using Tizen.NUI.Components;
using Tizen.NUI.BaseComponents;
using OpenTK.Graphics.ES20;

namespace GLViewSample
{
    class Program : NUIApplication
    {
        private int glProgram;
        private int width;
        private int height;
        private float[] vVertices;
        private GLView glView;

        protected override void OnCreate()
        {
            base.OnCreate();
            Initialize();
        }

        void Initialize()
        {
            Window.Instance.KeyEvent += OnKeyEvent;
            Window.Instance.BackgroundColor = Color.White;
            // Window.Instance.GetDefaultLayer().Add(glView);

            View layoutView = new View();
            var linearLayout = new LinearLayout();
            linearLayout.LinearOrientation = LinearLayout.Orientation.Vertical;
            linearLayout.CellPadding = new Size(0, 0);
            layoutView.Layout = linearLayout;
            layoutView.WidthResizePolicy = ResizePolicyType.FillToParent;
            layoutView.HeightResizePolicy = ResizePolicyType.FillToParent;
            Window.Instance.GetDefaultLayer().Add(layoutView);

            TextLabel label = new TextLabel("GLView");
            label.TextColor = Color.White;
            label.BackgroundColor = Color.MidnightBlue;
            label.Weight = 0.05f;

            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.WidthResizePolicy = ResizePolicyType.FillToParent;
            label.HeightResizePolicy = ResizePolicyType.FillToParent;


            glView = new GLView(GLView.ColorFormat.RGBA8888);
            glView.Weight = 0.6f;
            glView.WidthResizePolicy = ResizePolicyType.FillToParent;
            glView.HeightResizePolicy = ResizePolicyType.FillToParent;

            glView.SetGraphicsConfig(true, true, 0, GLESVersion.Version20);
            glView.RenderingMode = GLRenderingMode.Continuous;
            glView.RegisterGLCallbacks(this.InitializeGL, this.RenderFrameGL, this.TerminateGL);
            glView.SetResizeCallback(this.ResizeCallback);

            Button textButton = new Button();
            textButton.BackgroundColor = Color.MidnightBlue;
            textButton.TextLabel.Text = "Stop";
            textButton.Weight = 0.05f;
            textButton.WidthResizePolicy = ResizePolicyType.FillToParent;
            textButton.HeightResizePolicy = ResizePolicyType.FillToParent;


            layoutView.Add(label);
            layoutView.Add(glView);
            layoutView.Add(textButton);
        }

        public void ResizeCallback(int w, int h)
        {
            width = w;
            height = h;
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

        public void InitializeGL()
        {
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
                    Tizen.Log.Debug("GLViewSsample", "Couldn't link program: " + infoLog.ToString());
                }

                GL.DeleteProgram(programObject);
                return;
            }

            // Store the program obejct
            glProgram = programObject;

            vVertices = new float[] { 0.0f, 0.5f, 0.0f, -0.5f, -0.5f, 0.0f, 0.5f, -0.5f, 0.0f };
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, vVertices);
            GL.EnableVertexAttribArray(0);
        }

        public int RenderFrameGL()
        {
            Tizen.Log.Debug("GLViewSample", "RenderFrameGL");

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            // Set the viewport
            GL.Viewport(0, 0, width, height);

            // Clear the color buffer
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Use the program object
            GL.UseProgram(glProgram);

            // Load the vertex data
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            return 1;
        }

        public void TerminateGL()
        {
            GL.DeleteProgram(glProgram);
        }

        public void OnKeyEvent(object sender, Window.KeyEventArgs e)
        {
            if (e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape")
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
