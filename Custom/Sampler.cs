using FuturamaLib.NIF.Custom;
using FuturamaLib.NIF.Structures;

namespace Name
{
    class Sampler
    {
        public Sampler(NiTexturingProperty tex, ref Gltf gltf)
        {
            var sampler = new Dictionary<string, object>();
            var filter = tex.BaseTexture.FilterMode;
            switch (filter)
            {
                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_NEAREST:
                    sampler["magFilter"] = 9728;
                    sampler["minFilter"] = 9728;
                    break;

                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_BILERP:
                    sampler["magFilter"] = 9729;
                    sampler["minFilter"] = 9729;
                    break;

                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_TRILERP:
                    sampler["magFilter"] = 9729;
                    sampler["minFilter"] = 9987;
                    break;

                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_NEAREST_MIPNEAREST:
                    sampler["magFilter"] = 9728;
                    sampler["minFilter"] = 9984;
                    break;

                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_NEAREST_MIPLERP:
                    sampler["magFilter"] = 9728;
                    sampler["minFilter"] = 9986;
                    break;

                case FuturamaLib.NIF.Enums.TexFilterMode.FILTER_BILERP_MIPNEAREST:
                    sampler["magFilter"] = 9729;
                    sampler["minFilter"] = 9985;
                    break;

                default:
                    sampler["magFilter"] = 9728;
                    sampler["minFilter"] = 9728;
                    break;
            }

            var clamp = tex.BaseTexture.ClampMode;

            switch(clamp)
            {
                case FuturamaLib.NIF.Enums.TexClampMode.CLAMP_S_CLAMP_T:
                    sampler["wrapS"] = 33071;
                    sampler["wrapT"] = 33071;
                    break;

                case FuturamaLib.NIF.Enums.TexClampMode.CLAMP_S_WRAP_T:
                    sampler["wrapS"] = 33071;
                    sampler["wrapT"] = 10497;
                    break;

                case FuturamaLib.NIF.Enums.TexClampMode.WRAP_S_CLAMP_T:
                    sampler["wrapS"] = 10497;
                    sampler["wrapT"] = 33071;
                    break;

                case FuturamaLib.NIF.Enums.TexClampMode.WRAP_S_WRAP_T:
                    sampler["wrapS"] = 10497;
                    sampler["wrapT"] = 10497;
                    break;

                default:
                    sampler["wrapS"] = 33071;
                    sampler["wrapT"] = 33071;
                    break;
            }

            gltf.samplers.Add(sampler);
        }
    }
}