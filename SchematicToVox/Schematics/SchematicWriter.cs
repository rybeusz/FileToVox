﻿using SchematicToVox.Extensions;
using SchematicToVox.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public static class SchematicWriter
    {
        private static bool _excavate;
        private static int _heightmap;
        private static bool _color;


        public static Schematic WriteSchematic(string path, int heightmap, bool excavate, bool color)
        {
            _excavate = excavate;
            _heightmap = heightmap;
            _color = color;

            return WriteSchematicFromImage(path);
        }

        private static Schematic WriteSchematicFromImage(string path)
        {
            FileInfo info = new FileInfo(path);
            Bitmap bitmap = new Bitmap(info.FullName);

            if (bitmap.Width > 2016 || bitmap.Height > 2016)
                throw new Exception("Image is too big");

            Schematic schematic = new Schematic
            {
                Width = (short)bitmap.Width,
                Length = (short)bitmap.Height,
                Heigth = (short)_heightmap,
                Blocks = new List<HashSet<Block>>()
            };
            SchematicReader.LengthSchematic = schematic.Length;
            SchematicReader.WidthSchematic = schematic.Width;
            SchematicReader.HeightSchematic = schematic.Heigth;

            Bitmap grayScale = MakeGrayscale3(bitmap);

            schematic.Blocks.Add(new HashSet<Block>());
            using (var progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write schematic from picture...");
                int size = schematic.Width * schematic.Length;
                for (int i = 0, global = 0; i < size; i++)
                {

                    int x = i % schematic.Width;
                    int y = i / schematic.Width;
                    var color = bitmap.GetPixel(x, y);
                    var colorGray = grayScale.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (_heightmap != 1)
                        {
                            int intensity = colorGray.R + colorGray.G + colorGray.B;
                            float position = intensity / (float)765;
                            int height = (int)(position * _heightmap);

                            if (_excavate)
                            {
                                if (CheckCornerPixels(bitmap, color, x, y))
                                {
                                    AddMultipleBlocks(ref schematic, ref global, height, x, y, color, colorGray);
                                }
                                else
                                {
                                    Block block = (_color) ? new Block(x, height - 1, y, color) :
                                        new Block(x, height - 1, y, colorGray);
                                    AddBlock(ref schematic, ref global, block);
                                }
                            }
                            else
                            {
                                AddMultipleBlocks(ref schematic, ref global, height, x, y, color, colorGray);
                            }
                        }
                        else
                        {
                            Block block = new Block(x, 1, y, color);
                            AddBlock(ref schematic, ref global, block);
                        }
                    }
                    progressbar.Report((i / (float)size));
                }
            }
            Console.WriteLine("[LOG] Done.");
            return schematic;
        }

        private static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private static void AddMultipleBlocks(ref Schematic schematic, ref int global, int height, int x, int y, Color color, Color colorGray)
        {
            for (int z = 0; z < height; z++)
            {
                Block block = (_color) ? new Block(x, z, y, color) :
                                        new Block(x, z, y, colorGray);
                AddBlock(ref schematic, ref global, block);
            }
        }

        private static void AddBlock(ref Schematic schematic, ref int global, Block block)
        {
            try
            {
                schematic.Blocks[global].Add(block);
            }
            catch (OutOfMemoryException)
            {
                global++;
                schematic.Blocks.Add(new HashSet<Block>());
                schematic.Blocks[global].Add(block);
            }
        }

        private static bool CheckCornerPixels(Bitmap bitmap, Color color, int x, int y)
        {
            bool createAll = true;
            if (x - 1 > 0 && x + 1 < bitmap.Width && y - 1 > 0 && y + 1 < bitmap.Height)
            {
                var colorLeft = bitmap.GetPixel(x - 1, y);
                var colorTop = bitmap.GetPixel(x, y - 1);
                var colorRight = bitmap.GetPixel(x + 1, y);
                var colorBottom = bitmap.GetPixel(x, y + 1);

                if (color == colorLeft && color == colorTop && color == colorRight && color == colorBottom)
                {
                    createAll = false;
                }
            }
            return createAll;
        }
    }
}
