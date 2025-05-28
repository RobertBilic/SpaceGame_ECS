using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void OnShipSelected(string id);
public delegate void OnTurretSlotSelected(Vector3 position, int slotIndex, int currentlySelected, List<string> turretIds);
public delegate void OnTurretSelected(int index, string id);

public class FleetManager : MonoBehaviour
{
    public event OnTurretSlotSelected OnTurretSlotSelected;

    private const string TAG = "FleetManager <-> ";

    [SerializeField]
    private ShipPrefabDataHolder ships;

    [SerializeField]
    private TurretPrefabDataHolder turrets;


    [SerializeField]
    private CameraFramer framer;

    private List<ShipLoadout> shipLoadouts;
    private InputSystem_Actions inputActions;
    private CurrentlyInspectedShip currentInspected;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    public List<ShipLoadout> GetOwnedShips()
    {
        if(shipLoadouts == null)
            shipLoadouts = LoadShips();

        return shipLoadouts;
    }

    public void AddAdditionalWeapons(ShipLoadout loadout, DynamicBuffer<ShipTurretConstructionRequest> buffer)
    {
        var shipData = ships.Data.Find(x => x.Id == loadout.ShipId);

        if(shipData == null)
        {
            Debug.Log($"{TAG}Can not find ship with id {loadout.ShipId}");
        }

        var buildingData = shipData.Prefab.GetComponent<ShipBuilder>().GetData();

        foreach (var turret in loadout.TurretsBySlotIndex)
        {
            if (turret.Index < 0 || turret.Index > buildingData.TurretBuildingSlots.Count - 1)
            {
                Debug.LogWarning($"{TAG}Invalid turret index slot {turret.Index}, range is (0...{buildingData.TurretBuildingSlots.Count})");
                continue;
            }

            var targetSlot = buildingData.TurretBuildingSlots[turret.Index];

            buffer.Add(new ShipTurretConstructionRequest()
            {
                Id = turret.Id,
                Position = targetSlot.Position,
                Scale = targetSlot.Scale
            });
        }

    }

