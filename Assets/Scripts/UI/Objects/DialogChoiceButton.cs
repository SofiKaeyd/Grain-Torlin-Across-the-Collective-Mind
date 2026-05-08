using TMPro;
using UnityEngine;

public class DialogChoiceButton : BaseButton
{
    [SerializeField] private TextMeshProUGUI _text;

    public DialogNode Node { get; set; }

    public void SetText(string text)
    {
        _text.text = text;
    }

    public override void OnClick()
    {
        base.OnClick();
    }
}
