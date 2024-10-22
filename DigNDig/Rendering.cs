using System;
using Silk;
using Silk.NET;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata.Ecma335;
using System.Drawing;
using System.Numerics;
using Camera;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Metrics;

namespace MainProgram
{
    public class MainProgram
    {
        private static uint _program;

        private static uint _programT;

        public static GL _gl;
        private static uint _vao;
        private static uint _vbo;
        private static uint _ebo;

        public static bool isWindowRunning = false;

        private static uint _vaoT;
        private static uint _vboT;
        private static uint _eboT;
        
        public static Vector3 _cameraPosition;

        static Vector3 point1rect = new Vector3(0.5f,0.5f,0.0f);
        static Vector3 point2rect = new Vector3(0.5f,-0.5f,0.0f);
        static Vector3 point3rect = new Vector3(-0.5f,-0.5f,0.0f);
        static Vector3 point4rect = new Vector3(-0.5f,0.5f,0.0f);
        static float[] vertices = 
        {
            point1rect.X,point1rect.Y,point1rect.Z,
            point2rect.X,point2rect.Y,point2rect.Z,
            point3rect.X,point3rect.Y,point3rect.Z,
            point4rect.X,point4rect.Y,point4rect.Z
        };

        static Vector3 point1tri = new Vector3(-0.3f,0.3f,0.0f);
        static Vector3 point2tri = new Vector3(-0.3f,-0.3f,0.0f);
        static Vector3 point3tri = new Vector3(0.3f,-0.3f,0.0f);
        static float[] verticesT = 
        {
            point1tri.X,point1tri.Y,point1tri.Z,
            point2tri.X,point2tri.Y,point2tri.Z,
            point3tri.X,point3tri.Y,point3tri.Z
        };

        static float[] verticesTs = 
        {
            -0.03f,0.03f,0.0f,
            -0.03f,-0.03f,0.0f,
            0.03f,-0.03f,0.0f,
        };

        static uint[] indices = 
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        static uint[] indicesT = 
        {
            0u, 1u, 2u
        };

        static Vector2 point1texrect = new Vector2(1.0f,0.0f);
        static Vector2 point2texrect = new Vector2(1.0f,1.0f);
        static Vector2 point3texrect = new Vector2(0.0f,1.0f);
        static Vector2 point4texrect = new Vector2(0.0f,0.0f);
        static Vector2 point1textri = new Vector2(1.0f,1.0f);
        static Vector2 point2textri = new Vector2(1.0f,0.0f);
        static Vector2 point3textri = new Vector2(0.0f,0.0f);

        static float[] verticesTtex = 
        {
            point1tri.X,point1tri.Y,point1tri.Z, point1textri.X, point1textri.Y,
            point2tri.X,point2tri.Y,point2tri.Z, point2textri.X, point2textri.Y,
            point3tri.X,point3tri.Y,point3tri.Z,  point3textri.X, point3textri.Y
        };

        static float[] verticestex = 
        {
            point1rect.X,point1rect.Y,point1rect.Z, point1texrect.X, point1texrect.Y,
            point2rect.X,point2rect.Y,point2rect.Z, point2texrect.X, point2texrect.Y,
            point3rect.X,point3rect.Y,point3rect.Z, point3texrect.X, point3texrect.Y,
            point4rect.X,point4rect.Y,point4rect.Z, point4texrect.X, point4texrect.Y
        };

        static float[] multiplyVert =
        {
            2, 2, 2, 2, 2,
            2, 2, 2, 2, 2,
            2, 2, 2, 2, 2,
            2, 2, 2, 2, 2
        };
        public static IWindow _mainWindow;
        public static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 600);
            options.Title = "Dig N' Dig";
            options.VSync = false;
            options.FramesPerSecond = 18;

            _mainWindow = Window.Create(options);

            _mainWindow.Load += OnWindowLoad;

            _mainWindow.Render += OnWindowRenderDelta;

            _mainWindow.Resize += OnWindowChangedSize;

            _mainWindow.Run();

            isWindowRunning = true;
        }

        public static float GetAspectRatio()
        {
            if (isWindowRunning == true)
            {
                float aspectRatio = _mainWindow.Size.X / _mainWindow.Size.Y;
                return aspectRatio;
            } else {
                return 1.7066f;
            }
        }
        private static unsafe void OnWindowLoad()
        {
            _gl = GL.GetApi(_mainWindow);

            IInputContext input = _mainWindow.CreateInput();

            for (int i = 0; i < input.Keyboards.Count; i++)
                input.Keyboards[i].KeyDown += KeyDown;

            _gl.Viewport(0,0,(uint)CameraClass.width,(uint)CameraClass.height);
        }

        private static int counter;
        private static unsafe void OnWindowRenderDelta(double delta)
        {
            RenderHere(delta);
            _mainWindow.SwapBuffers();
            RenderHere(delta);
        }
        private static unsafe void RenderHere(double delta)
        {
            _gl.ClearColor(0.2f,0.25f,0.6f,1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            Polygons.PolygonsClass.DrawCube(verticestex,indices,"resources/textures/epicface.png");

            Polygons.PolygonsClass.RenderCube();
        }
        private static void KeyDown(IKeyboard keyboard, Key key, int keycode)
        {
            Vector3 dir = new Vector3();
            Vector3 _cameraTarget = Camera.CameraClass._camTarget;
            Vector3 _cameraUp = Camera.CameraClass._camUp1;
            Vector3 _cameraOrientation = Camera.CameraClass._cameraOri;

            if (key == Key.Escape)
                _mainWindow.Close();
            
            if (key == Key.W)
            {
                dir = Vector3.Normalize(_cameraTarget - _cameraPosition);
                _cameraPosition += CameraClass._cameraOri * CameraClass.speed;
            }

            if (key == Key.S)
            {
                dir = Vector3.Normalize(_cameraTarget - _cameraPosition);
                _cameraPosition += -CameraClass._cameraOri * CameraClass.speed;
            }

            if (key == Key.A)
            {
                dir = Vector3.Normalize(_cameraTarget - _cameraPosition);
                _cameraPosition += -Vector3.Normalize(Vector3.Cross(CameraClass._cameraOri,CameraClass._camUp1)) * CameraClass.speed;
            }

            if (key == Key.D)
            {
                dir = Vector3.Normalize(_cameraTarget - _cameraPosition);
                _cameraPosition += Vector3.Normalize(Vector3.Cross(CameraClass._cameraOri,CameraClass._camUp1)) * CameraClass.speed;
            }

            if (key == Key.Space)
            {
                dir = Vector3.Normalize(_cameraTarget - _cameraPosition);
                _cameraPosition += CameraClass._camUp1 * CameraClass.speed;
            }
        }
        private static void OnWindowChangedSize(Vector2D<int> size)
        {
            _mainWindow.Size = new Vector2D<int>(1024, 600);
        }
    }
}