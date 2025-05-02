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
    private Entity capitalShipEntity = Entity.Null;

    void Update()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        if (!World.DefaultGameObjectInjectionWorld.IsCreated)
            return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (capitalShipEntity == Entity.Null)
        {
            var query = entityManager.CreateEntityQuery(typeof(CapitalShipTag), typeof(SceneMovementData));
            if (!query.IsEmpty)
            {
                capitalShipEntity = query.GetSingletonEntity();
                var movement = entityManager.GetComponentData<SceneMovementData>(capitalShipEntity);
            }
        }

        if (!entityManager.Exists(capitalShipEntity))
            return;

        if (entityManager.HasComponent<SceneMovementData>(capitalShipEntity))
        {
            var movement = entityManager.GetComponentData<SceneMovementData>(capitalShipEntity);

            //TODO: change the speed to the scene movement data
            foreach (var dustLayer in dustLayers)
                dustLayer.ApplyChanges(speed);
        }
    }
}
