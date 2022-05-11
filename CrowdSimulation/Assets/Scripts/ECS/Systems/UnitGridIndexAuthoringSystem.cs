using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnitGridIndexAuthoringSystem : AuthoringSystem
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private GameObject map;

    private UnitGridIndexSystem unitGridIndexSystem;

    protected override void Start()
    {
        unitGridIndexSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitGridIndexSystem.grid = new MyGrid<int>(width, height, cellSize, map.transform.TransformPoint(map.GetComponent<MeshFilter>().mesh.bounds.min));
        unitGridIndexSystem.grid.ShowDebugText();
    }
}

public partial class UnitGridIndexSystem : SystemBase
{
    public MyGrid<int> grid;

    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        if (grid == null) return;

        //Entities
        //    .WithAll<UnitComponent>()
        //    .ForEach((ref Translation translation) => 
        //    {
        //        grid.SetCell(translation.Value, grid.GetCell(translation.Value) + 1);
        //    })
        //    .WithoutBurst()
        //    .Run();
    }
}
