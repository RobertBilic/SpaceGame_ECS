using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
public partial struct ProjectilePrefabMapBakerSystem : ISystem
{
    private BlobAssetReference<ProjectilePrefabLookup> blobRef;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(state.EntityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new[] { ComponentType.ReadOnly(typeof(ProjectilePrefab)) },
            Options = EntityQueryOptions.IncludePrefab
        }));
    }

    public void OnUpdate(ref SystemState state)
    {
        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<ProjectilePrefabLookup>();

        var projectilePrefabs = new NativeList<ProjectilePrefab>(Allocator.Temp);

        foreach (var bullet in SystemAPI.Query<RefRO<ProjectilePrefab>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            projectilePrefabs.Add(bullet.ValueRO);
        }

        var array = builder.Allocate(ref root.Entries, projectilePrefabs.Length);
        for (int i = 0; i < projectilePrefabs.Length; i++)
            array[i] = projectilePrefabs[i];

        blobRef = builder.CreateBlobAssetReference<ProjectilePrefabLookup>(Allocator.Persistent);
        builder.Dispose();

        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(entity, new ProjectilePrefabLookupSingleton { Lookup = blobRef });
        state.Enabled = false;
    }

    public void OnDestroy(ref SystemState state)
    {
        blobRef.Dispose();
    }
}
