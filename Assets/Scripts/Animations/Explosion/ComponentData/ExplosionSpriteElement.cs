using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Animations.Components
{
    public struct ExplosionSpriteElement : IBufferElementData
    {
        public Entity SpriteEntity;
        public float TimeOnElementMin;
        public float TimeOnElementMax;
    }
}