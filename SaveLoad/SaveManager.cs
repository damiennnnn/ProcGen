using System.Net.Mime;
using System.Runtime.Serialization;
using System.Net;
using System.Security.AccessControl;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
namespace penguin_new.SaveLoad
{
    public class SaveManager
    {
        public struct WorldData
        {
            public string Name;
            public string Seed;
            public int Id;
        }

        private WorldData _Data;
        private World _world;
        private List<string> Directories;
        private string[] WorldInfo;

        private string _Path;

        public string SaveDir { get => _Path; }

        public bool DirectoryExists(string _dir)
        {
            if (Directories == null) return false;
            else return (_dir.Contains(_dir));
        }
        public SaveManager(string path) // path to load world from
        {
            _Path = Path.Combine(Environment.CurrentDirectory, "worlds", path);
            Directories = Directory.GetDirectories(path).ToList();
        }
        public SaveManager(World world, string path) // path to save world to
        {
            _world = world;
            _Path = Path.Combine(Environment.CurrentDirectory, "worlds", path);
            Directory.CreateDirectory(_Path);
            Directories = new List<string>();
            JsonSerializer _serializer = new JsonSerializer();
            _Data = new WorldData() {Name = world.WorldName, Seed = world.Seed};
            
            using (StreamWriter _writer = File.CreateText(Path.Combine(_Path, "world.json")))
            {
                using (JsonWriter _json = new JsonTextWriter(_writer))
                {
                    _serializer.Serialize(_json, _Data);
                }
            }
        }

    }
    /// <summary>
    /// the 'world' file stores world info
    /// the 'entity' folder contains entity data
    /// 
    /// each chunk will be sorted as its own folder
    /// each folder will contain a file that contains the chunk info
    /// </summary>

    static class ChunkGrabber
    {

        static byte[] Compress(byte[] _input){ // Working routine, currently remains unused for debugging purposes
            using (var _result = new MemoryStream()){
                var _lengthBytes = BitConverter.GetBytes(_input.Length);
                _result.Write(_lengthBytes, 0, 4);
                using (var _compressStream = new GZipStream(_result, CompressionLevel.Fastest)){
                    _compressStream.Write(_input, 0, _input.Length);
                    _compressStream.Flush();
                }
                return _result.ToArray();
            }
        }
        static byte[] Decompress(byte[] _input){ // Working routine, currently remains unused for debugging purposes
            using (var _src = new MemoryStream(_input)){
                byte[] _lengthBytes = new byte[4];
                _src.Read(_lengthBytes, 0, 4);
                var _len = BitConverter.ToInt32(_lengthBytes);
                using (var _decompressStream = new GZipStream(_src, CompressionMode.Decompress)){
                    var _result = new byte[_len];
                    _decompressStream.Read(_result, 0, _len);
                    return _result;
                }
            }
        }
        private static string GetHexValueString(Vector3 _vec){
            int x = (int)_vec.X;
            int y = (int)_vec.Y;
            int z = (int)_vec.Z;
            string result = "";
                
            if (x < 0){
                result += "$";
                x *= -1;
            }
            result += (x.ToString("X") + "&");
            if (y < 0){
                result += "$";
                y *= -1;
            }
            result += (y.ToString("X") + "&");
            if (z < 0){
                result += "$";
                z *= -1;
            }
            result += (z.ToString("X"));

            return result;
        }
        
