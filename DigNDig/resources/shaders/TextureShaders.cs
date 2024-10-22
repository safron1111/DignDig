using Silk.NET.OpenGL;

public class TextureShader
{
    private static GL _gl = MainProgram.MainProgram._gl;
    private static uint _program = SecondaryRendering.TextureRendering._programTex;
    
    public static unsafe uint shadeVertexTex()
    {
        const string vertexCode = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aTextureCoord;

        out vec2 frag_texCoords;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
            frag_texCoords = aTextureCoord;
        }";

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("VERTEX FAILED" + _gl.GetShaderInfoLog(vertexShader));

        return vertexShader;
    }

    public static unsafe uint shadeFragmentTex()
    {
        const string fragmentCode = @"
        #version 330 core

        uniform sampler2D uTexture;

        in vec2 frag_texCoords;

        out vec4 out_color;
        
        void main()
        {
        //   -out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
            out_color = texture(uTexture, frag_texCoords);
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