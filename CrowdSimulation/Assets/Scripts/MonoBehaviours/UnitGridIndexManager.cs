using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitGridIndexManager : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private GameObject map;

    public NativeMultiHashMap<int2, Entity> indexMap;

    private MyGrid<int> grid;

    public MyGrid<int> Grid => grid;
    public int Width => width;
    public int Height => height;

    #region Singleton
    public static UnitGridIndexManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<UnitGridIndexManager>();
        }
        return instance;
    }

    private static UnitGridIndexManager instance;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        grid = new MyGrid<int>(width, height, cellSize, map.transform.TransformPoint(map.GetComponent<MeshFilter>().mesh.bounds.min));

        indexMap = new NativeMultiHashMap<int2,Entity>(width * height, Allocator.Persistent);
        grid.ShowDebugText();
    }

    void OnDestroy()
    {
        indexMap.Dispose();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
