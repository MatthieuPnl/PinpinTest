using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChipButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int index = 0;
    private bool selected = false;

    public void OnClick()
    {
        GameManager.GetInstance().SetSelectedChip(index);
    }

    private void Scale(float factor)
    {
        transform.DOKill(true);
        transform.DOScale(new Vector3(factor, factor, factor), 0.2f);
    }

    public void OnSelected()
    {
        selected = true;
        Scale(1.2f);
    }

    public void OnUnselected()
    {
        selected = false;
        Scale(0.8f);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selected)
            Scale(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected)
            Scale(0.8f);
    }
    
}
