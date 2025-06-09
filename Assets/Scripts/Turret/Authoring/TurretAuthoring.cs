using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class TurretAuthoring : MonoBehaviour
    {
        public GameObject RotationBase;
        public float RotationSpeed;

        [Header("Combat")]
        public float Damage;
        public float FiringRate;
        public float Range;
        public float ReloadTime;
        public int AmmoSize;

        [Header("Bullet")]
        public string BulletId;
        public List<Vector3> bulletSpawnPositionsLocal;

        [Header("Additional")]
        public List<AdditionalBakedComponentBase> AdditionalComponents;
        public List<AdditionalBakedBufferBase> AdditionalBuffers;
    }
}