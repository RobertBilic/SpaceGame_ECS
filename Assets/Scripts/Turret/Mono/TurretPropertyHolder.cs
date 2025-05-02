using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class TurretPropertyHolder : MonoBehaviour
{
    public GameObject RotationBase;
    public float RotationSpeed;

    public float Damage;
    public float FiringRate;
    public float Range;

    [Header("Recoil")]
    public GameObject RecoilTarget;
    public float RecoilDuration;
    public float MaxRecoilDistance;

    [Header("Bullet Spawn Positions")]
    public List<Vector3> bulletSpawnPositionsLocal;
}
