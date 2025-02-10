using Unity.Entities;
using UnityEngine;

public class SelectedAuthoring : MonoBehaviour
{
    public GameObject visaulGameObject;
    public float ShowScale;
}

public class SelectedAuthoringBaker : Baker<SelectedAuthoring>
{
    public override void Bake(SelectedAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity,new Selected
        {
            visualEntity = GetEntity(authoring.visaulGameObject,TransformUsageFlags.Dynamic),
            showScale = authoring.ShowScale
        });
        SetComponentEnabled<Selected>(entity,false);
    }
}

public struct Selected : IComponentData,IEnableableComponent
{
    public Entity visualEntity;
    //if enable, the selected indicator has a scale, otherwise it's zero
    //TODO: find a way to disable the selected completely and if it's worth it
    public float showScale;
}
