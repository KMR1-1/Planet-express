using System.IO;
using System.Numerics;
using Matrix = System.Numerics.Matrix4x4;

namespace FuturamaLib.NIF.Custom
{
    public class Node
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public List<float> Translations { get; set; }
        public float Scale { get; set; }
        public List<float> Rotations { get; set; }
        public Dictionary<string, object> Mesh { get; set; } // Ajout pour inclure les informations sur les meshes
        public List<Node> Children { get; set; }

        public Node()
        {
            Children = new List<Node>();
        }
    }
}