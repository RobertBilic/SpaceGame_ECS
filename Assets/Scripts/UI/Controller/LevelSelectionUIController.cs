using SpaceGame.Game.State.Component;
using SpaceGame.Movement.Components;
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class LevelSelectionUIController : GenericGameStateUIController<LevelSelectionUIState>
{
    [SerializeField]
    private LevelContainer levelContainer;
    [SerializeField]
    private FleetManager fleetManager;

    private LevelDataHolder selectedLevel;

    private void Awake()
    {
        //TODO: Proper level selection
        selectedLevel = levelContainer.Levels.FirstOrDefault();
        ui.SetOnPlayButton(StartLevel);
    }

    private void StartLevel()
    {
        if (selectedLevel == null)
            return;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var startRequestEntity = em.CreateEntity();

        em.AddComponent<LevelStartRequestTag>(startRequestEntity);
        var shipsInLevel = em.AddBuffer<LevelShipEntry>(startRequestEntity);

        foreach(var ship in selectedLevel.ShipEntries)
        {
            shipsInLevel.Add(new LevelShipEntry()
            {
                Count = ship.Count,
                Id = ship.Id,
                Position = ship.Position,
                SpawnRadius = ship.SpawnRadius,
                Team = ship.Team
            });
        }

        var ownedFleet = fleetManager.GetOwnedShips();

        //TODO: Fleet Selection for mission

        bool addedCameraFollowComponent = false;

        foreach(var ship in ownedFleet)
        {
            var createShipRequest = em.CreateEntity();

            var spawnPosition =  selectedLevel.PlayerSpawnPosition + new Vector3(UnityEngine.Random.Range(-selectedLevel.PlayerSpawnRadius, selectedLevel.PlayerSpawnRadius), UnityEngine.Random.Range(-selectedLevel.PlayerSpawnRadius, selectedLevel.PlayerSpawnRadius), 0);

            em.AddComponentData(createShipRequest, new ShipConstructionRequest()
            {
                Id = ship.ShipId,
                SpawnPosition = spawnPosition,
                Team = levelContainer.PlayerTeam
            });

            var addonBuffer = em.AddBuffer<ShipConstructionAddonRequest>(createShipRequest);

            if (!addedCameraFollowComponent)
            {
                addonBuffer.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<CameraFollowTag>() });
                addedCameraFollowComponent = true;
            }

            if(ship.TurretsBySlotIndex.Count != 0)
            {
                var turretRequest = em.AddBuffer<ShipTurretConstructionRequest>(createShipRequest);
                fleetManager.AddAdditionalWeapons(ship, turretRequest);
            }

            //TODO: Dynamic Forward weapons
            var changeGameStateRequest = em.CreateEntity();
            em.AddComponentData(changeGameStateRequest, new ChangeGameStateRequest() { Value = GameState.Combat });
        }

    }
}
