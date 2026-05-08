using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image[] _healthUI;

    public void SetHealth(int value)
    {
        for (int i = 0; i < _healthUI.Length; i++)
            _healthUI[i].enabled = i < value;
    }

}