using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using Blotch;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace penguin_new.Assets
{
    class Resource
    {
        public string path;
        public string type;
        public string id;
    }

    class ResRoot
    {
        public List<Resource> resources { get; set; }
    }

    public enum TextureID
    {
        Dirt,
        Stone,
        Deeprock,
        Bluerock

    }
    public enum ModelID
    {
        Penguin
    }
    public enum SoundID
    {

    }
    public class Resources // we can store and reuse all of our assets in a global dictionary
    {
        private Dictionary<TextureID, Texture2D> _Textures; // texture dictionary, we can reuse these assets 
        private Dictionary<ModelID, Model> _Models;
        public Resources(string _resFile, BlGraphicsDeviceManager _graphics, ContentManager _contentMan)
        {
            _Textures = new Dictionary<TextureID, Texture2D>();
            _Models = new Dictionary<ModelID, Model>();

            ResRoot _root = JsonConvert.DeserializeObject<ResRoot>(File.ReadAllText(_resFile));

            foreach (Resource r in _root.resources)
            {
                if (!int.TryParse(r.id, out int _Id)) continue; // if the id of the resource is invalid, continue
                switch (r.type)
                {
                    case "tex":
                        {
                            _Textures.Add(
                                (TextureID)_Id, _graphics.LoadFromImageFile(r.path)
                            );
                        }
                        break;
                    case "model":
                        {
                            _Models.Add(
                                (ModelID)_Id, _contentMan.Load<Model>(r.path)
                            );
                        }
                        break;
                    case "sound": { } break;
                }
            }
        }
        /* Resource types
            tex,
            model,
            sound
        */
        public void AddModel(ModelID _id, Model _model)
        {
            if (_model == null) return;
            _Models.Add(_id, _model);
        }
        public void AddTexture(TextureID _id, Texture2D _tex)
        {
            if (_tex == null) return; // do nothing if our texture is null
            _Textures.Add(_id, _tex);
        }

        public bool GetModel(ModelID _id, out Model _model)
        {
            _model = null;
            return _Models.TryGetValue(_id, out _model);
        }
        public bool GetModel(string _id, out Model _model)
        {
            _model = null;
            bool parse = Enum.TryParse<ModelID>(_id, true, out ModelID result);
            if (!parse) return false;

            return GetModel(result, out _model);
        }
        public bool GetTexture(string _id, out Texture2D _tex)
        {
            _tex = null;

            bool parse = Enum.TryParse<TextureID>(_id, true, out TextureID result);
            if (!parse) return false;

            return GetTexture(result, out _tex);
        }
        public bool GetTexture(TextureID _id, out Texture2D _tex)
        {
            _tex = null;
            return _Textures.TryGetValue(_id, out _tex); // returns false if there is no texture for the given id, otherwise passes out the texture
        }
    }
}
