using DG.Tweening;
using EditorCools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BallManager : Singleton<BallManager>
{
    [SerializeField] private Transform launchBallPosition = null;
    [SerializeField] private float launchBallTime = 1f;

    [SerializeField] private Transform ballPrefab = null;
    [SerializeField] private Transform ballParent = null;

    [Header("How long to do 2 turns")]
    [SerializeField] private float speed = 10f;
    [Header("Impacts the Rigidbody")]
    [SerializeField] private float forceFactor = 0.2f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

    private Vector3 basePosition = Vector3.zero;
    private Quaternion baseRotation = Quaternion.identity;

    private Transform ball = null;
    private bool isUsingRigidbody = false;
    public bool IsUsingRigidbody => isUsingRigidbody;

    void Start()
    {
        basePosition = ballPrefab.transform.position;
        baseRotation = ballPrefab.transform.rotation;
    }

    public Transform LaunchBall(int cheatNumber)
    {
        if (ball != null)
            GameObject.Destroy(ball.gameObject);
        ball = GameObject.Instantiate(ballPrefab, ballParent);
        ball.SetPositionAndRotation(basePosition, baseRotation);
        ball.gameObject.SetActive(true);
        isUsingRigidbody = false;
        StartCoroutine(BallCoroutine(cheatNumber));
       
        return ball;
    }

    private IEnumerator BallCoroutine(int cheatNumber)
    {
        bool finished = false;
        ball.DOMove(launchBallPosition.position, launchBallTime).From().SetEase(Ease.InExpo).OnComplete(() => finished = true);
        while (!finished)
            yield return new WaitForEndOfFrame();
        ball.rotation = baseRotation;
        ballParent.DORotate(new Vector3(0f, -720, 0f), speed, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.Linear).SetLoops(-1);
        yield return new WaitForSeconds(speed);

        if (cheatNumber != -1)
        {
            int cheatStartPoint = GameManager.GetInstance().GetCheatEndPoint(cheatNumber, cheatOffset);
            int prevPos = GameManager.GetInstance().CalculateResult(ball);
            while (GameManager.GetInstance().CalculateResult(ball) != cheatStartPoint || prevPos == GameManager.GetInstance().CalculateResult(ball))
            {
                prevPos = GameManager.GetInstance().CalculateResult(ball);
                yield return new WaitForEndOfFrame();
            }
        }
        ballParent.DOKill();

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        //if(cheatNumber == -1)
            rb.AddForce(rb.transform.forward * forceFactor, forceMode);
        isUsingRigidbody = true;
    }

#if UNITY_EDITOR
    [SerializeField] private int cheatOffset = 10;
    [Header("Debug")]
    public int cheatingNumber = -1;
    [Button]
    public void TestBall() => LaunchBall(cheatingNumber);
#endif
}
