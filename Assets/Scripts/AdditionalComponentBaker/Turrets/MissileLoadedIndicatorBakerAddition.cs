using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

class MissileLoadedIndicatorBakerAddition : AdditionalBakedBuffer<MissileLoadedIndicator>
{
    [SerializeField]
    private List<MissileLoadedIndicatorData> dataList;

    protected override List<MissileLoadedIndicator> GetBufferData<TAuthoring>(Unity.Entities.Baker<TAuthoring> baker)
    {
        return dataList
            .Select(x => new MissileLoadedIndicator()
            {
                Entity = baker.GetEntity(x.GameObject, Unity.Entities.TransformUsageFlags.Dynamic),
                LoadedPosition = x.LoadedPosition,
                UnloadedPosition = x.UnloadedPosition
            })
            .ToList();
    }
    
    [System.Serializable]
    private class MissileLoadedIndicatorData
    {
        public GameObject GameObject;
        public float3 LoadedPosition;
        public float3 UnloadedPosition;
    }

}
