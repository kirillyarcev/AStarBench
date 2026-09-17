using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x, y;
    public bool isWall = false;
    public bool isStart = false;
    public bool isTarget = false;

    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;
    public Cell parent;

    private SpriteRenderer sr;
    private Color defaultColor = Color.white;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }

    public void ResetColor()
    {
        if (sr != null) sr.color = defaultColor;
    }

    public void SetWall(bool wall)
    {
        isWall = wall;
        if (isWall) SetColor(Color.gray);
        else ResetColor();
    }
}
