using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GridManager gridManager;
    public Pathfinder pathfinder;
    public Text statsText;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (pathfinder == null) pathfinder = FindFirstObjectByType<Pathfinder>();

        ShowMessage("Готов к работе");
    }

    public void OnResetGrid()
    {
        gridManager.ResetGrid();
        pathfinder.ResetStats();
        pathfinder.ClearPath();
    }

    public void OnClearWalls()
    {
        gridManager.ClearWalls();
        pathfinder.ResetStats();
        pathfinder.ClearPath();
    }

    public void OnGenerateMaze()
    {
        gridManager.GenerateMaze();
        pathfinder.ResetStats();
        pathfinder.ClearPath();
    }

    public void OnSetStart()
    {
        gridManager.SetStartMode();
        ShowMessage("Кликните по клетке для старта");
    }

    public void OnSetTarget()
    {
        gridManager.SetTargetMode();
        ShowMessage("Кликните по клетке для цели");
    }

    public void UpdateStats()
    {
        if (pathfinder == null || statsText == null) return;

        string text = "";
        text += $"<b>Эвристика:</b> {pathfinder.lastHeuristicName}\n";
        text += $"<b>Время:</b> {pathfinder.lastTimeMs} мс\n";
        text += $"<b>Посещено узлов:</b> {pathfinder.lastNodesVisited}\n";
        text += $"<b>Длина пути:</b> {pathfinder.lastPathLength} клеток";
        statsText.text = text;
    }

    public void ShowMessage(string msg)
    {
        if (statsText != null) statsText.text = msg;
    }
}