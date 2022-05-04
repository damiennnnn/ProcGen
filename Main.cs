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
using MonoGame.ImGui.Standard;
using ImGuiNET;
namespace penguin_new
{

    public class Main : BlWindow3D
    {
        public static World MainWorld;
        public static Input.RawInputHandler InputHandler;
        List<BlSprite> GeoObj;
        BlSprite squareTest;
        public ImGUIRenderer ImGuiRenderer;

        public static BlGraphicsDeviceManager GlobalGraphics;

        /// <summary>
        /// This will be the font for the help menu we draw in the window
        /// </summary>
        SpriteFont Font;

        protected override void Setup()
        {
            Window.Title = string.Format("{0} - {1}", Globals.Title, Globals.Version);
            this.IsMouseVisible = false;
            ImGuiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();
            ImGui.GetIO().FontGlobalScale = 1.5f;

            (int x, int y) _resolution = (1280,720);
            Globals.ClientBounds = new Rectangle(0,0,_resolution.x,_resolution.y);
            
            Graphics.PreferredBackBufferWidth = _resolution.x;
            Graphics.PreferredBackBufferHeight = _resolution.y;
            Graphics.SynchronizeWithVerticalRetrace = false;
            Graphics.LimitFrameRate = false;
            Graphics.FarClip = 1000;
            Graphics.NearClip = 0.1f;
            Graphics.DepthStencilStateEnabled = DepthStencilState.Default;
            
            Graphics.ApplyChanges();

            GeoObj = new List<BlSprite>();
            var ConMan = new ContentManager(Services, "");

            Font = ConMan.Load<SpriteFont>("Content/DefaultFont");

            Globals.Resources = new Assets.Resources("resource.json", Graphics, ConMan);
            MainWorld = new World();
            
            VideoConfig.VideoSettings.Init();
        }


        float _rotateFactor = 0.3f;
        double updateTime = 0f;
        int z = 2;

        Vector3 viewAng = new Vector3(0, 0, 0);
        bool keyDown = false;


