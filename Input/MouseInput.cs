using System.Data;
using System.Threading.Tasks;
using System.Resources;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using Blotch;
using System.Collections.Generic;
using System.Linq;

namespace penguin_new.Input
{
    public class Handler
    {
        public float Sensitivity { get; set; }
        public Handler()
        {
            Sensitivity = 0.2f;
        }
        protected void ClampVec(ref Vector2 _vec)
        {
            if (_vec.X > 180f) _vec.X = (-180f + (_vec.X % 180f));
            if (_vec.X < -180f) _vec.X = (180f - (_vec.X % 180f));
            if (_vec.Y >= 90f) _vec.Y = 89f;
            if (_vec.Y <= -90f) _vec.Y = -89f;
        }
        public virtual void Update(GameTime gameTime, out Vector2 _angChange)
        {
            _angChange = Vector2.Zero;
        }
    }
    public class MouseInput : Handler
    {
        private MouseState _prevState;
        public MouseInput() : base() { }
        public override void Update(GameTime gameTime, out Vector2 _angChange)
        {
            (int X, int Y) WindowCentre = new(Globals.ClientBounds.Width / 2, Globals.ClientBounds.Height / 2); // get centre of the game window
            _angChange = Vector2.Zero;
            _angChange.X -= Sensitivity * (Mouse.GetState().X - _prevState.X);
            _angChange.Y -= Sensitivity * (Mouse.GetState().Y - _prevState.Y); // get the difference between the current mouse position and the last mouse position
            Mouse.SetPosition(WindowCentre.X, WindowCentre.Y); // move the cursor back to the window centre
            _prevState = Mouse.GetState();
        }
    }
}
