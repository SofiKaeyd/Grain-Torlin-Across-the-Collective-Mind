using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private static MouseManager _instance;
    private List<IClickable> _clickables = new List<IClickable>();
    private Camera _cachedCamera;

    public static Camera ActiveCamera => _instance._cachedCamera;
    public static Action<Collider2D> OnClick { get; set; }

    void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            UpdateCameraCache();
        }
        else
            Destroy(gameObject);

        CinemachineCore.CameraActivatedEvent.AddListener(OnCameraActivated);
    }

    private void OnCameraActivated(ICinemachineCamera.ActivationEventParams arg0)
    {
        UpdateCameraCache();
    }

    private void UpdateCameraCache()
    {
        var brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
        if (brain != null && brain.OutputCamera != null)
        {
            _cachedCamera = brain.OutputCamera;
            return;
        }

        _cachedCamera = Camera.main;

        if (_cachedCamera == null)
            _cachedCamera = FindAnyObjectByType<Camera>();
    }

    public static void AddClickable(IClickable clickable)
    {
        if (!_instance._clickables.Contains(clickable))
        {
            _instance._clickables.Add(clickable);
            OnClick += clickable.TryClick;
        }
    }

    public static void RemoveClickable(IClickable clickable)
    {
        OnClick -= clickable.TryClick;
        _instance._clickables.Remove(clickable);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_cachedCamera == null)
                UpdateCameraCache();
            if (_cachedCamera == null)
            {
                Debug.LogWarning("[MouseManager] No camera found in scene. Click ignored.");
                return;
            }

            var mousePosition = _cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            Debug.Log(hit.collider);
            OnClick?.Invoke(hit.collider);
        }
    }

    private void OnDestroy()
    {
        CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraActivated);
    }
}
