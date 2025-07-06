using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic.GoapModule.Path
{
    public class AStar
    {
        /// <summary>
        /// 通用A*寻路算法，支持动态障碍、拐弯cost、地形权重
        /// </summary>
        public static class PathfindingAStar
        {
            /// <summary>
            /// 查找路径
            /// </summary>
            /// <param name="start">起点</param>
            /// <param name="goal">终点</param>
            /// <param name="isWalkable">格子可走性检测（可支持动态障碍）</param>
            /// <param name="turnCost">转弯cost（转向时附加消耗，0=无拐弯惩罚）</param>
            /// <param name="diagCost">斜走消耗（对角线距离，默认1.4近似√2）</param>
            /// <param name="terrainCostFunc">地形消耗委托（为null则默认每格1）</param>
            /// <returns>路径点（含起终点），找不到返回null</returns>
            public static List<Vector2Int> FindPath(
                Vector2Int start,
                Vector2Int goal,
                Func<Vector2Int, bool> isWalkable,
                float turnCost = 0.0f,
                float diagCost = 1.4f,
                Func<Vector2Int, float> terrainCostFunc = null)
            {
                var open = new PriorityQueue<Node>();
                var closed = new HashSet<Vector2Int>();
                var nodeMap = new Dictionary<Vector2Int, Node>();

                var startNode = new Node(start, null, 0, Heuristic(start, goal), Vector2Int.zero);
                open.Enqueue(startNode);
                nodeMap[start] = startNode;

                // 八方向
                var directions = new Vector2Int[]
                {
                    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                    new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)
                };

                while (open.Count > 0)
                {
                    var current = open.Dequeue();

                    if (current.Pos == goal)
                        return ReconstructPath(current);

                    closed.Add(current.Pos);

                    foreach (var dir in directions)
                    {
                        var neighborPos = current.Pos + dir;
                        if (closed.Contains(neighborPos) || !isWalkable(neighborPos))
                            continue;

                        // 基础cost: 直走1，斜走diagCost
                        float stepCost = (dir.x != 0 && dir.y != 0) ? diagCost : 1f;

                        // 转弯惩罚（不包括起点）
                        if (current.Parent != null && current.Dir != dir)
                            stepCost += turnCost;

                        // 地形消耗
                        if (terrainCostFunc != null)
                            stepCost += terrainCostFunc(neighborPos);

                        float newG = current.G + stepCost;

                        if (!nodeMap.TryGetValue(neighborPos, out var neighbor) || newG < neighbor.G)
                        {
                            neighbor = new Node(neighborPos, current, newG, Heuristic(neighborPos, goal), dir);
                            nodeMap[neighborPos] = neighbor;
                            open.Enqueue(neighbor);
                        }
                    }
                }

                return null; // 无路可达
            }

            /// <summary> 曼哈顿距离（可替换欧几里得/对角线距离） </summary>
            private static float Heuristic(Vector2Int a, Vector2Int b)
                => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

            private static List<Vector2Int> ReconstructPath(Node node)
            {
                var path = new List<Vector2Int>();
                while (node != null)
                {
                    path.Add(node.Pos);
                    node = node.Parent;
                }

                path.Reverse();
                return path;
            }

            private class Node : IComparable<Node>
            {
                public Vector2Int Pos;
                public Node Parent;
                public float G;
                public float F;
                public Vector2Int Dir; // 从Parent到此Node的方向

                public Node(Vector2Int pos, Node parent, float g, float h, Vector2Int dir)
                {
                    Pos = pos;
                    Parent = parent;
                    G = g;
                    F = g + h;
                    Dir = dir;
                }

                public int CompareTo(Node other) => F.CompareTo(other.F);
            }

            /// <summary>
            /// 小顶堆优先队列
            /// </summary>
            private class PriorityQueue<T> where T : IComparable<T>
            {
                private List<T> data = new List<T>();
                public int Count => data.Count;

                public void Enqueue(T item)
                {
                    data.Add(item);
                    int ci = data.Count - 1;
                    while (ci > 0)
                    {
                        int pi = (ci - 1) / 2;
                        if (data[ci].CompareTo(data[pi]) >= 0)
                            break;
                        (data[ci], data[pi]) = (data[pi], data[ci]);
                        ci = pi;
                    }
                }

                public T Dequeue()
                {
                    int li = data.Count - 1;
                    T frontItem = data[0];
                    data[0] = data[li];
                    data.RemoveAt(li);
                    --li;
                    int pi = 0;
                    while (true)
                    {
                        int ci = pi * 2 + 1;
                        if (ci > li)
                            break;
                        int rc = ci + 1;
                        if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
                            ci = rc;
                        if (data[pi].CompareTo(data[ci]) <= 0)
                            break;
                        (data[pi], data[ci]) = (data[ci], data[pi]);
                        pi = ci;
                    }

                    return frontItem;
                }
            }
        }
    }
}