using FuturamaLib.NIF.Structures;

namespace FuturamaLib.NIF.Custom
{
    class Level
    {
        public List<List<Node>> levelStruct {get;set;}
        public List<Node> Instances {get;set;}
        public List<int> scene;
        public Dictionary<string, int> indent {get; set;}
        public string Out { get; }
        public string MainDir { get; private set;}
        public string Debug { get; }
        public string DebugDefs { get; }
        public List<string> StageNames { get; }
        public List<NIFReader> Readers { get; }
        public NIFReader defsReader{ get; private set;}
        public NIFReader effectReader{ get; private set;}
        
        public Level(string maindir, string levelname)
        {
            scene = new List<int>();
            MainDir = maindir;
            levelStruct = new List<List<Node>>();
            Instances = new List<Node>();
            indent = new Dictionary<string, int>
            {
                {"nindent",-1},
                {"mindent", -1},
                {"aindent", 0},
                {"tindent", 0}
            };
            var levelPath = $"{maindir}/levels/{levelname}"; 
            string DefsPath = $"{levelPath}/defs.nif";
            string effectPath = $"{maindir}/effects/{levelname}.nif";
            defsReader = new NIFReader(DefsPath);
            effectReader = new NIFReader(effectPath);

            
            
            Out = $"output/{levelname}";
            if(Directory.Exists($"{Out}"))
                Directory.Delete($"{Out}", recursive: true);
            Directory.CreateDirectory($"{Out}/data/");
            Directory.CreateDirectory($"{Out}/texture/");

            Debug = $"{Out}/debug";
            DebugDefs = $"{Out}/debug/defs.nif_structure.json";
            Directory.CreateDirectory($"{Out}/debug/");
            var stageManager = new StageManager(defsReader, levelPath);
            var debugOut = new List<string>();
            foreach(var stage in stageManager.StageNames) 
            {
                debugOut.Add($"{Debug}/{stage}_Structure.json");
            }
            StageNames = debugOut;
            
            Readers = stageManager.Readers;
            fixRef(Readers, defsReader);
        }
        public void fixRef(List<NIFReader> Readers,NIFReader defsReader)
        {
            for(var i=0; i< Readers.Count; i++)
            {
                foreach(var kvp in Readers[i].ObjectsByRef)
                {                
                    if(kvp.Value is NiTexturingProperty niTexturingProperty && niTexturingProperty.BaseTexture.Source.IsValid)
                    {
                        niTexturingProperty.BaseTexture.Source.SetRef(Readers[i]);
                    }
                }
            }
            foreach(var kvp in defsReader.ObjectsByRef)
            {                
                if(kvp.Value is NiTexturingProperty niTexturingProperty && niTexturingProperty.BaseTexture.Source.IsValid)
                {
                    niTexturingProperty.BaseTexture.Source.SetRef(defsReader);
                }
            }
        }
    }
}