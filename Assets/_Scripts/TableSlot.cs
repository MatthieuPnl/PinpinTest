using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class TableSlot : MonoBehaviour
{
    [Header("Slot Data")]
    [SerializeField] private SlotType slotType = SlotType.NUMBER;
    [SerializeField] private int number = 0;
    [SerializeField] private SlotColor color = SlotColor.RED;
    [SerializeField] private Vector2 range = new Vector2();

    [Header("Attributes")]
    [SerializeField] private Renderer planeRenderer = null;
    [SerializeField] private ChipsPile chipsPile = null;
    [SerializeField] private SimpleHelvetica multiplier3DText = null;
    [SerializeField] private CinemachineImpulseSource camImpulse = null;

    private List<ChipData> chipsDatas = new List<ChipData>();

    public Renderer PlaneRenderer => planeRenderer;
    public int Number => number;
    public SlotColor SlotColor_ => color;
    public SlotType Type => slotType;
    private int bet = 0;
    public int Bet => bet;

    public override string ToString()
    {
        return slotType switch
        {
            SlotType.NUMBER => "" + number,
            SlotType.COLOR => "" + color,
            SlotType.RANGE => "[" + range.x + " -" + range.y + "]",
            SlotType.ODD => "ODD",
            SlotType.EVEN => "EVEN",
            SlotType.COLUMN => "COLUMN " + number,
            _ => throw new Exception(),
        };
    }
    public bool IsAWin(int result_, SlotColor color_)
    {
        if (bet == 0)
            return false;
        return slotType switch
        {
            SlotType.NUMBER => result_ == number,
            SlotType.COLOR => color_ == color,
            SlotType.RANGE => result_ >= range.x && result_ <= range.y,
            SlotType.ODD => result_ % 2 == 1,
            SlotType.EVEN => result_ % 2 == 0,
            SlotType.COLUMN => result_ % 3 == number % 3,//Could just put number and use Column 3 as Column 0 but easier to understand the board this way
            _ => throw new Exception(),
        };
    }

    public void SetNumber(int number)
    {
        this.number = number;
        this.slotType = SlotType.NUMBER;
    }

    public void OnHoverEnter()
    {
        planeRenderer.enabled = true;
        if (slotType != SlotType.NUMBER)
        {
            Func<TableSlot, bool> cond = (x) => false;
            switch (slotType)
            {
                case SlotType.EVEN:
                case SlotType.ODD:
                    cond = (s) => s.Number % 2 == (slotType == SlotType.EVEN?0:1);
                    break;

                case SlotType.COLOR:
                    cond = (s) => s.SlotColor_ == color;
                    break;

                case SlotType.RANGE:
                    cond = (s) => s.Number >= range.x && s.Number <= range.y;
                    break;

                case SlotType.COLUMN:
                    cond = (s) => s.Number % 3 == number % 3;
                    break;
            }
            GameManager.GetInstance().SpecialHighlight(cond);
        }

        if(bet > 0)
        {
            chipsPile.Hover(true);
        }
    }

    public void OnHoverExit()
    {
        planeRenderer.enabled = false;
        if (slotType != SlotType.NUMBER)
        {
            GameManager.GetInstance().RemoveSpecialHighlight();
        }

        if (bet > 0)
        {
            chipsPile.Hover(false);
        }
    }

    public void OnClick()
    {
        ChipData chipData = GameManager.GetInstance().SelectedChipData;
        chipsDatas.Add(chipData);
        bet += chipData.value;
        chipsPile.AddChip(chipData.prefab, true);
    }

    public bool OnRightClick()
    {
        if(chipsDatas.Count > 0)
        {
            bet -= chipsDatas[^1].value;
            chipsPile.RemoveLastChip();
            chipsDatas.RemoveAt(chipsDatas.Count - 1);
            return true;
        }
        return false;
    }

    public int GetMultiplier()
    {
        return slotType switch
        {
            SlotType.NUMBER => 36,
            SlotType.COLOR or SlotType.EVEN or SlotType.ODD => 2,
            SlotType.COLUMN => 3,
            SlotType.RANGE => range.y - range.x == 11 ? 3 : 2,
            _ => throw new Exception(),
        };
    }

    public void HideText()
    {
        chipsPile.HideText();
    }

    public void OnFocus(bool win)
    {
        if (win)
        {
            multiplier3DText.gameObject.SetActive(true);
            multiplier3DText.transform.DOMoveY(multiplier3DText.transform.position.y + 1f, 0.5f).SetEase(Ease.InExpo).From().OnComplete(() => camImpulse.GenerateImpulse());
        }
    }

    public void OnLeaveFocus()
    {
        multiplier3DText.gameObject.SetActive(false);
    }

    public void Clear()
    {
        if (bet == 0)
            return;
        bet = 0;
        chipsPile.Clear();
    }
}

public enum SlotType
{
    NUMBER,
    COLOR,
    RANGE,
    ODD,
    EVEN,
    COLUMN
}

public enum SlotColor
{
    RED,
    BLACK
}