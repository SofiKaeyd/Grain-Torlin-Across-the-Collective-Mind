using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseWindow : MonoBehaviour
{
    [SerializeField] private bool _isPopup;

    public bool IsPopup => _isPopup;

    public void Open()
    {
        gameObject.SetActive(true);
        OnOpen();
    }

    public async UniTask Close()
    {
        await OnClose();
        Pool.Release(this);
    }

    public void CloseForce()
    {
        OnClose();
        Pool.Release(this);
    }

    public virtual async UniTask OnOpen()
    {

    }
    public virtual async UniTask OnClose()
    {

    }

    //public void SetupCameraForCanvas()
    //{
    //    var camera = GetMainCamera();
    //    if (camera == null) return;

    //    var canvases = GetComponentsInChildren<Canvas>();
    //    foreach (var c in canvases)
    //    {
    //        if (c.renderMode == RenderMode.ScreenSpaceCamera)
    //            c.worldCamera = camera;
    //    }
    //}

    //private static Camera GetMainCamera()
    //{
    //    var brain = FindFirstObjectByType<Unity.Cinemachine.CinemachineBrain>();
    //    if (brain?.OutputCamera != null)
    //        return brain.OutputCamera;
    //    return Camera.main;
    //}
}
