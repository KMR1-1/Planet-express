using System.Numerics;

namespace FuturamaLib.NIF.Custom
{
    class VertexAccessor
    {
        public uint count { get; set; }
        public List<float> min { get; set; }
        public List<float> max { get; set; }
        public int bvId { get; set; }
        public string type { get; set; }
        public Dictionary<string, object> ToDic()
        {
            var accessor =  new Dictionary<string, object>()
            {
                {"bufferView", bvId},
                {"componentType", 5126},
                {"count", count},
                {"type", type},
            };

            if(min.Count != 0)
                accessor["min"] = min;

            if(max.Count != 0)
                accessor["max"] = max;

            return accessor;
        }
    }

}