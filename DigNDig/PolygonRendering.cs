using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.GLFW;
using Camera;
using StbImageSharp;

namespace Polygons
{
    public class PolygonsClass
    {
        public static uint _program;
        private static GL _gl = MainProgram.MainProgram._gl;
        private static Glfw _glfw = Glfw.GetApi();
        private static uint _texture;
        private static uint _vaoTex3D;
        private static uint _vboTex3D;
        private static uint _eboTex3D;
        public unsafe static void DrawCube(float[] vertices,uint[] indices, string textureName, Vector3 position, float variation)
        {
            _model = GetModelMatrix(position,variation);
            _view = Camera.Camera.MatrixViewCamera(CameraClass._camPos,CameraClass._cameraFront,CameraClass._camUp1);
            _vaoTex3D = _gl.GenVertexArray();
            _gl.BindVertexArray(_vaoTex3D);

            _vboTex3D = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboTex3D);

            fixed (float* buf = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

            _eboTex3D = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _eboTex3D);

            fixed (uint* buf = indices)
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            uint vertexShader = PolygonShader.shadeVertex3DTex();
            uint fragmentShader = PolygonShader.shadeFragment3DTex();

            _program = _gl.CreateProgram();

            _gl.AttachShader(_program, vertexShader);
            _gl.AttachShader(_program, fragmentShader);

            _gl.LinkProgram(_program);

            _gl.UseProgram(_program);
            
            int modelLoc = _gl.GetUniformLocation(_program,"_model");

            fixed(float* ptr1 = &_model.M11)
                _gl.UniformMatrix4(modelLoc,1,false,ptr1); 

            int viewLoc = _gl.GetUniformLocation(_program,"_view");

            fixed(float* ptr2 = &_view.M11)
                _gl.UniformMatrix4(viewLoc,1,false,ptr2); 
            
            int projLoc = _gl.GetUniformLocation(_program,"_proj");

            fixed(float* ptr3 = &_proj.M11)
                _gl.UniformMatrix4(projLoc,1,false,ptr3);

            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("LINK FAILED" + _gl.GetProgramInfoLog(_program));
                
            _gl.DetachShader(_program, vertexShader);
            _gl.DetachShader(_program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            const uint positionLoc = 3;
            _gl.EnableVertexAttribArray(positionLoc);
            _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);

            const uint texCoordLoc = 4;
            _gl.EnableVertexAttribArray(texCoordLoc);
            _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

            _texture = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);

            ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(textureName), ColorComponents.RedGreenBlueAlpha);

            fixed (byte* ptr = result.Data)
                _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                    (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Nearest);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

            _gl.BindTexture(TextureTarget.Texture2D,_texture);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static unsafe void RenderCube()
        {
            _gl.BindVertexArray(_vaoTex3D);
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);
            _gl.DrawArrays(GLEnum.Triangles,0,36);
        }

        public static float[] GetCubeVertexMatrix()
        {
            float[] verticesC = 
            {
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

            return verticesC;
        }
        public static uint[] GetCubeIndicesMatrix()
        {
            uint[] indicesC = 
            {
                0,1,3,
                1,2,3
            };

            return indicesC;
        }
        public static Matrix4x4 GetModelMatrix(Vector3 position, float variation)
        {
            float degrees = -55.0f;
            float radians = degrees * (float)Math.PI/180.0f;
            Matrix4x4 model = Matrix4x4.Identity;
            model *= Matrix4x4.CreateRotationX((float)_glfw.GetTime() * radians * variation);
            model *= Matrix4x4.CreateRotationY((float)_glfw.GetTime() * (radians/2) * variation);
            model *= Matrix4x4.CreateTranslation(position);
            return model;
        }

        public static Matrix4x4 _model;
        public static Matrix4x4 _view = Camera.Camera.MatrixViewCamera(CameraClass._camPos,CameraClass._cameraFront,CameraClass._camUp1);
        public static Matrix4x4 _proj = CameraClass.Matrix_proj(CameraClass.aspectRatio,CameraClass.FOVdeg,CameraClass.nearP,CameraClass.farP);
    }
}