using UnityEngine;
using System.Collections.Generic;

public class FireballProjectile : MonoBehaviour
{
    // =========================================================
    // 射程
    // =========================================================

    [Header("射程距離")]
    public float maxRange = 30f;


    // =========================================================
    // 着弾エフェクト
    // =========================================================

    [Header("着弾エフェクト")]
    public GameObject hitEffect;

    [Header("着弾エフェクトの表示時間")]
    public float hitEffectLifetime = 2f;


    // =========================================================
    // 🔥 飛んでいる炎の音
    // =========================================================

    [Header("🔥 飛んでいる炎の音")]

    public List<AudioClip> flyingSounds =
        new List<AudioClip>();

    [Range(0f, 1f)]
    public float flyingSoundVolume = 1f;

    private List<AudioSource> flyingAudioSources =
        new List<AudioSource>();


    // =========================================================
    // 💥 着弾音
    // =========================================================

    [Header("💥 着弾音")]

    public List<AudioClip> hitSounds =
        new List<AudioClip>();

    [Range(0f, 3f)]
    public float hitSoundVolume = 1f;


    // =========================================================
    // 内部
    // =========================================================

    private Vector3 startPosition;

    private bool hasHit = false;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        startPosition =
            transform.position;

        StartFlyingSounds();
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        float distance =
            Vector3.Distance(
                startPosition,
                transform.position
            );


