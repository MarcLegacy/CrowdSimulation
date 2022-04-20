using Unity.Entities;

[GenerateAuthoringComponent]
[InternalBufferCapacity(100)]
public struct NeighborUnitBufferElement : IBufferElementData
{
    public Entity unit;
}
