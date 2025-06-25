using Unity.Entities;

namespace SpaceGame.Combat.Defences
{
    public struct ResistanceEntry : IBufferElementData
    {
        public DefenceLayerType Layer;
        public DamageType Type;
        public float Resistance; // 0 = full dmg, 1 = immune
    }
}
