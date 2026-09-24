using UnityEngine;

public class Ore : MonoBehaviour
{
    [Header("鉱石設定")]
    public int hitPoints = 3;

    [Header("ドロップ設定")]
    public GameObject elementPrefab;

    [Tooltip("1回の攻撃で落とす個数")]
    public int dropAmount = 1;

    [Header("ドロップ位置")]
    public Transform dropPoint;

    [Header("エフェクト")]
    public GameObject hitEffect;

    [Header("斧の設定")]
    public string axeTag = "Axe";


    // ========================================
    // 音設定
    // ========================================

    [Header("鉱石を叩いた音")]
    public AudioClip hitSound;

    [Header("鉱石を叩いた音量")]
    [Range(0f, 1f)]
    public float hitVolume = 1f;

    [Header("鉱石が壊れる音")]
    public AudioClip destroySound;

    [Header("鉱石が壊れる音量")]
    [Range(0f, 1f)]
    public float destroyVolume = 1f;


    private int currentHP;

    // 破壊処理中か
    private bool isDestroyed = false;


    // ========================================
    // 起動時
    // ========================================
    private void Start()
    {
        currentHP = hitPoints;

        Debug.Log(
            "⛏ 鉱石開始：" +
            gameObject.name +
            " HP=" +
            currentHP
        );
    }


    // ========================================
    // 鉱石を叩く
    // ========================================
    public void HitOre()
    {
        // ------------------------------------
        // すでに破壊済みなら何もしない
        // ------------------------------------
        if (isDestroyed)
        {
            return;
        }


        // ------------------------------------
        // HPが0以下なら何もしない
        // ------------------------------------
        if (currentHP <= 0)
        {
            return;
        }


        Debug.Log(
            "⛏ 鉱石ヒット：" +
            gameObject.name
        );


        // ====================================
        // 鉱石を叩いた音
        // ====================================

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(
                hitSound,
                transform.position,
                hitVolume
            );
        }


        // ====================================
        // 元素を1個ドロップ
        // ====================================
        DropElement();


        // ====================================
        // HPを減らす
        // ====================================
        currentHP--;


        Debug.Log(
            "💥 " +
            gameObject.name +
            " に斧が当たった！ 残りHP：" +
            currentHP
        );


        // ====================================
        // ヒットエフェクト
        // ====================================
        PlayHitEffect();


        // ====================================
        // HP0
        // ====================================
        if (currentHP <= 0)
        {
            isDestroyed = true;


            // ====================================
            // 鉱石が壊れる音
            // ====================================

            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(
                    destroySound,
                    transform.position,
                    destroyVolume
                );
            }


            Debug.Log(
                "💎 " +
                gameObject.name +
                " を破壊しました！"
            );

            Destroy(gameObject);
        }
    }


    // ========================================
    // 元素アイテムをドロップ
    // ========================================
    private void DropElement()
    {
        if (elementPrefab == null)
        {
            Debug.LogWarning(
                "⚠ elementPrefabが設定されていません！"
            );

            return;
        }


        // ====================================
        // ドロップ位置
        // ====================================
        Vector3 spawnPosition;

        if (dropPoint != null)
        {
            spawnPosition =
                dropPoint.position;
        }
        else
        {
            spawnPosition =
                transform.position +
                Vector3.up * 0.5f;
        }


        // ====================================
        // 1回の攻撃につき最大1個
        // ====================================
        int amount =
            Mathf.Clamp(
                dropAmount,
                1,
                1
            );


        Debug.Log(
            "🧪 元素ドロップ：" +
            elementPrefab.name +
            " × " +
            amount
        );


        for (int i = 0; i < amount; i++)
        {
            GameObject element =
                Instantiate(
                    elementPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            if (element == null)
            {
                Debug.LogError(
                    "❌ 元素Prefabの生成に失敗しました！"
                );

                continue;
            }


            Debug.Log(
                "✅ 元素Prefab生成：" +
                element.name
            );
        }
    }


    // ========================================
    // ヒットエフェクト
    // ========================================
    private void PlayHitEffect()
    {
        if (hitEffect == null)
        {
            return;
        }


        Vector3 effectPosition;

        if (dropPoint != null)
        {
            effectPosition =
                dropPoint.position;
        }
        else
        {
            effectPosition =
                transform.position;
        }


        GameObject effect =
            Instantiate(
                hitEffect,
                effectPosition,
                Quaternion.identity
            );


        Destroy(
            effect,
            3f
        );
    }
}