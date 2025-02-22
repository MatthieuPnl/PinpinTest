using DG.Tweening;
using EditorCools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ChipsPile : MonoBehaviour
{
    [SerializeField] private TableSlot slot = null;

    [SerializeField] private float animationSpeed = 0.5f;
    [SerializeField] private TextMeshPro chipsText = null;
    [SerializeField] private Transform chipsParent = null;

    private List<Transform> chips = new List<Transform>();
    private bool hovering = false;

    public void AddChip(Transform chip, bool anim)
    {
        Transform newChip = GameObject.Instantiate(chip, chipsParent);
        Vector3 chipSize = chip.GetComponentInChildren<Renderer>().bounds.size;
        newChip.localPosition = chipsParent.transform.localPosition + new Vector3(0, chipSize.y * chips.Count, 0);
        newChip.rotation *= Quaternion.Euler(0, 0, Random.Range(0, 360));
        chipsText.text = slot.Bet > 0 ? slot.Bet + "" : "";

        chips.Add(newChip);
        
        if (anim)
        {
            newChip.DOLocalMove(chipsParent.InverseTransformPoint(GameManager.GetInstance().HandLocation.position), animationSpeed).From();
            Sequence s = DOTween.Sequence();
            s.AppendInterval(0.2f).Append(newChip.DORotateQuaternion(newChip.rotation * Quaternion.Euler(new Vector3(Random.Range(-45, 45), 0, 0)), 0.3f).From());

            if (DOTween.TweensByTarget(chipsText.transform) != null)
                DOTween.Kill(chipsText.transform, true);
            chipsText.transform.DOPunchScale(new Vector3(0.03f, 0.03f, 0.03f), 0.5f, 0);
        }
        
    }

    public void RemoveLastChip()
    {
        Transform lastChip = chips[^1];

        chips.RemoveAt(chips.Count - 1);
        chipsText.text = slot.Bet > 0 ? slot.Bet + "" : "";

        lastChip.DOMove(GameManager.GetInstance().HandLocation.position, animationSpeed).OnComplete(() => GameObject.Destroy(lastChip.gameObject));

        chipsText.transform.DOKill(true);
        chipsText.transform.DOPunchScale(new Vector3(0.001f, 0.001f, 0.001f), 0.5f, 0);
    }

    public void HideText()
    {
        chipsText.text = "";
    }

    public void Clear()
    {
        for(int i = chips.Count - 1; i >= 0; i--)
            GameObject.Destroy(chips[i].gameObject);
        chips.Clear();
    }

    public void Hover(bool b)
    {
        transform.DOKill(true);
        if(b != hovering) {
            hovering = b;
            transform.DOLocalMoveY(0.5f * (hovering?1:-1), 0.2f).SetRelative();
        }
    }

    /*
    private void OnValidate()
    {
        //Use to scale correctly the pile relative to parent's scale
        transform.localScale = new Vector3(1 / transform.parent.localScale.x, 1 / transform.parent.localScale.y, 1 / transform.parent.localScale.z);
        EditorUtility.SetDirty(this);
    }
    */
}
