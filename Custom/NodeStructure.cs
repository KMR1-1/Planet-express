using FuturamaLib.NIF.Structures;
using FuturamaLib.NIF.Custom;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Numerics;

namespace FuturamaLib.NIF.Custom
{
    /// <summary>
    /// Classe principale pour gérer la structure des nœuds.
    /// </summary>
    class NodesStructure
    {
        public Level level;

        public NodesStructure(string maindir, string levelname)
        {
            level = new Level(maindir, levelname);

            GetDefsStructure(ref level);
            GetStagesStructure(ref level);



        }


        public static void GetDefsStructure(ref Level level)
        {
            foreach (var root in level.defsReader.Footer.RootNodes)
            {
                if (root.Object is NiNode niNode)
                {
                    level.indent["nindent"]++;
                    var rootNode = NodeBuilder.BuildNodeStructure(niNode, ref level);
                    level.Instances.Add(rootNode);
                }
            }
            level.levelStruct.Add(level.Instances);
        }

        public static void GetStagesStructure(ref Level level)
        {

            var nodes = new List<Node>();
            for (var i = 0; i < level.Readers.Count; i++)
            {
                foreach (var root in level.Readers[i].Footer.RootNodes)
                {
                    if (root.Object is NiNode inst && inst.Name.Value == "Instances")
                    {
                        
                        level.indent["nindent"]++;
                        level.scene.Add(level.indent["nindent"]);
                        var instnode = new Node
                        {
                            Type = "NiNode",
                            Id = level.indent["nindent"],
                            Name = inst.Name.Value,
                            Children = NodeBuilder.BuildInstances(inst, ref level)
                        };
                        nodes.Add(instnode);
                    }

                    if (root.Object is NiNode niNode && niNode.Name.Value != "Instances")
                    {
                        level.indent["nindent"]++;
                        level.scene.Add(level.indent["nindent"]);
                        var rootNode = NodeBuilder.BuildNodeStructure(niNode, ref level);
                        nodes.Add(rootNode);
                    }
                }
            }
            level.levelStruct.Add(nodes);
            CreateDebugStructure(level.levelStruct, $"{level.Out}/debuf.json");
        }
        public static void CreateDebugStructure(List<List<Node>> nodes, string Out)
        {
            string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);

