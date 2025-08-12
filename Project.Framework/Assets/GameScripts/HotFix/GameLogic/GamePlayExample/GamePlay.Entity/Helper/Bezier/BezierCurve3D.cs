using System;
using System.Collections.Generic;
using UnityEngine;

public static class math
{
    public static Vector3 Normalize(Vector3 v) => Vector3.Normalize(v);
    public static Vector3 Cross(Vector3 a, Vector3 b) => Vector3.Cross(a, b);
    public static float Distance(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
    public static float Cos(float a) => Mathf.Cos(a);
    public static float Sin(float a) => Mathf.Sin(a);
}

namespace NaughtyBezierCurves
{
    [Serializable]
    public class BezierCurve3D : ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("Segments used for evaluation; affects precision & performance.")]
        private int sampling = 25;

        [SerializeField, Range(0f, 1f)]
        private float normalizedTime = 0.5f;

        public Vector3 OriginPosition { get; set; }

        [SerializeField]
        private List<BezierPoint3D> keyPoints = new List<BezierPoint3D>();

        // ——— Properties ———
        public int Sampling
        {
            get => sampling;
            set => sampling = value;
        }

        public List<BezierPoint3D> KeyPoints
        {
            get => keyPoints;
            set => keyPoints = value;
        }

        public int KeyPointsCount => KeyPoints.Count;

        // ——— Public Methods ———
        public BezierPoint3D AddKeyPoint()
        {
            return AddKeyPointAt(KeyPointsCount);
        }

        public BezierPoint3D AddKeyPointAt(int index)
        {
            BezierPoint3D newPoint = new BezierPoint3D();
            newPoint.Curve = this;

            if (KeyPointsCount == 0 || KeyPointsCount == 1)
            {
                newPoint.LocalPosition = Vector3.zero;
            }
            else
            {
                if (index == 0)
                {
                    newPoint.Position = math.Normalize(KeyPoints[0].Position - KeyPoints[1].Position) + KeyPoints[0].Position;
                }
                else if (index == KeyPointsCount)
                {
                    newPoint.Position = math.Normalize(KeyPoints[index - 1].Position - KeyPoints[index - 2].Position) + KeyPoints[index - 1].Position;
                }
                else
                {
                    newPoint.Position = GetPointOnCubicCurve(0.5f, KeyPoints[index - 1], KeyPoints[index]);
                }
            }

            KeyPoints.Insert(index, newPoint);
            return newPoint;
        }

        public bool RemoveKeyPointAt(int index)
        {
            if (KeyPointsCount < 2) return false;
            // var point = KeyPoints[index];
            KeyPoints.RemoveAt(index);
            return true;
        }

        public Vector3 GetPoint(float time)
        {
            GetCubicSegment(time, out var startPoint, out var endPoint, out var t);
            return GetPointOnCubicCurve(t, startPoint, endPoint);
        }

        public Quaternion GetRotation(float time, Vector3 up)
        {
            GetCubicSegment(time, out var startPoint, out var endPoint, out var t);
            return GetRotationOnCubicCurve(t, up, startPoint, endPoint);
        }

        public Vector3 GetTangent(float time)
        {
            GetCubicSegment(time, out var startPoint, out var endPoint, out var t);
            return GetTangentOnCubicCurve(t, startPoint, endPoint);
        }

        public Vector3 GetBinormal(float time, Vector3 up)
        {
            GetCubicSegment(time, out var startPoint, out var endPoint, out var t);
            return GetBinormalOnCubicCurve(t, up, startPoint, endPoint);
        }

        public Vector3 GetNormal(float time, Vector3 up)
        {
            GetCubicSegment(time, out var startPoint, out var endPoint, out var t);
            return GetNormalOnCubicCurve(t, up, startPoint, endPoint);
        }

        public float GetApproximateLength()
        {
            if (KeyPointsCount < 2) return 0f;

            float length = 0;
            int subCurveSampling = (Sampling / (KeyPointsCount - 1)) + 1;
            for (int i = 0; i < KeyPointsCount - 1; i++)
            {
                length += GetApproximateLengthOfCubicCurve(KeyPoints[i], KeyPoints[i + 1], subCurveSampling);
            }

            return length;
        }

        public void GetCubicSegment(float time, out BezierPoint3D startPoint, out BezierPoint3D endPoint, out float timeRelativeToSegment)
        {
            startPoint = null;
            endPoint = null;
            timeRelativeToSegment = 0f;

            if (KeyPointsCount < 2)
            {
                return;
            }

            float subCurvePercent = 0f;
            float totalPercent = 0f;
            float approximateLength = GetApproximateLength();
            if (approximateLength <= Mathf.Epsilon)
            {
                startPoint = KeyPoints[0];
                endPoint = KeyPoints[KeyPointsCount - 1];
                timeRelativeToSegment = time;
                return;
            }

            int subCurveSampling = (Sampling / (KeyPointsCount - 1)) + 1;

            for (int i = 0; i < KeyPointsCount - 1; i++)
            {
                subCurvePercent = GetApproximateLengthOfCubicCurve(KeyPoints[i], KeyPoints[i + 1], subCurveSampling) / approximateLength;
                if (subCurvePercent + totalPercent > time)
                {
                    startPoint = KeyPoints[i];
                    endPoint = KeyPoints[i + 1];
                    break;
                }

                totalPercent += subCurvePercent;
            }

            if (endPoint == null)
            {
                startPoint = KeyPoints[KeyPointsCount - 2];
                endPoint = KeyPoints[KeyPointsCount - 1];
                totalPercent -= subCurvePercent;
            }

            timeRelativeToSegment = (time - totalPercent) / (subCurvePercent <= Mathf.Epsilon ? 1f : subCurvePercent);
        }

