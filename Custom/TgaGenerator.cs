using System;
using System.IO;
using FuturamaLib.NIF.Structures;

namespace FuturamaLib.NIF.Custom
{


    public class TGAGenerator
    {
        private Color4[] palette;
        private byte[] pixelData;
        private int width;
        private int height;

        public TGAGenerator(Color4[] palette, byte[] pixelData, int width, int height)
        {
            this.palette = palette;
            this.pixelData = pixelData;
            this.width = width;
            this.height = height;
        }

        public void SaveTGA(string filePath)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Write TGA header
                writer.Write((byte)0); // ID Length
                writer.Write((byte)1); // Color Map Type
                writer.Write((byte)1); // Image Type (Indexed, uncompressed)
                writer.Write((ushort)0); // Color Map Origin
                writer.Write((ushort)palette.Length); // Color Map Length
                writer.Write((byte)32); // Color Map Depth (32 bits = BGRA)
                writer.Write((ushort)0); // X-Origin
                writer.Write((ushort)0); // Y-Origin
                writer.Write((ushort)width); // Image Width
                writer.Write((ushort)height); // Image Height
                writer.Write((byte)8); // Pixel Depth (8 bits per pixel for indices)
                writer.Write((byte)0); // Image Descriptor

                foreach (var color in palette)
                {
                    byte b = (byte)(color.B * 255);
                    byte g = (byte)(color.G * 255);
                    byte r = (byte)(color.R * 255);
                    byte a = (byte)(color.A * 255);
                    writer.Write(b);
                    writer.Write(g);
                    writer.Write(r);
                    writer.Write(a);
                }

                for(var i=0; i < width * height - 1; i++)
                {
                    writer.Write(pixelData[i]);
                }
            }
        }
    }
}
