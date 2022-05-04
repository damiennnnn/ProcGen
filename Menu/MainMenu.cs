using System.IO;
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


namespace penguin_new
{
    public class MainMenu
    {
        public class MenuItem
        {
            public string _Name;
            public string Tag(bool isCurrent)
            {
                switch (ItemType)
                {
                    case 0:
                        return (string.Format(
                            ((isCurrent) ? " > " : "") +
                            _Name +
                            ((isCurrent) ? " < " : "")
                            , 0));
                    case 1:
                        return (string.Format(
                            ((isCurrent) ? "<< " : "") +
                            _Name + " {0}" +
                            ((isCurrent) ? " >>" : "")
                            , StoredInt));
                    case 2:
                        return (string.Format(
                            ((isCurrent) ? " > " : "") +
                            _Name + " {0}" +
                            ((isCurrent) ? "_ < " : "")
                            , StoredString));
                }
                return "";
            }
            public int ItemType = 0; // 0 - regular 1 - number select 2- text input

            public string StoredString = "";
            public int StoredInt = 0;
            public Action SelectionAction;
            public Color StringColor = Color.White;
            public Color HighlightColor = Color.Red;

            public MenuItem(string _str, Action _act, int _type = 0)
            {
                _Name = _str;
                SelectionAction = _act;
                ItemType = _type;
            }
        }
        private int _menuIndex = 0;
        private List<MenuItem> _menuItems = new List<MenuItem>();
        private List<MenuItem> _worldGenItems = new List<MenuItem>();
        private List<MenuItem> _worldLoadItems = new List<MenuItem>();
        private List<MenuItem> _gameOptionsItems = new List<MenuItem>();
        private List<MenuItem> _loadErrorItems = new List<MenuItem>();
        private List<MenuItem> _displayedItems = new List<MenuItem>();
        private List<Keys> _pressedKeys = new List<Keys>();


        public List<MenuItem> CurrentMenu { get => _displayedItems; }
        public int SelectionIndex { get => _menuIndex; }

        private enum MenuPage
        {
            Main = 0,
            WorldCreate,
            WorldLoad,
            Settings,
            LoadError
        }
        private int test = 0;
        private bool pageChange = true;
        private void AddWorldCreateItems()
        {

            var chunkX = new MenuItem("Chunk X:", new Action(() =>
            {
            }), 1);
            chunkX.StoredInt = 4;
            _worldGenItems.Add(chunkX);

            var chunkY = new MenuItem("Chunk Z:", new Action(() =>
            {
            }), 1);
            chunkY.StoredInt = 4;
            _worldGenItems.Add(chunkY);

            var seed = new MenuItem("Seed:", new Action(() =>
            {
            }), 2);
            seed.SelectionAction = new Action(() =>
            {
                seed._Name = Crypt.Hash.GetNumericHash(seed.StoredString);
            });
            seed.StoredString = "10000000";
            _worldGenItems.Add(seed);

            var name = new MenuItem("Name: ", new Action(() => { }), 2);
            name.SelectionAction = new Action(() =>
            {

            });
            name.StoredString = "World1";
            _worldGenItems.Add(name);

            _worldGenItems.Add(new MenuItem("Create", new Action(() =>
            {

                Entity ent = new Entity(Assets.ModelID.Penguin, Vector3.Zero, "penguin1", Main.GlobalGraphics); // just a testing entity 
                Player player = new Player(Main.GlobalGraphics);
                //MainWorld.AddEntity(ent);
                Main.MainWorld.AddEntity(ent, ent.Identifier);
                Main.MainWorld.AddEntity(player, "player");
                Main.MainWorld.GenerateNew(chunkX.StoredInt, chunkY.StoredInt, Crypt.Hash.GetNumericHash(seed.StoredString), name.StoredString);
            }), 0));

            _worldGenItems.Add(new MenuItem("Back", new Action(() =>
            {
                pageIndex = (int)MenuPage.Main;
                _menuIndex = 0;
                pageChange = true;
            })));
        }

        private void AddGameOptionsItems(){
            _gameOptionsItems.Add(new MenuItem("Back", new Action(() =>{
                pageIndex = (int)MenuPage.Main;
                _menuIndex = 0;
                pageChange = true;
                
            })));
            string _resolution = string.Format("Resolution: {0}", "1280x720");

        _gameOptionsItems.Add(new MenuItem(_resolution, new Action( () =>{

        })));

        }

