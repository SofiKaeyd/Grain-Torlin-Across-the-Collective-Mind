using System.Collections;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    private static bool _isFrozen = false;

    public static void HitStop(float duration)
    {
        if (_isFrozen) return;

        // Находим любой объект, чтобы запустить на нем Корутину
        // (так как статический метод сам по себе не может ждать время)
        var instance = FindFirstObjectByType<EffectsManager>();
        instance.StartCoroutine(ProcessHitStop(duration));
    }

    private static IEnumerator ProcessHitStop(float duration)
    {
        _isFrozen = true;
        float originalTimeScale = Time.timeScale;

        Time.timeScale = 0f;
        // Используем WaitForSecondsRealtime, потому что обычный Wait не работает при TimeScale = 0
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        _isFrozen = false;
    }
}