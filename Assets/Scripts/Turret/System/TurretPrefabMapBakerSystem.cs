using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
public partial struct TurretPrefabMapBakerSystem : ISystem
{
    private BlobAssetReference<TurretPrefabLookup> blobRef;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new[] { ComponentType.ReadOnly(typeof(TurretPrefab)) },
            Options = EntityQueryOptions.IncludePrefab
        }));
    }

    public void OnUpdate(ref SystemState state)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<TurretPrefabLookup>();

        var turretPrefabs = new NativeList<TurretPrefab>(Allocator.Temp);

        foreach (var turret in SystemAPI.Query<RefRO<TurretPrefab>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            turretPrefabs.Add(turret.ValueRO);
        }

        var array = builder.Allocate(ref root.Entries, turretPrefabs.Length);
        for (int i = 0; i < turretPrefabs.Length; i++)
            array[i] = turretPrefabs[i];

        blobRef = builder.CreateBlobAssetReference<TurretPrefabLookup>(Allocator.Persistent);
        builder.Dispose();

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new TurretPrefabLookupSingleton { Lookup = blobRef });
        state.Enabled = false;
    }

    public void OnDestroy(ref SystemState state)
    {
        blobRef.Dispose();
    }
}
