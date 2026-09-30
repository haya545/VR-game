using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeatCookable : MonoBehaviour
{
    // =========================================================
    // 🔥 焼き設定
    // =========================================================

    [Header("=== 🔥 焼き設定 ===")]

    [Tooltip("焼けるまでの時間")]
    public float cookingTime = 5f;


    // =========================================================
    // 🍖 焼き肉Prefab
    // =========================================================

    [Header("=== 🍖 焼き肉設定 ===")]

    [Tooltip("焼き終わったときに出す焼き肉Prefab")]
    public GameObject cookedMeatPrefab;

    [Tooltip("焼き上がったときに生成する焼き肉の個数")]
    [Min(1)]
    public int cookedMeatAmount = 1;


    // =========================================================
    // 🔥 焼いている間のエフェクト
    // =========================================================

    [System.Serializable]
    public class CookingEffect
    {
        [Tooltip("焼いている間に表示するエフェクト")]
        public GameObject effectPrefab;

        [Tooltip("エフェクトを発生させる間隔")]
        public float loopInterval = 1f;

        [Tooltip("エフェクトを表示する位置")]
        public Transform effectPoint;

        [Tooltip("生成したエフェクトを自動削除する時間")]
        public float effectLifetime = 2f;
    }


    [Header("=== 🔥 焼いている間のエフェクト ===")]

    [Tooltip("＋で複数のエフェクトを追加できます")]
    public List<CookingEffect> cookingEffects =
        new List<CookingEffect>();


    // =========================================================
    // 🔊 焼いている間の音
    // =========================================================

    [Header("=== 🔊 焼いている間の音 ===")]

    [Tooltip("焼いている間に再生する音")]
    public List<AudioClip> cookingSounds =
        new List<AudioClip>();

    [Tooltip("焼いている間の音量")]
    [Range(0f, 3f)]
    public float cookingSoundVolume = 1f;


    // =========================================================
    // ✨ 焼き上がった瞬間のエフェクト
    // =========================================================

    [System.Serializable]
    public class FinishedCookingEffect
    {
        [Tooltip("焼き上がった瞬間に再生するエフェクト")]
        public GameObject effectPrefab;

        [Tooltip("エフェクトを発生させる位置")]
        public Transform effectPoint;

        [Tooltip("エフェクトの表示時間")]
        public float effectLifetime = 2f;
    }


    [Header("=== ✨ 焼き上がった瞬間のエフェクト ===")]

    [Tooltip("＋で複数の完成エフェクトを追加できます")]
    public List<FinishedCookingEffect> finishedCookingEffects =
        new List<FinishedCookingEffect>();


    // =========================================================
    // 🔊 焼き上がった瞬間の効果音
    // =========================================================

    [Header("=== 🔊 焼き上がった瞬間の効果音 ===")]

    [Tooltip("焼き上がった瞬間に再生する効果音")]
    public List<AudioClip> finishedCookingSounds =
        new List<AudioClip>();

    [Tooltip("焼き上がり効果音の音量")]
    [Range(0f, 3f)]
    public float finishedCookingSoundVolume = 1f;


    // =========================================================
    // 内部
    // =========================================================

    private bool isCooking = false;

    private List<GameObject> spawnedEffects =
        new List<GameObject>();

    private List<AudioSource> cookingAudioSources =
        new List<AudioSource>();

    private List<Coroutine> effectCoroutines =
        new List<Coroutine>();


    // =========================================================
    // 🔥 焼き開始
    // =========================================================

    public void StartCooking()
    {
        if (isCooking)
        {
            return;
        }


        if (cookedMeatPrefab == null)
        {
            Debug.LogWarning(
                "⚠️ " +
                gameObject.name +
                " に焼き肉Prefabが設定されていません！"
            );

            return;
        }


        isCooking = true;


        Debug.Log(
            "🔥🔥🔥 肉に炎が命中！焼き始めます！" +
            " / " +
            gameObject.name +
            " / " +
            cookingTime +
            "秒"
        );


        // =====================================================
        // 🚫 物理停止
        // =====================================================

        StopMeatPhysics();


        // =====================================================
        // 🔥 焼いている間のエフェクト開始
        // =====================================================

        StartCookingEffects();


        // =====================================================
        // 🔊 焼いている間の音開始
        // =====================================================

        StartCookingSounds();


        // =====================================================
        // ⏱ 焼き時間開始
        // =====================================================

        StartCoroutine(
            CookingCoroutine()
        );
    }


    // =========================================================
    // 🚫 肉の物理移動停止
    // =========================================================

    private void StopMeatPhysics()
    {
        Rigidbody rb =
            GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;

            rb.isKinematic =
                true;


            Debug.Log(
                "🚫 肉のRigidbodyを停止しました。"
            );
        }
    }


    // =========================================================
    // ⏱ 焼き時間
    // =========================================================

    private IEnumerator CookingCoroutine()
    {
        yield return new WaitForSeconds(
            cookingTime
        );

        FinishCooking();
    }


    // =========================================================
    // 🔥 焼きエフェクト開始
    // =========================================================

    private void StartCookingEffects()
    {
        foreach (
            CookingEffect effect
            in cookingEffects
        )
        {
            if (
                effect == null ||
                effect.effectPrefab == null
            )
            {
                continue;
            }


            Coroutine coroutine =
                StartCoroutine(
                    CookingEffectLoop(
                        effect
                    )
                );


            effectCoroutines.Add(
                coroutine
            );
        }
    }


    // =========================================================
    // 🔥 エフェクトループ
    // =========================================================

    private IEnumerator CookingEffectLoop(
        CookingEffect setting
    )
    {
        // 最初のエフェクトはすぐ表示
        SpawnCookingEffect(
            setting
        );


        while (isCooking)
        {
            yield return new WaitForSeconds(
                Mathf.Max(
                    setting.loopInterval,
                    0.01f
                )
            );


            if (!isCooking)
            {
                yield break;
            }


            SpawnCookingEffect(
                setting
            );
        }
    }


    // =========================================================
    // 🔥 焼きエフェクト生成
    // =========================================================

    private void SpawnCookingEffect(
        CookingEffect setting
    )
    {
        if (
            setting == null ||
            setting.effectPrefab == null
        )
        {
            return;
        }


        Vector3 spawnPosition =
            transform.position;

        Quaternion spawnRotation =
            Quaternion.identity;


        if (setting.effectPoint != null)
        {
            spawnPosition =
                setting.effectPoint.position;

            spawnRotation =
                setting.effectPoint.rotation;
        }


        GameObject effect =
            Instantiate(
                setting.effectPrefab,
                spawnPosition,
                spawnRotation
            );


        // 肉について動く
        effect.transform.SetParent(
            transform
        );


        spawnedEffects.Add(
            effect
        );


        Debug.Log(
            "🔥 焼きエフェクト生成: " +
            setting.effectPrefab.name
        );


        float lifetime =
            Mathf.Max(
                setting.effectLifetime,
                0.01f
            );


        StartCoroutine(
            DestroyEffectAfterTime(
                effect,
                lifetime
            )
        );
    }


    // =========================================================
    // 🔥 焼きエフェクト削除
    // =========================================================

    private IEnumerator DestroyEffectAfterTime(
        GameObject effect,
        float lifetime
    )
    {
        yield return new WaitForSeconds(
            lifetime
        );


        if (effect != null)
        {
            spawnedEffects.Remove(
                effect
            );

            Destroy(
                effect
            );
        }
    }


    // =========================================================
    // 🔊 焼いている間の音開始
    // =========================================================

    private void StartCookingSounds()
    {
        foreach (
            AudioClip clip
            in cookingSounds
        )
        {
            if (clip == null)
            {
                continue;
            }


            AudioSource source =
                gameObject.AddComponent<AudioSource>();


            source.clip =
                clip;

            source.volume =
                cookingSoundVolume;

            source.loop =
                true;

            source.playOnAwake =
                false;

            source.spatialBlend =
                1f;


            source.Play();


            cookingAudioSources.Add(
                source
            );


            Debug.Log(
                "🔊 焼いている音開始: " +
                clip.name
            );
        }
    }


    // =========================================================
    // 🍖 焼き上がり
    // =========================================================

    private void FinishCooking()
    {
        Debug.Log(
            "🍖🔥 肉が焼けました！"
        );


        isCooking = false;


        // =====================================================
        // 🔥 焼いている間のエフェクト停止
        // =====================================================

        StopCookingEffects();


        // =====================================================
        // 🔊 焼いている間の音停止
        // =====================================================

        StopCookingSounds();


        // =====================================================
        // ✨ 焼き上がりエフェクト
        // =====================================================

        PlayFinishedCookingEffects();


        // =====================================================
        // 🔊 焼き上がり効果音
        // =====================================================

        PlayFinishedCookingSounds();


        // =====================================================
        // 🍖 焼き肉を指定個数生成
        // =====================================================

        SpawnCookedMeat();


        // =====================================================
        // 🥩 生肉削除
        // =====================================================

        Destroy(
            gameObject
        );
    }


    // =========================================================
    // 🍖 焼き肉生成
    // =========================================================

    private void SpawnCookedMeat()
    {
        if (cookedMeatPrefab == null)
        {
            Debug.LogWarning(
                "⚠️ 焼き肉Prefabが設定されていません！"
            );

            return;
        }


        int amount =
            Mathf.Max(
                cookedMeatAmount,
                1
            );


        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition =
                transform.position;

            Quaternion spawnRotation =
                transform.rotation;


            // =================================================
            // 複数個生成するときに少し位置をずらす
            // =================================================

            if (amount > 1)
            {
                float offset =
                    0.05f;

                float x =
                    Random.Range(
                        -offset,
                        offset
                    );

                float y =
                        Random.Range(
                        0f,
                        offset
                    );

                float z =
                    Random.Range(
                        -offset,
                        offset
                    );


                spawnPosition +=
                    new Vector3(
                        x,
                        y,
                        z
                    );
            }


            Instantiate(
                cookedMeatPrefab,
                spawnPosition,
                spawnRotation
            );
        }


        Debug.Log(
            "🍖 焼き肉を " +
            amount +
            "個生成しました！"
        );
    }


    // =========================================================
    // ✨ 焼き上がりエフェクト
    // =========================================================

    private void PlayFinishedCookingEffects()
    {
        foreach (
            FinishedCookingEffect setting
            in finishedCookingEffects
        )
        {
            if (
                setting == null ||
                setting.effectPrefab == null
            )
            {
                continue;
            }


            Vector3 spawnPosition =
                transform.position;

            Quaternion spawnRotation =
                Quaternion.identity;


            if (setting.effectPoint != null)
            {
                spawnPosition =
                    setting.effectPoint.position;

                spawnRotation =
                    setting.effectPoint.rotation;
            }


            GameObject effect =
                Instantiate(
                    setting.effectPrefab,
                    spawnPosition,
                    spawnRotation
                );


            Debug.Log(
                "✨ 焼き上がりエフェクト再生: " +
                setting.effectPrefab.name
            );


            float lifetime =
                Mathf.Max(
                    setting.effectLifetime,
                    0.01f
                );


            StartCoroutine(
                DestroyFinishedEffectAfterTime(
                    effect,
                    lifetime
                )
            );
        }
    }


    // =========================================================
    // ✨ 焼き上がりエフェクト削除
    // =========================================================

    private IEnumerator DestroyFinishedEffectAfterTime(
        GameObject effect,
        float lifetime
    )
    {
        yield return new WaitForSeconds(
            lifetime
        );


        if (effect != null)
        {
            Destroy(
                effect
            );
        }
    }


    // =========================================================
    // 🔊 焼き上がり効果音
    // =========================================================

    private void PlayFinishedCookingSounds()
    {
        foreach (
            AudioClip clip
            in finishedCookingSounds
        )
        {
            if (clip == null)
            {
                continue;
            }


            AudioSource.PlayClipAtPoint(
                clip,
                transform.position,
                finishedCookingSoundVolume
            );


            Debug.Log(
                "🔊 焼き上がり効果音再生: " +
                clip.name
            );
        }
    }


    // =========================================================
    // 🔥 焼いている間のエフェクト停止
    // =========================================================

    private void StopCookingEffects()
    {
        foreach (
            Coroutine coroutine
            in effectCoroutines
        )
        {
            if (coroutine != null)
            {
                StopCoroutine(
                    coroutine
                );
            }
        }


        effectCoroutines.Clear();


        foreach (
            GameObject effect
            in spawnedEffects
        )
        {
            if (effect != null)
            {
                Destroy(
                    effect
                );
            }
        }


        spawnedEffects.Clear();
    }


    // =========================================================
    // 🔊 焼いている間の音停止
    // =========================================================

    private void StopCookingSounds()
    {
        foreach (
            AudioSource source
            in cookingAudioSources
        )
        {
            if (source != null)
            {
                source.Stop();

                Destroy(
                    source
                );
            }
        }


        cookingAudioSources.Clear();
    }
}