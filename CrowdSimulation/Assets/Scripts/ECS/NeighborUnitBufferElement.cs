using Unity.Entities;

[InternalBufferCapacity(100)]
public struct NeighborUnitBufferElement : IBufferElementData
{
    public Entity unit;
    public bool inAlignmentRadius;
    public bool inCohesionRadius;
    public bool inSeparationRadius;

}
