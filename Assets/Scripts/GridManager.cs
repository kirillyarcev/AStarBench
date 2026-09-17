using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 15;
    public int height = 15;
    public GameObject cellPrefab;
    public Transform gridParent;

    public Cell[,] grid;
    public Cell startCell;
    public Cell targetCell;

    private bool isSettingStart = false;
    private bool isSettingTarget = false;
    private Pathfinder pathfinder;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreateGrid();
        pathfinder = FindFirstObjectByType<Pathfinder>();
    }

    void CreateGrid()
    {
        grid = new Cell[width, height];
        Vector3 offset = new Vector3(-width / 2f + 0.5f, -height / 2f + 0.5f, 0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0) + offset;
                GameObject go = Instantiate(cellPrefab, pos, Quaternion.identity, gridParent);
                Cell cell = go.GetComponent<Cell>();
                cell.x = x;
                cell.y = y;
                grid[x, y] = cell;
            }
        }

        SetDefaultStartAndTarget();
    }

    private void SetDefaultStartAndTarget()
    {
        if (startCell != null)
        {
            startCell.isStart = false;
            startCell.ResetColor();
        }
        if (targetCell != null)
        {
            targetCell.isTarget = false;
            targetCell.ResetColor();
        }

        startCell = grid[2, 2];
        startCell.isStart = true;
        startCell.SetColor(Color.blue);

        targetCell = grid[12, 12];
        targetCell.isTarget = true;
        targetCell.SetColor(Color.red);

        if (pathfinder != null)
        {
            pathfinder.ResetStats();
            pathfinder.ClearPath();
        }
    }

    void Update()
    {
        if (isSettingStart)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cell cell = GetCellUnderMouse();
                if (cell != null && cell != targetCell && !cell.isWall)
                {
                    if (startCell != null)
                    {
                        startCell.isStart = false;
                        startCell.ResetColor();
                    }
                    startCell = cell;
                    startCell.isStart = true;
                    startCell.SetColor(Color.blue);
                    isSettingStart = false;

                    if (pathfinder != null)
                    {
                        pathfinder.ResetStats();
                        pathfinder.ClearPath();
                    }
                    Debug.Log("Старт установлен");
                }
            }
            return;
        }

        if (isSettingTarget)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cell cell = GetCellUnderMouse();
                if (cell != null && cell != startCell && !cell.isWall)
                {
                    if (targetCell != null)
                    {
                        targetCell.isTarget = false;
                        targetCell.ResetColor();
                    }
                    targetCell = cell;
                    targetCell.isTarget = true;
                    targetCell.SetColor(Color.red);
                    isSettingTarget = false;

                    if (pathfinder != null)
                    {
                        pathfinder.ResetStats();
                        pathfinder.ClearPath();
                    }
                    Debug.Log("Цель установлена");
                }
            }
            return;
        }

        if (Input.GetMouseButton(0))
        {
            Cell cell = GetCellUnderMouse();
            if (cell != null && cell != startCell && cell != targetCell && !cell.isWall)
            {
                cell.SetWall(true);
                if (pathfinder != null)
                {
                    pathfinder.ResetStats();
                    pathfinder.ClearPath();
                }
            }
        }
        else if (Input.GetMouseButton(1))
        {
            Cell cell = GetCellUnderMouse();
            if (cell != null && cell != startCell && cell != targetCell && cell.isWall)
            {
                cell.SetWall(false);
                if (pathfinder != null)
                {
                    pathfinder.ResetStats();
                    pathfinder.ClearPath();
                }
            }
        }
    }

    private Cell GetCellUnderMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Cell>();
        }
        return null;
    }

    public void SetStartMode()
    {
        isSettingStart = true;
        isSettingTarget = false;
        Debug.Log("Режим: кликните по клетке, чтобы установить старт");
    }

    public void SetTargetMode()
    {
        isSettingTarget = true;
        isSettingStart = false;
        Debug.Log("Режим: кликните по клетке, чтобы установить цель");
    }

    public void ClearWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell.isWall)
                {
                    cell.SetWall(false);
                }
            }
        }
    }

    public void ResetGrid()
    {
        ClearWalls();
        SetDefaultStartAndTarget();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell != startCell && cell != targetCell && !cell.isWall)
                {
                    cell.ResetColor();
                }
            }
        }
        isSettingStart = false;
        isSettingTarget = false;
        Debug.Log("Сетка сброшена");
    }

    public void GenerateMaze()
    {
        ClearWalls();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell == startCell || cell == targetCell)
                    continue;

                if (Random.Range(0f, 1f) < 0.35f)
                {
                    cell.SetWall(true);
                }
            }
        }

        int attempts = 0;
        while (attempts < 10)
        {
            if (IsPathExist())
            {
                Debug.Log("Лабиринт сгенерирован, путь существует");
                return;
            }
            else
            {
                RemoveRandomWalls(5);
                attempts++;
            }
        }
        Debug.LogWarning("Не удалось сгенерировать лабиринт с путём за 10 попыток");
    }

    private bool IsPathExist()
    {
        if (startCell == null || targetCell == null) return false;

        Queue<Cell> queue = new Queue<Cell>();
        HashSet<Cell> visited = new HashSet<Cell>();
        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();
            if (current == targetCell) return true;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    Cell neighbor = grid[nx, ny];
                    if (!neighbor.isWall && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        return false;
    }

    private void RemoveRandomWalls(int count)
    {
        List<Cell> walls = new List<Cell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
                if (cell.isWall && cell != startCell && cell != targetCell)
                {
                    walls.Add(cell);
                }
            }
        }

        for (int i = 0; i < count && walls.Count > 0; i++)
        {
            int index = Random.Range(0, walls.Count);
            walls[index].SetWall(false);
            walls.RemoveAt(index);
        }
    }
}