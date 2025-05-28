using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ShipBuilder : MonoBehaviour
{
    [SerializeField]
    private ShipBuildingData Data;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        foreach (var turretSlot in Data.TurretBuildingSlots)
        {
            var scale = turretSlot.Scale * Vector3.one * transform.lossyScale.x;
            scale.z = 0.001f;
            Gizmos.DrawWireCube(turretSlot.Position, scale);
        }
        Gizmos.color = Color.blue;

        foreach (var weaponSlot in Data.WeaponBuildingSlots)
        {
            var scale = weaponSlot.Scale * Vector3.one * transform.lossyScale.x;
            scale.z = 0.001f;
            Gizmos.DrawWireCube(weaponSlot.Position, scale);
        }
    }

    public ShipBuildingData GetData() => Data;
}