        private void AddWorldLoadItems()
        {
            _worldLoadItems.Add(new MenuItem("Back", new Action(() =>
            {
                pageIndex = (int)MenuPage.Main;
                _menuIndex = 0;
                pageChange = true;
                _worldLoadItems.Clear();
            })));
            string _path = Path.Combine(Environment.CurrentDirectory, "worlds");
            if (!Directory.Exists((_path)))
                Directory.CreateDirectory((_path));
            foreach (var _dir in Directory.GetDirectories(_path))
            {
                string _folderName = Path.GetFileName(_dir);
                _worldLoadItems.Add(new MenuItem(_folderName, new Action(() =>
                {
                    if (!Main.MainWorld.Load(_dir)){
                        pageIndex = (int)MenuPage.LoadError;
                        _menuIndex = 0;
                        pageChange = true;
                        _worldLoadItems.Clear();
                    }
                }), 0));
            }
        }
        private void AddLoadErrorItems(){
            _loadErrorItems.Add(new MenuItem("Back", new Action(() =>
            {
                pageIndex = (int)MenuPage.Main;
                _menuIndex = 0;
                pageChange = true;
                _loadErrorItems.Clear();
            })));
            var errString = new MenuItem("Error loading world. Invalid or corrupt?", new Action(() => { }));
            errString.StringColor = Color.Red;
            errString.HighlightColor = Color.MidnightBlue;
            _loadErrorItems.Add(errString);
        }
        private void AddMenuItems()
        {
            _menuItems.Add(new MenuItem("New World", new Action(() =>
            {
                pageIndex = (int)MenuPage.WorldCreate;
                _menuIndex = 0;
                pageChange = true;
            })));
            _menuItems.Add(new MenuItem("Load World", new Action(() =>
            {
                pageIndex = (int)MenuPage.WorldLoad;
                _menuIndex = 0;
                pageChange = true;
            })));
            _menuItems.Add(new MenuItem("Settings", new Action(() =>{
                pageIndex = (int)MenuPage.Settings;
                _menuIndex = 0;
                pageChange = true;
            })));
            _menuItems.Add(new MenuItem("Exit", new Action(() =>
            {
                Program.MainThread.Exit();
            })));
            var verString = new MenuItem(Globals.Version, new Action(() => { }));
            verString.StringColor = Color.Crimson;
            verString.HighlightColor = Color.MidnightBlue;
            _menuItems.Add(verString);
        }
        private int pageIndex = 0;
        //mainWorld.GenerateNew();

        public void Update(GameTime timeInfo)
        {
            if (_menuItems.Count == 0)
                AddMenuItems();
            if (_worldGenItems.Count == 0)
                AddWorldCreateItems();
            if (_gameOptionsItems.Count == 0)
                AddGameOptionsItems();

            if (pageChange)
            {
                switch (pageIndex)
                {
                    case (int)MenuPage.Main: _displayedItems = _menuItems.ToList(); break;
                    case (int)MenuPage.WorldCreate: _displayedItems = _worldGenItems.ToList(); break;
                    case (int)MenuPage.WorldLoad: AddWorldLoadItems(); _displayedItems = _worldLoadItems.ToList(); break;
                    case (int)MenuPage.Settings: _displayedItems = _gameOptionsItems.ToList(); break;
                }
                pageChange = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !_pressedKeys.Contains(Keys.Right))
            {
                if (_displayedItems[_menuIndex].ItemType == 1) { _displayedItems[_menuIndex].StoredInt++; }
                _pressedKeys.Add(Keys.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !_pressedKeys.Contains(Keys.Left))
            {
                if (_displayedItems[_menuIndex].ItemType == 1) { _displayedItems[_menuIndex].StoredInt--; }
                _pressedKeys.Add(Keys.Left);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !_pressedKeys.Contains(Keys.Down))
            {
                _menuIndex++;
                _pressedKeys.Add(Keys.Down);
            }

            else if (Keyboard.GetState().IsKeyDown(Keys.Up) && !_pressedKeys.Contains(Keys.Up))
            {
                _menuIndex--;
                _pressedKeys.Add(Keys.Up);
            }



            if (_menuIndex < 0) _menuIndex = _displayedItems.Count - 1;
            else if (_menuIndex > _displayedItems.Count - 1) _menuIndex = 0;

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !_pressedKeys.Contains(Keys.Enter))
            {
                _pressedKeys.Add(Keys.Enter);
                var t = new System.Threading.Thread(() => { _displayedItems[_menuIndex].SelectionAction(); });
                t.IsBackground = true;
                t.Start();
            }

            if (_displayedItems[_menuIndex].ItemType == 2)
            {
                var keysPressed = Keyboard.GetState().GetPressedKeys();
                foreach (var key in keysPressed)
                {
                    if ((((int)key) >= 48) && (((int)key) <= 90))
                    {
                        if (!_pressedKeys.Contains(key))
                        {
                            _displayedItems[_menuIndex].StoredString += (char)((int)key);
                            _pressedKeys.Add(key);
                        }
                    }
                    if (key == Keys.Back && !_pressedKeys.Contains(key))
                    {
                        if (string.IsNullOrEmpty(_displayedItems[_menuIndex].StoredString)) continue;
                        _displayedItems[_menuIndex].StoredString = _displayedItems[_menuIndex].StoredString.Remove(_displayedItems[_menuIndex].StoredString.Length - 1);
                        _pressedKeys.Add(key);
                    }

                }
            }


            for (int i = 0; i < 254; i++)
            {
                Keys _key = (Keys)i;

                if (Keyboard.GetState().IsKeyUp(_key) && _pressedKeys.Contains(_key))
                    _pressedKeys.Remove(_key);
            }

        }
    }
}
