using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipLoadout
{
    public string ShipId;
    public string LocalId;
    public List<PlacementData> TurretsBySlotIndex; 
    public List<PlacementData> WeaponsBySlotIndex; 
}

[System.Serializable]
public class PlacementData
{
    public int Index;
    public string Id;
}

[System.Serializable]
public class ShipBuildingData
{
    public List<TurretBuildingSlot> TurretBuildingSlots;
    public List<ForwardWeaponBuildingSlot> WeaponBuildingSlots;
}

[System.Serializable]
public class TurretBuildingSlot
{
    /// <summary>
    /// Local position offset
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Scale of the turret, cosmetic only 
    /// </summary>
    public float Scale;
    //TODO: Turret Filter
}

[System.Serializable]
public class ForwardWeaponBuildingSlot
{
    /// <summary>
    /// Local position offset
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Default rotation for the weapon
    /// </summary>
    public Quaternion Rotation;
    /// <summary>
    /// Scale of the turret, cosmetic only 
    /// </summary>
    public float Scale;
    //TODO: Weapon Filter
}