        private static Vector3 FromVec3String(string _str){
            string[] _data = _str.Split(';');
            Vector3 _vec = Vector3.Zero;
            try{
                _vec = new Vector3(float.Parse(_data[0]), float.Parse(_data[1]), float.Parse(_data[2]));
            }
            catch (Exception e){}
            return _vec;
        }
        private static string ToVec3String(Vector3 _vec){
            _vec.Round();
            string _data = string.Format("{0};{1};{2}", _vec.X, _vec.Y, _vec.X);
            return _data;
        }
        public static void SaveWorldData(ref SaveManager _saveMan, World _world){
            if (!Directory.Exists(_saveMan.SaveDir)) Directory.CreateDirectory(_saveMan.SaveDir);

            using (MemoryStream _stream = new MemoryStream())
            using (StreamWriter _writer = new StreamWriter(_stream)){
                foreach (var _ent in _world.Entities){
                    string _data = string.Format("");

                }
            }
        }
        public static void SaveChunk(ChunksBlocks.Chunk _chunk, ref SaveManager _saveMan){

            if (!Directory.Exists(_saveMan.SaveDir)) Directory.CreateDirectory(_saveMan.SaveDir);

            Vector3 _chunkGroupPos = (_chunk.Position / 16);
            _chunkGroupPos.Round();
            string _chunkGroupDir = "g";
            _chunkGroupDir += GetHexValueString(_chunkGroupPos);
            // g{0}&{1}&{2} == chunkgroup format
            _chunkGroupDir = Path.Combine(_saveMan.SaveDir,"chunks", _chunkGroupDir);
            if (!Directory.Exists(_chunkGroupDir)) Directory.CreateDirectory(_chunkGroupDir);

            string _chunkDir = "c";
            _chunkDir += GetHexValueString(_chunk.Position);
            _chunkDir = Path.Combine(_chunkGroupDir, _chunkDir);
            if (!Directory.Exists(_chunkDir)) Directory.CreateDirectory(_chunkDir);

            using (MemoryStream _stream = new MemoryStream())
            using (StreamWriter _writer = new StreamWriter(_stream)){
                _writer.WriteLine(string.Format("{0};{1};{2}", (int)_chunk.Position.X, (int)_chunk.Position.Y, (int)_chunk.Position.Z));
                
                _writer.Flush();
                File.WriteAllBytes(Path.Combine(_chunkDir, "chunk.dltd"), _stream.ToArray());
            } // saving chunk data
            using (MemoryStream _stream = new MemoryStream())
            using (StreamWriter _writer = new StreamWriter(_stream)){
                foreach (var keyVal in _chunk.Blocks){
                    var block = keyVal.Value;
                    var pos = keyVal.Key;
                    pos.Round();

                    string _data = string.Format("{0};{1};{2};{3}", (int)pos.X, (int)pos.Y, (int)pos.Z, (int)block.Type);
                    _writer.WriteLine(_data);
                }
                _writer.Flush();
                File.WriteAllBytes(Path.Combine(_chunkDir,"blockdata.dltd"), _stream.ToArray());
            } // saving block data

            using (MemoryStream _stream = new MemoryStream())
            using (StreamWriter _writer = new StreamWriter(_stream)){
                
            }
        }
        public static bool GetChunk(Vector3 _chunkPos, SaveManager _saveMan, out ChunksBlocks.Chunk _chunk){
            _chunk = null; // _chunk needs to be given a value to be passed as an out parameter
            if (_saveMan == null) return false;

            Vector3 _chunkGroupPos = (_chunkPos / 16);
            string _chunkGroupDir = "g";
            _chunkGroupDir += GetHexValueString(_chunkGroupPos);
            string _chunkDir = "c";
            _chunkDir += GetHexValueString(_chunkPos);

            string _folderName = Path.Combine(_saveMan.SaveDir,"chunks", _chunkGroupDir, _chunkDir);

            string _blockDataFile  = Path.Combine(_folderName, "blockdata.dltd");
            string _chunkDataFile = Path.Combine(_folderName, "chunk.dltd");

            if (!Directory.Exists(_saveMan.SaveDir) || !Directory.Exists(_folderName) || !File.Exists(_blockDataFile) || !File.Exists(_chunkDataFile))
                return false;
            
            byte[] _chunkArr = File.ReadAllBytes(_chunkDataFile);
            byte[] _blockArr = File.ReadAllBytes(_blockDataFile);
            //_chunkArr = Decompress(_chunkArr);
            //_blockArr = Decompress(_blockArr);
            
            List<string> _chnk = new List<string>();
            using (MemoryStream _stream = new MemoryStream(_chunkArr))
            using (StreamReader _reader = new StreamReader(_stream)){
                    while (!_reader.EndOfStream)
                        _chnk.Add(_reader.ReadLine());
            }

            List<string> _blck = new List<string>();
            using (MemoryStream _stream = new MemoryStream(_blockArr))
            using (StreamReader _reader = new StreamReader(_stream)){
                while (!_reader.EndOfStream)
                    _blck.Add(_reader.ReadLine());
            }
            string[] _chunkData = _chnk.ToArray();
            string[] _blockData = _blck.ToArray();

            _chunk = new ChunksBlocks.Chunk();
            _chunk.Position = _chunkPos;
            for (int i = 0; i < _blockData.Length; i++){
                var _newBlock = new ChunksBlocks.Block();
                string[] _data = _blockData[i].Split(';');
                if (_data.Length < 4) continue;
                if (int.TryParse(_data[0], out int x) && int.TryParse(_data[1], out int y) &&
                    int.TryParse(_data[2], out int z) && int.TryParse(_data[3], out int type))
                {
                    _newBlock.Position = new Vector3(x, y, z);
                    _newBlock.Type = (ChunksBlocks.BlockType)type;
                }
                else continue;
                _newBlock.SetTexture((Assets.TextureID)_newBlock.Type);
                _newBlock.ChunkPosition = _chunkPos;
                _chunk.AddBlock(_newBlock);
            }

            return true;
        }
        public static void SaveChunk2(ChunksBlocks.Chunk _chunk, ref SaveManager _saveMan)
        {
            if (!Directory.Exists(_saveMan.SaveDir)) Directory.CreateDirectory(_saveMan.SaveDir);

            string x = ((int)_chunk.Position.X).ToString("X"); // use the chunks position to save as its own folder, will be useful for lookup later
            string y = ((int)_chunk.Position.Y).ToString("X"); // stored as a hexadecimal value
            string z = ((int)_chunk.Position.Z).ToString("X");

            string _folderName = string.Format("c{0}&{1}&{2}", x, y, z);
            _folderName = Path.Combine(_saveMan.SaveDir, _folderName);

            string _fileName = Path.Combine(_folderName, "chunk.dat");
            Directory.CreateDirectory(_folderName);

            using (StreamWriter _writer = File.CreateText(_fileName))
            {
                _writer.WriteLine(string.Format("{0};{1};{2}", x, y, z)); // first line will always set out chunk info

                foreach (var keyVal in _chunk.Blocks)
                {
                    var block = keyVal.Value;
                    var pos = keyVal.Key; // position of block in the chunk
                    pos.Round();

                    string _data = string.Format("{0};{1};{2};{3}", (int)pos.X, (int)pos.Y, (int)pos.Z, (int)block.Type);
                    _writer.WriteLine(_data);
                }
            }
        }
        public static bool GetChunk2(Vector3 _chunkPos, SaveManager _saveMan, out ChunksBlocks.Chunk _chunk) // returns false if the chunk couldnt be found, allowing us to create a new one
        {
            _chunk = null; // _chunk needs to have a value as an out parameter

            string x = ((int)_chunkPos.X).ToString("X");
            string y = ((int)_chunkPos.Y).ToString("X");
            string z = ((int)_chunkPos.Z).ToString("X");

            string _folderName = string.Format("c{0}&{1}&{2}", x, y, z);
            _folderName = Path.Combine(_saveMan.SaveDir, _folderName);

            string _fileName = Path.Combine(_folderName, "chunk.dat");


            if (!_saveMan.DirectoryExists(_folderName) && !File.Exists(_fileName))
                return false; // chunk load failed, can create a new chunk

            string[] _chunkInfo = File.ReadAllLines(_fileName); // load our chunk info to a string array
            string[] _chunkData = _chunkInfo[0].Split(';'); // first line should always be chunkinfo


            _chunk = new ChunksBlocks.Chunk();
            _chunk.Position = _chunkPos;

            for (int i = 1; i < _chunkInfo.Length; i++)
            {
                var _newBlock = new ChunksBlocks.Block();
                string[] _data = _chunkInfo[i].Split(';'); // index 0->2 should give us block location and index 3 should be block type

                _newBlock.Position = new Vector3(
                int.Parse(_data[0]),
                int.Parse(_data[1]),
                int.Parse(_data[2]));
                _newBlock.Type = (ChunksBlocks.BlockType)int.Parse(_data[3]);
                _newBlock.SetTexture(Assets.TextureID.Dirt);

                _newBlock.ChunkPosition = _chunkPos;
                _chunk.AddBlock(_newBlock);
            }


            return true;
        }
    }
}
