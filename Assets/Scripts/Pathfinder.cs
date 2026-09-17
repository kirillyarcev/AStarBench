using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinder : MonoBehaviour
{
    private GridManager gridManager;

    public float lastTimeMs;
    public int lastNodesVisited;
    public int lastPathLength;
    public string lastHeuristicName;

    private List<Cell> currentPath = new List<Cell>();

    void Start()
    {
        gridManager = GridManager.Instance;
    }

    public void FindPathManhattan()
    {
        FindPath(HeuristicType.Manhattan, "Манхэттен");
    }

    public void FindPathEuclidean()
    {
        FindPath(HeuristicType.Euclidean, "Евклид");
    }

    public void FindPathChebyshev()
    {
        FindPath(HeuristicType.Chebyshev, "Чебышёв");
    }

    public void FindPathDiagonal()
    {
        FindPath(HeuristicType.Diagonal, "Диагональная");
    }

    private void FindPath(HeuristicType heuristic, string heuristicName)
    {
        ClearPath();

        lastHeuristicName = heuristicName;
        lastNodesVisited = 0;
        lastPathLength = 0;
        lastTimeMs = 0;

        if (gridManager.startCell == null || gridManager.targetCell == null)
        {
            UIManager.Instance.ShowMessage("Ошибка: старт или цель не установлены");
            return;
        }

        ResetCells();

        List<Cell> openList = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();

        Cell start = gridManager.startCell;
        Cell target = gridManager.targetCell;

        start.GCost = 0;
        start.HCost = CalculateHeuristic(start, target, heuristic);
        start.parent = null;
        openList.Add(start);

        Stopwatch sw = Stopwatch.StartNew();

        while (openList.Count > 0)
        {
            Cell current = GetLowestFCostCell(openList);
            openList.Remove(current);
            closedSet.Add(current);
            lastNodesVisited++;

            if (current == target)
            {
                sw.Stop();
                lastTimeMs = sw.ElapsedMilliseconds;
                ReconstructPath(current);
                lastPathLength = currentPath.Count;
                UIManager.Instance.UpdateStats(); // только при успехе
                return;
            }

            List<Cell> neighbors = GetNeighbors(current);

            foreach (Cell neighbor in neighbors)
            {
                if (neighbor.isWall || closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = current.GCost + 1;

                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.GCost)
                {
                    neighbor.parent = current;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor, target, heuristic);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        sw.Stop();
        lastTimeMs = sw.ElapsedMilliseconds;
        lastPathLength = 0;
        lastNodesVisited = 0;
        UIManager.Instance.ShowMessage($"Путь не найден!\n<b>Эвристика:</b> {heuristicName}\n<b>Время:</b> {lastTimeMs} мс");
    }

    private Cell GetLowestFCostCell(List<Cell> list)
    {
        Cell best = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].FCost < best.FCost ||
                (Mathf.Approximately(list[i].FCost, best.FCost) && list[i].HCost < best.HCost))
            {
                best = list[i];
            }
        }
        return best;
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.x;
        int y = cell.y;

        TryAddNeighbor(x + 1, y, neighbors);
        TryAddNeighbor(x - 1, y, neighbors);
        TryAddNeighbor(x, y + 1, neighbors);
        TryAddNeighbor(x, y - 1, neighbors);

        return neighbors;
    }

    private void TryAddNeighbor(int x, int y, List<Cell> neighbors)
    {
        if (x >= 0 && x < gridManager.width && y >= 0 && y < gridManager.height)
        {
            neighbors.Add(gridManager.grid[x, y]);
        }
    }

    private float CalculateHeuristic(Cell a, Cell b, HeuristicType type)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        switch (type)
        {
            case HeuristicType.Manhattan:
                return dx + dy;
            case HeuristicType.Euclidean:
                return Mathf.Sqrt(dx * dx + dy * dy);
            case HeuristicType.Chebyshev:
                return Mathf.Max(dx, dy);
            case HeuristicType.Diagonal:
                return (dx + dy) + (Mathf.Sqrt(2) - 2) * Mathf.Min(dx, dy);
            default:
                return dx + dy;
        }
    }

    private void ReconstructPath(Cell target)
    {
        Cell current = target;
        while (current != null)
        {
            currentPath.Add(current);
            current = current.parent;
        }
        currentPath.Reverse();

        foreach (Cell cell in currentPath)
        {
            if (cell != gridManager.startCell && cell != gridManager.targetCell)
            {
                cell.SetColor(Color.green);
            }
        }
    }

    public void ClearPath()
    {
        foreach (Cell cell in currentPath)
        {
            if (cell != gridManager.startCell && cell != gridManager.targetCell && !cell.isWall)
            {
                cell.ResetColor();
            }
        }
        currentPath.Clear();
    }

    public void ResetStats()
    {
        lastHeuristicName = "";
        lastTimeMs = 0;
        lastNodesVisited = 0;
        lastPathLength = 0;
        UIManager.Instance.ShowMessage("Готов к работе");
    }

    private void ResetCells()
    {
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Cell cell = gridManager.grid[x, y];
                cell.GCost = 0;
                cell.HCost = 0;
                cell.parent = null;
            }
        }
    }

    public enum HeuristicType
    {
        Manhattan,
        Euclidean,
        Chebyshev,
        Diagonal
    }
}