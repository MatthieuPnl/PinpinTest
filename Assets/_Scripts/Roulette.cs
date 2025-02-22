using UnityEngine;
using DG.Tweening;

public class Roulette : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotation = 720f;

    void Start()
    {
        //720f because the model doesn't seem to work with 360 (due to the base 3D model)
        transform.DORotate(new Vector3(0f, rotation, 0f), speed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
    }
}
