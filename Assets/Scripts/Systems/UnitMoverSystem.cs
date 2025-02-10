using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob()
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        unitMoverJob.ScheduleParallel();
    }
}
[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute( ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 targetPos = unitMover.targetPosition;
        float3 moveDir = targetPos - localTransform.Position;
        float reachedTargetDistanceSq = 2f;
        if (math.lengthsq(moveDir) < reachedTargetDistanceSq)
        {
            //reached the target position
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
        };
        moveDir = math.normalize(moveDir);

        localTransform.Rotation = math.slerp(localTransform.Rotation,
            quaternion.LookRotation(moveDir, math.up()),
            deltaTime * unitMover.rotationSpeed);
        physicsVelocity.Linear = moveDir * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}