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
    public class Player : Entity
    {
        public Player(BlGraphicsDeviceManager _graphics) : base(Assets.ModelID.Penguin, Vector3.Zero, "player", _graphics) {
            Crosshair = new UI.Crosshair();
            InputHandler = new Input.MouseInput();
         }

        public Input.Handler InputHandler;
        public UI.Crosshair Crosshair;
        private Vector3 _oldPos = new Vector3(-1,-1,-1);

        private void ChunkUpdate(){ // player moved into other chunk, update surroudnign chunks
            
            Vector3 _chunkPos = (Position / ChunksBlocks.Chunk.ChunkSize); _chunkPos.Round();
            Vector3[] _chunkVecs = ChunksBlocks.Chunk.GetSurroundingChunkCoords(_chunkPos); // get the coordinates of the chunks surrounding the player

            for (int i =0 ; i < _chunkVecs.Length; i++){
                if (Main.MainWorld.Chunks.ContainsKey(_chunkVecs[i]))
                    continue;

                if (!Main.MainWorld.LoadChunk(_chunkVecs[i]))
                    Main.MainWorld.CreateChunk(_chunkVecs[i]);
            }
            Main.MainWorld.ChunkArray = Main.MainWorld.Chunks.Values.ToArray(); // unloads all chunks no longer surrounding the player
            //Main.MainWorld.AddChunkDictionary(_dic);
        }
        public new void Update(GameTime gameTime, bool _inputUpdate = true){
            if (_inputUpdate)
                InputHandler.Update(gameTime, out var change); // update mouse input
            base.Update(gameTime);
            
            
            Vector3 _vec = (Position/ChunksBlocks.Chunk.ChunkSize); // chunk position
            _vec.Round();
            if (_oldPos != _vec){
                _oldPos = _vec;
                var t = new System.Threading.Thread( x => {ChunkUpdate();}); // if chunk position change, update surrounding chunks on other thread
                t.Start();
            }
        }
    }
}
