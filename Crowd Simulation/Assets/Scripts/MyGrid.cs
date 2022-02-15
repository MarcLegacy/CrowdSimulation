using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class MyGrid
{
    private readonly int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private int[,] gridArray;
    private TextMesh[,] debugtextArray;

    public MyGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];
        debugtextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                debugtextArray[x, y] = CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f, (int)cellSize * 2, Color.black, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.black, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.black, 100f);
            }
        }

        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.black, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.black, 100f);
    }

    public int[,] GetGridArray()
    {
        return gridArray;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return GetWorldPosition(gridPosition.x, gridPosition.y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        Vector2Int gridPosition = new Vector2Int
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize), 
            y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize)
        };

        return gridPosition;
    }

    public int GetValue(Vector3 worldPosition)
    {
        return GetValue(GetGridPosition(worldPosition));
    }

    public int GetValue(Vector2Int gridPosition)
    {
        return GetValue(gridPosition.x, gridPosition.y);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
            return 0;
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        SetValue(GetGridPosition(worldPosition), value);
    }

    public void SetValue(Vector2Int gridPosition, int value)
    {
        SetValue(gridPosition.x, gridPosition.y, value);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            gridArray[x, y] = value;
            debugtextArray[x, y].text = gridArray[x, y].ToString();
        }
        else
        {
            Debug.LogWarning(this + ": " + MethodBase.GetCurrentMethod()?.Name + ": Trying to set a value on (" + x + ", " + y +
                             ") in a grid of size (" + gridArray.GetLength(0) + ", " + gridArray.GetLength(1) + ")");
        }
    }

    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40,
        Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, 
        int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;

        return CreateWorldText(parent, text, localPosition, fontSize, (Color) color, textAnchor, textAlignment, sortingOrder);
    }

    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color,
        TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.RotateAround(localPosition, new Vector3(1, 0), 90);
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }



}
