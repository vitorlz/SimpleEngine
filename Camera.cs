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
        public Transform Transform { get; set; }
        private Matrix4 ModelMatrix = Matrix4.Identity;

        public float Velocity { get; set; } = 3.0f;
        public float Sensitivity { get; set; } = 10.0f;

        public Camera(Transform transform)
        {
           Transform = transform;
        }

        public void Update(double dt, KeyboardState input, MouseState mouse)
        {
            ModelMatrix.Transpose();

            Vector3 right = ModelMatrix.Column0.Xyz;
            Vector3 up = ModelMatrix.Column1.Xyz;
            Vector3 front = -ModelMatrix.Column2.Xyz;

            Vector2 mouseDelta = mouse.Delta;

            Quaternion pitch = Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(-mouse.Delta.Y * (float)dt * Sensitivity));
            Quaternion yaw = Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(-mouse.Delta.X * (float)dt * Sensitivity));

            Transform.Rotation = yaw * Transform.Rotation * pitch;

            float boost = 1f;
            if(input.IsKeyDown(Keys.LeftShift))
            {
                boost = 3f;
            }

            if (input.IsKeyDown(Keys.D))
            {
                Transform.Position = Transform.Position + right * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.A))
            {
                Transform.Position = Transform.Position - right * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.W))
            {
                Transform.Position = Transform.Position + front * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.S))
            {
                Transform.Position = Transform.Position - front * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                Transform.Position = Transform.Position + up * Velocity * (float)dt * boost;
            }
            if (input.IsKeyDown(Keys.LeftControl))
            {
                Transform.Position = Transform.Position - up * Velocity * (float)dt * boost;
            }

            ModelMatrix = Matrix4.CreateFromQuaternion(Transform.Rotation);
            ModelMatrix *= Matrix4.CreateTranslation(Transform.Position);

            ViewMatrix = ModelMatrix.Inverted();
        }
    }
}
