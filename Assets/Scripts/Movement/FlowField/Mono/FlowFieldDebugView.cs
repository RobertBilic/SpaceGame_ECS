using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FlowFieldDebugView : MonoBehaviour
{
    public Color arrowColor = Color.cyan;
    public float arrowLength = 2f;

    private EntityManager em;
    private Entity flowFieldEntity;
    private DynamicBuffer<FlowFieldCell> flowBuffer;
    private FlowFieldSettings flowSettings;

    private bool ready = false;

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!ready)
        {
            var query = em.CreateEntityQuery(typeof(FlowFieldSettings), typeof(FlowFieldCell));
            if (query.CalculateEntityCount() == 1)
            {
                flowFieldEntity = query.GetSingletonEntity();
                flowSettings = em.GetComponentData<FlowFieldSettings>(flowFieldEntity);
                flowBuffer = em.GetBuffer<FlowFieldCell>(flowFieldEntity);
                ready = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (flowFieldEntity == Entity.Null)
            return;

        if (!em.Exists(flowFieldEntity))
            return;

        var flowSettings = em.GetComponentData<FlowFieldSettings>(flowFieldEntity);
        var flowBuffer = em.GetBuffer<FlowFieldCell>(flowFieldEntity);

        int gridX = (int)math.ceil(flowSettings.WorldSize.x / flowSettings.CellSize);
        int gridY = (int)math.ceil(flowSettings.WorldSize.y / flowSettings.CellSize);

        Gizmos.color = arrowColor;

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                int index = y * gridX + x;
                if (index >= flowBuffer.Length)
                    continue;

                var cell = flowBuffer[index];

                float3 worldPos = new float3(
                    (x + 0.5f) * flowSettings.CellSize - flowSettings.WorldSize.x * 0.5f,
                    (y + 0.5f) * flowSettings.CellSize - flowSettings.WorldSize.y * 0.5f,
                    0.0f
                );

                float3 dir = new float3(cell.Direction.x,  cell.Direction.y, 0) * arrowLength;

                Gizmos.DrawRay(worldPos, dir);
                DrawArrowHead(worldPos + dir, dir);
            }
        }
    }


    private void DrawArrowHead(Vector3 pos, Vector3 direction)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
        Gizmos.DrawRay(pos, right * 0.3f);
        Gizmos.DrawRay(pos, left * 0.3f);
    }
}
