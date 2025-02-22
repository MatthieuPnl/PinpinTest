using DG.Tweening;
using TMPro;
using UnityEngine;

public class AnimatedIncrement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text = null;
    public TextMeshProUGUI Text => text;

    private int currentShownValue = 0;
    private Tween currentTween = null;

    public void UpdateValue(int newValue)
    {
        if (newValue == currentShownValue)
            return;

        currentTween?.Kill(true);
        currentTween = DOVirtual.Int(currentShownValue, newValue, 0.5f, (x) =>
        {
            text.text = x.ToString();
        }).OnComplete(() => currentShownValue = newValue);

        DOTween.Kill(text.transform, true);
        //Punch up or down depending on new value
        text.transform.DOPunchPosition(new Vector3(0, newValue > currentShownValue ? 10f : -10f, 0), 0.5f, 10, 0);
    }
}
