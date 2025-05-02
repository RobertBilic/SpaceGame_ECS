using Unity.Entities;
using UnityEngine;

class TestEnemyPrefabAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public uint Seed;
}

class TestEnemyPrefabAuthoringBaker : Baker<TestEnemyPrefabAuthoring>
{
    public override void Bake(TestEnemyPrefabAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new TestEnemyPrefab() { Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic) });
        AddComponent(entity, new RandomGenerator() { Value = Unity.Mathematics.Random.CreateFromIndex(authoring.Seed) });
    }
}
