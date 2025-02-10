using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

//Specific class naming to make this actually work
public class UnitMoverAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;

    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            //Return the primary Entity
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover
            {
                //Bake value to component
                moveSpeed=authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed
            });
        }
    }
}

public struct UnitMover:IComponentData
{
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
}
