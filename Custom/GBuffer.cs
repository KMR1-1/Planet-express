namespace FuturamaLib.NIF.Custom
{
    class GBuffer
    {
        public GBuffer(string binpath, ref Gltf gltf)
        {
            var buffer = new Dictionary<string, object>
            {
                {"uri", binpath},
                {"byteLength", gltf.offset},
            };
            gltf.buffers.Add(buffer);
        }
    }
}