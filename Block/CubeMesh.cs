using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;

namespace penguin_new.ChunksBlocks
{
    public class CubeMesh
    {
        public BlSprite[] MeshSprites;
        public BoundingBox[] Bounding;
        public Debug.BoundingBoxBuffers[] BoundingBuffers;

        private static VertexPositionNormalTexture[][] _blockVerts;
        // private static VertexBuffer[] _buffers = new VertexBuffer[64];

        public static Dictionary<string, VertexBuffer> Buffers = new Dictionary<string, VertexBuffer>();
        public static BoundingBox CubeBounding = new BoundingBox();
        private static void BlockVerts()
        {
            _blockVerts = new VertexPositionNormalTexture[6][]; // storage for our manipulated triangles
            var verts = new VertexPositionNormalTexture[6]; // 'verts' is a standard flat 1x1 square, which will be rotated and modified to form each face of a cube in code
            var norm = new Vector3(0, 0, 1);

            verts[0].Position = new Vector3(-1, -1, 0);
            verts[0].TextureCoordinate = new Vector2(0, 0);
            verts[0].Normal = norm;

            verts[1].Position = new Vector3(-1, 1, 0);
            verts[1].TextureCoordinate = new Vector2(0, 1);
            verts[1].Normal = norm;

            verts[2].Position = new Vector3(1, -1, 0);
            verts[2].TextureCoordinate = new Vector2(1, 0);
            verts[2].Normal = norm;

            verts[3].Position = verts[1].Position;
            verts[3].TextureCoordinate = new Vector2(0, 1);
            verts[3].Normal = norm;

            verts[4].Position = new Vector3(1, 1, 0);
            verts[4].TextureCoordinate = new Vector2(1, 1);
            verts[4].Normal = norm;

            verts[5].Position = verts[2].Position;
            verts[5].TextureCoordinate = new Vector2(1, 0);
            verts[5].Normal = norm;

            
            Matrix[] translationMatrices = new Matrix[]{
                Matrix.CreateTranslation(1f,0,-1f),//Front
                Matrix.CreateTranslation(-1f,0,-1f),//Back
                Matrix.CreateTranslation(0,-1f,-1f),//Left
                Matrix.CreateTranslation(0f,1f,-1f),//Right
                Matrix.CreateTranslation(0f,0f,0f),//Top
                Matrix.CreateTranslation(0f,0f,-2f)//Bottom
            };
            Matrix[] transformMatrices = new Matrix[]{

                (Matrix.CreateRotationY(MathHelper.ToRadians(90f))) , // front
				(Matrix.CreateRotationY(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f))), // back
				(Matrix.CreateRotationY(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90f)) ), // left
				(Matrix.CreateRotationY(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90f))), // right
				(Matrix.CreateRotationY(MathHelper.ToRadians(0f))), // top
				( Matrix.CreateRotationY(MathHelper.ToRadians(180f))) //bottom
			};
            
            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;
            for (int x = 0; x < 6; x++) // for each side of a cube, 6 sides
            {
                var vert = BlGeometry.TransformVertices((verts.Clone() as VertexPositionNormalTexture[]), transformMatrices[x] * translationMatrices[x]); // manipulate a clone of 'vert' to form a cube side
                vert = BlGeometry.CalcFacetNormals(vert); // calculate the normal for lighting later

                

                _blockVerts[x] = vert; // '_blockVerts' will be used for creating the buffer
            }
            CubeBounding = new BoundingBox(min, max);
            /*
                front 1
                back 2
                left 3
                right 4
                top 5
                bottom 6

                011111
                111111

                Bottom - Top - Right - Left - Back - Front
            */
            for (int c = 1; c < 64; c++)
            {
                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[0];
                var bin = Convert.ToString(c, 2); // 000000 - 111111 // 64 represents each possible combination of visible cube faces

                while (bin.Length < 6)
                    bin = bin.Insert(0, "0");

                int count = bin.Where(x => x == '1').Count();
                var cubeVerts = new List<VertexPositionNormalTexture>(); // we will add to this as needed
                int cubeVertOffset = 0;
                for (int p = 0; p < 6; p++)
                {
                    if (bin[5 - p] == '0') continue;

                    cubeVerts.AddRange(_blockVerts[p]);
                }
                var arr = cubeVerts.ToArray();
                vertices = new VertexPositionNormalTexture[arr.Length];

                for (int v = 0; v < arr.Length; v++)
                {
                    vertices[cubeVertOffset + v] = arr[v];
                }
                Buffers.Add(bin, BlGeometry.TrianglesToVertexBuffer(Main.GlobalGraphics.GraphicsDevice, vertices));
                // convert our mesh to a buffer, and add to our dictionary along with its binary representation 
            }


        }

        static ConcurrentDictionary<VertexPositionNormalTexture[], VertexBuffer> buffers = new ConcurrentDictionary<VertexPositionNormalTexture[], VertexBuffer>();

        public BlSprite[] CreateCubeMesh(Block[] Blocks, BlGraphicsDeviceManager Graphics)
        {
            if (_blockVerts == null)
                BlockVerts();
            ConcurrentBag<BoundingBox> boxes = new ConcurrentBag<BoundingBox>();
            ConcurrentBag<BlSprite> GeoObj = new ConcurrentBag<BlSprite>();
            
            List<VertexPositionNormalTexture[]> sides = new List<VertexPositionNormalTexture[]>();

            Parallel.For(0, Blocks.Length, i =>
            {
                string _binStr = "";
                for (int w = 0; w < 6; w++)
                {
                    if (!Blocks[i].IsSideVisible(w)) { _binStr = _binStr.Insert(0, "0"); }
                    else _binStr = _binStr.Insert(0, "1"); // create our binary representation of the visible cube faces
                }

                if (_binStr == "000000") // if any side isnt visible
                    return;
                
                using (var block = new BlSprite(Main.GlobalGraphics, "bl"))
                {
                    block.Name = _binStr;
                    block.LODs.Add(Buffers[_binStr]); // look up the combination in our buffers dictionary
                    block.Mipmap = Blocks[i].Texture;
                    block.Color = new Vector3(0.6f, 0.4f, 0.4f);

                    Vector3 _pos = (Blocks[i].GetWorldPosition());
                    Vector3 _pos2 = (Blocks[i].GetWorldPosition() + Vector3.One );
                    BoundingBox _box = new BoundingBox(_pos, _pos2);
                    boxes.Add(_box);
                    block.Matrix = Matrix.CreateTranslation(_pos * 2);
                    GeoObj.Add(block);
                }
            });

            Bounding = boxes.ToArray();
            return GeoObj.ToArray();
        }

    }
}