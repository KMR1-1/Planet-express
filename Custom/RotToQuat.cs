using System.IO;
using System.Numerics;
using Matrix = System.Numerics.Matrix4x4;


namespace FuturamaLib.NIF.Custom
{
    public class RotToQuat
    {
        public static Quaternion Quat(Matrix rot)
        {
            
            var matrix = new Matrix4x4(rot.M11, rot.M12, rot.M13, rot.M14,rot.M21, rot.M22, rot.M23, rot.M24,rot.M31, rot.M32, rot.M33, rot.M34, rot.M41, rot.M42, rot.M43, rot.M44);
            Quaternion quaternions = Matrix4x4.MatrixToQuaternionList(matrix);
            return quaternions;

        }
    }
}