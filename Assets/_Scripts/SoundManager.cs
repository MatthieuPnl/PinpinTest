using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource sfxSource = null;

    [SerializeField] private AudioClip chipAddedSound = null;
    [SerializeField] private AudioClip chipRemovedSound = null;
    [SerializeField] private AudioClip errorSound = null;

    [SerializeField] private AudioClip winSound = null;
    [SerializeField] private AudioClip loseSound = null;

    [SerializeField] private AudioClip chipButtonSound = null;
    [SerializeField] private AudioClip endOfBetSound = null;


    public void OnChipAdded()
    {
        PlaySound(chipAddedSound);
    }

    public void OnChipRemoved()
    {
        PlaySound(chipRemovedSound);
    }
    public void OnError()
    {
        PlaySound(errorSound);
    }

    public void OnResult(bool win)
    {
        PlaySound(win?winSound:loseSound);
    }

    public void OnChipButton()
    {
        PlaySound(chipButtonSound);
    }

    public void OnEndOfBets()
    {
        PlaySound(endOfBetSound);
    }

    private void PlaySound(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.Play();
    }
}
