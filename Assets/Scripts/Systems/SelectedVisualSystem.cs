using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SelectedVisualSystem : ISystem
{


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //Setup the selected visual indicator scale
        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>())
        {
            RefRW<LocalTransform> visualLocalTransform =SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
            visualLocalTransform.ValueRW.Scale = selected.ValueRO.showScale;
        }
        //Setup the Unselected visual indicator scale
        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>())
        {
            RefRW<LocalTransform> visualLocalTransform =SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
            visualLocalTransform.ValueRW.Scale = 0f;
        }
    }


}
