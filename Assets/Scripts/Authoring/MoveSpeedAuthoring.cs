using UnityEngine;
using Unity.Entities;

//Specific class naming to make this actually work
public class MoveSpeedAuthoring : MonoBehaviour
{
    public float value;

    public class Baker : Baker<MoveSpeedAuthoring>
    {
        public override void Bake(MoveSpeedAuthoring authoring)
        {
            //Return the primary Entity
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveSpeed
            {
                //Bake value to component
                value=authoring.value
            });
        }
    }
}

public struct MoveSpeed:IComponentData
{
    public float value;
}
