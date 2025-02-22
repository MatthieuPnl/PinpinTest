using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OverlayManager : Singleton<OverlayManager>
{
    [SerializeField] private RectTransform infosPanel = null;
    [SerializeField] private RectTransform chipsButtonsPanel = null;
    [SerializeField] private RectTransform betButton = null;
    [SerializeField] private RectTransform dialogZone = null;
    [SerializeField] private Transform dialogPrefab = null;
    [SerializeField] private TextMeshPro multiplierText = null;
    [SerializeField] private CanvasGroup noMoreBetsText = null;
    [SerializeField] private CanvasGroup winningsText = null;

    [SerializeField] private AnimatedIncrement chipsText = null;

    [SerializeField] private List<string> possibleDialogs = new List<string>();

    private void Start()
    {
        StartCoroutine(DialogCoroutine());
    }

    private IEnumerator DialogCoroutine()
    {
        while (true)
        {
            if (!GameManager.GetInstance().BetsOver)
            {
                yield return new WaitForSeconds(1f);
                Transform newDialog = GameObject.Instantiate(dialogPrefab, dialogZone);
                dialogPrefab.GetComponentInChildren<TextMeshProUGUI>().text = possibleDialogs[Random.Range(0, possibleDialogs.Count)];
                newDialog.transform.localPosition = new Vector3(Random.Range(-400, 400), 0, 0);
                newDialog.DOMoveY(dialogZone.position.y + 150f, 0.5f).From().SetEase(Ease.InQuint);
                yield return new WaitForSeconds(2f);
                newDialog.GetComponent<CanvasGroup>().DOFade(0, 1f).OnComplete(() => GameObject.Destroy(newDialog.gameObject));
            }

            yield return new WaitForSeconds(Random.Range(4f,8f));
        }
    }

    public void SwitchOverlay()
    {
        chipsButtonsPanel.DOAnchorPosY(-chipsButtonsPanel.anchoredPosition.y, 0.5f);
        betButton.DOAnchorPosY(-betButton.anchoredPosition.y, 0.5f);
    }

    public void OnEndOfBets()
    {
        SwitchOverlay();
        AnimateNoMoreBets();
    }

    private void AnimateNoMoreBets()
    {
        noMoreBetsText.gameObject.SetActive(true);
        Sequence s = DOTween.Sequence();
        s.Append(noMoreBetsText.transform.DOMoveX(noMoreBetsText.transform.position.x + 2000f, 0.5f).From()).AppendInterval(0.5f);
        s.Append(noMoreBetsText.DOFade(0, 0.5f)).AppendInterval(0.5f);
        s.AppendCallback(() =>
        {
            noMoreBetsText.alpha = 1f;
            noMoreBetsText.gameObject.SetActive(false);
        });
    }

    public void OnWin(int winnings, int totalChips)
    {
        winningsText.GetComponent<TextMeshProUGUI>().text = "+" + winnings;
        winningsText.gameObject.SetActive(true);
        winningsText.alpha = 0;
        Vector3 basePos = winningsText.transform.position;
        Sequence s = DOTween.Sequence();
        s.Append(winningsText.DOFade(1, 1f)).AppendInterval(0.8f);
        s.Append(winningsText.transform.DOMove(chipsText.transform.position, 1f));
        s.Join(winningsText.transform.DOScale(new Vector3(0.2f,0.2f,0.2f),1f));
        s.Join(winningsText.DOFade(0, 1f).SetEase(Ease.Linear));
        s.InsertCallback(2.8f, () => chipsText.UpdateValue(totalChips));
        s.AppendCallback(() =>
        {
            winningsText.alpha = 0f;
            winningsText.gameObject.SetActive(false);
            winningsText.transform.position = basePos;
            winningsText.transform.localScale = Vector3.one;
        });
    }

    public void SetMultiplier(int m)
    {
        multiplierText.text = m == 0 ? "" : "x" + m;
    }

}
