using SpaceGame.Game.Initialization.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Game.Initialization.Authoring
{
    [System.Serializable]
    class HealthBarColorPerTeamData
    {
        public Color color;
        public int Team;
    }

    class ConfigAuthoring : MonoBehaviour
    {
        public int GameSize;
        [Header("Health Bar")]
        public List<HealthBarColorPerTeamData> HealthBarSettings;
        [Header("Spatial Database Settings")]
        public GameObject SpatialDatabasePrefab;
        public int SpatialDatabaseSubdivisions = 5;
        public int ShipsSpatialDatabaseCellCapacity = 256;
    }

    class ConfigAuthoringBaker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Config()
            {
                SpatialDatabasePrefab = GetEntity(authoring.SpatialDatabasePrefab, TransformUsageFlags.None),
                ShipsSpatialDatabaseCellCapacity = authoring.ShipsSpatialDatabaseCellCapacity,
                SpatialDatabaseSubdivisions = authoring.SpatialDatabaseSubdivisions,
                GameSize = authoring.GameSize,
                IsInitialized = false
            });

            var colorBuffer = AddBuffer<HealthBarColorPerTeam>(entity);

            foreach (var data in authoring.HealthBarSettings)
                colorBuffer.Add(new HealthBarColorPerTeam()
                {
                    Color = new Unity.Mathematics.float4(data.color.r, data.color.g, data.color.b, data.color.a),
                    Team = data.Team
                });
        }
    }
}