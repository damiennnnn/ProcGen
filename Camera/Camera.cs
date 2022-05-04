using System.Linq.Expressions;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;
namespace penguin_new
{
    public class Camera
    {
        private Vector3 _eyePosition;
        public Vector3 EyePosition {get => _eyePosition;}
        private Vector3 _forward;
        private Vector3 _up;
        private Vector3 _right;
        private float _fov;

        private Matrix _projMatrix;
        private Matrix _viewMatrix;
        public Matrix ViewMatrix { get => _viewMatrix; }
        public Matrix ProjectionMatrix { get => _projMatrix; }
        private Vector2 _viewAngle;

        public Camera(float _fov, Vector3 _eyePos)
        {
            _eyePosition = _eyePos;
            _viewMatrix = Matrix.CreateTranslation(_eyePos);
            _projMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(_fov), Globals.GetAspectRatio(), 0.01f, 1000f);
            this._fov = _fov;
            _viewAngle = new Vector2(0f, 0f);
        }

        public Vector2 GetViewAngle { get => _viewAngle; }

        private void ClampAngles() // we must clamp our y-axis angle between -90f and 90f, to prevent the player from being able to rotate excessively around the x-axis
        {
            if (_viewAngle.Y > 89f) _viewAngle.Y = 89f;
            if (_viewAngle.Y < -89f) _viewAngle.Y = -89f;
            if (_viewAngle.X > 180f) _viewAngle.X -= 360f;
            if (_viewAngle.X < -180f) _viewAngle.X += 360f;
        }

        public void AdjustViewAngle(float x, float y)
        {
            _viewAngle.X += x;
            _viewAngle.Y += y;
            ClampAngles();
        }
        public void SetViewAngle(float x, float y)
        {
            _viewAngle.X = x;
            _viewAngle.Y = y;
            ClampAngles();
        }
        public float GetFOV() { return _fov; }
        public void UpdateFOV(float _fov)
        {
            this._fov = _fov;
            _projMatrix = Matrix.CreatePerspectiveFieldOfView(_fov, Globals.GetAspectRatio(), 0.01f, 1000f);
        }
        public void UpdateViewMatrix(Entity _ent)
        {
            Matrix _rotationX = Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(_viewAngle.X));
            Matrix _rotationY = Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, _ent.WorldMatrix.Forward), MathHelper.ToRadians(-_viewAngle.Y));

            _ent.RotationMatrix = _rotationX;
            _eyePosition = Vector3.Transform(_ent.WorldMatrix.Forward, _rotationY);
            _viewMatrix = Matrix.CreateLookAt(_ent.Position, _ent.Position + _eyePosition, Vector3.Up);
        }
    }
}
