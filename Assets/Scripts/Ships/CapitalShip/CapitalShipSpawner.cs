using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CapitalShipSpawner : MonoBehaviour
{
    [SerializeField]
    private CameraFollow followScript;

    EntityManager em;

    Entity shipPrefabBaker;
    Entity turretPrefabBaker; 
    bool prefabReady = false;

    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (!prefabReady)
        {
            EntityQuery query = em.CreateEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                ComponentType.ReadOnly<Prefab>(),
                ComponentType.ReadOnly<CapitalShipPrefab>()
                },

                Options = EntityQueryOptions.IncludePrefab
            });

            EntityQuery query2 = em.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<TurretPrefab>()
                },
                Options = EntityQueryOptions.IncludePrefab
            });

            if (query.CalculateEntityCount() == 1 && query2.CalculateEntityCount() == 1)
            {
                shipPrefabBaker = query.GetSingletonEntity();
                turretPrefabBaker = query2.GetSingletonEntity();
                prefabReady = true;

                var entity = em.CreateEntity();

                var shipPrefabBakerData = em.GetComponentData<CapitalShipPrefab>(shipPrefabBaker);
                var turretPrefabBakerData = em.GetComponentData<TurretPrefab>(turretPrefabBaker);

                em.AddComponentData(entity, new CapitalShipConstructionRequest()
                {
                    CapitalShipPrefab = shipPrefabBakerData.Value,
                    MoveSpeed = 2.0f,
                    Scale = 5.0f,
                    RotationSpeed = 50,
                    SpawnPosition = float3.zero
                });

                var buffer = em.AddBuffer<CapitalShipTurret>(entity);
                buffer.Add(new CapitalShipTurret()
                {
                    Position = new float3(5.0f, 0.0f, 0.0f),
                    Scale = 0.2f,
                    TurretPrefab = turretPrefabBakerData.PrefabEntity,
                });
                buffer.Add(new CapitalShipTurret()
                {
                    Position = new float3(0.0f, 0.0f, 0.0f),
                    Scale = 0.2f,
                    TurretPrefab = turretPrefabBakerData.PrefabEntity,
                });
                buffer.Add(new CapitalShipTurret()
                {
                    Position = new float3(-2.5f, 0.0f, 0.0f),
                    Scale = 0.2f,
                    TurretPrefab = turretPrefabBakerData.PrefabEntity,
                });
                buffer.Add(new CapitalShipTurret()
                {
                    Position = new float3(2.5f, 0.0f, 0.0f),
                    Scale = 0.2f,
                    TurretPrefab = turretPrefabBakerData.PrefabEntity,
                });
                buffer.Add(new CapitalShipTurret()
                {
                    Position = new float3(-5.0f, 0.0f, 0.0f),
                    Scale = 0.2f,
                    TurretPrefab = turretPrefabBakerData.PrefabEntity,
                });
            }
        }
    }

}
