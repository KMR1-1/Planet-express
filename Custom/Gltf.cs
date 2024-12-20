namespace FuturamaLib.NIF.Custom
{
    class Gltf
    {
            public Dictionary<string, object> asset { get; set; }
            public List<Dictionary<string, object>> scenes { get; set; }
            public List<Dictionary<string, object>> nodes { get; set; }
            public List<Dictionary<string, object>> meshes { get; set; }
            public List<Dictionary<string, object>> accessors { get; set; }
            public List<Dictionary<string, object>> bufferViews { get; set; }
            public List<Dictionary<string, object>> buffers { get; set; }
            public List<Dictionary<string, object>> materials { get; set; }
            public List<Dictionary<string, object>> samplers { get; set; }
            public List<Dictionary<string, object>> textures { get; set; }
            public List<Dictionary<string, object>> images { get; set; }
            public Level level { get; set; }
            public int ncount { get; set; }
            public int mcount { get; set; }
            public int acount { get; set; }
            public int tcount { get; set; }
            public int offset { get; set; }

            public Gltf(string maindir, string levelname)
            {
                meshes = new List<Dictionary<string, object>>();
                accessors = new List<Dictionary<string, object>>();
                bufferViews = new List<Dictionary<string, object>>();
                buffers = new List<Dictionary<string, object>>();
                materials = new List<Dictionary<string, object>>();
                textures = new List<Dictionary<string, object>>();
                samplers = new List<Dictionary<string, object>>();
                images = new List<Dictionary<string, object>>();
                mcount = 0;
                acount = 0;
                tcount = 0;
                
                level = new Level(maindir, levelname);

                asset = new Dictionary<string, object>
                {
                    {"version", "2.0"},
                };
            }
            public Dictionary<string, object> ToDict()
            {
                return new Dictionary<string, object>
                {
                    {"asset", asset},
                    //{"scenes", scenes},
                    //{"nodes", nodes},
                    {"meshes", meshes},
                    {"accessors", accessors},
                    {"bufferViews", bufferViews},
                    {"buffers", buffers},
                    {"materials", materials},
                    {"textures", textures},
                    {"samplers", samplers},
                    {"images", images},
                };
            }
    }
}