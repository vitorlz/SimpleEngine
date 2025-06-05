using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleEngine.Res;
using SimpleEngine.Helper;
using System.Threading.Tasks.Dataflow;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using SimpleEngine.Assets;
using SimpleEngine.Cam;
using SimpleEngine.Types;
using OpenTK.Windowing.Common.Input;
using SimpleEngine.Voxels;

namespace SimpleEngine.Core
{
    class Game : GameWindow
    {
        public Game(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings() { 
                ClientSize = (width, height), 
                Title = title,

                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 6),
                Profile = ContextProfile.Core,
            }) 
        {
            CursorState = CursorState.Grabbed;
            VSync = VSyncMode.On;
        }

        private Shader s;
        private int VAO;
        private int VBO;
        private int EBO;
        private bool wireFrameMode = false;
        Stopwatch time = new Stopwatch();
        Camera cam;
        //private Chunk chunk;
        private ChunkManager chunkManager;

        Texture container;
        protected override void OnLoad()
        {
            base.OnLoad();

            s = new Shader(FileHelper.FromProjectRoot("shaders/vertex.vert"), FileHelper.FromProjectRoot("shaders/fragment.frag"));

            GL.Enable(EnableCap.DepthTest);
       
            GL.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);

            //GL.Enable(EnableCap.CullFace);

            container = new Texture(FileHelper.FromProjectRoot("textures/container.jpg"));

            Transform camTransform = new Transform();

            camTransform.Position = new Vector3(0f, 64f, 0f);
            camTransform.Rotation = Quaternion.Identity;
            camTransform.Scale = new Vector3(1.0f);

            cam = new Camera(camTransform);
            chunkManager = new ChunkManager(cam);

            time.Start();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
            


            if(KeyboardState.IsKeyPressed(Keys.L))
            {
                if(wireFrameMode)
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                }
                else
                {
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                }

                wireFrameMode = !wireFrameMode;
            }
            

            cam.Update(args.Time, KeyboardState, MouseState);
           
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)800 / (float)600, 0.1f, 1000.0f);

            // the order of multiplication is different here because opentk's math library is row major and glsl is column major
            // Matrix4 model = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = cam.ViewMatrix;

            // we transpose all matrices because we want to send them in column-major format to glsl
            proj.Transpose();
            model.Transpose();
            view.Transpose();

            s.Use();
            
            s.SetMat4("m", model);
            s.SetMat4("v", view);
            s.SetMat4("p", proj);

            s.SetInt("tex", 0);
            container.Use();

            chunkManager.Update();
            chunkManager.RenderActiveChunks();

            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
