using UnityEngine;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    // =========================================================
    // HP設定
    // =========================================================

    [Header("HP設定")]
    public int maxHP = 100;

    private int currentHP;


    // =========================================================
    // 攻撃力設定
    // =========================================================

    [Header("敵へのダメージ設定")]
    public int axeDamage = 10;
    public int fireDamage = 25;


    // =========================================================
    // 被弾設定
    // =========================================================

    [System.Serializable]
    public class HitSound
    {
        [Tooltip("被弾時に再生する音")]
        public AudioClip clip;

        [Tooltip("この音だけの音量")]
        [Range(0f, 5f)]
        public float volume = 1f;
    }


    [Header("=== 被弾時設定 ===")]

    [Tooltip("被弾時に再生する効果音")]
    public List<HitSound> hitSounds =
        new List<HitSound>();

    [Tooltip("被弾時に表示するエフェクト")]
    public List<GameObject> hitEffects =
        new List<GameObject>();

    [Tooltip("被弾エフェクトの表示時間")]
    public float hitEffectLifetime = 2f;


    // =========================================================
    // ドロップ設定
    // =========================================================

    [System.Serializable]
    public class DropItem
    {
        [Tooltip("ドロップするアイテム")]
        public GameObject itemPrefab;

        [Tooltip("ドロップ個数")]
        public int amount = 1;

        [Tooltip("ドロップする確率（0～100）")]
        [Range(0f, 100f)]
        public float dropChance = 100f;
    }


    [Header("=== ドロップ設定 ===")]

    [Tooltip("敵が倒されたときにドロップするアイテム")]
    public List<DropItem> dropItems =
        new List<DropItem>();

    [Tooltip("ドロップする位置")]
    public Transform dropPoint;

    [Tooltip("Drop Pointがない場合、この高さだけ上に出す")]
    public float defaultDropHeight = 1f;


    // =========================================================
    // オーディオ
    // =========================================================

    private AudioSource audioSource;


    // =========================================================
    // 初期化
    // =========================================================

    private void Start()
    {
        currentHP = maxHP;

        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();

            Debug.Log(
                "🔊 EnemyController: AudioSourceが無かったため自動追加しました。"
            );
        }

        audioSource.playOnAwake = false;


        Debug.Log(
            "👹 敵生成: " +
            gameObject.name +
            " / HP: " +
            currentHP +
            "/" +
            maxHP
        );


        // =====================================================
        // 被弾設定チェック
        // =====================================================

        Debug.Log(
            "🔍 被弾設定確認: " +
            gameObject.name +
            " / 被弾音=" +
            hitSounds.Count +
            "個 / 被弾エフェクト=" +
            hitEffects.Count +
            "個"
        );


        // 被弾音チェック
        for (
            int i = 0;
            i < hitSounds.Count;
            i++
        )
        {
            if (
                hitSounds[i] == null ||
                hitSounds[i].clip == null
            )
            {
                Debug.LogWarning(
                    "⚠️ " +
                    gameObject.name +
                    " / Hit Sounds Element " +
                    i +
                    " が空です。"
                );
            }
            else
            {
                Debug.Log(
                    "🔊 被弾音[" +
                    i +
                    "] = " +
                    hitSounds[i].clip.name +
                    " / 音量=" +
                    hitSounds[i].volume
                );
            }
        }


        // 被弾エフェクトチェック
        for (
            int i = 0;
            i < hitEffects.Count;
            i++
        )
        {
            if (hitEffects[i] == null)
            {
                Debug.LogWarning(
                    "⚠️ " +
                    gameObject.name +
                    " / Hit Effects Element " +
                    i +
                    " が空です。"
                );
            }
            else
            {
                Debug.Log(
                    "✨ 被弾エフェクト[" +
                    i +
                    "] = " +
                    hitEffects[i].name
                );
            }
        }
    }


    // =========================================================
    // 🪓 斧ダメージ
    // =========================================================

    public void TakeAxeDamage(
        Vector3 hitPosition
    )
    {
        Debug.Log(
            "🪓 TakeAxeDamage呼び出し！" +
            " / 敵=" +
            gameObject.name +
            " / ダメージ=" +
            axeDamage +
            " / 命中位置=" +
            hitPosition
        );

        TakeDamage(
            axeDamage,
            "斧",
            hitPosition
        );
    }


    // =========================================================
    // 🔥 炎ダメージ
    // =========================================================

    public void TakeFireDamage(
        Vector3 hitPosition
    )
    {
        Debug.Log(
            "🔥 TakeFireDamage呼び出し！" +
            " / 敵=" +
            gameObject.name +
            " / ダメージ=" +
            fireDamage +
            " / 命中位置=" +
            hitPosition
        );

        TakeDamage(
            fireDamage,
            "炎",
            hitPosition
        );
    }


    // =========================================================
    // 💥 ダメージ処理
    // =========================================================

    private void TakeDamage(
        int damage,
        string attackType,
        Vector3 hitPosition
    )
    {
        if (currentHP <= 0)
        {
            Debug.LogWarning(
                "⚠️ " +
                gameObject.name +
                " はすでに死亡しています。"
            );

            return;
        }


        int previousHP = currentHP;

        currentHP -= damage;

        currentHP =
            Mathf.Max(
                currentHP,
                0
            );


        Debug.Log(
            "💥💥💥 ダメージ処理！" +
            " / 敵=" +
            gameObject.name +
            " / 攻撃=" +
            attackType +
            " / ダメージ=" +
            damage +
            " / HP=" +
            previousHP +
            "→" +
            currentHP +
            "/" +
            maxHP +
            " / 命中位置=" +
            hitPosition
        );


        // 被弾エフェクト
        PlayHitEffects(
            hitPosition
        );


        // 被弾音
        PlayHitSounds(
            hitPosition
        );


        // 死亡判定
        if (currentHP <= 0)
        {
            Debug.Log(
                "💀 HPが0になったため死亡処理へ移行します。"
            );

            Die();
        }
    }


    // =========================================================
    // 💥 被弾エフェクト
    // =========================================================

    private void PlayHitEffects(
        Vector3 hitPosition
    )
    {
        Debug.Log(
            "✨✨ 被弾エフェクト処理開始！" +
            " / 登録数=" +
            hitEffects.Count +
            " / 位置=" +
            hitPosition
        );


        if (hitEffects.Count == 0)
        {
            Debug.LogWarning(
                "⚠️ 被弾エフェクトが1つも登録されていません！"
            );

            return;
        }


        int createdCount = 0;


        foreach (
            GameObject effectPrefab
            in hitEffects
        )
        {
            if (effectPrefab == null)
            {
                Debug.LogWarning(
                    "⚠️ 被弾エフェクトの中にNULLがあります。"
                );

                continue;
            }


            Debug.Log(
                "✨ 被弾エフェクト生成: " +
                effectPrefab.name +
                " / 位置=" +
                hitPosition
            );


            GameObject effect =
                Instantiate(
                    effectPrefab,
                    hitPosition,
                    Quaternion.identity
                );


            createdCount++;


            Debug.Log(
                "✅ 被弾エフェクト生成成功！" +
                " / " +
                effect.name
            );


            Destroy(
                effect,
                hitEffectLifetime
            );
        }


        Debug.Log(
            "✨ 被弾エフェクト処理終了。" +
            " / 生成数=" +
            createdCount
        );
    }


    // =========================================================
    // 🔊 被弾音
    // =========================================================

    private void PlayHitSounds(
        Vector3 hitPosition
    )
    {
        Debug.Log(
            "🔊🔊 被弾音処理開始！" +
            " / 登録数=" +
            hitSounds.Count +
            " / 位置=" +
            hitPosition
        );


        if (hitSounds.Count == 0)
        {
            Debug.LogWarning(
                "⚠️ 被弾音が1つも登録されていません！"
            );

            return;
        }


        int playedCount = 0;


        foreach (
            HitSound hitSound
            in hitSounds
        )
        {
            if (
                hitSound == null ||
                hitSound.clip == null
            )
            {
                Debug.LogWarning(
                    "⚠️ 被弾音の中にNULLがあります。"
                );

                continue;
            }


            Debug.Log(
                "🔊 被弾音再生: " +
                hitSound.clip.name +
                " / 個別音量=" +
                hitSound.volume
            );


            AudioSource.PlayClipAtPoint(
                hitSound.clip,
                hitPosition,
                hitSound.volume
            );


            playedCount++;
        }


        Debug.Log(
            "🔊 被弾音処理終了。" +
            " / 再生数=" +
            playedCount
        );
    }


    // =========================================================
    // 💀 死亡
    // =========================================================

    private void Die()
    {
        Debug.Log(
            "💀💀💀 " +
            gameObject.name +
            " を倒した！"
        );


        DropItems();


        Destroy(gameObject);
    }


    // =========================================================
    // 🎁 ドロップ処理
    // =========================================================

    private void DropItems()
    {
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
                Vector3.up *
                defaultDropHeight;
        }


        Debug.Log(
            "🎁 ドロップ処理開始！" +
            " / 登録数=" +
            dropItems.Count +
            " / 位置=" +
            spawnPosition
        );


        foreach (
            DropItem drop
            in dropItems
        )
        {
            if (drop.itemPrefab == null)
            {
                Debug.LogWarning(
                    "⚠️ ドロップアイテムPrefabが設定されていません。"
                );

                continue;
            }


            float randomValue =
                Random.Range(
                    0f,
                    100f
                );


            if (
                randomValue >
                drop.dropChance
            )
            {
                Debug.Log(
                    "🎁 ドロップしませんでした: " +
                    drop.itemPrefab.name +
                    " / 抽選=" +
                    randomValue +
                    " / 確率=" +
                    drop.dropChance
                );

                continue;
            }


            for (
                int i = 0;
                i < drop.amount;
                i++
            )
            {
                Vector3 randomOffset =
                    new Vector3(
                        Random.Range(
                            -0.3f,
                            0.3f
                        ),
                        Random.Range(
                            0f,
                            0.3f
                        ),
                        Random.Range(
                            -0.3f,
                            0.3f
                        )
                    );


                Instantiate(
                    drop.itemPrefab,
                    spawnPosition +
                    randomOffset,
                    Quaternion.identity
                );


                Debug.Log(
                    "🎁 ドロップ生成: " +
                    drop.itemPrefab.name
                );
            }
        }
    }


    // =========================================================
    // 現在HP取得
    // =========================================================

    public int GetCurrentHP()
    {
        return currentHP;
    }


    // =========================================================
    // 最大HP取得
    // =========================================================

    public int GetMaxHP()
    {
        return maxHP;
    }
}