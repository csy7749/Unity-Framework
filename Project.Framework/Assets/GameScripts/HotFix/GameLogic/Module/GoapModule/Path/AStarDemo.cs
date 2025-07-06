using UnityEngine;

namespace GameLogic.GoapModule.Path
{
    public class AStarDemo
    {

        public void Run(int width = 10, int height = 8)
        {
            int mapWidth = width, mapHeight = height;
            bool[,] walkableMap = new bool[mapWidth, mapHeight];
            bool[,] mudMap = new bool[mapWidth, mapHeight];
            bool[,] swampMap = new bool[mapWidth, mapHeight];

            for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                walkableMap[x, y] = true;

            walkableMap[3, 2] = false;
            walkableMap[4, 2] = false;
            walkableMap[5, 2] = false;

            System.Random rnd = new System.Random();
            for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                mudMap[x, y] = rnd.NextDouble() < 0.10;
                swampMap[x, y] = rnd.NextDouble() < 0.05;
            }

            float TerrainCost(Vector2Int pos)
            {
                if (swampMap[pos.x, pos.y]) return 2f;
                if (mudMap[pos.x, pos.y]) return 1f;
                return 0f;
            }

            bool IsWalkable(Vector2Int pos)
            {
                if (pos.x < 0 || pos.x >= mapWidth || pos.y < 0 || pos.y >= mapHeight)
                    return false;
                return walkableMap[pos.x, pos.y];
            }

            Vector2Int start = new Vector2Int(0, 0);
            Vector2Int goal = new Vector2Int(9, 7);

            var path = AStar.PathfindingAStar.FindPath(
                start, goal,
                IsWalkable,
                turnCost: 0.5f,
                diagCost: 1.4f,
                terrainCostFunc: TerrainCost
            );

            if (path != null)
            {
                foreach (var pos in path)
                    Debug.Log(pos);
            }
            else
            {
                Debug.LogWarning("无法找到可达路径");
            }

        }
    }
}