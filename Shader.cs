using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SimpleEngine.Res
{
    public class Shader : IDisposable
    {
        private int _id;
        public Shader(string vertexShader, string fragmentShader)
        {
            string vertexShaderSource = File.ReadAllText(vertexShader);
            string fragmentShaderSource = File.ReadAllText(fragmentShader);

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.GetShader(vertexShaderHandle, ShaderParameter.CompileStatus, out int successVert);
            if(successVert == 0)
            {
                string infolog = GL.GetShaderInfoLog(vertexShaderHandle);
                Console.WriteLine(infolog);    
            }

            GL.CompileShader(fragmentShaderHandle);
            GL.GetShader(fragmentShaderHandle, ShaderParameter.CompileStatus, out int successFrag);
            if (successFrag == 0)
            {
                string infolog = GL.GetShaderInfoLog(vertexShaderHandle);
                Console.WriteLine(infolog);
            }

            _id = GL.CreateProgram();

            GL.AttachShader(_id, vertexShaderHandle);
            GL.AttachShader(_id, fragmentShaderHandle);
            GL.LinkProgram(_id);

            GL.GetProgram(_id, GetProgramParameterName.LinkStatus, out int successProgram);
            if(successProgram == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_id);
                Console.WriteLine(infoLog);
            }

            GL.DetachShader(_id, vertexShaderHandle);
            GL.DetachShader(_id, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
        }

        private bool hasDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if(!hasDisposed)
            {
                GL.DeleteBuffer(_id);
                hasDisposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Shader()
        {
            if(!hasDisposed)
            {
                Console.WriteLine("Did not dispose shader program");
            }
        }

        // we don't set a set accessor because we don't want id to be set from outside of this class. This only allows the 
        // set accessor to be called from the class constructor. 
        public int id { get { return _id; } }

        public void Use()
        {
            GL.UseProgram(_id);
        }

        public void SetMat4(string name, Matrix4 mat4)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(id, name), true, ref mat4);
        }
        public void SetVec2(string name, Vector2 vec2)
        {
            GL.Uniform2(GL.GetUniformLocation(id, name), vec2);
        }
        public void SetVec2(string name, float x, float y)
        {
            GL.Uniform2(GL.GetUniformLocation(id, name), x, y);
        }
        public void SetVec3(string name, Vector3 vec3)
        {
            GL.Uniform3(GL.GetUniformLocation(id, name), vec3);
        }
        public void SetVec3(string name, float x, float y, float z)
        {
            GL.Uniform3(GL.GetUniformLocation(id, name), x, y, z);
        }
        public void SetVec4(string name, Vector4 vec4)
        {
            GL.Uniform4(GL.GetUniformLocation(id, name), vec4);
        }
        public void SetVec4(string name, float x, float y, float z, float w)
        {
            GL.Uniform4(GL.GetUniformLocation(id, name), x, y, z, w);
        }
        public void SetFloat(string name, float x)
        {
            GL.Uniform1(GL.GetUniformLocation(id, name), x);
        }
        public void SetInt(string name, int x)
        {
            GL.Uniform1(GL.GetUniformLocation(id, name), x);
        }
    }
}
