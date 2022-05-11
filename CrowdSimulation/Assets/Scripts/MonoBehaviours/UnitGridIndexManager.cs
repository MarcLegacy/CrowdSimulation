using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGridIndexManager : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private GameObject map;

    private MyGrid<int> grid;

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

        grid.ShowDebugText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        grid.ShowGrid(Color.red);
    }
}
