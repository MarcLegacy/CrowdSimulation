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
        NativeMultiHashMap<int2, Entity> indexMap = unitGridIndexManager.indexMap;

        Entities
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, ref GridIndexComponent gridIndexComponent, in Translation translation) =>
            {
                if (gridIndexComponent.gridPosition.Equals(
                        Utilities.Vector2IntToInt2(gridIndexGrid.GetCellGridPosition(translation.Value)))) return;

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

                gridIndexGrid.SetCell(Utilities.Int2toVector2Int(gridIndexComponent.gridPosition),
                    indexMap.CountValuesForKey(gridIndexComponent.gridPosition));

                gridIndexComponent.gridPosition = Utilities.Vector2IntToInt2(gridIndexGrid.GetCellGridPosition(translation.Value));

                indexMap.Add(gridIndexComponent.gridPosition, entity);

                gridIndexGrid.SetCell(Utilities.Int2toVector2Int(gridIndexComponent.gridPosition),
                    indexMap.CountValuesForKey(gridIndexComponent.gridPosition));
            })
            .WithoutBurst()
            .Run();
    }
}
