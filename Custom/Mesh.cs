
using System.Security.AccessControl;
using FuturamaLib.NIF.Structures;
using Gtk;
using Name;

namespace FuturamaLib.NIF.Custom
{
    class Mesh
    {
        public string name { get; set; }
        public string binpath { get; set; }
        public Dictionary<string, object> mdict  { get; set; }
        public Gltf gltf;
        public Mesh(NiTriStrips mesh, ref Gltf gltf1)
        {
            gltf = gltf1;
            name = GetName(mesh, gltf);
            binpath = GetBinPath(name, gltf);
            var attributes = new Dictionary<string, object>();
            var primitives = new Dictionary<string, object>();
            mdict = new Dictionary<string, object>();

            if(mesh.Data.IsValid && mesh.Data.Object is NiTriStripsData data && data != null)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(binpath, FileMode.Create)))
                {        
                    gltf.offset = 0;
                    if(data.HasVertices)
                    {
                        attributes["POSITION"] = gltf.acount;
                        ProcessVertex(writer, data, ref gltf);
                    }
                    if(data.HasNormals)
                    {
                        attributes["NORMAL"] = gltf.acount;
                        ProcessNormal(writer, data, ref gltf);
                    }
                    if(data.UVSets.Length != 0)
                    {
                        for (int i = 0; i < data.UVSets.Length; i++)
                        {
                            attributes[$"TEXCOORD_{i}"] = gltf.acount;
                            ProcessUV(i, writer, data, ref gltf);
                        }
                    }
                    if (data.HasVertexColors)
                    {
                        attributes["COLOR_0"] = gltf.acount;
                        ProcessVertexColor(writer, data, ref gltf);
                    }
                    primitives["attributes"] = attributes;
                    primitives["indices"] = gltf.acount;
                    ProcessIndices(writer, data, ref gltf);
                }


                new GBuffer(binpath, ref gltf);

                foreach (var prop in mesh.Properties)
                {
                    if(prop.Object is NiTexturingProperty || prop.Object is NiMaterialProperty)
                    {
                        new Material(name, mesh, ref gltf);
                        primitives["material"] = gltf.tcount-1;
                    }
                }
                
                mdict["primitives"]= new List<Dictionary<string, object>>{primitives};   
                
            }
            
        }
        public void AddMat(int id)
        {
            if(mdict.TryGetValue("primitives", out var p) && p is List<Dictionary<string, object>> primitives)
            {
                var primdic = primitives[0];
                primdic["material"] = id;
            }
        }
        public void AddToGltf()
        {
            gltf.meshes.Add(mdict);
            gltf.mcount++;
        }
        
        public static void ProcessVertex(BinaryWriter writer, NiTriStripsData data, ref Gltf gltf)
        {
            var v = data.Vertices;
            var min = new List<float>{v[0].X, v[0].Y, v[0].Z};
            var max = new List<float>{v[0].X, v[0].Y, v[0].Z};
            
            foreach (var vx in v)
            {
                writer.Write(vx.X);
                writer.Write(vx.Y);
                writer.Write(vx.Z);
                min[0] = Math.Min(min[0], vx.X);
                min[1] = Math.Min(min[1], vx.Y);
                min[2] = Math.Min(min[2], vx.Z);

                max[0] = Math.Max(max[0], vx.X);
                max[1] = Math.Max(max[1], vx.Y);
                max[2] = Math.Max(max[2], vx.Z);
            }
            new GAccessor(data.NumVertices, "VEC3", ref gltf, min, max);
        }
        public static void ProcessNormal(BinaryWriter writer, NiTriStripsData data, ref Gltf gltf)
        {
            foreach (var n in data.Normals)
            {
                writer.Write(n.X);
                writer.Write(n.Y);
                writer.Write(n.Z);
            }

            new GAccessor(data.NumVertices, "VEC3", ref gltf);
        }
        public static void ProcessUV(int i,BinaryWriter writer, NiTriStripsData data, ref Gltf gltf)
        {
            
            foreach(var u in data.UVSets[i])
            {
                writer.Write(u.X);
                writer.Write(u.Y);
            } 
            new GAccessor((uint)data.UVSets[i].Length, "VEC2", ref gltf);
                       
        }
        public static void ProcessVertexColor(BinaryWriter writer, NiTriStripsData data, ref Gltf gltf)
        {
            foreach (var vc in data.VertexColors)
            {
                writer.Write(vc.R);
                writer.Write(vc.G);
                writer.Write(vc.B);
                writer.Write(vc.A);
            }            

            new GAccessor(data.NumVertices, "VEC4", ref gltf);
        }
        public static void ProcessIndices(BinaryWriter writer, NiTriStripsData data, ref Gltf gltf)
        {
            int numtriangle = 0;
            foreach (var p in data.Points)
            {
                for (int i = 2; i < p.Length; i++)
                {
                    // Chaque triangle est défini par 3 sommets consécutifs
                    var triangle = new[] { p[i - 2], p[i - 1], p[i] };
                    if(i%2 == 0)
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
            new GAccessor((uint)(data.NumTriangles*3), "SCALAR", ref gltf);
        }
        public static string GetName(NiTriStrips mesh, Gltf gltf)
        {
            string name = mesh.Name.Value;
            if (string.IsNullOrEmpty(name))
                name = $"{gltf.mcount}";

            return name;
        }
        public static string GetBinPath(string name, Gltf gltf)
        {
            string binpath = $"{gltf.level.Out}/data/{name}.bin";
            if (File.Exists(binpath))
            {
                name = $"{name}_{gltf.level.indent["mindent"]}";
                binpath = $"{gltf.level.Out}/data/{name}.bin";
            }
            return binpath;
        }
    }
}