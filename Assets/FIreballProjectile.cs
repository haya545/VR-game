using UnityEngine;
using System.Collections.Generic;

public class FireballProjectile : MonoBehaviour
{
    [Header("射程距離")]
    public float maxRange = 30f;

    [Header("着弾エフェクト")]
    public GameObject hitEffect;

    [Header("着弾エフェクトの表示時間")]
    public float hitEffectLifetime = 2f;


    // =========================================================
    // 🔥 飛んでいる炎の音
    // =========================================================

    [Header("🔥 飛んでいる炎の音")]
    public List<AudioClip> flyingSounds = new List<AudioClip>();

    [Range(0f, 1f)]
    public float flyingSoundVolume = 1f;

    private List<AudioSource> flyingAudioSources =
        new List<AudioSource>();


    // =========================================================
    // 💥 着弾音
    // =========================================================

    [Header("💥 着弾音")]
    public List<AudioClip> hitSounds = new List<AudioClip>();

    // ★ 最大音量を3.0に変更
    [Range(0f, 3f)]
    public float hitSoundVolume = 1f;


    // =========================================================
    // 内部変数
    // =========================================================

    private Vector3 startPosition;

    private bool hasHit = false;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        startPosition = transform.position;

        StartFlyingSounds();
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        float distance = Vector3.Distance(
            startPosition,
            transform.position
        );

        // 最大射程に到達
        if (distance >= maxRange)
        {
            StopFlyingSounds();

            Destroy(gameObject);
        }
    }


    // =========================================================
    // Collision
    // =========================================================

    private void OnCollisionEnter(Collision collision)
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
    // Trigger
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        // =====================================================
        // 🎵 BGMZoneは完全に無視
        // =====================================================

        if (other.GetComponent<BGMZone>() != null)
        {
            return;
        }


        // =====================================================
        // 🛡 Playerは無視
        // =====================================================

        if (IsPlayer(other.gameObject))
        {
            return;
        }


        Vector3 hitPosition =
            other.ClosestPoint(transform.position);

        Hit(
            hitPosition,
            other.gameObject
        );
    }


    // =========================================================
    // Player判定
    // =========================================================

    private bool IsPlayer(GameObject obj)
    {
        // 自分自身
        if (obj == gameObject)
        {
            return true;
        }

        // 直接Player
        if (obj.CompareTag("Player"))
        {
            return true;
        }

        // 親を確認
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            if (parent.CompareTag("Player"))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }


    // =========================================================
    // 💥 着弾
    // =========================================================

    private void Hit(
        Vector3 hitPosition,
        GameObject hitObject
    )
    {
        // 二重着弾防止
        if (hasHit)
        {
            return;
        }

        hasHit = true;


        // =====================================================
        // 🔥 飛行音停止
        // =====================================================

        StopFlyingSounds();


        // =====================================================
        // 💥 着弾音
        // =====================================================

        foreach (AudioClip clip in hitSounds)
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


        // =====================================================
        // 💥 着弾エフェクト
        // =====================================================

        if (hitEffect != null)
        {
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


        // =====================================================
        // 🔥 炎の球を削除
        // =====================================================

        Destroy(gameObject);
    }


    // =========================================================
    // 🔥 飛行音開始
    // =========================================================

    private void StartFlyingSounds()
    {
        StopFlyingSounds();

        foreach (AudioClip clip in flyingSounds)
        {
            if (clip == null)
            {
                continue;
            }

            AudioSource source =
                gameObject.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = flyingSoundVolume;
            source.loop = true;
            source.playOnAwake = false;

            // 3Dサウンド
            source.spatialBlend = 1f;

            source.Play();

            flyingAudioSources.Add(source);
        }
    }


    // =========================================================
    // 🔇 飛行音停止
    // =========================================================

    private void StopFlyingSounds()
    {
        foreach (AudioSource source in flyingAudioSources)
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