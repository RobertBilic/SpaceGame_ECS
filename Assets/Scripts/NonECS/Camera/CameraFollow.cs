using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CameraFollow : MonoBehaviour
{
    public Entity targetEntity;
    private EntityManager em;
    private Camera cam; 

    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 moveVelocity;

    [SerializeField]
    private float followSmoothTime = 0.3f;
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float minZoom = 20f;
    [SerializeField]
    private float maxZoom = 50f;

    private float currentZoom = 30f;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        cam = GetComponent<Camera>(); 
        if (cam == null)
        {
            Debug.LogError("CameraFollow script must be attached to a Camera GameObject!");
        }

        if (!cam.orthographic)
        {
            Debug.LogWarning("Camera is not orthographic! Zoom will still affect orthographicSize but perspective cameras behave differently.");
        }

        cam.orthographicSize = currentZoom; 
    }

    private void LateUpdate()
    {
        if (targetEntity == Entity.Null || !em.Exists(targetEntity))
        {
            EntityQuery query = em.CreateEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {
                ComponentType.ReadOnly<MoveSpeed>(),
                ComponentType.ReadOnly<CapitalShipTag>()
                },
                Options = EntityQueryOptions.Default
            });

            if(query.CalculateEntityCount() == 1)
            {
                targetEntity = query.GetSingletonEntity();
            }
        }

        if (!em.HasComponent<LocalTransform>(targetEntity))
            return;

        var shipTransform = em.GetComponentData<LocalTransform>(targetEntity);

        HandleZoom();

        Vector3 targetPosition = shipTransform.Position;
        Vector3 desiredPosition = targetPosition + offset.normalized;
        desiredPosition.z = -20.0f;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref moveVelocity,
            followSmoothTime
        );
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f) 
        {
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
        }

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, currentZoom, Time.deltaTime * 10f); 
        }
    }
}
