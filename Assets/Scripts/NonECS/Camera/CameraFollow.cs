using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.Universal;

public class CameraFollow : MonoBehaviour
{
    public Entity targetEntity;
    private EntityManager em;
    private Camera cam;

    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 moveVelocity;
    private Vector3 manualPanPosition;

    [SerializeField]
    private float followSmoothTime = 0.3f;
    [SerializeField]
    private float zoomSpeed = 5f;
    [SerializeField]
    private float minZoom = 20f;
    [SerializeField]
    private float maxZoom = 50f;
    [SerializeField]
    private float panSpeed = 50.0f;
    [SerializeField]
    private float recentreDelay = 3f;

    private float currentZoom = 30f;
    private float timeSinceLastInput = 0f;
    private bool isRecentering = false;

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        cam = GetComponent<Camera>();

        if (!cam) Debug.LogError("Attach to Camera!");
        cam.orthographicSize = currentZoom;

        manualPanPosition = transform.position;
        timeSinceLastInput = float.PositiveInfinity;
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandleManualPan();

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

            if (query.CalculateEntityCount() == 1)
                targetEntity = query.GetSingletonEntity();
        }

        if (em.HasComponent<LocalTransform>(targetEntity))
        {
            var shipTransform = em.GetComponentData<LocalTransform>(targetEntity);
            Vector3 targetPos = (Vector3)shipTransform.Position + offset.normalized;
            targetPos.z = -20f;

            if (isRecentering)
            {
                manualPanPosition = Vector3.SmoothDamp(
                    manualPanPosition,
                    targetPos,
                    ref moveVelocity,
                    followSmoothTime
                );

                if (Vector3.Distance(manualPanPosition, targetPos) < 0.1f)
                    isRecentering = false;
            }
        }

        transform.position = manualPanPosition;
    }

    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, currentZoom, Time.deltaTime * 10f);

            if(cam.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                foreach (var overlayCamera in cameraData.cameraStack)
                    overlayCamera.orthographicSize = cam.orthographicSize;
            }
        }
    }

    private void HandleManualPan()
    {
        Vector2 input = new Vector2(
            Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0,
            Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0
        );

        if (input != Vector2.zero)
        {
            manualPanPosition += (Vector3)(input * panSpeed * Time.deltaTime);
            timeSinceLastInput = 0f;
            isRecentering = false;
        }
        else
        {
            timeSinceLastInput += Time.deltaTime;
            if (timeSinceLastInput > recentreDelay)
                isRecentering = true;
        }
    }
}
