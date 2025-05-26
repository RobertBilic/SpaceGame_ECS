using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BackgroundDustLayerManager : MonoBehaviour
{
    [SerializeField]
    private Vector2 speed;
    [SerializeField]
    private List<BackgroundDustLayer> dustLayers;
    private Entity sceneMovementDataEntity;

    void Update()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        if (!World.DefaultGameObjectInjectionWorld.IsCreated)
            return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (sceneMovementDataEntity == Entity.Null)
        {
            var query = entityManager.CreateEntityQuery(typeof(SceneMovementData));
            if (!query.IsEmpty)
            {
                sceneMovementDataEntity = query.GetSingletonEntity();
                var movement = entityManager.GetComponentData<SceneMovementData>(sceneMovementDataEntity);
            }
        }

        if (!entityManager.Exists(sceneMovementDataEntity))
        {
            ApplySpeed(speed);
            return;
        }

        if (entityManager.HasComponent<SceneMovementData>(sceneMovementDataEntity))
        {
            var movement = entityManager.GetComponentData<SceneMovementData>(sceneMovementDataEntity);
            ApplySpeed(movement.Value);
        }
        else
        {
            ApplySpeed(speed);
        }
    }

    private void ApplySpeed(Vector2 speed)
    {
        foreach (var dustLayer in dustLayers)
            dustLayer.ApplyChanges(speed);
    }
}
