using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct EntityPrefabComponent : IComponentData
{
    public Entity entityPrefab; 
}
