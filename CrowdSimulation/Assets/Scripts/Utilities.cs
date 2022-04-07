using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Utilities
{
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit raycastHit) ? raycastHit.point : Vector3.zero;
    }
    
    public static List<GameObject> FindGameObjects(int layer)
    {
        GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
        List<GameObject> gameObjects = new List<GameObject>();

        foreach (GameObject gameObject in allGameObjects)
        {
            if (gameObject.layer == layer)
            {
                gameObjects.Add(gameObject);
            }
        }

        return gameObjects;
    }
    public static List<GameObject> FindGameObjects(string maskString)
    {
        return FindGameObjects(LayerMask.NameToLayer(maskString));
    }

    public static Vector3 GetRandomPositionInBox(Vector3 positionA, Vector3 positionB)
    {
        return new Vector3(Random.Range(positionA.x, positionB.x), Random.Range(positionA.y, positionB.y), Random.Range(positionA.z, positionB.z));
    }

    #region Draw Functions
    #region Draw Giszmos Functions

    public static void DrawGizmosArrow(Vector3 position, Vector3 direction, float size = 1f, Color? color = null)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Utilities: " + MethodBase.GetCurrentMethod()?.Name + ": direction == Vector3.zero");
            return;
        }

        float arrowHeadLength = 0.5f * size;
        float arrowHeadAngle = 20.0f;
        Vector3 startPosition = new Vector3(position.x - direction.x * size * 0.5f, position.y - direction.y * size * 0.5f,
            position.z - direction.z * size * 0.5f);
        Vector3 endPosition = startPosition + (direction * size);

        if (color == null) color = Color.white;

        Gizmos.color = (Color)color;
        Gizmos.DrawLine(startPosition, endPosition);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawLine(endPosition, endPosition + (right * arrowHeadLength));
        Gizmos.DrawLine(endPosition, endPosition + (left * arrowHeadLength));
    }

    public static void DrawGizmosPathLines(List<Vector3> worldLocations, Color? color = null)
    {
        if (worldLocations == null || worldLocations.Count == 0)
        {
            Debug.LogWarning(nameof(worldLocations) + " is invalid!");
            return;
        }

        if (color == null) color = Color.black;

        Gizmos.color = (Color) color;

        for (int i = 0; i < worldLocations.Count - 1; i++)
        {
            Gizmos.DrawLine(worldLocations[i], worldLocations[i + 1]);
        }
    }

    public static void DrawGizmosPortal(Portal portal, Color? color = null)
    {
        if (color == null) color = Color.black;

        Gizmos.color = (Color) color;

        Vector3 posA = (portal.GetCellCenterWorldPosition(portal.AreaACells[0]) + portal.GetCellCenterWorldPosition(portal.AreaBCells[0])) * 0.5f;
        Vector3 posB = (portal.GetCellCenterWorldPosition(portal.AreaACells[portal.AreaACells.Count - 1]) + portal.GetCellCenterWorldPosition(portal.AreaBCells[portal.AreaBCells.Count - 1])) * 0.5f;

        Gizmos.DrawLine(posA, portal.GetEntranceCellAreaACenterWorldPosition());
        Gizmos.DrawLine(portal.GetEntranceCellAreaACenterWorldPosition(), posB);
        Gizmos.DrawLine(posB, portal.GetEntranceCellAreaBCenterWorldPosition());
        Gizmos.DrawLine(portal.GetEntranceCellAreaBCenterWorldPosition(), posA);
    }
    #endregion
    #region Draw Debug Functions

    public static void DrawDebugArrow(Vector3 position, Vector3 direction, float size = 1f, Color? color = null, float duration = 100f)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Utilities: " + MethodBase.GetCurrentMethod()?.Name + ": direction == Vector3.zero");
            return;
        }

        float arrowHeadLength = 0.5f * size;
        float arrowHeadAngle = 20.0f;
        Vector3 startPosition = new Vector3(position.x - direction.x * size * 0.5f, position.y - direction.y * size * 0.5f,
            position.z - direction.z * size * 0.5f);
        Vector3 endPosition = startPosition + (direction * size);

        if (color == null) color = Color.white;

        Debug.DrawLine(startPosition, endPosition, (Color) color, duration);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawLine(endPosition, endPosition + (right * arrowHeadLength), (Color) color, duration);
        Debug.DrawLine(endPosition, endPosition + (left * arrowHeadLength), (Color) color, duration);
    }

    public static void DrawDebugPathLines(List<Vector3> worldLocations, Color? color = null, float duration = 100f)
    {
        if (worldLocations == null || worldLocations.Count == 0)
        {
            Debug.LogWarning(nameof(worldLocations) + " is invalid!");
            return;
        }

        if (color == null) color = Color.black;
        
        for (int i = 0; i < worldLocations.Count - 1; i++)
        {
            Debug.DrawLine(worldLocations[i], worldLocations[i + 1], (Color)color, duration);
        }
    }

    public static void DrawDebugPortal(Portal portal, Color? color = null, float duration = 100f)
    {
        if (color == null) color = Color.black;

        Vector3 posA = (portal.GetCellCenterWorldPosition(portal.AreaACells[0]) + portal.GetCellCenterWorldPosition(portal.AreaBCells[0])) * 0.5f;
        Vector3 posB = (portal.GetCellCenterWorldPosition(portal.AreaACells[portal.AreaACells.Count - 1]) +
                        portal.GetCellCenterWorldPosition(portal.AreaBCells[portal.AreaBCells.Count - 1])) * 0.5f;

        Debug.DrawLine(posA, portal.GetEntranceCellAreaACenterWorldPosition(), (Color) color, duration);
        Debug.DrawLine(portal.GetEntranceCellAreaACenterWorldPosition(), posB, (Color) color, duration);
        Debug.DrawLine(posB, portal.GetEntranceCellAreaBCenterWorldPosition(), (Color) color, duration);
        Debug.DrawLine(portal.GetEntranceCellAreaBCenterWorldPosition(), posA, (Color) color, duration);
    }  
    #endregion
    #endregion

    #region CodeMonkeyFunctions
    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CacheQuaternionEuler()
    {
        if (cachedQuaternionEulerArr != null) return;
        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i = 0; i < 360; i++)
        {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0, 0, i);
        }
    }
    public static Quaternion GetQuaternionEuler(float rotFloat)
    {
        int rot = Mathf.RoundToInt(rotFloat);
        rot = rot % 360;
        if (rot < 0) rot += 360;
        //if (rot >= 360) rot -= 360;
        if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
        return cachedQuaternionEulerArr[rot];
    }

    public static void CreateEmptyMeshArrays(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[4 * quadCount];
        uvs = new Vector2[4 * quadCount];
        triangles = new int[6 * quadCount];
    }

    public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize,
        Vector2 uv00, Vector2 uv11)
    {
        // Relocate vertices
        int vIndex = index * 4;
        int vIndex0 = vIndex;
        int vIndex1 = vIndex + 1;
        int vIndex2 = vIndex + 2;
        int vIndex3 = vIndex + 3;

        baseSize *= 0.5f;

        bool skewed = baseSize.x != baseSize.y;
        if (skewed)
        {
            vertices[vIndex0] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, 0, baseSize.z);
            vertices[vIndex1] = pos + GetQuaternionEuler(rot) * new Vector3(-baseSize.x, 0, -baseSize.z);
            vertices[vIndex2] = pos + GetQuaternionEuler(rot) * new Vector3(baseSize.x, 0, -baseSize.z);
            vertices[vIndex3] = pos + GetQuaternionEuler(rot) * baseSize;
        }
        else
        {
            //vertices[vIndex0] = pos + GetQuaternionEuler(rot - 270) * baseSize;
            //vertices[vIndex1] = pos + GetQuaternionEuler(rot - 180) * baseSize;
            //vertices[vIndex2] = pos + GetQuaternionEuler(rot - 90) * baseSize;
            //vertices[vIndex3] = pos + GetQuaternionEuler(rot - 0) * baseSize;
            vertices[vIndex0] = new Vector3(pos.x - baseSize.x, 0, pos.z + baseSize.z);
            vertices[vIndex1] = new Vector3(pos.x - baseSize.x, 0, pos.z - baseSize.z);
            vertices[vIndex2] = new Vector3(pos.x + baseSize.x, 0, pos.z + baseSize.z);
            vertices[vIndex3] = new Vector3(pos.x + baseSize.x, 0, pos.z - baseSize.z);
        }

        // Relocate UVs
        uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
        uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
        uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
        uvs[vIndex3] = new Vector2(uv11.x, uv11.y);

        //Create triangles
        int tIndex = index * 6;

        triangles[tIndex + 0] = vIndex0;
        triangles[tIndex + 1] = vIndex3;
        triangles[tIndex + 2] = vIndex1;

        triangles[tIndex + 3] = vIndex1;
        triangles[tIndex + 4] = vIndex3;
        triangles[tIndex + 5] = vIndex2;
    }

    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40,
        Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left,
        int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;

        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
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
    #endregion
}
