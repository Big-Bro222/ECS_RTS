using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


public class UnitSelectionManager : SingletonBase<UnitSelectionManager>
{
    public event EventHandler OnSelectionAreaStart;
    public event EventHandler OnSelectionAreaEnd;

    private Vector2 selectionStartMousePosition;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;
            OnSelectionAreaStart.Invoke(this, EventArgs.Empty);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;
            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            //Setup a minimum selection area for multiple unit selection
            float multipleSelectionSizeMin = 40f;
            bool isMultipleSelectionEnabled = selectionAreaSize > multipleSelectionSizeMin;
            
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            //Logic for deselecting Query, this is compute consuming, should change to event-base at a later step
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(em);

            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            
            for (int i = 0; i < entityArray.Length; i++)
            {
                em.SetComponentEnabled<Selected>(entityArray[i],false);
            }

            if (isMultipleSelectionEnabled)
            {
                //Multiple selection
                //Query for Units
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform,Unit>().WithPresent<Selected>().Build(em);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> LocalTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
                for (int i = 0; i < LocalTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = LocalTransformArray[i];
                    Vector2 unitScreenPos = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);
                    if (selectionAreaRect.Contains(unitScreenPos))
                    {
                        em.SetComponentEnabled<Selected>(entityArray[i],true);
                    }
                }
            }
            else
            {
                //Single selection
                entityQuery= em.CreateEntityQuery(typeof(PhysicsWorldSingleton));
                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();
                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
                UnityEngine.Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                int unitsLayer = 6;
                if (collisionWorld.CastRay(
                        new RaycastInput
                        {
                            Start = cameraRay.GetPoint(0),
                            End = cameraRay.GetPoint(999f),
                            Filter = new CollisionFilter
                            {
                                BelongsTo = ~0u,
                                CollidesWith = 1u << unitsLayer,
                                GroupIndex = 0
                            }
                        }, out Unity.Physics.RaycastHit raycastHit))
                {
                    if (em.HasComponent<Unit>(raycastHit.Entity))
                    {
                        em.SetComponentEnabled<Selected>(raycastHit.Entity,true);
                    }
                };
            }
            
            OnSelectionAreaEnd.Invoke(this, EventArgs.Empty);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = MouseWorldPosition.GetMouseWorldPosition();
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover, Selected>().Build(em);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = mouseWorldPosition;
                unitMoverArray[i] = unitMover;
            }

            entityQuery.CopyFromComponentDataArray(unitMoverArray);
        }
    }

    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;
        Vector2 lowerLeftCorner = new Vector2(Mathf.Min(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Min(selectionEndMousePosition.y, selectionStartMousePosition.y));
        Vector2 upperRightCorner = new Vector2(Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionEndMousePosition.y, selectionStartMousePosition.y));
        return new Rect(
            lowerLeftCorner.x,
            lowerLeftCorner.y,
            upperRightCorner.x - lowerLeftCorner.x,
            upperRightCorner.y - lowerLeftCorner.y
        );
    }
}