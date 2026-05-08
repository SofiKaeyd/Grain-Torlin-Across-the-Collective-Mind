using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SceneAutoLoader
{
    static SceneAutoLoader()
    {
        // Указываем путь к вашей сцене меню
        // Замените "Assets/Scenes/Menu.unity" на ваш путь
        EditorSceneManager.playModeStartScene =
            AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu");
    }
}