using Silk.NET.OpenGL;

public class FlatShader
{
    public static GL _gl = MainProgram.MainProgram._gl;
    public static unsafe uint shadeVertex()
    {
        const string vertexCode = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
        }";

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);

        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("VERTEX FAILED" + _gl.GetShaderInfoLog(vertexShader));

        return vertexShader;
    }

    public static unsafe uint shadeFragment()
    {
        const string fragmentCode = @"
        #version 330 core

        out vec4 out_color;
        
        void main()
        {
            out_color = vec4(0.2, 0.5, 0.2, 1.0);
        }";

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("FRAGMENT FAILED" + _gl.GetShaderInfoLog(fragmentShader));

        return fragmentShader;
    }
}