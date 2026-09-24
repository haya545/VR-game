using UnityEngine;

public class BGMZone : MonoBehaviour
{
    [Header("このエリアで流すBGM")]
    public AudioClip bgm;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBGM(bgm, volume);
        }
    }
}