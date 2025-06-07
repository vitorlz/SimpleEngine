using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SimpleEngine.Types;

namespace SimpleEngine.Cam
{
    public class Camera
    {
        public Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;

        private Transform _transform;
        public ref Transform Transform { get { return ref _transform; }}
        private Matrix4 ModelMatrix = Matrix4.Identity;

        private float _pitch = 0;

        public float Velocity { get; set; } = 30.0f;
        public float Sensitivity { get; set; } = 10.0f;
        public float Boost { get; set; } = 3.0f;

        public Camera(Transform transform)
        {
           _transform = transform;
        }

        public void Update(double dt, KeyboardState input, MouseState mouse)
        {
            ModelMatrix.Transpose();

            Vector3 right = ModelMatrix.Column0.Xyz;
            Vector3 up = ModelMatrix.Column1.Xyz;
            Vector3 front = -ModelMatrix.Column2.Xyz;

            Vector2 mouseDelta = mouse.Delta;
            float deltaYaw = -mouse.Delta.X * (float)dt * Sensitivity;
            float deltaPitch = -mouse.Delta.Y * (float)dt * Sensitivity;
            _pitch += deltaPitch;

            if (_pitch >= 89f)
            {
                _pitch = 89f;
                deltaPitch = 0f;
            }
            else if (_pitch <= -89f)
            {
                _pitch = -89f;
                deltaPitch = 0f;
            }

            Quaternion pitch = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(deltaPitch));
            Quaternion yaw = Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(deltaYaw));

            Transform.rotation = yaw * _transform.rotation * pitch;

            float boost = 1f;
            if(input.IsKeyDown(Keys.LeftShift))
            {
                boost = Boost;
            }

            if (input.IsKeyDown(Keys.D))
            {
                _transform.position = _transform.position + right * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.A))
            {
                _transform.position = _transform.position - right * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.W))
            {
                _transform.position = _transform.position + front * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.S))
            {
                _transform.position = _transform.position - front * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _transform.position = _transform.position + up * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.LeftControl))
            {
                _transform.position = _transform.position - up * Velocity * (float)dt * boost;
            }

            ModelMatrix = Matrix4.CreateFromQuaternion(_transform.rotation);
            ModelMatrix *= Matrix4.CreateTranslation(_transform.position);

            if(input.IsKeyPressed(Keys.O))
            {
                Console.WriteLine("Camera rotation: " + (_transform.rotation.ToEulerAngles() * MathHelper.RadToDeg).ToString());
                Console.WriteLine("Camera position: " + _transform.position.ToString());
            }

            ViewMatrix = ModelMatrix.Inverted();
        }
    }
}
