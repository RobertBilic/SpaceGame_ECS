using SpaceGame.Combat;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial struct TrailRendererCollectionSystem : ISystem
{
    private ComponentLookup<LocalToWorld> ltwLookup;
    private ComponentLookup<BoundingRadius> radiusLookup;
    private NativeHashSet<Entity> EntitiesInView;
    private CachedSpatialDatabaseRO CachedDB;

    private ComponentLookup<SpatialDatabase> dbLookup;
    private BufferLookup<SpatialDatabaseCell> cellLookup;
    private BufferLookup<SpatialDatabaseElement> elementLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialDatabaseSingleton>();
        state.RequireForUpdate<CameraData>();

        EntitiesInView = new NativeHashSet<Entity>(1024, Allocator.Persistent);
        ltwLookup = state.GetComponentLookup<LocalToWorld>(true);
        radiusLookup = state.GetComponentLookup<BoundingRadius>(true);

        dbLookup = state.GetComponentLookup<SpatialDatabase>(true);
        elementLookup = state.GetBufferLookup<SpatialDatabaseElement>(true);
        cellLookup = state.GetBufferLookup<SpatialDatabaseCell>(true);

        if (!SystemAPI.HasSingleton<TrailRendererRequest>())
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<TrailRendererRequest>(entity);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out var singleton))
            return;

        if (!SystemAPI.TryGetSingleton<CameraData>(out var cameraData))
            return;

        if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
            return;

        dbLookup.Update(ref state);
        elementLookup.Update(ref state);
        cellLookup.Update(ref state);

        CachedDB = new CachedSpatialDatabaseRO()
        {
            SpatialDatabaseEntity = singleton.TrailDatabase,
            CellsBufferLookup = cellLookup,
            SpatialDatabaseLookup = dbLookup,
            ElementsBufferLookup = elementLookup
        };

        CachedDB.CacheData();

        ltwLookup.Update(ref state);
        radiusLookup.Update(ref state);

        var buffer = SystemAPI.GetSingletonBuffer<TrailRendererRequest>();
        buffer.Clear();
        EntitiesInView.Clear();

        var camPos = cameraData.Position;
        camPos.z = 0.0f;
        float verticalHalfSize = cameraData.OrtographicSize;
        float horizontalHalfSize = verticalHalfSize * cameraData.Aspect;
        float camRange = math.sqrt(verticalHalfSize * verticalHalfSize + horizontalHalfSize * horizontalHalfSize);
        float3 halfExtents = new float3(horizontalHalfSize, verticalHalfSize, 1.0f); // Z is 0 for 2D

        var collector = new TrailCollector(ref EntitiesInView, state.EntityManager, camPos, camRange);
        SpatialDatabase.QueryAABB(CachedDB._SpatialDatabase, CachedDB._SpatialDatabaseCells, CachedDB._SpatialDatabaseElements, camPos, halfExtents, ref collector);
        
        foreach(var (ltw, trailRenderer, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<TrailRendererTag>>()
            .WithEntityAccess())
        {
            if (!EntitiesInView.Contains(entity))
                continue;

            buffer.Add(new TrailRendererRequest()
            {
                Entity = entity,
                Forward = ltw.ValueRO.Forward,
                Position = ltw.ValueRO.Position,
                TrailId = trailRenderer.ValueRO.Id,
            });
            
        }

        CachedDB.Dispose();
    } 

    public void OnDestroy(ref SystemState state)
    {
        EntitiesInView.Dispose();
    }


    private struct TrailCollector : ISpatialQueryCollector
    {
        public TrailCollector(ref NativeHashSet<Entity> collectedEnemies, EntityManager manager, float3 position, float range)
        {
            this.em = manager;
            this.myPosition = position;
            this.myRange = range;
            this.collectedEnemies = collectedEnemies;
        }

        public NativeHashSet<Entity> collectedEnemies;

        private EntityManager em;
        private float3 myPosition;
        private float myRange;

        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            if (cell.ElementsCount == 0)
                return;

            for (int i = cell.StartIndex; i < cell.StartIndex + cell.ElementsCount; i++)
            {
                var entity = elements[i].Entity;

                if (entity == Entity.Null || !em.Exists(entity))
                    continue;

                if (!em.HasComponent<LocalToWorld>(entity))
                    continue;

                var targetPosition = em.GetComponentData<LocalToWorld>(entity);

                float distanceSq = math.distancesq(myPosition, targetPosition.Position);

                if (distanceSq > myRange * myRange)
                    continue;

                if (!collectedEnemies.Contains(entity))
                    collectedEnemies.Add(entity);
            }
        }
    }
}