            File.WriteAllText(Out, json);
        }
    }
    static class NodeBuilder
    {
        public static List<Node> BuildInstances(NiNode niNode, ref Level level)
        {
            //each NiUDSNode
            var nodes = new List<Node>();
            foreach (var child in niNode.Children)
            {
                if (child.Object is NiUDSNode niUDSNode)
                {
                    var name = child.Object.Name.Value.Split("|")[0];

                    foreach (var inst in level.Instances)
                    {
                        if (inst.Name == name)
                        {
                            level.indent["nindent"]++;
                            var vnode = new Node
                            {
                                Type = "NiUDSNode",
                                Id = level.indent["nindent"],
                                Name = $"{name}-{level.indent["nindent"]}",
                                Translations = new List<float>{niUDSNode.Translation.X, niUDSNode.Translation.Y, niUDSNode.Translation.Z},
                                Scale = niUDSNode.Scale,
                            };
                            var rot = RotToQuat.Quat(niUDSNode.Rotation);
                            vnode.Rotations = new List<float>{rot.X, rot.Y, rot.Z, rot.W};
                            nodes.Add(vnode);


                        }
                    };
                }
            }
            return nodes;
        }
        
        
        public static Node BuildNodeStructure(NiNode niNode, ref Level level)
        {
            // Création du nœud actuel

            var node = new Node
            {
                Type = "NiNode",
                Name = string.IsNullOrEmpty(niNode.Name.Value) ? $"Node{level.indent["nindent"]}" : niNode.Name.Value,
                Id = level.indent["nindent"],
                Translations = new List<float>{niNode.Translation.X, niNode.Translation.Y, niNode.Translation.Z},
                Scale = niNode.Scale,
            };
            var rot = RotToQuat.Quat(niNode.Rotation);
            node.Rotations = new List<float>{rot.X, rot.Y, rot.Z, rot.W};

            if(niNode.Name.Value.Contains("::"))
            {
                node.Name = niNode.Name.Value.Split("::")[1];   
            }


            // Gestion des meshes
            ProcessMeshes(niNode, ref node, ref level);

            // Construction des enfants
            foreach (var child in niNode.Children)
            {
                if(child.Object is NiSwitchNode niSwitchNode && niSwitchNode.Children.Length != 0)
                {
                    if(niSwitchNode.Children[niSwitchNode.Index].Object is NiNode ninode)
                    {
                        level.indent["nindent"]++;
                        var childStructure = BuildNodeStructure(ninode, ref level);
                        node.Children.Add(childStructure);
                    }
                }
                if (child.Object is NiNode childNode)
                {
                    level.indent["nindent"]++;
                    var childStructure = BuildNodeStructure(childNode, ref level);
                    node.Children.Add(childStructure);
                }
            }

            

            return node;
        }

        private static void ProcessMeshes(NiNode niNode, ref Node node, ref Level level)
        {
            var meshlist = ExtractMeshes(niNode);

            if (meshlist.Count == 1)
            {
                level.indent["mindent"]++;
                node.Mesh = CreateMeshDictionary(meshlist[0], ref level);
            }
            else if (meshlist.Count > 1)
            {
                level.indent["mindent"]++;
                node.Mesh = CreateMeshDictionary(meshlist[0], ref level);

                for (int i = 1; i < meshlist.Count; i++)
                {
                    level.indent["mindent"]++;
                    var meshDict = CreateMeshDictionary(meshlist[i], ref level);

                    level.indent["nindent"]++;
                    var vnode = new Node
                    {
                        Type = "VNode",
                        Name = $"meshnode{level.indent["nindent"]}",
                        Id = level.indent["nindent"],
                        Mesh = meshDict
                    };

                    node.Children.Add(vnode);
                }
            }
        }

        private static List<NiTriStrips> ExtractMeshes(NiNode niNode)
        {
            var meshlist = new List<NiTriStrips>();

            foreach (var childmesh in niNode.Children)
            {
                if (childmesh.Object is NiTriStrips niTriStrips)
                {
                    meshlist.Add(niTriStrips);
                }
            }

            return meshlist;
        }

        private static Dictionary<string, object> CreateMeshDictionary(NiTriStrips mesh, ref Level level)
        {
            var mdict = new Dictionary<string, object>{{"Id", level.indent["mindent"]}};
            string name = mesh.Name.Value;
            if (string.IsNullOrEmpty(name))
            {
                name = level.indent["mindent"].ToString();
            }
            string binpath = $"{level.Out}/data/{name}.bin";
            if (File.Exists(binpath))
            {
                name = $"{name}_{level.indent["mindent"]}";
                binpath = $"{level.Out}/data/{name}.bin";
            }
            mdict["name"] = name;

            ProcessGeometry(mesh, binpath, ref mdict, ref level);

            string tgapath = $"{level.Out}/texture/{name}.tga";
            ProcessTexture(mesh, tgapath, ref mdict, ref level);

            return mdict;
        }
        public static void ProcessGeometry(NiTriStrips mesh, string binpath, ref Dictionary<string, object> mdict, ref Level level)
        {
            if (mesh.Data.Object is NiTriStripsData data)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(binpath, FileMode.Create)))
                {
                    if (data.HasVertices)
                    {
                        mdict["vertex"] = WriteVertex(writer, data, ref level);
                        level.indent["aindent"]++;
                    }
                    if (data.HasNormals)
                    {
                        mdict["normal"] = WriteNormal(writer, data, ref level);
                        level.indent["aindent"]++;
                    }
                    if (data.UVSets.Length != 0)
                    {
                        mdict["uvsets"] = WriteUV(writer, data, ref level);

                    }
                    if (data.HasVertexColors)
                    {
                        mdict["vcolor"] = WriteVColor(writer, data, ref level);
                        level.indent["aindent"]++;
                    }
                    if (data.Points.Length != 0)
                    {
                        mdict["indice"] = WriteIndices(writer, data, ref level);
                        level.indent["aindent"]++;
                    }
                }
            }
        }


        private static Dictionary<string, object> WriteVertex(BinaryWriter writer, NiTriStripsData data, ref Level level)
        {
            var vx = data.Vertices;
            var minX = vx[0].X;
            var maxX = vx[0].X;
            var minY = vx[0].Y;
            var maxY = vx[0].Y;
            var minZ = vx[0].Z;
            var maxZ = vx[0].Z;
            foreach (var v in vx)
            {
                writer.Write(v.X);
                writer.Write(v.Y);
                writer.Write(v.Z);

                minX = Math.Min(minX, v.X);
                maxX = Math.Max(maxX, v.X);
                minY = Math.Min(minY, v.Y);
                maxY = Math.Max(maxY, v.Y);
                minZ = Math.Min(minZ, v.Z);
                maxZ = Math.Max(maxZ, v.Z);
            }

            return new Dictionary<string, object>
            {
                {"accessor", level.indent["aindent"]},
                {"numvertex", data.NumVertices},
                {"min", new List<float>{minX, minY, minZ}},
                {"max", new List<float>{maxX, maxY, maxZ}},

            };
        }
        private static Dictionary<string, object> WriteNormal(BinaryWriter writer, NiTriStripsData data, ref Level level)
        {
            var ns = data.Normals;
            foreach (var n in ns)
            {
                writer.Write(n.X);
                writer.Write(n.Y);
                writer.Write(n.Z);
            }
            return new Dictionary<string, object>
            {
                {"accessor", level.indent["aindent"]},
                {"numnormal", ns.Length},
            };
        }
        private static List<Dictionary<string, object>> WriteUV(BinaryWriter writer, NiTriStripsData data, ref Level level)
        {
            var uvSetsList = new List<Dictionary<string, object>>();
            foreach (var uvSet in data.UVSets)
            {
                var uvdict = new Dictionary<string, object>
                {
                    {"accessor",level.indent["aindent"]},
                    {"numuv",uvSet.Length},
                };
                uvSetsList.Add(uvdict);
                foreach (var uv in uvSet)
                {
                    writer.Write(uv.X);
                    writer.Write(uv.Y);
                }

                level.indent["aindent"]++;
            }
            return uvSetsList;
        }
        private static Dictionary<string, object> WriteVColor(BinaryWriter writer, NiTriStripsData data, ref Level level)
        {
            var vcs = data.VertexColors;
            foreach (var vc in vcs)
            {
                writer.Write(vc.R);
                writer.Write(vc.G);
                writer.Write(vc.B);
                writer.Write(vc.A);
            }
            return new Dictionary<string, object>
            {
                {"numvcolor", vcs.Length},
                {"accessor", level.indent["aindent"]}
            };
        }
        private static Dictionary<string, object> WriteIndices(BinaryWriter writer, NiTriStripsData data, ref Level level)
        {
            int numtriangle = 0;
            foreach (var points in data.Points)
            {
                for (int i = 2; i < points.Length; i++)
                {
                    //triangle strip to tri shape converter
                    //indices[1,2,3,4] => tri[1,2,3], tri[2,4,3]
                    //triangle counter clockwise or clockwise order for normals
                    var triangle = new[] { points[i - 2], points[i - 1], points[i] };
                    if (i % 2 == 0)
                    {
                        writer.Write((ushort)triangle[0]);
                        writer.Write((ushort)triangle[1]);
                        writer.Write((ushort)triangle[2]);
                    }
                    else
                    {
                        writer.Write((ushort)triangle[0]);
                        writer.Write((ushort)triangle[2]);
                        writer.Write((ushort)triangle[1]);
                    }

                    numtriangle++;
                }
            }
            return new Dictionary<string, object>
            {
                {"accessor", level.indent["aindent"]},
                {"numtriangle", numtriangle},
            };
        }

        private static void ProcessTexture(NiTriStrips mesh, string tgapath, ref Dictionary<string, object> mdict, ref Level level)
        {

            var material = new Dictionary<string, object>();
            var texture = new Dictionary<string, object>();
            var Source = new Dictionary<string, object>();
            var texdata = new Dictionary<string, object>();

            for (var i = 0; i < mesh.Properties.Length; i++)
            {
                var prop = mesh.Properties[i].Object;

                if (prop is NiMaterialProperty niMaterialProperty)
                {
                    var d = niMaterialProperty.DiffuseColor;
                    var s = niMaterialProperty.SpecularColor;
                    var e = niMaterialProperty.EmissiveColor;
                    material["id"] = level.indent["tindent"];
                    material["diffuse"] = new List<float> { d.R, d.G, d.B };
                    material["specular"] = new List<float> { s.R, s.G, s.B, s.A };
                    material["glossiness"] = niMaterialProperty.Glossiness;
                    material["emissive"] = new List<float> { e.R, e.G, e.B, e.A };
                    material["alpha"] = niMaterialProperty.Alpha;
                    if (!string.IsNullOrEmpty(niMaterialProperty.Name.Value))
                    {
                        material["name"] = niMaterialProperty.Name.Value;
                    }
                    mdict["material"] = material;
                }

                if (prop is NiTexturingProperty tex)
                {
                    if (tex.HasBaseTexture)
                    {
                        texture["TextureCount"] = tex.TextureCount;
                        texture["ClampMode"] = tex.BaseTexture.ClampMode.ToString();
                        texture["UVSetIndex"] = tex.BaseTexture.UVSetIndex;

                        mdict["texture"] = texture;

                    }

                    if (tex.BaseTexture.Source.Object is NiSourceTexture source)
                    {
                        Source["UseExternal"] = source.UseExternal;
                        Source["AlphaFormat"] = source.AlphaFormat.ToString();
                        Source["UseMipmaps"] = source.UseMipmaps.ToString();
                        Source["PixelLayout"] = source.PixelLayout.ToString();
                        Source["IsStatic"] = source.IsStatic;

                        mdict["source"] = Source;


                        if (!source.UseExternal && source.InternalTexture.Object is NiPixelData niPixelData)
                        {
                            texdata["NumPixels"] = niPixelData.NumPixels;
                            texdata["NumFaces"] = niPixelData.NumFaces;
                            texdata["NumMipMap"] = niPixelData.NumMipMaps;
                            texdata["PixelFormat"] = niPixelData.PixelFormat.ToString();
                            var maps = new List<Dictionary<string, object>>();
                            foreach (var mip in niPixelData.MipMaps)
                            {
                                var mipMap = new Dictionary<string, object>();
                                mipMap["Width"] = mip.Width;
                                mipMap["Height"] = mip.Height;
                                mipMap["Offset"] = mip.Offset;
                                maps.Add(mipMap);
                            }
                            texdata["mipmap"] = maps;
                            texdata["NumFaces"] = niPixelData.NumFaces;


                            texdata["BitsPerPixel"] = niPixelData.BitsPerPixel;
                            texdata["BytesPerPixel"] = niPixelData.BytesPerPixel;
                            texdata["Mask"] = new List<uint>
                            {
                                niPixelData.RedMask,
                                niPixelData.BlueMask,
                                niPixelData.GreenMask,
                                niPixelData.AlphaMask
                            };

                            mdict["texdata"] = texdata;


                            var palette = niPixelData.Palette.Object.Palette;
                            var pixelData = niPixelData.PixelData;
                            var width = (int)niPixelData.MipMaps[0].Width;
                            var height = (int)niPixelData.MipMaps[0].Height;

                            TGAGenerator tgaGenerator = new TGAGenerator(palette, pixelData, width, height);

                            tgaGenerator.SaveTGA(tgapath);
                            level.indent["tindent"]++;

                        }
                    }
                }
            }
        }
    }
}
