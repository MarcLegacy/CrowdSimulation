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
    private UnitGridIndexSystem unitGridIndexSystem;

    protected override void Start()
    {
        unitGridIndexSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitGridIndexSystem>();

        base.Start();
    }
}

public partial class UnitGridIndexSystem : SystemBase
{
    private UnitGridIndexManager unitGridIndexManager;

    protected override void OnCreate()
    {
        unitGridIndexManager = UnitGridIndexManager.GetInstance();
    }

    protected override void OnUpdate()
    {
        if (unitGridIndexManager == null || unitGridIndexManager.Grid == null || !unitGridIndexManager.indexMap.IsCreated) return;

        MyGrid<int> gridIndexGrid = unitGridIndexManager.Grid;
        float3 gridOriginPosition = gridIndexGrid.OriginPosition;
        float cellSize = gridIndexGrid.CellSize;
        NativeMultiHashMap<int2, Entity> indexMap = unitGridIndexManager.indexMap;
        NativeHashSet<int2> changedCellGridPositions = new NativeHashSet<int2>(unitGridIndexManager.Width * unitGridIndexManager.Height, Allocator.TempJob);

        Entities
            .WithName("Units_IndexToGrid")
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, ref GridIndexComponent gridIndexComponent, in Translation translation) =>
            {
                if (gridIndexComponent.gridPosition.Equals(
                        GetCellGridPosition(translation.Value, gridOriginPosition, cellSize))) return;

                if (indexMap.TryGetFirstValue(gridIndexComponent.gridPosition, out Entity currentEntity,
                        out NativeMultiHashMapIterator<int2> iterator))
                {
                    if (currentEntity.Equals(entity))
                    {
                        indexMap.Remove(iterator);
                    }
                    else
                    {
                        while (indexMap.TryGetNextValue(out currentEntity, ref iterator))
                        {
                            if (currentEntity.Equals(entity))
                            {
                                indexMap.Remove(iterator);
                                break;
                            }
                        }
                    }
                }

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);

                gridIndexComponent.gridPosition = GetCellGridPosition(translation.Value, gridOriginPosition, cellSize);

                indexMap.Add(gridIndexComponent.gridPosition, entity);

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);
            })
            .Run();

        //CompleteDependency();

        foreach (var changedCellGridPosition in changedCellGridPositions)
        {
            gridIndexGrid.SetCell(Utilities.Int2toVector2Int(changedCellGridPosition), indexMap.CountValuesForKey(changedCellGridPosition));
        }

        changedCellGridPositions.Dispose();
    }

    private static int2 GetCellGridPosition(float3 worldPosition, float3 gridOriginPosition, float gridCellSize)
    {
        int x = Mathf.FloorToInt((worldPosition - gridOriginPosition).x / gridCellSize);
        int y = Mathf.FloorToInt((worldPosition - gridOriginPosition).z / gridCellSize);

        return new int2(x, y);
    }
}