        private Keys[] _pressedKeys = new Keys[0];
        /// <summary>
        /// Main game update thread
        /// </summary>
        /// <param name="timeInfo"></param>
        private void MainGameUpdate(GameTime timeInfo)
        {         
            float rotationScale = (500f * (float)timeInfo.ElapsedGameTime.TotalSeconds);
            float movementScale = 5f;
            
            _pressedKeys = Keyboard.GetState().GetPressedKeys();
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                MainWorld.GetLocalPlayer().GetCamera().AdjustViewAngle(_rotateFactor * rotationScale, 0f);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                MainWorld.GetLocalPlayer().GetCamera().AdjustViewAngle(-_rotateFactor * rotationScale, 0f);
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                MainWorld.GetLocalPlayer().GetCamera().AdjustViewAngle(0f, _rotateFactor * rotationScale);
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                MainWorld.GetLocalPlayer().GetCamera().AdjustViewAngle(0f, -_rotateFactor * rotationScale);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                MainWorld.GetLocalPlayer().AddImpulse(Vector3.Up, 0.5f);
            if (Keyboard.GetState().IsKeyDown(Keys.RightShift))
                MainWorld.GetLocalPlayer().AddImpulse(Vector3.Down, 0.5f);
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                MainWorld.GetLocalPlayer().AddImpulse(MainWorld.GetLocalPlayer().WorldMatrix.Forward, 0.5f);


            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                movementScale = 10f;

            for (int i = 0; i < MainWorld.Entities.Length; i++)
            {
                if (MainWorld.Entities[i] != null) MainWorld.Entities[i].Update(timeInfo);
            }

            for (int i = 0; i < MainWorld.ChunkArray.Count(); i++)
            {
                if (MainWorld.ChunkArray[i].ShouldUpdate)
                {
                    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                    watch.Start();
                    MainWorld.ChunkArray[i].UpdateMesh(Graphics, out int l);
                    watch.Stop();
                    Console.WriteLine(string.Format("chunk mesh gen: {0}", watch.Elapsed.TotalSeconds));
                }
            }
            if (keyDown == true && Keyboard.GetState().IsKeyUp(Keys.D0)) keyDown = false;

            if (Keyboard.GetState().IsKeyDown(Keys.D0) && !keyDown)
            {
                showDebug = !showDebug;
                IsMouseVisible = showDebug;
                keyDown = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (!showDebug){
                MainWorld.GetLocalPlayer().InputHandler.Update(timeInfo, out var _vec);
                MainWorld.GetLocalPlayer().GetCamera().AdjustViewAngle(_vec.X, _vec.Y);
            }
            MainWorld.GetLocalPlayer().Update(timeInfo, !showDebug);
            updateTime = timeInfo.ElapsedGameTime.TotalSeconds;
        }

        MainMenu menu;
        private bool showDebug = false;
        protected override void Update(GameTime timeInfo)
        {
            if (MainWorld.WorldInitialised)
                MainGameUpdate(timeInfo);
            else
            {
                if (menu == null)
                {
                    menu = new MainMenu(); // create main menu if doesnt exist
                    Main.GlobalGraphics = Graphics;
                    if (Program.Args.GetArgument("--default")) // if project launched with --default command line option
                    {
                        Entity ent = new Entity(Assets.ModelID.Penguin, Vector3.Zero, "penguin1", Graphics); // just a testing entity 
                        Player player = new Player(Graphics);
                        MainWorld.AddEntity(ent, ent.Identifier);
                        MainWorld.AddEntity(player, "player");
                        var t = new System.Threading.Thread(x => { MainWorld.GenerateNew(2, 2, Crypt.Hash.GetNumericHash("default seed"), "World1"); });
                        t.Start(); // run our generation routine on separate thread
                    }
                }
                menu.Update(timeInfo);
            }


            base.Update(timeInfo);
        }


        private Texture2D _whiteRectangle;
        private bool _rectInitalised = false;

        private void WorldGenDraw(GameTime timeInfo)
        {
            if (!_rectInitalised)
            {
                _whiteRectangle = new Texture2D(Graphics.GraphicsDevice, 1, 1);
                _whiteRectangle.SetData(new[] { Color.White });
                _rectInitalised = true;
            }
            var _titleLength = Font.MeasureString(Globals.Title) / 2f;
            Graphics.DrawText(Globals.Title, Font, new Vector2(Window.ClientBounds.Width / 2, 170) - _titleLength, Color.Magenta);


            switch (MainWorld.Status) // gets the current status of world generation, relays information to user with progress bar
            {

                case 1:
                    {
                        Graphics.DrawTexture(_whiteRectangle,
                        new Rectangle(((Window.ClientBounds.Width / 2) - 150), 200, 300, 20), Color.Black
                        );
                        Graphics.DrawTexture(_whiteRectangle,
                         new Rectangle(((Window.ClientBounds.Width / 2) - 150), 200, MainWorld.GetGenerationProgress() * 3, 20), Color.DarkBlue
                         );
                    }
                    break;
                case 2:
                    {
                        Graphics.DrawTexture(_whiteRectangle,
                        new Rectangle(((Window.ClientBounds.Width / 2) - 100), 200, 200, 20), Color.Black
                        );
                        Graphics.DrawTexture(_whiteRectangle,
                         new Rectangle(((Window.ClientBounds.Width / 2) - 100), 200, MainWorld.GetMeshProgress() * 2, 20), Color.LightGreen
                         );
                    }
                    break;
                case 3:
                    {
                        Graphics.DrawTexture(_whiteRectangle,
                        new Rectangle(((Window.ClientBounds.Width / 2) - 100), 200, 200, 20), Color.Black
                        );
                        Graphics.DrawTexture(_whiteRectangle,
                         new Rectangle(((Window.ClientBounds.Width / 2) - 100), 200, MainWorld.GetGenerationProgress() * 2, 20), Color.BlueViolet
                         );
                    }
                    break;


            }

            var _statusLength = Font.MeasureString(MainWorld.GetStatusMessage()).X / 2f;
            Graphics.DrawText(MainWorld.GetStatusMessage(), Font, new Vector2((Window.ClientBounds.Width / 2) - _statusLength, 230), Color.White);

        }
        private void MainMenuDraw(GameTime timeInfo)
        {
            if (MainWorld.GetGenerationProgress() > 0) { WorldGenDraw(timeInfo); return; };
            var _titleLength = Font.MeasureString(Globals.Title) / 2f;
            Graphics.DrawText(Globals.Title, Font, new Vector2(Window.ClientBounds.Width / 2, 170) - _titleLength, Color.Magenta);

            for (int i = 0; i < menu.CurrentMenu.Count; i++)
            {
                bool _isSelected = (i == menu.SelectionIndex);
                string _text = menu.CurrentMenu[i].Tag(_isSelected);
                var _length = Font.MeasureString(_text) / 2f;
                Vector2 _windowPos = new Vector2(Window.ClientBounds.Width / 2, 200 + (30 * i)) - _length; // centres text, stacks on top of each other dynamically

                Graphics.DrawText(_text,
                 Font, _windowPos,
                _isSelected ? menu.CurrentMenu[i].HighlightColor : menu.CurrentMenu[i].StringColor); // draw all of the menu options
            }
        }
        // imgui options
        private bool infoOpen = false;
        private bool cameraPopup = false;
        private void ImGuiMenu(GameTime timeInfo) // used for debugging purposes
        {
            ImGuiRenderer.BeginLayout(timeInfo);

            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Main"))
                {
                    if (ImGui.MenuItem("Info"))
                        infoOpen = true;
                    if (ImGui.MenuItem("Camera"))
                        cameraPopup = true;
                    if (ImGui.MenuItem("Exit"))
                        Exit();
                    ImGui.EndMenu();
                }
                if (ImGui.Button("Save world"))
                {
                    MainWorld.SaveChunk(Vector3.Zero);
                }
                if (ImGui.Button("Open world dir")){
                    MainWorld.OpenWorldDirectory();
                }
                ImGui.Text($@"{1 / timeInfo.ElapsedGameTime.TotalSeconds}");
                ImGui.EndMainMenuBar();
            }

            if (cameraPopup && ImGui.Begin("CameraPopup", ref cameraPopup, ImGuiWindowFlags.Popup))
            {
                string _cameraInfoString = $@"
                CameraForward {Graphics.CameraForward}
                CameraLookAt {Graphics.LookAt}
                CameraEye {Graphics.Eye}
                CameraUp {Graphics.CameraUp}
                CameraRight {Graphics.CameraRight}
                peepeepoopoo cool
                ";
                ImGui.TextWrapped(_cameraInfoString);
                ImGui.End();
            }

            if (infoOpen && ImGui.Begin("InfoPopup", ref infoOpen, ImGuiWindowFlags.Popup))
            {
                string infoString = $@"
Eye: {Graphics.Eye}
ViewAngle: {viewAng.ToString()}
Updatetime: {timeInfo.ElapsedGameTime.TotalSeconds}
{(float)(1 / timeInfo.ElapsedGameTime.TotalSeconds)}
EntityCount: {MainWorld.Entities.Length}
ChunkCount: {MainWorld.ChunkArray.Count()}
BlocksRendering: {MainWorld.BlockCount}
WorldSeed: {MainWorld.Seed}
LookAt: {Graphics.LookAt}
";
                ImGui.TextWrapped(infoString);

                ImGui.End();
            }
            ImGuiRenderer.EndLayout();
        }

