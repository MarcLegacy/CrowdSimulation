using System.Collections.Generic;
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
    [SerializeField] private bool showDebugText = false;
    [SerializeField] private DebugInfo gridDebug;

    private UnitGridIndexSystem unitGridIndexSystem;

    protected override void Start()
    {
        unitGridIndexSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>();

        base.Start();
    }

    protected override void SetVariables()
    {
        unitGridIndexSystem.grid =
            new MyGrid<int>(width, height, cellSize, map.transform.TransformPoint(map.GetComponent<MeshFilter>().mesh.bounds.min));
        unitGridIndexSystem.indexMap = new NativeMultiHashMap<int2, Entity>(width * height, Allocator.Persistent);
        if (showDebugText) unitGridIndexSystem.grid.ShowDebugText();
    }

    private void OnDrawGizmos()
    {
        if (gridDebug.show) unitGridIndexSystem.grid.ShowGrid(gridDebug.color);
    }
}

public partial class UnitGridIndexSystem : SystemBase
{
    public MyGrid<int> grid;
    public NativeMultiHashMap<int2, Entity> indexMap;

    protected override void OnDestroy()
    {
        indexMap.Dispose();
    }

    protected override void OnUpdate()
    {
        if (grid == null || !indexMap.IsCreated) return;

        MyGrid<int> _grid = grid;
        float3 gridOriginPosition = _grid.OriginPosition;
        float cellSize = _grid.CellSize;
        NativeMultiHashMap<int2, Entity> _indexMap = indexMap;
        NativeHashSet<int2> changedCellGridPositions = new NativeHashSet<int2>(grid.Width * grid.Height, Allocator.TempJob);

        Entities
            .WithName("Units_IndexToGrid")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, ref GridIndexComponent gridIndexComponent, in Translation translation) =>
            {
                if (gridIndexComponent.gridPosition.Equals(
                        Utilities.CalculateCellGridPosition(translation.Value, gridOriginPosition, cellSize))) return;

                if (_indexMap.TryGetFirstValue(gridIndexComponent.gridPosition, out Entity currentEntity,
                        out NativeMultiHashMapIterator<int2> iterator))
                {
                    if (currentEntity.Equals(entity))
                    {
                        _indexMap.Remove(iterator);
                    }
                    else
                    {
                        while (_indexMap.TryGetNextValue(out currentEntity, ref iterator))
                        {
                            if (currentEntity.Equals(entity))
                            {
                                _indexMap.Remove(iterator);
                                break;
                            }
                        }
                    }
                }

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);

                gridIndexComponent.gridPosition = Utilities.CalculateCellGridPosition(translation.Value, gridOriginPosition, cellSize);

                _indexMap.Add(gridIndexComponent.gridPosition, entity);

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);
            })
            .Run();

        //CompleteDependency();

        foreach (var changedCellGridPosition in changedCellGridPositions)
        {
            _grid.SetCell(Utilities.Int2toVector2Int(changedCellGridPosition), _indexMap.CountValuesForKey(changedCellGridPosition));
        }

        changedCellGridPositions.Dispose();
    }


}
