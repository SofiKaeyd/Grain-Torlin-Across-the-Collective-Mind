using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private Canvas _canvasWindow;
    [SerializeField] private Canvas _canvasPopup;
    [SerializeField] private Transform _windowParent;
    [SerializeField] private Transform _popupParent;

    private static WindowManager _instance;
    private BaseWindow _currentWindow;
    private Stack<BaseWindow> _windowsStack = new Stack<BaseWindow>();

    public void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            //_canvasWindow.renderMode = RenderMode.ScreenSpaceCamera;
            //_canvasPopup.renderMode = RenderMode.ScreenSpaceCamera;
            //_canvasWindow.worldCamera = Camera.main;
            //_canvasPopup.worldCamera = Camera.main;
            //SceneManager.sceneLoaded += OnSceneLoaded;

        }
        else
            Destroy(gameObject);

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _canvasWindow.worldCamera = Camera.main;
        _canvasPopup.worldCamera = Camera.main;
    }

    public void Start()
    {
        //Open<MainMenuWindow>();

        if (!PlayerPrefs.HasKey("FirstLaunch"))
        {
            //Open<UserNamePopup>();
            PlayerPrefs.SetInt("FirstLaunch", 1);
        }
    }

    public static void Open<T>() where T : BaseWindow
    {
        _instance.OpenInternal<T>();
    }

    public static async UniTask ClosePopup()
    {
        await _instance.ClosePopupInternal();
    }

    public static async UniTask Close()
    {
        await _instance.CloseInternal();
    }

    private async void OpenInternal<T>() where T : BaseWindow
    {
        var window = Pool.Get<T>(_canvasWindow.transform);

        window.transform.SetParent(window.IsPopup ? _popupParent : _windowParent, false);
        Physics2D.SyncTransforms();

        if (window.IsPopup)
            ClosePopupToOpenInternal();
        else
            CloseToOpenInternal();

        window.Open();
        _windowsStack.Push(window);
        _currentWindow = window;
    }

    private async UniTask ClosePopupToOpenInternal()
    {
        if (_windowsStack.Count > 1 && _currentWindow.IsPopup)
            await _currentWindow.Close();
    }

    private async UniTask CloseToOpenInternal()
    {
        CloseAllPopups();
        if (_windowsStack.Count > 0)
            await _currentWindow.Close();
    }

    private async UniTask ClosePopupInternal()
    {
        if (_windowsStack.Count < 2)
            return;

        if (_currentWindow.IsPopup)
        {
            await _windowsStack.Pop().Close();
            _currentWindow = _windowsStack.Peek();
        }

        if (_currentWindow.IsPopup)
            _currentWindow.Open();
    }

    private void CloseAllPopups()
    {
        while (_windowsStack.Count > 0 && _currentWindow.IsPopup)
        {
            _windowsStack.Pop().CloseForce();
            _currentWindow = _windowsStack.Peek();
        }
    }

    private async UniTask CloseInternal()
    {
        CloseAllPopups();

        if (_windowsStack.Count < 2)
            return;

        await _windowsStack.Pop().Close();
        _currentWindow = _windowsStack.Peek();
        _currentWindow.Open();
    }
}
