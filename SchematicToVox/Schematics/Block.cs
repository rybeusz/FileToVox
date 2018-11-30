﻿using SchematicToVox.Schematics.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public struct Block : IEquatable<Block>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly byte BlockID;
        public readonly byte Data;
        public readonly Color32 Color;

        public Block(int x, int y, int z, byte blockID, byte data, Color32 color)
        {
            X = x;
            Y = y;
            Z = z;
            BlockID = blockID;
            Data = data;
            Color = color;
        }

        public override int GetHashCode()
        {
            //the index of the block at X,Y,Z is (Y×length + Z)×width + X
            return (Y * SchematicReader.LengthSchematic + Z) * SchematicReader.WidthSchematic + X;
        }

        public bool Equals(Block other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("ID: {3}:{4}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID, Data);
        }

    }
}