using UnityEngine;

public class ElementSource : MonoBehaviour
{
    [Header("このアイテムを撃ったときに出現する元素")]
    public GameObject elementPrefab;

    [Header("変換エフェクト")]
    public GameObject conversionEffect;

    [Header("変換時に消えるか")]
    public bool destroyAfterConversion = true;


    // =========================
    // 変換音
    // =========================

    [Header("変換時の効果音")]
    public AudioClip conversionSound;

    [Header("変換音の音量")]
    [Range(0f, 1f)]
    public float conversionSoundVolume = 1f;


    public void Convert(Vector3 hitPosition)
    {
        // =========================
        // 変換エフェクト
        // =========================

        if (conversionEffect != null)
        {
            Instantiate(
                conversionEffect,
                hitPosition,
                Quaternion.identity
            );
        }


        // =========================
        // 変換音
        // =========================

        if (conversionSound != null)
        {
            AudioSource.PlayClipAtPoint(
                conversionSound,
                hitPosition,
                conversionSoundVolume
            );
        }


        // =========================
        // 元素を生成
        // =========================

        Vector3 position = transform.position;

        if (elementPrefab != null)
        {
            Instantiate(
                elementPrefab,
                position,
                Quaternion.identity
            );
        }


        // =========================
        // 元のアイテムを消す
        // =========================

        if (destroyAfterConversion)
        {
            Destroy(gameObject);
        }
    }
}