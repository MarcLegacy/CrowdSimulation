using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct DebugInfo
{
    public bool show;
    public Color color;
}

public static class Utilities
{
    /// <summary>
    /// Gets the position in the world that the mouse is hovering over.
    /// </summary>
    /// <returns> Returns the first position that the mouse hovers over. </returns>
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit raycastHit) ? raycastHit.point : Vector3.zero;
    }
    
    /// <summary>
    /// Finds all the game objects with a certain layer.
    /// Heavy Operation!
    /// </summary>
    /// <param name="layer"> The layer to find the game objects with. </param>
    /// <returns> Returns all the game objects with the layer. </returns>
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
    /// <summary>
    /// Finds all the game objects with a certain mask.
    /// Heavy Operation!
    /// </summary>
    /// <param name="maskString"> The mask to find the game objects with. </param>
    /// <returns> Returns all the game objects with the mask. </returns>
    public static List<GameObject> FindGameObjects(string maskString)
    {
        return FindGameObjects(LayerMask.NameToLayer(maskString));
    }

    /// <summary>
    /// Gets a random position in a box between the two corners.
    /// </summary>
    /// <param name="cornerA"> Begin corner. </param>
    /// <param name="cornerB"> End corner. </param>
    /// <returns> Returns a random position. </returns>
    public static Vector3 GetRandomPositionInBox(Vector3 cornerA, Vector3 cornerB)
    {
        return new Vector3(Random.Range(cornerA.x, cornerB.x), Random.Range(cornerA.y, cornerB.y), Random.Range(cornerA.z, cornerB.z));
    }

    /// <summary>
    /// Converts an Vector2Int to a int2.
    /// </summary>
    /// <param name="vector2Int"> Variable to convert. </param>
    /// <returns> Returns the converted variable. </returns>
    public static int2 Vector2IntToInt2(Vector2Int vector2Int)
    {
        return new int2(vector2Int.x, vector2Int.y);
    }

    /// <summary>
    /// Converts an int2 to a Vector2Int.
    /// </summary>
    /// <param name="int2"> Variable to convert. </param>
    /// <returns> Returns the converted variable. </returns>
    public static Vector2Int Int2ToVector2Int(int2 int2)
    {
        return new Vector2Int(int2.x, int2.y);
    }

    /// <summary>
    /// Calculates what the grid position of the cell is on the given world position.
    /// </summary>
    /// <param name="worldPosition"> Position to get the cell from. </param>
    /// <param name="gridOriginPosition"> Origin position of the grid. </param>
    /// <param name="gridCellSize"> Cell size of the grid. </param>
    /// <returns> Returns the grid position of the cell </returns>
    public static Vector2Int CalculateCellGridPosition(Vector3 worldPosition, Vector3 gridOriginPosition, float gridCellSize)
    {
        int x = Mathf.FloorToInt((worldPosition - gridOriginPosition).x / gridCellSize);
        int y = Mathf.FloorToInt((worldPosition - gridOriginPosition).z / gridCellSize);

        return new Vector2Int(x, y);
    }
    /// <summary>
    /// Calculates what the grid position of the cell is on the given world position.
    /// </summary>
    /// <param name="worldPosition"> Position to get the cell from. </param>
    /// <param name="gridOriginPosition"> Origin position of the grid. </param>
    /// <param name="gridCellSize"> Cell size of the grid. </param>
    /// <returns> Returns the grid position of the cell </returns>
    public static int2 CalculateCellGridPosition(float3 worldPosition, float3 gridOriginPosition, float gridCellSize)
    {
        int x = Mathf.FloorToInt((worldPosition - gridOriginPosition).x / gridCellSize);
        int y = Mathf.FloorToInt((worldPosition - gridOriginPosition).z / gridCellSize);

        return new int2(x, y);
    }

    #region Rotation Functions

    /// <summary>
    /// Rotates the vector around the angles in clockwise direction.
    /// </summary>
    /// <param name="vector"> vector to rotate. </param>
    /// <param name="angles"> the angles to rotate with. </param>
    /// <returns> Returns the rotated vector </returns>
    public static Vector3 RotateVector(Vector3 vector, Vector3 angles)
    {
        return Quaternion.Euler(angles) * vector;
    }

    /// <summary>
    /// Rotates the vector around the x-axis in clockwise direction.
    /// </summary>
    /// <param name="vector"> vector to rotate. </param>
    /// <param name="angle"> the angle to rotate with. </param>
    /// <returns> Returns the rotated vector </returns>
    public static Vector3 RotateVectorXAxis(Vector3 vector, float angle)
    {
        return Quaternion.Euler(angle, 0f, 0f) * vector;
    }

    /// <summary>
    /// Rotates the vector around the y-axis in clockwise direction.
    /// </summary>
    /// <param name="vector"> vector to rotate. </param>
    /// <param name="angle"> the angle to rotate with. </param>
    /// <returns> Returns the rotated vector </returns>
    public static Vector3 RotateVectorYAxis(Vector3 vector, float angle)
    {
        return Quaternion.Euler(0f, angle, 0f) * vector;
    }

    /// <summary>
    /// Rotates the vector around the z-axis in clockwise direction.
    /// </summary>
    /// <param name="vector"> vector to rotate. </param>
    /// <param name="angle"> the angle to rotate with. </param>
    /// <returns> Returns the rotated vector </returns>
    public static Vector3 RotateVectorZAxis(Vector3 vector, float angle)
    {
        return Quaternion.Euler(0f, 0f, angle) * vector;
    }
    #endregion

    #region Draw Functions
    #region Draw Giszmos Functions

    /// <summary>
    /// Draws a Gizmos arrow with.
    /// </summary>
    /// <param name="centerWorldPosition"> Position that the arrow will center around. </param>
    /// <param name="direction"> The direction to point the arrow towards to. </param>
    /// <param name="size"> The size of the arrow. </param>
    /// <param name="color"> The color of the arrow. </param>
    public static void DrawGizmosArrow(Vector3 centerWorldPosition, Vector3 direction, float size = 1f, Color? color = null)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("No valid direction given!");
            return;
        }

        float arrowHeadLength = 0.5f * size;
        float arrowHeadAngle = 20.0f;
        Vector3 startPosition = new Vector3(centerWorldPosition.x - direction.x * size * 0.5f, centerWorldPosition.y - direction.y * size * 0.5f,
            centerWorldPosition.z - direction.z * size * 0.5f);
        Vector3 endPosition = startPosition + (direction * size);

        if (color == null) color = Color.white;

        Gizmos.color = (Color)color;
        Gizmos.DrawLine(startPosition, endPosition);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawLine(endPosition, endPosition + (right * arrowHeadLength));
        Gizmos.DrawLine(endPosition, endPosition + (left * arrowHeadLength));
    }

    /// <summary>
    /// Draws a Gizmos path.
    /// </summary>
    /// <param name="worldLocations"> The positions to draw lines between. </param>
    /// <param name="color"> The color of the path. </param>
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

    /// <summary>
    /// Draws a Gizmos portal.
    /// </summary>
    /// <param name="portal"> The Portal to draw. </param>
    /// <param name="color"> The color of the Portal. </param>
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

    /// <summary>
    /// Draws a Gizmos circle.
    /// </summary>
    /// <param name="centerWorldPosition"> Position to draw the circle around with. </param>
    /// <param name="radius"> The radius of the circle. </param>
    /// <param name="color"> The color of the circle. </param>
    public static void DrawGizmosCircle(Vector3 centerWorldPosition, float radius, Color? color = null)
    {
        if (radius == 0)
        {
            Debug.LogWarning(radius + " == 0");
            return;
        }
        if (color == null) color = Color.black;

        float thetaScale = 0.01f;
        float theta = 0f;

        int size = (int)(1f / thetaScale + 1f);
        Vector3 previousPosition = Vector3.zero;

        Gizmos.color = (Color)color;

        for (int i = 0; i < size; i++)
        {
            theta += (2f * Mathf.PI * thetaScale);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            if (i != 0)
            {
                Gizmos.DrawLine(previousPosition, centerWorldPosition + new Vector3(x, 0, y));
            }

            previousPosition = centerWorldPosition + new Vector3(x, 0, y);
        }
    }
    #endregion
    #region Draw Debug Functions

    /// <summary>
    /// Draws a debug arrow with.
    /// </summary>
    /// <param name="centerWorldPosition"> Position that the arrow will center around. </param>
    /// <param name="direction"> The direction to point the arrow towards to. </param>
    /// <param name="size"> The size of the arrow. </param>
    /// <param name="color"> The color of the arrow. </param>
    /// <param name="duration"> The duration of the arrow. </param>
    public static void DrawDebugArrow(Vector3 centerWorldPosition, Vector3 direction, float size = 1f, Color? color = null, float duration = 0.0f)
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Utilities: " + MethodBase.GetCurrentMethod()?.Name + ": m_velocity == Vector3.zero");
            return;
        }

        float arrowHeadLength = 0.5f * size;
        float arrowHeadAngle = 20.0f;
        Vector3 startPosition = new Vector3(centerWorldPosition.x - direction.x * size * 0.5f, centerWorldPosition.y - direction.y * size * 0.5f,
            centerWorldPosition.z - direction.z * size * 0.5f);
        Vector3 endPosition = startPosition + (direction * size);

        if (color == null) color = Color.white;

        Debug.DrawLine(startPosition, endPosition, (Color) color, duration);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawLine(endPosition, endPosition + (right * arrowHeadLength), (Color) color, duration);
        Debug.DrawLine(endPosition, endPosition + (left * arrowHeadLength), (Color) color, duration);
    }

    /// <summary>
    /// Draws a debug path.
    /// </summary>
    /// <param name="worldLocations"> The positions to draw lines between. </param>
    /// <param name="color"> The color of the path. </param>
    /// <param name="duration"> The duration of the path. </param>
    public static void DrawDebugPathLines(List<Vector3> worldLocations, Color? color = null, float duration = 0.0f)
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

    /// <summary>
    /// Draws a debug portal.
    /// </summary>
    /// <param name="portal"> The Portal to draw. </param>
    /// <param name="color"> The color of the Portal. </param>
    /// <param name="duration"> The duration of the Portal. </param>
    public static void DrawDebugPortal(Portal portal, Color? color = null, float duration = 0.0f)
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

    /// <summary>
    /// Draws a debug circle.
    /// </summary>
    /// <param name="centerWorldPosition"> Position to draw the circle around with. </param>
    /// <param name="radius"> The radius of the circle. </param>
    /// <param name="color"> The color of the circle. </param>
    /// <param name="duration"> The duration of the circle. </param>
    public static void DrawDebugCircle(Vector3 centerWorldPosition, float radius, Color? color = null, float duration = 0.0f)
    {
        if (radius == 0)
        {
            Debug.LogWarning(radius + " == 0");
            return;
        }
        if (color == null) color = Color.black;

        float thetaScale = 0.01f;
        float theta = 0f;

        int size = (int)(1f / thetaScale + 1f);
        Vector3 previousPosition = Vector3.zero;

        for (int i = 0; i < size; i++)
        {
            theta += (2f * Mathf.PI * thetaScale);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            if (i != 0)
            {
                Debug.DrawLine(previousPosition, centerWorldPosition + new Vector3(x, 0, y), (Color)color, duration);
            }

            previousPosition = centerWorldPosition + new Vector3(x, 0, y);
        }
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
