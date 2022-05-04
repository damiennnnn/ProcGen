using System.Linq.Expressions;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;

namespace penguin_new.Vector
{
    public static class Extensions
    {
        public static Vector3 Modulo(Vector3 _first, Vector3 _second){ // gets the modulo of two Vector3s
            Vector3 _newVec = new Vector3(_first.X % _second.X, _first.Y%_second.Y,_first.Z%_second.Z);
            return _newVec;
        }
    }
}
