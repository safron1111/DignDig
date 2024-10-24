using Silk.NET.OpenGL;

public class PolygonShader
{
    private static GL _gl = MainProgram.MainProgram._gl;
    private static uint _program = Polygons.PolygonsClass._program;
    
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
            gl_Position = _proj * _view * _model * vec4(aPosition.x,aPosition.y,aPosition.z, 1.0);
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
            //out_color = vec4(frag_tex3dCoords.x, frag_tex3dCoords.y, 0.0, 1.0);
            out_color = texture(uTexture, frag_tex3dCoords);
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