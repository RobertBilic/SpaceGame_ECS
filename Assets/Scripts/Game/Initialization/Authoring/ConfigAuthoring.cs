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
        public int TeamCount;
        [Header("Health Bar")]
        public List<HealthBarColorPerTeamData> HealthBarSettings;
        [Header("Spatial Database Settings")]
        public GameObject SpatialDatabasePrefab;
        public int SpatialDatabaseSubdivisions = 5;
        public int ShipsSpatialDatabaseCellCapacity = 256;
        [Header("HitEffect")]
        public bool OnHitEffectsEnabled;
        [Header("Fleets")]
        [Tooltip("Enables fleets to be created dynamically")]
        public bool EnableDynamicFleets;
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
                IsInitialized = false,
                TeamCount = authoring.TeamCount
            });

            AddComponent(entity, new HitEffectEnabled() { Enabled = authoring.OnHitEffectsEnabled });

            var colorBuffer = AddBuffer<HealthBarColorPerTeam>(entity);

            foreach (var data in authoring.HealthBarSettings)
                colorBuffer.Add(new HealthBarColorPerTeam()
                {
                    Color = new Unity.Mathematics.float4(data.color.r, data.color.g, data.color.b, data.color.a),
                    Team = data.Team
                });

            if (authoring.EnableDynamicFleets)
                AddComponent(entity, new EnableDynamicFleets());
        }
    }
}