using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class TrailRendererManager : MonoBehaviour
{
    [SerializeField]
    private TrailPrefabHolder prefabHolder;
    private const int framesUnseenBeforeDeactivation = 15;

    private EntityManager em;
    private Entity trailRequestEntity;

    private Dictionary<Entity, TrailHolder> trailMap = new();
    private Dictionary<FixedString32Bytes, TrailRenderer> prefabMap;
    private HashSet<Entity> seen;
    private Transform trailHolder;

    private void Start()
    {
        seen = new HashSet<Entity>();
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        trailHolder = new GameObject("Trail Holder").transform;

        prefabMap = new Dictionary<FixedString32Bytes, TrailRenderer>();
        foreach (var prefab in prefabHolder.Prefabs)
            prefabMap.Add(prefab.Id, prefab.Renderer);
    }

    void LateUpdate()
    {
        if (trailRequestEntity == Entity.Null || !em.Exists(trailRequestEntity))
        {
            TryGetTrailBufferEntity();
            return;
        }

        var buffer = GetTrailRendererBuffer();
        seen.Clear();

        foreach (var req in buffer)
        {
            if (!prefabMap.ContainsKey(req.TrailId))
                continue;

            var trailPrefab = prefabMap[req.TrailId];
            seen.Add(req.Entity);
            if (!trailMap.TryGetValue(req.Entity, out var trailHolder))
            {
                trailHolder = new TrailHolder()
                {
                    FramesNotSeen = 0,
                    RootObj = Instantiate(trailPrefab.gameObject, this.trailHolder, true)
                };

                trailMap[req.Entity] = trailHolder;
            }
            trailHolder.FramesNotSeen = 0;
            var go = trailHolder.RootObj;

            if (!go.activeSelf)
                go.SetActive(true);
            go.transform.position = req.Position;
            go.transform.forward = req.Forward;
        }

        foreach (var entity in trailMap.Keys.Except(seen).ToList())
        {
            trailMap[entity].FramesNotSeen++;

            if (trailMap[entity].FramesNotSeen >= framesUnseenBeforeDeactivation)
                trailMap[entity].RootObj.SetActive(false);
        }
    }
    private void TryGetTrailBufferEntity()
    {
        var query = em.CreateEntityQuery(ComponentType.ReadOnly<TrailRendererRequest>());
        if (!query.IsEmpty)
            trailRequestEntity = query.GetSingletonEntity();
    }

    private DynamicBuffer<TrailRendererRequest> GetTrailRendererBuffer()
    {
        if (em.Exists(trailRequestEntity))
            return em.GetBuffer<TrailRendererRequest>(trailRequestEntity);

        return default;
    }

    private class TrailHolder
    {
        public GameObject RootObj;
        public int FramesNotSeen;
    }
}
