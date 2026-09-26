using UnityEngine;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("BGMを再生するAudioSource")]
    public AudioSource audioSource;

    [Header("フェード時間")]
    public float fadeTime = 1f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("BGMが設定されていません。");
            return;
        }

        // 同じ曲なら何もしない
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        // 現在のフェードを停止
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(ChangeBGM(clip, volume));
    }

    private IEnumerator ChangeBGM(AudioClip newClip, float targetVolume)
    {
        float startVolume = audioSource.volume;

        // フェード時間が0以下の場合は即時切替
        if (fadeTime <= 0f)
        {
            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.loop = true;
            audioSource.volume = targetVolume;
            audioSource.Play();
            fadeCoroutine = null;
            yield break;
        }

        // 現在のBGMをフェードアウト
        if (audioSource.isPlaying)
        {
            float time = 0f;

            while (time < fadeTime)
            {
                time += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);
                yield return null;
            }

            audioSource.volume = 0f;
            audioSource.Stop();
        }

        // 新しいBGMを設定
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.Play();

        // 新しいBGMをフェードイン
        float fadeTimeIn = 0f;

        while (fadeTimeIn < fadeTime)
        {
            fadeTimeIn += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, fadeTimeIn / fadeTime);
            yield return null;
        }

        audioSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
    }
}