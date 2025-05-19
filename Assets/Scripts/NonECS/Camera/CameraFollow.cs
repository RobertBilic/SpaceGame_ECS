using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CameraFollow : MonoBehaviour
{
    public Entity targetEntity;
    private EntityManager em;
    private Camera cam;

    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 moveVelocity;
    private Vector3 manualPanPosition;

    [SerializeField] private float followSmoothTime = 0.3f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 50.0f;
    [SerializeField] private float maxZoom = 100.0f;
    [SerializeField] private float panSpeed = 50.0f;
    [SerializeField] private float recentreDelay = 3f;

    private float currentZoom = 50.0f;
    private float timeSinceLastInput = 0f;
    private bool isRecentering = false;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        cam = GetComponent<Camera>();
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        manualPanPosition = transform.position;
        timeSinceLastInput = float.PositiveInfinity;
    }

    private void OnDestroy()
    {
        inputActions.Disable();
    }

    private void LateUpdate()
    {
        HandleZoom();
        HandleManualPan();
        UpdateTargetEntity();
        FollowTarget();
    }

    private void HandleZoom()
    {
        float scroll = inputActions.Camera.Zoom.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
        }

        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, currentZoom, Time.deltaTime * 10f);

            if (cam.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                foreach (var overlayCamera in cameraData.cameraStack)
                {
                    overlayCamera.orthographicSize = cam.orthographicSize;
                }
            }
        }
    }

    private void HandleManualPan()
    {
        Vector2 input = inputActions.Camera.Pan.ReadValue<Vector2>();

        if (input.sqrMagnitude > 0.01f)
        {
            manualPanPosition += new Vector3(input.x, input.y, 0f) * panSpeed * Time.deltaTime;
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

    private void UpdateTargetEntity()
    {
        if (targetEntity != Entity.Null && em.Exists(targetEntity))
            return;

        EntityQuery query = em.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<MoveSpeed>(),
                ComponentType.ReadOnly<CapitalShipTag>()
            }
        });

        if (query.CalculateEntityCount() == 1)
            targetEntity = query.GetSingletonEntity();
    }

    private void FollowTarget()
    {
        if (!em.HasComponent<LocalTransform>(targetEntity))
            return;

        var shipTransform = em.GetComponentData<LocalTransform>(targetEntity);
        Vector3 targetPos = (Vector3)shipTransform.Position + offset.normalized;
        targetPos.z = -20f;

        if (isRecentering)
        {
            manualPanPosition = Vector3.SmoothDamp(manualPanPosition, targetPos, ref moveVelocity, followSmoothTime);
            if (Vector3.Distance(manualPanPosition, targetPos) < 0.1f)
                isRecentering = false;
        }

        transform.position = manualPanPosition;
    }
}
