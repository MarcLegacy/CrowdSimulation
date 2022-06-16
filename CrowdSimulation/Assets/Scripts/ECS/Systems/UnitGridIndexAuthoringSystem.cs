using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
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
        unitGridIndexSystem.m_grid =
            new MyGrid<int>(width, height, cellSize, map.transform.TransformPoint(map.GetComponent<MeshFilter>().mesh.bounds.min));
        unitGridIndexSystem.m_indexMap = new NativeMultiHashMap<int2, Entity>(width * height, Allocator.Persistent);
        if (showDebugText) unitGridIndexSystem.m_grid.ShowDebugText();

        base.Start();
    }

    private void OnDrawGizmos()
    {
        if (gridDebug.show && unitGridIndexSystem != null) unitGridIndexSystem.m_grid.ShowGrid(gridDebug.color);
    }
}

public partial class UnitGridIndexSystem : SystemBase
{
    public event EventHandler<OnSpawnLeftEventArgs> OnSpawnLeft;
    public class OnSpawnLeftEventArgs : EventArgs
    {
        public List<Entity> entities;
    }

    public event EventHandler<OnTargetReachedEventArgs> OnTargetReached;

    public class OnTargetReachedEventArgs : EventArgs
    {
        public List<Entity> entities;
    }

    public MyGrid<int> m_grid;
    public NativeMultiHashMap<int2, Entity> m_indexMap;

    private NativeList<int2> m_spawnGridPositions;
    private int2 m_targetGridPosition;

    protected override void OnCreate()
    {
        PathingManager.GetInstance().OnCellsInfoCollected += OnCellsInfoCollected;
        m_spawnGridPositions = new NativeList<int2>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        m_indexMap.Dispose();
        m_spawnGridPositions.Dispose();
    }

    protected override void OnUpdate()
    {
        if (m_grid == null || !m_indexMap.IsCreated) return;

        MyGrid<int> grid = m_grid;
        float3 gridOriginPosition = grid.OriginPosition;
        float cellSize = grid.CellSize;
        NativeMultiHashMap<int2, Entity> indexMap = m_indexMap;
        NativeHashSet<int2> changedCellGridPositions = new NativeHashSet<int2>(m_grid.Width * m_grid.Height, Allocator.TempJob);
        NativeList<Entity> entitiesSpawnLeft = new NativeList<Entity>(Allocator.TempJob);
        NativeList<int2> spawnGridPositions = m_spawnGridPositions;
        int2 targetGridPosition = m_targetGridPosition;
        NativeList<Entity> entitiesReachedTarget = new NativeList<Entity>(Allocator.TempJob);

        Entities
            .WithName("Units_IndexToGrid")
            .WithReadOnly(spawnGridPositions)
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, ref GridIndexComponent gridIndexComponent, in Translation translation) =>
            {
                if (gridIndexComponent.gridPosition.Equals(
                        Utilities.CalculateCellGridPosition(translation.Value, gridOriginPosition, cellSize))) return;

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

                int2 newGridPosition = Utilities.CalculateCellGridPosition(translation.Value, gridOriginPosition, cellSize);

                if (spawnGridPositions.Length != 0)
                {
                    if (spawnGridPositions.Contains(gridIndexComponent.gridPosition) &&
                        !spawnGridPositions.Contains(newGridPosition))
                    {
                        entitiesSpawnLeft.Add(entity);
                    }
                }

                if (!gridIndexComponent.gridPosition.Equals(targetGridPosition) && newGridPosition.Equals(targetGridPosition))
                {
                    entitiesReachedTarget.Add(entity);
                }

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);

                gridIndexComponent.gridPosition = newGridPosition;

                indexMap.Add(gridIndexComponent.gridPosition, entity);

                changedCellGridPositions.Add(gridIndexComponent.gridPosition);
            })
            .Schedule();

        CompleteDependency();

        foreach (var changedCellGridPosition in changedCellGridPositions)
        {
            grid.SetCell(Utilities.Int2ToVector2Int(changedCellGridPosition), indexMap.CountValuesForKey(changedCellGridPosition));
        }

        if (entitiesSpawnLeft.Length != 0)
        {
            List<Entity> newEntitiesList = new List<Entity>(entitiesSpawnLeft.Length);

            foreach (Entity entity in entitiesSpawnLeft)
            {
                newEntitiesList.Add(entity);
            }

            OnSpawnLeft?.Invoke(this, new OnSpawnLeftEventArgs() { entities = newEntitiesList });
        }

        if (entitiesReachedTarget.Length != 0)
        {
            List<Entity> newEntitiesList = new List<Entity>(entitiesReachedTarget.Length);

            foreach (Entity entity in entitiesReachedTarget)
            {
                newEntitiesList.Add(entity);
            }

            OnTargetReached?.Invoke(this, new OnTargetReachedEventArgs() { entities = newEntitiesList });
        }

        changedCellGridPositions.Dispose();
        entitiesSpawnLeft.Dispose();
        entitiesReachedTarget.Dispose();
    }

    private void OnCellsInfoCollected(object sender, PathingManager.OnCellsInfoCollectedEventArgs eventArgs)
    {
        m_spawnGridPositions.Clear();

        foreach (var cellInfo in eventArgs.cellsInfo)
        {
            switch (cellInfo.Value)
            {
                case CellType.Spawn:
                    m_spawnGridPositions.Add(Utilities.Vector2IntToInt2(cellInfo.Key));
                    break;
                case CellType.Target:
                    m_targetGridPosition = Utilities.Vector2IntToInt2(cellInfo.Key);
                    break;
            }
        }
    }
}
