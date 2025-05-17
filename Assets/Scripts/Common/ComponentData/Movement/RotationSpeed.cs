using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Movement.Components
{
    public struct RotationSpeed : IComponentData
    {
        public float Value;
    }
}