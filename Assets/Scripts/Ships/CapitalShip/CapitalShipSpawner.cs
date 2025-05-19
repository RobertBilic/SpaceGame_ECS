using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CapitalShipSpawner : MonoBehaviour
{
    EntityManager em;

    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager; var entity = em.CreateEntity();
        em.AddComponentData(entity, new ShipConstructionRequest()
        {
            Scale = 5.0f,
            SpawnPosition = float3.zero,
            Id = "ships_capital_ship",
            Team = 1
        });

        var addOns = em.AddBuffer<ShipConstructionAddonRequest>(entity);

        addOns.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<CapitalShipTag>() });
        addOns.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<SceneMovementData>() });

        var turretBuffer = em.AddBuffer<ShipTurretConstructionRequest>(entity);

        turretBuffer.Add(new ShipTurretConstructionRequest()
        {
            Position = new float3(5.0f, 0.0f, 0.0f),
            Scale = 0.2f,
            Id = "turret_basic_1"
        });
        turretBuffer.Add(new ShipTurretConstructionRequest()
        {
            Position = new float3(0.0f, 0.0f, 0.0f),
            Scale = 0.2f,
            Id = "turret_basic_1"
        });
        turretBuffer.Add(new ShipTurretConstructionRequest()
        {
            Position = new float3(-2.5f, 0.0f, 0.0f),
            Scale = 0.2f,
            Id = "turret_basic_1"
        });
        turretBuffer.Add(new ShipTurretConstructionRequest()
        {
            Position = new float3(2.5f, 0.0f, 0.0f),
            Scale = 0.2f,
            Id = "turret_basic_1"
        });
        turretBuffer.Add(new ShipTurretConstructionRequest()
        {
            Position = new float3(-5.0f, 0.0f, 0.0f),
            Scale = 0.2f,
            Id = "turret_basic_1"
        });
    }
}
