using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

namespace GameLogic
{
    public interface IPosition
    {
        Vector3 Position { get; set; }
    }
}
public static class RotationExtension
{
#if EGAMEPLAY_3D
        public static Vector3 GetForward(this Quaternion rotation)
        {
            return rotation * Vector3.forward;
        }

        public static Quaternion GetRotation(this Vector3 up)
        {
            return Quaternion.LookRotation(Vector3.forward, up);
        }
#else
    public static Vector3 GetForward(this Quaternion rotation)
    {
        return rotation * Vector3.right;
    }

    public static Quaternion GetRotation(this Vector3 right)
    {
        var up = math.cross(Vector3.forward, right);
        return Quaternion.LookRotation(Vector3.forward, up);
    }
#endif
}