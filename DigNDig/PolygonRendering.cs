using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.GLFW;
using Camera;
using SecondaryRendering;
using StbImageSharp;

namespace Polygons
{
    public class PolygonsClass
    {
        private static uint _program;
        private static GL _gl = MainProgram.MainProgram._gl;
        private static uint _texture;
        private static uint _vaoTex3D;
        private static uint _vboTex3D;
        private static uint _eboTex3D;
        public unsafe static void DrawCube(float[] vertices, uint[] indices, string textureName)
        {
            Matrix4x4 __model = GetModelMatrix();

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

            uint vertexShader = shadeVertex3DTex();
            uint fragmentShader = shadeFragment3DTex();

            _program = _gl.CreateProgram();

            _gl.AttachShader(_program, vertexShader);
            _gl.AttachShader(_program, fragmentShader);

            _gl.LinkProgram(_program);

            int modelLoc = _gl.GetUniformLocation(_program,"_model");

            fixed(float* ptr1 = &_model.M11)
                _gl.UniformMatrix4(modelLoc,1,false,ptr1); 

            int viewLoc = _gl.GetUniformLocation(_program,"_view");

            fixed(float* ptr2 = &_view.M11)
                _gl.UniformMatrix4(viewLoc,1,false,ptr2); 
            
            int projLoc = _gl.GetUniformLocation(_program,"_proj");

            fixed(float* ptr3 = &_proj.M11)
                _gl.UniformMatrix4(projLoc,1,false,ptr3);

            Console.WriteLine($"modelLoc: {modelLoc}, viewLoc: {viewLoc}, projLoc: {projLoc}");

            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("LINK FAILED" + _gl.GetProgramInfoLog(_program));
                
            _gl.DetachShader(_program, vertexShader);
            _gl.DetachShader(_program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            const uint positionLoc = 3;
            _gl.EnableVertexAttribArray(positionLoc);
            _gl.VertexAttribPointer(positionLoc , 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);

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
            _gl.UseProgram(_program);
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
        }
        public static Matrix4x4 GetModelMatrix()
        {
            Matrix4x4 model = Matrix4x4.Identity;
            model *= Matrix4x4.CreateRotationX(-55.0f);
            return model;
        }

        public static Matrix4x4 _model = GetModelMatrix();
        public static Matrix4x4 _view = CameraClass.Matrix_view();
        public static Matrix4x4 _proj = CameraClass.Matrix_proj(CameraClass.aspectRatio,CameraClass.FOVdeg,CameraClass.nearP,CameraClass.farP);
        public static unsafe uint shadeVertex3DTex()
        {
            const string vertexCode = @"
            #version 330 core

            layout (location = 3) in vec3 aPosition;
            layout (location = 4) in vec2 aTexture3dCoord;

            uniform mat4 _model;
            uniform mat4 _proj;
            uniform mat4 _view;

            out vec2 frag_tex3dCoords;

            void main()
            {
                gl_Position = _proj * _view * _model * vec4(aPosition, 1.0);
                frag_tex3dCoords = vec2(aTexture3dCoord.x, 1.0 - aTexture3dCoord.y);
            }";

            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, vertexCode);

            _gl.CompileShader(vertexShader);

            _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int) GLEnum.True)
                throw new Exception("VERTEX FAILED" + _gl.GetShaderInfoLog(vertexShader));
            return vertexShader;
        }

        public static unsafe uint shadeFragment3DTex()
        {
            const string fragmentCode = @"
            #version 330 core

            uniform sampler2D uTexture;

            in vec2 frag_tex3dCoords;

            out vec4 out_color;
            
            void main()
            {
                out_color = vec4(frag_tex3dCoords.x, frag_tex3dCoords.y, 0.0, 1.0);
            //   -out_color = texture(uTexture, frag_tex3dCoords);
            }";

            int location = _gl.GetUniformLocation(_program, "uTexture");
            _gl.Uniform1(location, 0);

            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, fragmentCode);

            _gl.CompileShader(fragmentShader);

            _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int) GLEnum.True)
                throw new Exception("FRAGMENT FAILED" + _gl.GetShaderInfoLog(fragmentShader));

            return fragmentShader;
        }
    }
}