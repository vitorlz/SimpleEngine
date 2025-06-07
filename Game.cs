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
                Profile = ContextProfile.Core
            }) 
        {
            CursorState = CursorState.Grabbed;
            VSync = VSyncMode.Off;
        }

        private Shader s;
        private bool wireFrameMode = false;
        Camera _cam;
        //private Chunk chunk;
        private ChunkManager _chunkManager;

        protected override void OnLoad()
        {
            base.OnLoad();

            s = new Shader(FileHelper.FromProjectRoot("shaders/vertex.vert"), FileHelper.FromProjectRoot("shaders/fragment.frag"));

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.039f, 0.8f, 1f, 1.0f);

            Transform camTransform = new Transform();

            camTransform.position = new Vector3(0f, 64f, 0f);
            camTransform.rotation = Quaternion.Identity;
            camTransform.scale = new Vector3(1.0f);

            _cam = new Camera(camTransform);
            _chunkManager = new ChunkManager(_cam);
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
            
            _cam.Update(args.Time, KeyboardState, MouseState);
            _chunkManager.Update();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // the order of multiplication is different here because opentk's math library is row major and glsl is column major
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)this.ClientSize.X / (float)this.ClientSize.Y, 0.1f, 2000.0f);
            Matrix4 view = _cam.ViewMatrix;

            // we transpose all matrices because we want to send them in column-major format to glsl
            proj.Transpose();
            view.Transpose();

            s.Use();
            s.SetMat4("v", view);
            s.SetMat4("p", proj);
           
            _chunkManager.RenderActiveChunks();
            
            SwapBuffers();
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}