        private void UpdateCamera(Player _player) // update graphics values to our own entity camera values
        {
            Camera _cam = _player.Cam;
            Graphics.ResetCamera();
            Graphics.Eye = _player.Position + _cam.EyePosition;
            Graphics.View = _cam.ViewMatrix;
            Graphics.Projection = _cam.ProjectionMatrix;
            Graphics.Zoom = _cam.GetFOV();
        }

        private int _meshRenderCount = 0;


        private BasicEffect CreateBasicEffect() // nee to set up some base values for our basic effect once
        {
            var _eff = new BasicEffect(GraphicsDevice);
            _eff.EnableDefaultLighting();
            _eff.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            _eff.SpecularColor = Color.Transparent.ToVector3();
            _eff.SpecularPower = 0.01f;
            _eff.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            _eff.LightingEnabled = true;
            return _eff;
        }
        private BasicEffect _effect;
        private BoundingFrustum _frustum; // these are passed into our drawmesh routines, significant performance gains as we dont need to create new objects in loops
        

        Player _localPlayer;
        private async void MainGameDraw(GameTime timeInfo)
        {
            if (_effect == null)
                _effect = CreateBasicEffect(); // only need to initialise once
            if (_localPlayer == null)
                _localPlayer = MainWorld.GetLocalPlayer();

            UpdateCamera(_localPlayer);

            _meshRenderCount = 0;
            
            _effect.Projection = MainWorld.EntityList["player"].Cam.ProjectionMatrix;
            _effect.View = MainWorld.EntityList["player"].Cam.ViewMatrix;
            _effect.TextureEnabled = true;
            _frustum = new BoundingFrustum(_effect.View * _effect.Projection);            // configure MonoGame rendering
            for (int i = 0; i < MainWorld.ChunkArray.Count(); i++)
            {
                MainWorld.ChunkArray[i].DrawMesh(ref Graphics,out int draw, ref _effect,  ref _frustum,  ref _localPlayer, i);
                Console.WriteLine(draw); // draw all chunk meshes of all chunks currently in memory
            }

            for (int i = 0; i < MainWorld.Entities.Length; i++)
            {
                if (MainWorld.Entities[i] == null) continue;
                MainWorld.Entities[i].Draw(); // draw all entities in world

            }

            Graphics.DrawText(
                $@"{Globals.Version}", Font, new Vector2(20, 20), Color.Crimson
            );
            Graphics.DrawText(
                $@"{1 / timeInfo.ElapsedGameTime.TotalSeconds}
{_meshRenderCount}
{_localPlayer.Position}
{_localPlayer.Cam.EyePosition}
{MainWorld.ChunkArray.Length}
{_localPlayer.Collided}", Font, new Vector2(20, 60), Color.Orange 
            ); // display debug information, such as player position and camera imformation
            for (int i =0 ; i < _pressedKeys.Count(); i++)
                Graphics.DrawText(_pressedKeys[i].ToString(), Font, new Vector2(400, 20 + (16 * i)), Color.Purple); // show keyboard pressed keys
            Main.MainWorld.GetLocalPlayer().Crosshair.Draw(Graphics); // render the crosshair in hud

            if (Main.MainWorld.GetLocalPlayer()._buf != null){
                for (int i = 0 ; i < Main.MainWorld.GetLocalPlayer()._buf.Count; i++){
                    Debug.Debug.DrawDebugBox(Main.MainWorld.GetLocalPlayer()._buf[i]); // debug draw bounding boxes
                }
            }
            if (showDebug) ImGuiMenu(timeInfo); // Debugging IMGUI menu
        }


        bool _inputInit = false;
        protected override void FrameDraw(GameTime timeInfo)
        {
            Graphics.ClearColor = Color.SkyBlue;
            if (MainWorld.WorldInitialised)
                MainGameDraw(timeInfo); // main game draw routine
            else
                MainMenuDraw(timeInfo); // mainmenu
            
        }
    }
}