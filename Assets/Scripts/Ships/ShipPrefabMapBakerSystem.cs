using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
public partial struct ShipPrefabMapBakerSystem : ISystem
{
    private BlobAssetReference<ShipPrefabLookup> blobRef;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new[] { ComponentType.ReadOnly(typeof(ShipPrefab)) },
            Options = EntityQueryOptions.IncludePrefab
        }));
    }

    public void OnUpdate(ref SystemState state)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<ShipPrefabLookup>();

        var shipPrefabs = new NativeList<ShipPrefab>(Allocator.Temp);

        foreach (var ship in SystemAPI.Query<RefRO<ShipPrefab>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            shipPrefabs.Add(ship.ValueRO);
        }

        var array = builder.Allocate(ref root.Entries, shipPrefabs.Length);
        for (int i = 0; i < shipPrefabs.Length; i++)
            array[i] = shipPrefabs[i];

        blobRef = builder.CreateBlobAssetReference<ShipPrefabLookup>(Allocator.Persistent);
        builder.Dispose();

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new ShipPrefabLookupSingleton { Lookup = blobRef });
        state.Enabled = false;
    }
    
    public void OnDestroy(ref SystemState state)
    {
        blobRef.Dispose();
    }
}
