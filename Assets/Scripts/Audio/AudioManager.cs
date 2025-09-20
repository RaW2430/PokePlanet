using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    private AudioSource audioSource;
    public AudioClip clickSound;
    // 成功和失败音效的公开引用
    public AudioClip mergeSound;
    public AudioClip failMergeSound;
    public AudioClip incubateSound;
    public AudioClip winSound;
    public AudioClip coinSound;
    public AudioClip failSound;
    private void Awake()
    {
        // 确保只有一个AudioManager实例
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    public void playClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(clickSound);
        }
    }

    // 播放合并成功音效
    public void PlayMergeSound()
    {
        if (audioSource != null && mergeSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(mergeSound);
        }
    }
    
    // 播放合并失败音效
    public void PlayFailMergeSound()
    {
        if (audioSource != null && failMergeSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(failMergeSound);
        }
    }
    
    // 播放孵化音效
    public void PlayIncubateSound()
    {
        if (audioSource != null && incubateSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(incubateSound);
        }
    }
    
    // 播放胜利音效
    public void PlayWinSound()
    {
        if (audioSource != null && winSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(winSound);
        }
    }
    
    // 播放金币音效
    public void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(coinSound);
        }
    }
    public void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
        {
            audioSource.enabled = true;
            audioSource.PlayOneShot(failSound);
        }
    }
}