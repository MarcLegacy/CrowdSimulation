using System.Reflection;
using UnityEngine;

public class Utilities
{
    // CodeMonkey function
    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40,
        Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left,
        int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;

        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
    }

    // CodeMonkey function
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

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit raycastHit) ? raycastHit.point : Vector3.zero;
    }

    public static void DrawArrow(Vector3 position, Vector3 direction, float size = 1f, Color? color = null, float duration = 100f)
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

        Debug.DrawLine(startPosition, endPosition, (Color)color, duration);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawLine(endPosition, endPosition + (right * arrowHeadLength), (Color)color, duration);
        Debug.DrawLine(endPosition, endPosition + (left * arrowHeadLength), (Color)color, duration);
    }
}
