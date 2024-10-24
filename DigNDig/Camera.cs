using Silk.NET.Maths;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using System;
using System.Numerics;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace Camera
{
    public class CameraClass
    {
        private static GL _gl = MainProgram.MainProgram._gl;
        public static Vector3 _camPos = new Vector3(0.0f,0.0f,3.0f);
        public static Vector3 _camTarget = new Vector3(0.0f,0.0f,0.0f);
        public static Vector3 _camUp1 = new Vector3(0.0f,1.0f,0.0f);
        public static Vector3 _cameraRight = Vector3.Normalize(Vector3.Cross(_camUp1,_cameraDir));
        public static Vector3 _cameraFront = new Vector3(0.0f,0.0f,-1.0f);
        public static Vector3 _cameraUp2 = Vector3.Cross(_cameraDir,_cameraRight);
        public static Vector3 _cameraDir = Vector3.Normalize(_camPos - _camTarget);
        public static Vector3 _cameraOri = new Vector3(0.0f,0.0f,-1.0f);

        public static float width = 1024;
        public static float height = 600;
        public static float speed = 0.1f;
        public static float FOVdeg = 90.0f;
        public static float nearP = 0.1f;
        public static float farP = 1000f;
        public static float yaw = -90.0f;
        public static float pitch = 0.0f;
        public static float aspectRatio = MainProgram.MainProgram.GetAspectRatio();
        public static Matrix4x4 Matrix_view()
        {
            Matrix4x4 view = Matrix4x4.Identity;
            Matrix4x4 viewTranslation = Matrix4x4.CreateTranslation(new Vector3(0.0f,-0.2f,-3.0f));
            view = Matrix4x4.Multiply(view,viewTranslation);

            return view;
        }


        public static Matrix4x4 Matrix_proj(float aspectRatio, float FOVdeg, float nearP, float farP)
        {
            Matrix4x4 proj = Matrix4x4.Identity;
            proj = Matrix4x4.CreatePerspectiveFieldOfView(FOVdeg*((float)Math.PI/180.0f),aspectRatio,nearP,farP);

            return proj;
        }
    }

    public class Camera
    {
        Vector3 startPos = CameraClass._camPos;
        Vector3 pos;
        float width = CameraClass.width;
        float height = CameraClass.height;
        public static Matrix4x4 MatrixViewCamera(Vector3 camPos, Vector3 camFro, Vector3 camUp)
        {
            Matrix4x4 view = Matrix4x4.Identity;
            view *= Matrix4x4.CreateLookAt(camPos,camPos + camFro,camUp);

            return view;
        }    
    }
}