    private List<ShipLoadout> LoadShips()
    {
        //TODO: Real ship loading
        var loadedShipLoadouts = new List<ShipLoadout>();

        loadedShipLoadouts.Add(new ShipLoadout()
        {
            ShipId = "ships_capital_ship",
            LocalId = Guid.NewGuid().ToString(),
            TurretsBySlotIndex = new List<PlacementData>(),
            WeaponsBySlotIndex = new List<PlacementData>()
        });

        loadedShipLoadouts.Add(new ShipLoadout()
        {
            ShipId = "ships_cruiser_1",
            TurretsBySlotIndex = new List<PlacementData>()
            {
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 0
                },
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 1
                },
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 2
                },
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 3
                },
            },
            WeaponsBySlotIndex = new List<PlacementData>(),
            LocalId = Guid.NewGuid().ToString()
        });
        loadedShipLoadouts.Add(new ShipLoadout()
        {
            ShipId = "ships_cruiser_1",
            TurretsBySlotIndex = new List<PlacementData>()
            {
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 0
                },
                new PlacementData()
                {
                    Id = "turret_basic_1",
                    Index = 3
                },
            },
            WeaponsBySlotIndex = new List<PlacementData>(),
            LocalId = Guid.NewGuid().ToString()
        });

        return loadedShipLoadouts;
    }

    public void ShowShip(string id)
    {
        var ownedShip = shipLoadouts.Find(x => x.LocalId == id);

        if(ownedShip == null)
        {
            Debug.LogWarning($"{TAG}Owned ship couldn't be found, id: {id}");
            return;
        }

        var shipPrefab = ships.Data.Find(x => x.Id == ownedShip.ShipId);

        if(shipPrefab == null)
        {
            Debug.LogWarning($"{TAG}Can not find ship prefab with id {id}");
            return;
        }

        if (currentInspected != null)
            GameObject.Destroy(currentInspected.GameObj);

        var inspectedShip = GameObject.Instantiate(shipPrefab.Prefab);

        currentInspected = new CurrentlyInspectedShip()
        {
            Builder = inspectedShip.GetComponent<ShipBuilder>(),
            GameObj = inspectedShip.gameObject,
            Loadout = ownedShip
        };

        ApplyAdditionalWeapons(ownedShip, currentInspected.GameObj);

        framer.FrameHitboxes(inspectedShip.transform, inspectedShip.Hitboxes);
    }

    private void ApplyAdditionalWeapons(ShipLoadout ownedShip, GameObject currentInspected)
    {
        var shipBuilder = currentInspected.GetComponent<ShipBuilder>();

        if(shipBuilder == null)
        {
            Debug.LogWarning($"{TAG}The ship with id {ownedShip.ShipId} doesn't have an ShipBuilder component");
            return;
        }

        var buildingData = shipBuilder.GetData();

        foreach(var turret in ownedShip.TurretsBySlotIndex)
        {
            var turretPrefab = turrets.Data.Find(x => x.Id == turret.Id);

            if(turretPrefab == null)
            {
                Debug.LogWarning($"{TAG}Turret with the {turret.Id} doesn't exist");
                continue;
            }

            var turretObj = GameObject.Instantiate(turretPrefab.Prefab.gameObject, currentInspected.transform);
            
            if(turret.Index < 0 || turret.Index > buildingData.TurretBuildingSlots.Count - 1)
            {
                Debug.LogWarning($"{TAG}Invalid turret index slot {turret.Index}, range is (0...{buildingData.TurretBuildingSlots.Count})");
                continue;
            }

            var targetSlot = buildingData.TurretBuildingSlots[turret.Index];

            turretObj.transform.localPosition = targetSlot.Position;
            turretObj.transform.localScale = targetSlot.Scale * Vector3.one;
        }

        //TODO: Dynamic forward weapons
    }

    public void AddTurret(int index, string id)
    {
        if (currentInspected == null)
            return;

        if (index < 0 || index > currentInspected.Builder.GetData().TurretBuildingSlots.Count - 1)
            return;

        var data = currentInspected.Loadout.TurretsBySlotIndex.Find(x => x.Index == index);

        if (data == null)
            currentInspected.Loadout.TurretsBySlotIndex.Add(new PlacementData() { Index = index, Id = id });
        else
            data.Id = id;

        ShowShip(currentInspected.Loadout.LocalId);
    }

    public void Clear()
    {
        if (currentInspected != null)
            GameObject.Destroy(currentInspected.GameObj);
        currentInspected = null;
        framer.UnFrameHitboxes();
    }

    internal void Enable()
    {
        inputActions.UI.Click.performed += Click_performed;
        inputActions.Enable();
    }

    private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var clickedOn = inputActions.UI.Point.ReadValue<Vector2>();
        var worldSpace = Camera.main.ScreenToWorldPoint(clickedOn);

        worldSpace.z = 0.0f;

        if (currentInspected == null)
            return;

        var shipBuilder = currentInspected.Builder;

        if (shipBuilder == null)
            return;

        var clickPadding = 1.1f;

        TurretBuildingSlot selectedTurretSlot = null;
        var index = 0;
        var turretSlots = shipBuilder.GetData().TurretBuildingSlots;

        for(int i=0;i<turretSlots.Count;i++)
        {
            var turret = turretSlots[i];

            var position = currentInspected.GameObj.transform.TransformPoint(turret.Position);
            if (Vector3.Distance(position, worldSpace) > turret.Scale * clickPadding)
                continue;

            index = i;
            selectedTurretSlot = turret;
            break;
        }

        if(selectedTurretSlot != null)
        {
            //TODO: Filter
            var ids = turrets.Data
                .Select(x => x.Id)
                .ToList();

            var loadoutForSlot = currentInspected.Loadout.TurretsBySlotIndex.Find(x => x.Index == index);
            int currentlySelected = -1;
            
            if(loadoutForSlot != null)
                currentlySelected = ids.IndexOf(loadoutForSlot.Id);

            OnTurretSlotSelected?.Invoke(clickedOn, index, currentlySelected, ids);
        }

        //TODO: Forward weapons
    }

    internal void Disable()
    {
        inputActions.UI.Click.performed -= Click_performed;
        inputActions.Disable();
    }

    [System.Serializable]
    private class CurrentlyInspectedShip
    {
        public GameObject GameObj;
        public ShipBuilder Builder;
        public ShipLoadout Loadout;
    }
}
