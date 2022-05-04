using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;

namespace penguin_new
{
    public class Variable
    {
        public string identifier{get;set;}
        public Type type {get; set;}
        public object val {get; set;}
        public string desc {get;set;}
                public Action onChange; // this action will be called when specific variables are modified by the user
        public Variable(string id, object obj, Type t)
        {
            identifier = id;
            val = obj;
            type = t;
        }
        public void Update(object newVal)
        {
            val = newVal;
            onChange();
        }
    }
    public static class Logger{

        public static void WriteLine(){

        }
    }
    public static class Globals
    {

        public static Dictionary<string, Variable> ConsoleVars = new Dictionary<string, Variable>();

        public static void RegisterVariable(string id, object obj, string desc = "")
        {
            var variable = new Variable(id, obj, obj.GetType());
            if (!string.IsNullOrEmpty(desc))
                variable.desc = desc;

            ConsoleVars.Add(id, variable); // bind variable to modify to the new consolevar
            ConsoleVars[id].onChange = new Action(() => { }); // do nothing
        }
        public static void RegisterVariable(string id, object val, Action update)
        {
            RegisterVariable(id, val); // use original method 
            ConsoleVars[id].onChange = update; // action to invoke when the variable is changed
        }

        private static class VersionInfo
        {
            public static int _Major = 0;
            public static int _Minor = 2;
            public static int _Patch = 1;
        }
        private static string VersionString()
        {
            return string.Format("v{0}.{1}.{2} - damien.", VersionInfo._Major, VersionInfo._Minor, VersionInfo._Patch);
        }

        public static float GetAspectRatio()
        {
            Rectangle _rec = Main.GlobalGraphics.GraphicsDevice.PresentationParameters.Bounds;
            return ((float)_rec.Width / (float)_rec.Height);
        }

        public static Rectangle ClientBounds = new Rectangle();
    public static string Version { get => VersionString(); }
        public static string Title = "game project";
        public static Assets.Resources Resources { get; set; }
    }

    
}
