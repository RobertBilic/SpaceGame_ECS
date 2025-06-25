using SpaceGame.Combat.Defences;
using SpaceGame.Game.Initialization.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Game.Initialization.Authoring
{
    [System.Serializable]
    class HealthBarColoringPerLayerData
    {
        public Color Color;
        public DefenceLayerType Layer;
        public int Team;
    }

    class ConfigAuthoring : MonoBehaviour
    {
        public int GameSize;
        public int TeamCount;
        [Header("Health Bar")]
        public List<HealthBarColoringPerLayerData> HealthBarSettings;
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

            var colorBuffer = AddBuffer<HealthBarColorPerDefenceLayer>(entity);

            foreach (var data in authoring.HealthBarSettings)
                colorBuffer.Add(new HealthBarColorPerDefenceLayer()
                {
                    Color = new Unity.Mathematics.float4(data.Color.r, data.Color.g, data.Color.b, data.Color.a),
                    Layer = data.Layer,
                    Team = data.Team
                });

            if (authoring.EnableDynamicFleets)
                AddComponent(entity, new EnableDynamicFleets());
        }
    }
}