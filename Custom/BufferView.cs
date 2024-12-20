namespace FuturamaLib.NIF.Custom
{
    class BufferView
    {
        public BufferView(string type, uint count, ref Gltf gltf)
        {
            int btlen;
            switch(type)
            {
                case "VEC3":
                    btlen = (int)(count*12);
                    break;
                case "VEC2":
                    btlen = (int)(count*8);
                    break;
                case "VEC4":
                    btlen = (int)(count*16);
                    break;
                case "SCALAR":
                    btlen = (int)(count*2);
                    break;
                default:
                    throw new ArgumentException($"Type '{type}' non reconnu.");
            }

            var bufferView = new Dictionary<string, object>
            {
                {"buffer", gltf.mcount},
                {"byteOffset", gltf.offset},
                {"byteLength", btlen},
                {"target", 34962},
            };
            if(type == "SCALAR")
                bufferView["target"] = 34963;

            gltf.bufferViews.Add(bufferView);
            gltf.offset += btlen;
        }
    }
}