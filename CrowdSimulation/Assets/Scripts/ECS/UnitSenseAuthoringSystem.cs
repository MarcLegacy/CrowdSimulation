using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

public class UnitSenseAuthoringSystem : AuthoringSystem
{
    private UnitSenseSystem unitSenseSystem;

    protected override void Start()
    {
        unitSenseSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitSenseSystem>();

        base.Start();
    }
}

public partial class UnitSenseSystem : SystemBase
{
    private const float senseRayAngleOffset = 20f;
     
    protected override void OnUpdate()
    {
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld; ;

        Entities
            .WithName("Unit_Sensing")
            .WithReadOnly(physicsWorld)
            .WithAll<UnitComponent>()
            .ForEach((Entity entity, ref Translation translation, ref UnitSenseComponent unitSenseComponent, in Rotation rotation) =>
            {
                float3 leftRayStartPos =
                    translation.Value + (float3)(Quaternion.Euler(0, -senseRayAngleOffset, 0) * math.forward(rotation.Value));
                float3 leftRayEndPos = translation.Value +
                                       (float3)(Quaternion.Euler(0, -senseRayAngleOffset, 0) * math.forward(rotation.Value)) *
                                       unitSenseComponent.distance;
                float3 rightRayStartPos =
                    translation.Value + (float3)(Quaternion.Euler(0, senseRayAngleOffset, 0) * math.forward(rotation.Value));
                float3 rightRayEndPos = translation.Value +
                                        (float3)(Quaternion.Euler(0, senseRayAngleOffset, 0) * math.forward(rotation.Value)) *
                                        unitSenseComponent.distance;

                //Debug.DrawLine(leftRayStartPos, leftRayEndPos, Color.red);
                //Debug.DrawLine(rightRayStartPos, rightRayEndPos, Color.red);

                CollisionFilter collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };

                var leftRayInput = new RaycastInput
                {
                    Start = leftRayStartPos,
                    End = leftRayEndPos,
                    Filter = collisionFilter
                };

                var rightRayInput = new RaycastInput
                {
                    Start = rightRayStartPos,
                    End = rightRayEndPos,
                    Filter = collisionFilter
                };

                unitSenseComponent.isBlocking = false;
                unitSenseComponent.leftIsBlocking = false;
                unitSenseComponent.rightIsBlocking = false;

                if (physicsWorld.CastRay(leftRayInput, out Unity.Physics.RaycastHit hit))
                {
                    unitSenseComponent.isBlocking = true;
                    unitSenseComponent.leftIsBlocking = true;
                }

                if (physicsWorld.CastRay(rightRayInput))
                {
                    unitSenseComponent.isBlocking = true;
                    unitSenseComponent.rightIsBlocking = true;
                }
            })
            .ScheduleParallel();
    }
}
