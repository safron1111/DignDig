using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace SecondaryRendering
{
    public class TextureRendering
    {
        private static uint _texture;
        static GL _gl = MainProgram.MainProgram._gl;
        private static uint _vaoTex;
        private static uint _vboTex;
        private static uint _eboTex;
        public static uint _programTex;
        public static unsafe void DrawTexRectangle(float[] vertices, uint[] indices, string textureName)
        {
            _vaoTex = _gl.GenVertexArray();
            _gl.BindVertexArray(_vaoTex);

            _vboTex = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboTex);

            fixed (float* buf = vertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

            _eboTex = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _eboTex);

            fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);

            uint vertexShader = TextureShader.shadeVertexTex();
            uint fragmentShader = TextureShader.shadeFragmentTex();

            _programTex = _gl.CreateProgram();

            _gl.AttachShader(_programTex, vertexShader);
            _gl.AttachShader(_programTex, fragmentShader);

            _gl.LinkProgram(_programTex);

            _gl.GetProgram(_programTex, ProgramPropertyARB.LinkStatus, out int lStatus);
            if (lStatus != (int) GLEnum.True)
                throw new Exception("LINK FAILED" + _gl.GetProgramInfoLog(_programTex));

            _gl.DetachShader(_programTex, vertexShader);
            _gl.DetachShader(_programTex, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            const uint positionLoc = 0;
            _gl.EnableVertexAttribArray(positionLoc);
            _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);

            const uint texCoordLoc = 1;
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

            _gl.BindTexture(TextureTarget.Texture2D,0);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static unsafe void RenderTexRectangle()
        {
            _gl.BindVertexArray(_vaoTex);
            _gl.UseProgram(_programTex);
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _texture);
            _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
        }
    }
}