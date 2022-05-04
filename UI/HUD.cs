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
namespace penguin_new.UI
{
    public class Crosshair
    {
        public static GraphicsDevice gDevice;
        public Vector2 Centre = new Vector2();
        private Color col = Color.Red;
        public Texture2D CrosshairHorizontal;
        public Vector2[] CrosshairPosition = new Vector2[4];
        public Texture2D CrosshairVertical;
        private readonly string gapId = "xhair_gap";

        private readonly string lenId = "xhair_length";
        private readonly string thiId = "xhair_thickness";

        public Crosshair()
        {
            Globals.RegisterVariable(lenId, 5, Create);
            Globals.RegisterVariable(thiId, 3, Create);
            Globals.RegisterVariable(gapId, 3, Create);

            Create();
        }

        public int Length
        {
            get => (int) Globals.ConsoleVars[lenId].val;
            set
            {
                Globals.ConsoleVars[lenId].val = value;
                Create();
            }
        }

        public int Thickness
        {
            get => (int) Globals.ConsoleVars[thiId].val;
            set
            {
                Globals.ConsoleVars[thiId].val = value;
                Create();
            }
        }

        public int CentreGap
        {
            get => (int) Globals.ConsoleVars[gapId].val;
            set
            {
                Globals.ConsoleVars[gapId].val = value;
                Create();
            }
        }

        public Color Colour
        {
            get => col;
            set
            {
                col = value;
                Create();
            }
        } // crosshair will automatically update when the values are changed

        public void Draw(BlGraphicsDeviceManager _graphics){
                _graphics.SpriteBatch.Begin();
                _graphics.SpriteBatch.Draw(CrosshairVertical, CrosshairPosition[0], Color.White);
                _graphics.SpriteBatch.Draw(CrosshairVertical, CrosshairPosition[1], Color.White);
                _graphics.SpriteBatch.Draw(CrosshairHorizontal, CrosshairPosition[2], Color.White);
                _graphics.SpriteBatch.Draw(CrosshairHorizontal, CrosshairPosition[3], Color.White);
                _graphics.SpriteBatch.End();
        }
        public void Create()
        {
            (int X, int Y) WindowCentre = new (Globals.ClientBounds.Width/ 2, Globals.ClientBounds.Height/2);
            

            CrosshairVertical = new Texture2D(Main.GlobalGraphics.GraphicsDevice, Thickness, Length);
            CrosshairHorizontal = new Texture2D(Main.GlobalGraphics.GraphicsDevice, Length, Thickness);

            var data = new Color[Length * Thickness];
            for (var i = 0; i < data.Length; ++i) data[i] = Colour;
            CrosshairVertical.SetData(data);
            CrosshairHorizontal.SetData(data);
            CrosshairPosition[0] = new Vector2(WindowCentre.X - Thickness / 2, WindowCentre.Y + CentreGap);
            CrosshairPosition[1] =
                new Vector2(WindowCentre.X - Thickness / 2, WindowCentre.Y - Length - CentreGap);
            CrosshairPosition[2] = new Vector2(WindowCentre.X + CentreGap, WindowCentre.Y - Thickness / 2);
            CrosshairPosition[3] =
                new Vector2(WindowCentre.X - Length - CentreGap, WindowCentre.Y - Thickness / 2);
        }
    }
    public class HUD
    {

    }
}
