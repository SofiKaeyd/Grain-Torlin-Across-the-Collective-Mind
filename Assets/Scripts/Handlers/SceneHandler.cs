using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    private static SceneHandler _instance { get; set; }

    public static string MainMenu => "MainMenu";
    public static string Level_1 => "Level_1";

    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public static async UniTask LoadScene(string sceneName)
    {
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    public static void QuitProgram()
    {
        Application.Quit();
    }
}
