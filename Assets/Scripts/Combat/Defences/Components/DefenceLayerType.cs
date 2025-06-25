using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Defences
{
    public enum DefenceLayerType : byte
    {
        Shield,
        Armor,
        Hull
    }

    public static class DefenceLayerTypeUtility
    {
        public static NativeList<DefenceLayerType> GetOrderedDefenceLayerList(Allocator allocator = Allocator.Persistent)
        {
            return new NativeList<DefenceLayerType>(allocator)
            {
                DefenceLayerType.Shield,
                DefenceLayerType.Armor,
                DefenceLayerType.Hull
            };
        }
    }
}
