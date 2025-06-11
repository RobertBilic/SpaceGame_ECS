using System;
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

    public bool Enabled = true;
    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 moveVelocity;
    private Vector3 manualPanPosition;
    private Vector3 targetPanPosition;
    private Vector3 targetPosition;

    [SerializeField] private float panSmoothTime = 0.1f;
    [SerializeField] private float followSmoothTime = 0.3f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 50.0f;
    [SerializeField] private float maxZoom = 100.0f;
    [SerializeField] private float panSpeed = 50.0f;
    [SerializeField] private float recentreDelay = 3f;
    [SerializeField] private float touchScreenPanMultiplier = 0.1f;

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
        if (!Enabled)
            return;

        HandleZoom();
        HandleManualPan();
        UpdateTargetEntity();
        FollowTarget();
    }

    private void HandleZoom()
    {
        HandleMouseZoom();
        HandleTouchZoom();

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

    private void HandleTouchZoom()
    {
        if (Touchscreen.current == null || Touchscreen.current.touches.Count < 2)
            return;

        var touch0 = Touchscreen.current.touches[0];
        var touch1 = Touchscreen.current.touches[1];

        if (!touch0.press.isPressed || !touch1.press.isPressed)
            return;

        Vector2 touch0PrevPos = touch0.position.ReadValue() - touch0.delta.ReadValue();
        Vector2 touch1PrevPos = touch1.position.ReadValue() - touch1.delta.ReadValue();

        float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
        float currentMagnitude = (touch0.position.ReadValue() - touch1.position.ReadValue()).magnitude;

        float difference = currentMagnitude - prevMagnitude;

        currentZoom = Mathf.Clamp(currentZoom - difference * 0.1f, minZoom, maxZoom);
    }

    private void HandleMouseZoom()
    {
        float scroll = inputActions.Camera.Zoom.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.01f)
            currentZoom = Mathf.Clamp(currentZoom - scroll * zoomSpeed, minZoom, maxZoom);
    }

    private Vector2 HandleTouchPan()
    {
        if (Touchscreen.current == null)
            return Vector2.zero;

        var activeTouches = 0;

        for (int i = 0; i < Touchscreen.current.touches.Count; i++)
        {
            if (Touchscreen.current.touches[i].press.isPressed)
                activeTouches++;
        }

        if (activeTouches != 1)
            return Vector2.zero;

        var touch = Touchscreen.current.touches[0];

        return -touch.delta.ReadValue() * touchScreenPanMultiplier;
    }

    private void HandleManualPan()
    {
        Vector2 input = inputActions.Camera.Pan.ReadValue<Vector2>();

        if (input.sqrMagnitude < 0.01f)
            input = HandleTouchPan();

        if (input.sqrMagnitude > 0.01f)
        {
            targetPanPosition += new Vector3(input.x, input.y, 0f) * panSpeed * Time.deltaTime;
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
                ComponentType.ReadOnly<CameraFollowTag>()
            }
        });

        if (query.CalculateEntityCount() == 1)
            targetEntity = query.GetSingletonEntity();
    }

    private void FollowTarget()
    {
        bool canRecenter = false;
        if (em.HasComponent<LocalTransform>(targetEntity))
        {
            var shipTransform = em.GetComponentData<LocalTransform>(targetEntity);
            targetPosition = shipTransform.Position;
            canRecenter = true;
        }

        Vector3 targetPos = targetPosition + offset.normalized;
        targetPos.z = -20f;

        if (canRecenter && isRecentering)
        {
            manualPanPosition = targetPanPosition = Vector3.SmoothDamp(manualPanPosition, targetPos, ref moveVelocity, followSmoothTime);
            if (Vector3.Distance(manualPanPosition, targetPos) < 0.1f)
                isRecentering = false;
        }
        else
        {
            manualPanPosition = Vector3.SmoothDamp(manualPanPosition, targetPanPosition, ref moveVelocity, panSmoothTime);
        }

        transform.position = manualPanPosition;
    }
}
