﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter.Image
{
    public class FolderImageToSchematic : AbstractToSchematic
    {
        public FolderImageToSchematic(string path) : base(path)
        {
        }

        public override Schematic WriteSchematic()
        {
            int height = Directory.GetFiles(_path).Length;
            Console.WriteLine("[INFO] Count files in the folder : " + height);

            Schematic schematic = new Schematic();
            schematic.Heigth = (ushort) height;
            schematic.Length = (ushort) height;
            schematic.Width = (ushort) height;
            schematic.Blocks = new HashSet<Block>();

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;

            using (ProgressBar progressbar = new ProgressBar())
            {
                string[] files = Directory.GetFiles(_path);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    Bitmap bitmap = new Bitmap(file);
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            Color color = bitmap.GetPixel(x, y);
                            if (color.A != 0)
                            {
                                schematic.Blocks.Add(new Block((ushort) x, (ushort) i, (ushort) y, Color.AliceBlue.ColorToUInt()));
                            }
                        }
                    }

                    progressbar.Report(i / (float)files.Length);
                }
            }
            Console.WriteLine("[LOG] Done.");
            return schematic;
        }
    }
}