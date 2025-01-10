using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioClip OnClickNode;
    [SerializeField] private AudioClip OnDragNode;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Instance = this;
    }

    public void PlaySoundClick()
    {
        audioSource.clip = OnClickNode;
        audioSource.Play();
    }

    public void PlaySoundDrag()
    {
        audioSource.clip = OnDragNode;
        audioSource.Play();
    }
    public void StopDrag()
    {
        
        audioSource.Stop();
    }
}
