using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHUD : BaseWindow
{
    [SerializeField] private PlayerHealthUI _healthUI;

    public override async UniTask OnOpen()
    {
        Player.OnHealthChanged += UpdateHealth;
        UpdateHealth(Player.Instance.Health);
    }

    public override async UniTask OnClose()
    {
        Player.OnHealthChanged -= UpdateHealth;
    }

    private void UpdateHealth(int health)
    {
        _healthUI.SetHealth(health);
    }
}
