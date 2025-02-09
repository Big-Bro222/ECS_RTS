using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform, RefRO<MoveSpeed> moveSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<MoveSpeed>>())
        {
            //The Ref is for unmanaged data structs and use RW and RO to specify if it's a readonly parameter or not. This will be useful when it comes to Multi-Thread since people can read on multi-Thread but only write on the main thread

            float3 targetPos = localTransform.ValueRO.Position + math.right() * 10;
            float3 moveDir = targetPos - localTransform.ValueRO.Position;
            moveDir = math.normalize(moveDir);
            localTransform.ValueRW.Position += moveDir * SystemAPI.Time.DeltaTime * moveSpeed.ValueRO.value;
            localTransform.ValueRW.Rotation = quaternion.LookRotation(moveDir, math.up());
        }
    }
}