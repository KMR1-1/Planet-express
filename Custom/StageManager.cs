using FuturamaLib.NIF.Structures;

namespace FuturamaLib.NIF.Custom
{
    // Gestion des stages
    class StageManager
    {
        public List<string> StageNames { get; private set; }
        public List<NIFReader> Readers { get; private set; }

        public StageManager(NIFReader defsReader, string levelPath)
        {
            (StageNames, Readers) = LoadStages(defsReader, levelPath);
        }

        private static (List<string>, List<NIFReader>) LoadStages(NIFReader defsReader, string levelPath)
        {
            var stageNames = new List<string>();
            var readers = new List<NIFReader>();
            string nodeName = "";

            foreach (var root in defsReader.Footer.RootNodes)
            {
                if (root.Object is NiNode niNode && niNode.Name.Value is string name && name.Contains("::") && name.Split("::")[0] == "Stage")
                {
                    nodeName = $"{name.Split("::")[1].ToLower()}.nif";
                    stageNames.Add(nodeName);
                }
            }

            while (true)
            {
                var reader = new NIFReader($"{levelPath}/{nodeName}");
                readers.Add(reader);

                bool hasNextStage = false;
                foreach (var root in reader.Footer.RootNodes)
                {
                    if (root.Object is NiUDSNode niUDSNode && niUDSNode.Name.Value is string name && name.Contains(":") && name.Split(":")[0] == "LinkEnd")
                    {
                        nodeName = $"{name.Split(":")[1].ToLower()}.nif";
                        if (nodeName == "null.nif")
                            return (stageNames, readers);

                        stageNames.Add(nodeName);
                        hasNextStage = true;
                    }
                }

                if (!hasNextStage) break;
            }

            return (stageNames, readers);
        }
    }
}
    // Gestion des répertoires de sortie
    