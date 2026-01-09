using UnityEngine;

public interface IClickable
{
    public Collider2D Collider { get; }
    public void OnClick();

    public void TryClick(Collider2D collider)
    {
        if (Collider == collider)
            OnClick();
    }
}

