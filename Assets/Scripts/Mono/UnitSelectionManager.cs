using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            NativeArray<float3> movePositionArray =
                GenerateMovePositionArray(mouseWorldPosition, unitMoverArray.Length);
            
            for (int i = 0; i < unitMoverArray.Length; i++)
            {
                UnitMover unitMover = unitMoverArray[i];
                unitMover.targetPosition = movePositionArray[i];
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

    private NativeArray<float3> GenerateMovePositionArray(float3 targetPos, int unitCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(unitCount, Allocator.Temp);
        if (unitCount == 0)
        {
            return positionArray;
        }

        positionArray[0] = targetPos;
        if (unitCount == 1)
        {
            return positionArray;
        }
        float ringSize = 2.2f;
        int ringIndex = 0;
        int positionIndex = 1;
        while (positionIndex<unitCount)
        {
            //assignable position of the ring should be different and the outer the ring is, the more unit it can contain
            int ringPositionCount = 3 + ringIndex * 2;
            for (int i = 0; i < ringPositionCount; i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector =
                    math.rotate(quaternion.RotateY(angle), new float3(ringSize * (ringIndex + 1), 0, 0));
                float3 ringPosition = targetPos + ringVector;
                positionArray[positionIndex] = ringPosition;
                positionIndex++;
                if (positionIndex >= unitCount)
                {
                    break;
                }
            }
            ringIndex++;
        }
        return positionArray;
    }
}