        if (distance >= maxRange)
        {
            StopFlyingSounds();

            Destroy(gameObject);
        }
    }


    // =========================================================
    // 💥 Collision
    // =========================================================

    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (IsPlayer(collision.gameObject))
        {
            return;
        }


        Vector3 hitPosition =
            collision.GetContact(0).point;


        Hit(
            hitPosition,
            collision.gameObject
        );
    }


    // =========================================================
    // 💥 Trigger
    // =========================================================

    private void OnTriggerEnter(
        Collider other
    )
    {
        // BGM Zoneは無視
        if (
            other.GetComponent<BGMZone>() != null
        )
        {
            return;
        }


        // Playerは無視
        if (
            IsPlayer(other.gameObject)
        )
        {
            return;
        }


        Vector3 hitPosition =
            other.ClosestPoint(
                transform.position
            );


        Hit(
            hitPosition,
            other.gameObject
        );
    }


    // =========================================================
    // Player判定
    // =========================================================

    private bool IsPlayer(
        GameObject obj
    )
    {
        if (obj == gameObject)
        {
            return true;
        }


        if (obj.CompareTag("Player"))
        {
            return true;
        }


        Transform current =
            obj.transform;


        while (current != null)
        {
            if (
                current.CompareTag("Player")
            )
            {
                return true;
            }


            current =
                current.parent;
        }


        return false;
    }


    // =========================================================
    // 💥 命中処理
    // =========================================================

    private void Hit(
        Vector3 hitPosition,
        GameObject hitObject
    )
    {
        if (hasHit)
        {
            return;
        }


        hasHit = true;


        Debug.Log(
            "🔥 炎が命中！" +
            " / 対象=" +
            hitObject.name +
            " / 位置=" +
            hitPosition
        );


        // =====================================================
        // 🔥🔥🔥 肉判定
        // =====================================================

        MeatCookable meat =
            FindMeatCookable(
                hitObject
            );


        if (meat != null)
        {
            Debug.Log(
                "🔥🔥🔥 炎が肉に命中しました！" +
                " / 肉=" +
                meat.gameObject.name
            );


            // 肉を焼く
            meat.StartCooking();


            // 炎の飛翔音を停止
            StopFlyingSounds();


            // 着弾エフェクト
            PlayHitEffect(
                hitPosition
            );


            // 着弾音
            PlayHitSounds(
                hitPosition
            );


            // 炎を削除
            Destroy(gameObject);


            return;
        }


        // =====================================================
        // 👹 敵判定
        // =====================================================

        EnemyController enemy =
            FindEnemyController(
                hitObject
            );


        if (enemy != null)
        {
            Debug.Log(
                "🔥🔥🔥 炎が敵に命中！" +
                " / 対象=" +
                enemy.gameObject.name +
                " / ダメージ=" +
                enemy.fireDamage +
                " / 着弾位置=" +
                hitPosition
            );


            enemy.TakeFireDamage(
                hitPosition
            );
        }
        else
        {
            Debug.Log(
                "🔥 炎が命中しましたが、" +
                "敵も肉も見つかりません。"
            );
        }


        // =====================================================
        // 着弾処理
        // =====================================================

        StopFlyingSounds();


        PlayHitEffect(
            hitPosition
        );


        PlayHitSounds(
            hitPosition
        );


        Destroy(gameObject);
    }


    // =========================================================
    // 🥩 肉検索
    // =========================================================

    private MeatCookable FindMeatCookable(
        GameObject hitObject
    )
    {
        if (hitObject == null)
        {
            return null;
        }


        // 直接
        MeatCookable meat =
            hitObject.GetComponent<MeatCookable>();


        if (meat != null)
        {
            return meat;
        }


        // 親を検索
        Transform current =
            hitObject.transform.parent;


        while (current != null)
        {
            meat =
                current.GetComponent<MeatCookable>();


            if (meat != null)
            {
                return meat;
            }


            current =
                current.parent;
        }


        // Rootを検索
        Transform root =
            hitObject.transform.root;


        if (root != null)
        {
            meat =
                root.GetComponent<MeatCookable>();


            if (meat != null)
            {
                return meat;
            }
        }


        return null;
    }


    // =========================================================
    // 👹 敵検索
    // =========================================================

    private EnemyController FindEnemyController(
        GameObject hitObject
    )
    {
        if (hitObject == null)
        {
            return null;
        }


        EnemyController enemy =
            hitObject.GetComponent<EnemyController>();


        if (enemy != null)
        {
            return enemy;
        }


        Transform current =
            hitObject.transform.parent;


        while (current != null)
        {
            enemy =
                current.GetComponent<EnemyController>();


            if (enemy != null)
            {
                return enemy;
            }


            current =
                current.parent;
        }


        Transform root =
            hitObject.transform.root;


        if (root != null)
        {
            enemy =
                root.GetComponent<EnemyController>();


            if (enemy != null)
            {
                return enemy;
            }
        }


        return null;
    }


    // =========================================================
    // 💥 着弾エフェクト
    // =========================================================

    private void PlayHitEffect(
        Vector3 hitPosition
    )
    {
        if (hitEffect == null)
        {
            return;
        }


        GameObject effect =
            Instantiate(
                hitEffect,
                hitPosition,
                Quaternion.identity
            );


        Destroy(
            effect,
            hitEffectLifetime
        );
    }


    // =========================================================
    // 🔊 着弾音
    // =========================================================

    private void PlayHitSounds(
        Vector3 hitPosition
    )
    {
        foreach (
            AudioClip clip
            in hitSounds
        )
        {
            if (clip == null)
            {
                continue;
            }


            AudioSource.PlayClipAtPoint(
                clip,
                hitPosition,
                hitSoundVolume
            );
        }
    }


    // =========================================================
    // 🔥 飛行音開始
    // =========================================================

    private void StartFlyingSounds()
    {
        StopFlyingSounds();


        foreach (
            AudioClip clip
            in flyingSounds
        )
        {
            if (clip == null)
            {
                continue;
            }


            AudioSource source =
                gameObject.AddComponent<AudioSource>();


            source.clip = clip;

            source.volume =
                flyingSoundVolume;

            source.loop = true;

            source.playOnAwake = false;

            source.spatialBlend = 1f;


            source.Play();


            flyingAudioSources.Add(
                source
            );
        }
    }


    // =========================================================
    // 🔇 飛行音停止
    // =========================================================

    private void StopFlyingSounds()
    {
        foreach (
            AudioSource source
            in flyingAudioSources
        )
        {
            if (source != null)
            {
                source.Stop();

                Destroy(source);
            }
        }


        flyingAudioSources.Clear();
    }
}