using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;
namespace penguin_new.ChunksBlocks
{
    public enum BlockType
    {
        Dirt = 0,
        Stone
    };
    public class Block
    {
        public BlockType Type { get; set; }
        /// <summary>
        /// Position relative to chunk position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Position of the chunk containing this block
        /// </summary>
        public Vector3 ChunkPosition { get; set; }
        public Texture2D Texture { get => TextureGet(); }

        private Assets.TextureID _textureId;
        public void SetTexture(Assets.TextureID _id)
        {
            _textureId = _id;
        }
        private Texture2D TextureGet()
        {
            bool _textureExists = Globals.Resources.GetTexture(_textureId, out Texture2D tex);
            if (!_textureExists) return null;

            return tex;
        }
        public bool[] SideVisible = new bool[]
        {
            true, // Forward Face
            true, // Back Face
            true, // Left Face
            true, // Right Face
            true, // Top Face
            true  // Bottom Face
        };
        /// <summary>
        /// Real position is the position in the world
        /// </summary>
        /// <returns></returns>
        public Vector3 GetWorldPosition()
        {
            Vector3 _pos = (ChunkPosition * Chunk.ChunkSize);
            _pos += Position;
            return _pos;
        }
        public bool IsSideVisible(int i) => SideVisible[i];
    }
}
