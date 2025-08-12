using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace NaughtyBezierCurves
{
    [Serializable]
    public class BezierPoint3D
    {
        public enum HandleType
        {
            Connected,
            Broken
        }

        // —— 关键修复：不让 Unity 序列化对子曲线的回指 ——
        [NonSerialized]
        private BezierCurve3D _curve;

        // 你可以通过属性在运行时/反序列化后设置它
        public BezierCurve3D Curve
        {
            get => _curve;
            set => _curve = value;
        }

        [SerializeField]
        private HandleType handleType = HandleType.Connected;

        [SerializeField]
        private Vector3 leftHandleLocalPosition = new(-0.5f, 0f, 0f);

        [SerializeField]
        private Vector3 rightHandleLocalPosition = new(0.5f, 0f, 0f);

        public Vector3 CurvePosition => _curve?.OriginPosition ?? Vector3.zero;

        public HandleType HandleStyle
        {
            get => handleType;
            set => handleType = value;
        }

        public Vector3 Position
        {
            get => CurvePosition + LocalPosition;
            set => LocalPosition = value - CurvePosition;
        }

        /// <summary>
        /// 点的局部位置（相对曲线 OriginPosition）。
        /// </summary>
        public Vector3 LocalPosition;

        public Vector3 LeftHandleLocalPosition
        {
            get => leftHandleLocalPosition;
            set
            {
                this.leftHandleLocalPosition = value;
                if (handleType == HandleType.Connected)
                {
                    rightHandleLocalPosition = -value;
                }
            }
        }

        public Vector3 InTangent
        {
            get => LeftHandleLocalPosition;
            set => LeftHandleLocalPosition = value;
        }

        public Vector3 RightHandleLocalPosition
        {
            get => rightHandleLocalPosition;
            set
            {
                rightHandleLocalPosition = value;
                if (handleType == HandleType.Connected)
                {
                    leftHandleLocalPosition = -value;
                }
            }
        }

        public Vector3 OutTangent
        {
            get => RightHandleLocalPosition;
            set => RightHandleLocalPosition = value;
        }

        public Vector3 LeftHandlePosition
        {
            get
            {
                if (handleType == HandleType.Broken) return Position;
                return Position + LeftHandleLocalPosition;
            }
        }

        public Vector3 RightHandlePosition
        {
            get
            {
                if (handleType == HandleType.Broken) return Position;
                return Position + RightHandleLocalPosition;
            }
        }
    }
}
