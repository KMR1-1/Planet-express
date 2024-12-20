using System.Diagnostics.CodeAnalysis;
using FuturamaLib.NIF.Custom;
using FuturamaLib.NIF.Structures;

namespace Name
{
    class Material
    {
        public Material(string name, NiAVObject mesh, ref Gltf gltf)
        {
            var pbrMetallicRoughness = new Dictionary<string, object>();
            var materialdict = new Dictionary<string, object>
                {
                    {"name", name}
                };

            foreach (var prop in mesh.Properties)
            {
                if (prop.Object is NiTexturingProperty tex)
                {
                    var texture = new Dictionary<string, object>
                        {
                            {"source", gltf.tcount},
                            {"sampler", gltf.tcount},
                        };
                    gltf.textures.Add(texture);

                    new Sampler(tex, ref gltf);

                    //image creation
                    if (tex.BaseTexture.Source.Object.InternalTexture.Object is NiPixelData texdata)
                    {
                        var palette = texdata.Palette.Object.Palette;
                        var pixelData = texdata.PixelData;
                        var width = (int)texdata.MipMaps[0].Width;
                        var height = (int)texdata.MipMaps[0].Height;

                        string tgapath = $"{gltf.level.Out}/texture/{name}.tga";
                        TGAGenerator tgaGenerator = new TGAGenerator(palette, pixelData, width, height);
                        tgaGenerator.SaveTGA(tgapath);
                        gltf.images.Add(new Dictionary<string, object> { { "uri", tgapath } });
                    }

                    var baseColorTexture = new Dictionary<string, object>
                        {
                            {"index", gltf.tcount},
                            {"texcoord", tex.BaseTexture.UVSetIndex}
                        };
                    pbrMetallicRoughness["baseColorTexture"] = baseColorTexture;
                }

                if (prop.Object is NiMaterialProperty mat)
                {
                    var d = mat.DiffuseColor;
                    var e = mat.EmissiveColor;
                    var a = mat.Alpha;
                    pbrMetallicRoughness["baseColorFactor"] = new List<float> { d.R, d.G, d.B, a };
                    materialdict["emmissiveFactor"] = new List<float> { e.R, e.G, e.B };
                    materialdict["roughnessFactor"] = 1 - mat.Glossiness;
                }
            }

            if (pbrMetallicRoughness.Count != 0)
                materialdict["pbrMetallicRoughness"] = pbrMetallicRoughness;
            gltf.materials.Add(materialdict);
            gltf.tcount++;
        }
    }
}