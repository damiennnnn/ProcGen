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
    public class Force{
        public Vector3 Direction;
        public float Intensity;
        public bool Dead = false;
        public virtual Vector3 GetForce(){
            return (Direction * Intensity);
        }

        public Force(Vector3 _dir, float _ins){
            Direction = _dir;
            Intensity = _ins;
        }
    }
    public class Impulse : Force{ // "one-time" force, applied once
        
        public Impulse(Vector3 _dir, float _ins) : base(_dir,_ins){

        }
        public override Vector3 GetForce()
        {
            Dead = true;
            return base.GetForce();
        }
    }
    public class FOverTime : Force{ // applied over time
        public FOverTime(Vector3 _dir, float _ins) : base(_dir, _ins){

        }

        public override Vector3 GetForce()
        {
            return base.GetForce();
        }
    }
    public class Entity
    {
        /// <summary>
        /// Camera of the entity
        /// </summary>
        public Camera Cam;
        public Vector3 _relativePos = Vector3.Zero;
        public Camera GetCamera()
        {
            return Cam;
        }

        /// <summary>
        /// Position of the entity in World
        /// </summary>
        private Vector3 _Pos;
        public Vector3 Position { get => _Pos; }
        private Vector3 _Vel;
        private Vector3 _Accel;
        public Vector3 Velocity { get => _Vel; }
        public Vector3 Acceleration { get => _Accel; }
        public float GetVelocityMagnitude() { return _Vel.Length(); }
        public float GetAccelMagnitude() { return _Accel.Length(); }
        public List<Impulse> Impulses = new List<Impulse>();
        public BoundingBox Bounding;
        private Matrix _World;
        /// <summary>
        /// World matrix for the entity, read-only
        /// </summary>
        public Matrix WorldMatrix { get => (RotationMatrix * _World); } // world matrix for the entity
        public Matrix RotationMatrix { get; set; }
        /// <summary>
        /// Unique ID for entity
        /// </summary>
        public string Identifier { get; set; } // unique ID for entity

        private BlSprite _Sprite;
        private Assets.ModelID _ModelId;
        private Assets.TextureID _TextureId;
        public bool Collided;
        public Entity(Assets.ModelID _id, Vector3 _initalPos, string _uid, BlGraphicsDeviceManager _graphics)
        {
            Bounding = new BoundingBox();
            _ModelId = _id;
            Collided = false;
            _Pos = _initalPos;
            _World = Matrix.CreateTranslation(Position);
            _Sprite = new BlSprite(_graphics, _uid);
            Identifier = _uid;
            _Sprite.Matrix = _World;
            _Sprite.LODs.Add(_GetModel());
            _Sprite.Color = (Color.White.ToVector3() * 0.2f);
            Cam = new Camera(90f, _Pos);
        }
        /// <summary>
        /// Applies an impulse in a specific direction.
        /// </summary>
        /// <param name="_dir">Direction to apply impulse. (must be normalised)</param>
        /// <param name="_intensity">Intensity of impulse.</param>
        public void AddImpulse(Vector3 _dir, float _intensity)
        {
            Impulses.Add(new Impulse(_dir, _intensity));
        }

        public List<Debug.BoundingBoxBuffers> _buf = new List<Debug.BoundingBoxBuffers>();
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Impulses.Count; i++)
            {
                if (Impulses[i].Dead) continue;
                _Pos += Impulses[i].GetForce();
            }

            _Pos += _Vel;
            _World = Matrix.CreateTranslation(_Pos);
            Bounding = new BoundingBox(_Pos, _Pos + Vector3.One);

            Collided = false;
            var _chunk = Main.MainWorld.CheckChunkCollisions(this, out var _bb); // _bb represents the bounding box we collide with
            if (_chunk!= null){ // do collision check, CheckChunkCollisions() returns a chunk if we have a collision within it
                Collided = true;
                _buf.Clear();
                for (int i =0 ; i < _bb.Count(); i++){
                    _buf.Add(Debug.Debug.CreateBoundingBoxBuffers(_bb[i], Main.GlobalGraphics.GraphicsDevice));
                }
            }
            

            Cam.UpdateViewMatrix(this);
        }
        public void Draw()
        {
            _Sprite.Draw(WorldMatrix);
        }
        private Model _GetModel()
        {
            Model _model = null;
            Globals.Resources.GetModel(_ModelId, out _model);
            return _model;

        }
    }
}
