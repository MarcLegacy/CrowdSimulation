using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 10f;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private HeatMap heatMap;
    private MyGrid<int> grid;

    private void Start()
    {
        Vector3 originPosition =
            new Vector3(mapObject.transform.position.x - (mapObject.transform.localScale.x * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER),
                mapObject.transform.position.y,
                mapObject.transform.position.z - (mapObject.transform.localScale.z * GlobalConstants.SCALE_TO_SIZE_MULTIPLIER));
        grid = new MyGrid<int>(gridWidth, gridHeight, cellSize, Vector3.zero);

        heatMap.SetGrid(grid);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
