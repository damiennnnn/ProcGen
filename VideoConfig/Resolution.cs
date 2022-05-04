using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace penguin_new.VideoConfig
{

    public class GameResolution
    {   
        public int X {get;set;}
        public int Y {get;set;}
        public GameResolution(int x, int y)
        {
            X = x;
            Y = y;
        }

    }
    public static class VideoSettings{
        public static Dictionary<string, GameResolution> Resolutions = new Dictionary<string, GameResolution>();
        public static GameResolution Resolution {get;set;}

        public static void ChangeResolution(){
            Main.GlobalGraphics.PreferredBackBufferWidth = Resolution.X;
            Main.GlobalGraphics.PreferredBackBufferHeight = Resolution.Y;
            Main.GlobalGraphics.ApplyChanges();
            Globals.ClientBounds = new Rectangle(0,0,Resolution.X, Resolution.Y);
            
        }
        private static void SetupResolutions(){
            
            for (int i =0; i < 200 ; i +=5){
                int x = (i*16);
                int y = (i*9);
                string _str = string.Format("{0}p", y);
                Resolutions.Add(_str, new GameResolution(x,y));
            }
        }
    public static void Init(){
            SetupResolutions();
        }

    }

}