        public static Vector3 GetPointOnCubicCurve(float time, BezierPoint3D startPoint, BezierPoint3D endPoint)
        {
            return GetPointOnCubicCurve(time, startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition);
        }

        public static Vector3 GetPointOnCubicCurve(float time, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            float t = time;
            float u = 1f - t;
            float t2 = t * t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t3 = t2 * t;

            Vector3 result =
                (u3) * startPosition +
                (3f * u2 * t) * startTangent +
                (3f * u * t2) * endTangent +
                (t3) * endPosition;

            return result;
        }

        public static Quaternion GetRotationOnCubicCurve(float time, Vector3 up, BezierPoint3D startPoint, BezierPoint3D endPoint)
        {
            return GetRotationOnCubicCurve(time, up, startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition);
        }

        public static Quaternion GetRotationOnCubicCurve(float time, Vector3 up, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            Vector3 tangent = GetTangentOnCubicCurve(time, startPosition, endPosition, startTangent, endTangent);
            Vector3 normal = GetNormalOnCubicCurve(time, up, startPosition, endPosition, startTangent, endTangent);
            return Quaternion.LookRotation(tangent, normal);
        }

        public static Vector3 GetTangentOnCubicCurve(float time, BezierPoint3D startPoint, BezierPoint3D endPoint)
        {
            return GetTangentOnCubicCurve(time, startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition);
        }

        public static Vector3 GetTangentOnCubicCurve(float time, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            float t = time;
            float u = 1f - t;
            float u2 = u * u;
            float t2 = t * t;

            Vector3 tangent =
                (-u2) * startPosition +
                (u * (u - 2f * t)) * startTangent -
                (t * (t - 2f * u)) * endTangent +
                (t2) * endPosition;

            return math.Normalize(tangent);
        }

        public static Vector3 GetBinormalOnCubicCurve(float time, Vector3 up, BezierPoint3D startPoint, BezierPoint3D endPoint)
        {
            return GetBinormalOnCubicCurve(time, up, startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition);
        }

        public static Vector3 GetBinormalOnCubicCurve(float time, Vector3 up, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            Vector3 tangent = GetTangentOnCubicCurve(time, startPosition, endPosition, startTangent, endTangent);
            Vector3 binormal = math.Cross(up, tangent);
            return math.Normalize(binormal);
        }

        public static Vector3 GetNormalOnCubicCurve(float time, Vector3 up, BezierPoint3D startPoint, BezierPoint3D endPoint)
        {
            return GetNormalOnCubicCurve(time, up, startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition);
        }

        public static Vector3 GetNormalOnCubicCurve(float time, Vector3 up, Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent)
        {
            Vector3 tangent = GetTangentOnCubicCurve(time, startPosition, endPosition, startTangent, endTangent);
            Vector3 binormal = GetBinormalOnCubicCurve(time, up, startPosition, endPosition, startTangent, endTangent);
            Vector3 normal = math.Cross(tangent, binormal);
            return math.Normalize(normal);
        }

        public static float GetApproximateLengthOfCubicCurve(BezierPoint3D startPoint, BezierPoint3D endPoint, int sampling)
        {
            return GetApproximateLengthOfCubicCurve(startPoint.Position, endPoint.Position, startPoint.RightHandlePosition, endPoint.LeftHandlePosition, sampling);
        }

        public static float GetApproximateLengthOfCubicCurve(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, int sampling)
        {
            float length = 0f;
            Vector3 fromPoint = GetPointOnCubicCurve(0f, startPosition, endPosition, startTangent, endTangent);

            for (int i = 0; i < sampling; i++)
            {
                float time = (i + 1) / (float)sampling;
                Vector3 toPoint = GetPointOnCubicCurve(time, startPosition, endPosition, startTangent, endTangent);
                length += math.Distance(fromPoint, toPoint);
                fromPoint = toPoint;
            }

            return length;
        }

        // —— 反序列化回填：修复循环序列化根因 —— 
        public void OnAfterDeserialize()
        {
            if (keyPoints == null) return;
            for (int i = 0; i < keyPoints.Count; i++)
            {
                var p = keyPoints[i];
                if (p != null) p.Curve = this;
            }
        }

        public void OnBeforeSerialize() { }

        // 可视化 Gizmos
        // protected virtual void OnDrawGizmos()
        // {
        //     if (this.KeyPointsCount > 1)
        //     {
        //         Vector3 fromPoint = this.GetPoint(0f);
        //         for (int i = 0; i < this.Sampling; i++)
        //         {
        //             float time = (i + 1) / (float)this.Sampling;
        //             Vector3 toPoint = this.GetPoint(time);
        //             Gizmos.DrawLine(fromPoint, toPoint);
        //             fromPoint = toPoint;
        //         }
        //     }
        // }
    }
}
