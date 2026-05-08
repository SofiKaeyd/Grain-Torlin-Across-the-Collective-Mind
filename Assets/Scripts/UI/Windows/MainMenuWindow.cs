using UnityEngine;
using UnityEngine.UI;

public class MainMenuWindow : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _authorsButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayClick);
        _settingsButton.onClick.AddListener(OnSettingsClick);
        _authorsButton.onClick.AddListener(OnAuthorsClick);
        _quitButton.onClick.AddListener(OnQuitButton);
    }

    private async void OnPlayClick()
    {
        Debug.Log("OnPlayClick");
        await SceneHandler.LoadScene(SceneHandler.Level_1);
        //LevelManager.LoadLevel(LevelManager.LastId);
        WindowManager.Open<PlayerHUD>();
        //WindowManager.Open<DialogWindow>();
    }

    private void OnSettingsClick()
    {
        Debug.Log("OnSettingsClick");
        //WindowManager.Open<SettingsPopup>();
    }

    private void OnAuthorsClick()
    {
        Debug.Log("OnAuthorsClick");
        //WindowManager.Open<AuthorsPopup>();
    }

    private void OnQuitButton()
    {
        Debug.Log("OnQuitButton");
        SceneHandler.QuitProgram();
    }    
}
