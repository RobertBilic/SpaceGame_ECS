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

        foreach(var turretSlot in Data.TurretBuildingSlots)
            Gizmos.DrawWireSphere(transform.TransformPoint(turretSlot.Position), turretSlot.Scale * transform.lossyScale.x);

        Gizmos.color = Color.blue;

        foreach (var weaponSlot in Data.WeaponBuildingSlots)
            Gizmos.DrawWireSphere(transform.TransformPoint(weaponSlot.Position), weaponSlot.Scale * transform.lossyScale.x);
    }

    public ShipBuildingData GetData() => Data;
}
