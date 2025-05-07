using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
public partial struct BulletPrefabMapBakerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new[] { ComponentType.ReadOnly(typeof(BulletPrefab)) },
            Options = EntityQueryOptions.IncludePrefab
        }));
    }

    public void OnUpdate(ref SystemState state)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<BulletPrefabLookup>();

        var bulletPrefabs = new NativeList<BulletPrefab>(Allocator.Temp);

        foreach (var bullet in SystemAPI.Query<RefRO<BulletPrefab>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            bulletPrefabs.Add(bullet.ValueRO);
        }

        var array = builder.Allocate(ref root.Entries, bulletPrefabs.Length);
        for (int i = 0; i < bulletPrefabs.Length; i++)
            array[i] = bulletPrefabs[i];

        var blobRef = builder.CreateBlobAssetReference<BulletPrefabLookup>(Allocator.Persistent);
        builder.Dispose();

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new BulletPrefabLookupSingleton { Lookup = blobRef });
        state.Enabled = false;
    }
}
