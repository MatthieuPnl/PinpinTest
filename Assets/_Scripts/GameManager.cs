using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private List<ChipData> chipDatas = new List<ChipData>();
    [SerializeField] private Material gridHoverMaterial = null;
    [SerializeField] private Color baseGridHoverMaterialColor = Color.white;

    private ChipData selectedChipData;
    public ChipData SelectedChipData => selectedChipData;

    [SerializeField] private Transform chipPrefab = null;
    public Transform ChipPrefab => chipPrefab;
    [SerializeField] private Transform handLocation = null;
    public Transform HandLocation => handLocation;


    [SerializeField] private List<TableSlot> slots = new List<TableSlot>();
    [SerializeField] private CinemachineVirtualCamera rouletteVCam = null;
    [SerializeField] private CinemachineVirtualCamera tableEdgeVCam = null;
    [SerializeField] private CinemachineVirtualCamera numberSlotVCam = null;
    [SerializeField] private CinemachineVirtualCamera ballVCam = null;

    [SerializeField] private AnimatedIncrement betText = null;
    [SerializeField] private Button betButton = null;

    [SerializeField] private AnimatedIncrement chipsText = null;

    [SerializeField] private RectTransform maxWins = null;
    [SerializeField] private AnimatedIncrement maxWinsText = null;

    [SerializeField] private Transform wheel = null;
    private int[] wheelNumbers = new int[] { 17, 25, 2, 21, 4, 19, 15, 32, 0, 26, 3, 35, 12, 28, 7, 29, 18, 22, 9, 31, 14, 20, 1, 33, 16, 24, 5, 10, 23, 8, 30, 11, 36, 13, 27, 6, 34 };
    public int[] WheelNumbers => wheelNumbers;

    private TableSlot currentHoverSlot = null;
    private List<TableSlot> currentHighlights = new List<TableSlot>();

    private bool betsOver = false;
    public bool BetsOver => betsOver;
    private bool maxWinsShown = false;
    private int currentChips = 1000;

    [SerializeField] private int cheatNumber = -1;
    [SerializeField] private TextMeshProUGUI cheatNumberText = null;

    private void Start()
    {
        chipsText.Text.text = currentChips + "";
        selectedChipData = chipDatas[1];
        SetSelectedChip(0);
    }

    void Update()
    {
        if (betsOver)
            return;

        CheckHover();
        if (currentHoverSlot != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnSlotClick(0);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                OnSlotClick(1);
            }
        }

    }

    private void CheckHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f))
        {
            Transform objectHit = hit.transform;
            if (objectHit.CompareTag("Interactable"))
            {
                if (currentHoverSlot == null || currentHoverSlot.transform != objectHit.transform)
                {
                    if (currentHoverSlot != null)
                        OnHoverEnd();
                    currentHoverSlot = objectHit.GetComponent<TableSlot>();
                    OnHoverStart();

                    DOTween.Kill(gridHoverMaterial);
                    gridHoverMaterial.color = baseGridHoverMaterialColor;
                    gridHoverMaterial.DOFade(0.4f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                }
                return;
            }
        }
        if (currentHoverSlot != null)
            OnHoverEnd();
    }

    private void OnHoverStart()
    {
        currentHoverSlot.OnHoverEnter();
        OverlayManager.GetInstance().SetMultiplier(currentHoverSlot.GetMultiplier());
    }

    private void OnHoverEnd()
    {
        currentHoverSlot.OnHoverExit();
        OverlayManager.GetInstance().SetMultiplier(0);
        currentHoverSlot = null;
    }

    private void OnSlotClick(int mouseButton)
    {
        if (mouseButton == 0)
        {
            if (GetTotalBet() + selectedChipData.value > currentChips)
                OnChipsLimit();
            else
            {
                currentHoverSlot.OnClick();
                SoundManager.GetInstance().OnChipAdded();
            }
        }
        else if (currentHoverSlot.OnRightClick())
            SoundManager.GetInstance().OnChipRemoved();

        OnBetChange();
    }
    public void RollNumber()
    {
        betsOver = true;
        OverlayManager.GetInstance().OnEndOfBets();
        SoundManager.GetInstance().OnEndOfBets();
        StartCoroutine(RollCoroutine());
    }

    private void HideTextsOnRoll(int newChipsCount)
    {
        chipsText.UpdateValue(newChipsCount);
        foreach (TableSlot s in slots)
            s.HideText();
        OverlayManager.GetInstance().SetMultiplier(0);
        maxWins.DOAnchorPosY(maxWins.rect.height * 0.9f, 0.5f).SetRelative().OnComplete(() => { maxWinsShown = false; maxWinsText.UpdateValue(0); });
    }

    private void OnEndRoll()
    {
        betsOver = false;
        betButton.interactable = false;
        numberSlotVCam.Priority = 8;
        tableEdgeVCam.Priority = 8;
        rouletteVCam.Priority = 9;
        ClearTable();
        OverlayManager.GetInstance().SwitchOverlay();
    }

    private IEnumerator RollCoroutine()
    {
        yield return new WaitForSeconds(1f);

        //Roll down chips count and hide texts
        int newChipsCount = currentChips - GetTotalBet();
        HideTextsOnRoll(newChipsCount);

        yield return new WaitForSeconds(1f);

        //Switch the Camera to Roulette
        currentChips = newChipsCount;
        rouletteVCam.Priority = 11;

        yield return new WaitForSeconds(1f);

        //Launch the ball
        Transform ball = BallManager.GetInstance().LaunchBall(cheatNumber);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        //Wait for the ball to finish rolling
        yield return new WaitForSeconds(0.3f);
        while (rb.isKinematic)
            yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.3f);
        while (rb.velocity.magnitude > 0.035f)
            yield return new WaitForEndOfFrame();


        //Calculate result
        int result = CalculateResult(ball);

        yield return new WaitForSeconds(1f);

        //Find the matching slot and the winnings
        TableSlot rollSlot = slots.Find(x => x.Type == SlotType.NUMBER && x.Number == result);
        Debug.Log("Rolled " + rollSlot.Number + " " + rollSlot.SlotColor_);
        List<TableSlot> winningSlots = new List<TableSlot>();
        int totalWin = 0;
        foreach (TableSlot s in slots)
        {
            if (s.IsAWin(rollSlot.Number, rollSlot.SlotColor_))
            {
                winningSlots.Add(s);
                totalWin += s.Bet * s.GetMultiplier();
                Debug.Log("Win on " + s.ToString());
            }
        }
        bool win = winningSlots.Count > 0;

        //Either the top win slot or the result number if no win
        TableSlot focusSlot = win ? winningSlots.OrderBy((x) => x.GetMultiplier() * x.Bet).ElementAt(0) : rollSlot;


        //Switch to ball cam and prepare slot cam
        ballVCam.LookAt = ball.transform;
        ballVCam.transform.position = Vector3.Lerp(wheel.position, ball.position, 0.7f) + new Vector3(0, 0.5f, 0);
        ballVCam.Priority = 12;
        numberSlotVCam.LookAt = focusSlot.transform;
        numberSlotVCam.Follow = focusSlot.transform;

        yield return new WaitForSeconds(2f);

        //Switch to table edge cam
        ballVCam.Priority = 9;
        tableEdgeVCam.Priority = 11;

        yield return new WaitForSeconds(0.5f);

        //Switch to slot cam
        numberSlotVCam.Priority = 12;

        yield return new WaitForSeconds(0.5f);

        //Winning effects
        SoundManager.GetInstance().OnResult(win);
        focusSlot.OnFocus(win);
        if (win)
        {
            currentChips += totalWin;
            yield return new WaitForSeconds(1.2f);
            OverlayManager.GetInstance().OnWin(totalWin, currentChips);
            yield return new WaitForSeconds(1.5f);
        }

        yield return new WaitForSeconds(2.5f);

        //Reset
        focusSlot.OnLeaveFocus();
        OnEndRoll();
    }

    public void SpecialHighlight(Func<TableSlot, bool> highlightCondition)
    {
        foreach (TableSlot s in slots)
        {
            if (s.Type == SlotType.NUMBER && s.Number > 0 && highlightCondition(s))
            {
                s.OnHoverEnter();
                currentHighlights.Add(s);
            }
        }

    }

    public void RemoveSpecialHighlight()
    {
        foreach (TableSlot t in currentHighlights)
            t.OnHoverExit();
        currentHighlights.Clear();
    }

    private void OnBetChange()
    {
        int totalBet = GetTotalBet();
        betText.UpdateValue(totalBet);
        betButton.interactable = totalBet > 0;

        if (totalBet > 0 && !maxWinsShown)
        {
            maxWinsShown = true;
            DOTween.Kill(maxWins, true);
            maxWins.DOAnchorPosY(-maxWins.rect.height * 0.9f, 0.5f).SetRelative();
        }
        else if (totalBet == 0 && maxWinsShown)
        {
            maxWinsShown = false;
            DOTween.Kill(maxWins, true);
            maxWins.DOAnchorPosY(maxWins.rect.height * 0.9f, 0.5f).SetRelative();
        }
        maxWinsText.UpdateValue(GetMaxPossibleWin());
    }

    private int GetTotalBet()
    {
        int totalBet = 0;
        foreach (TableSlot s in slots)
            totalBet += s.Bet;
        return totalBet;
    }

    private int GetMaxPossibleWin()
    {
        int maxWins = 0;

        List<TableSlot> possibleRolls = slots.FindAll(x => x.Type == SlotType.NUMBER);
        foreach (TableSlot p in possibleRolls)
        {
            int possibleWins = 0;
            foreach (TableSlot s in slots)
            {
                if (s.IsAWin(p.Number, p.SlotColor_))
                    possibleWins += s.Bet * s.GetMultiplier();
            }
            maxWins = Math.Max(maxWins, possibleWins);
        }
        return maxWins;
    }

    public void SetSelectedChip(int index)
    {
        if (selectedChipData.value != chipDatas[index].value)
        {
            selectedChipData.chipButton.OnUnselected();
            selectedChipData = chipDatas[index];
            selectedChipData.chipButton.OnSelected();
            SoundManager.GetInstance().OnChipButton();
        }
    }

    private void OnChipsLimit()
    {
        DOTween.Kill(chipsText.transform, true);
        chipsText.Text.color = Color.red;
        chipsText.transform.DOPunchPosition(new Vector3(10f, 0f, 0), 0.5f, 10, 0).OnComplete(() => chipsText.Text.color = Color.white);
        SoundManager.GetInstance().OnError();
    }

    private void ClearTable()
    {
        foreach (TableSlot s in slots)
            s.Clear();
        betText.UpdateValue(0);
    }

    public int CalculateResult(Transform t)
    {
        Vector3 ballDirection = t.position - wheel.position;
        float angle = Vector2.SignedAngle(new Vector2(-wheel.forward.x, -wheel.forward.z), new Vector2(ballDirection.x, ballDirection.z));
        if (angle < 0)
            angle = 360 - (-angle);
        return wheelNumbers[(int)((double)angle / (360d / 37d))];
    }


    //CHEATS

    public void OnCheatNumberChanged(float i)
    {
        cheatNumber = (int)i;
        cheatNumberText.text = (cheatNumber > -1 ? cheatNumber : "") + "";
    }

    public int GetCheatEndPoint(int c, int offset)
    {
        int cIndex = 0;
        for(int i = 0; i < wheelNumbers.Count(); i++)
        {
            if (wheelNumbers[i] == c)
            {
                cIndex = i;
                break;
            }
        }

        cIndex -= offset;
        if (cIndex < 0)
            cIndex = wheelNumbers[wheelNumbers.Count() + cIndex];
        else
            cIndex = wheelNumbers[cIndex];
        return cIndex;
    }
}

[Serializable]
public struct ChipData
{
    public Transform prefab;
    public int value;
    public ChipButton chipButton;
}