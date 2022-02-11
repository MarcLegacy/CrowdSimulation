using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float SCROLLMULTIPLIER = 100f;

    public float panSpeed = 20f;
    public float panSpeedMultiplier = 2f;
    public float scrollSpeed = 2f;
    public float panBorderThickness = 10f;
    public TerrainData TerrainData;
    public float scrollminY = 20f;
    public float scrollMaxY = 120f;

    void Update()
    {
        Vector3 pos = transform.position;
        float speed = panSpeed;

        speed = Input.GetKey(KeyCode.LeftShift) ? panSpeed * panSpeedMultiplier : panSpeed;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || 
            (Input.mousePosition.y >= Screen.height - panBorderThickness && Input.mousePosition.y <= Screen.height))
        {
            pos.z += speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || 
            (Input.mousePosition.x <= panBorderThickness && Input.mousePosition.x >= 0))
        {
            pos.x -= speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || 
            (Input.mousePosition.y <= panBorderThickness && Input.mousePosition.y >= 0))
        {
            pos.z -= speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || 
            (Input.mousePosition.x >= Screen.width - panBorderThickness && Input.mousePosition.x <= Screen.width))
        {
            pos.x += speed * Time.deltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * SCROLLMULTIPLIER * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, 0, TerrainData.size.x);
        pos.y = Mathf.Clamp(pos.y, scrollminY, scrollMaxY);
        pos.z = Mathf.Clamp(pos.z, 0, TerrainData.size.z);

        transform.position = pos;
    }
}
