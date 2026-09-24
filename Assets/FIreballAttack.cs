using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireballAttack : MonoBehaviour
{
    [Header("発射設定")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("速度")]
    public float projectileSpeed = 20f;

    [Header("手に持っている炎")]
    public GameObject handEffect;

    [Header("発射間隔")]
    public float attackCooldown = 0.5f;

    [Header("待機炎の再表示")]
    public float handEffectReturnDelay = 0.5f;

    [Header("武器切り替え")]
    public WeaponSwitch weaponSwitch;


    // =========================================================
    // 🔥 待機中の炎の音
    // =========================================================

    [Header("🔥 待機炎の音")]
    public List<AudioClip> idleFlameSounds = new List<AudioClip>();

    [Range(0f, 1f)]
    public float idleFlameVolume = 1f;

    private List<AudioSource> idleAudioSources =
        new List<AudioSource>();


    // =========================================================
    // 🔫 発射音
    // =========================================================

    [Header("🔫 発射音")]
    public List<AudioClip> shootSounds = new List<AudioClip>();

    [Range(0f, 1f)]
    public float shootSoundVolume = 1f;


    // =========================================================
    // 内部変数
    // =========================================================

    private float lastAttackTime = -999f;

    private Coroutine handEffectCoroutine;

    private bool triggerWasPressed = false;


    // =========================================================
    // 有効になったとき
    // =========================================================

    private void OnEnable()
    {
        if (handEffect != null)
        {
            handEffect.SetActive(true);
        }

        triggerWasPressed = OVRInput.Get(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        StartIdleFlameSounds();
    }


    // =========================================================
    // 無効になったとき
    // =========================================================

    private void OnDisable()
    {
        StopIdleFlameSounds();
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (weaponSwitch == null)
        {
            return;
        }

        if (weaponSwitch.CurrentWeapon != 3)
        {
            return;
        }

        bool triggerPressed = OVRInput.Get(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        if (triggerPressed && !triggerWasPressed)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ShootFireball();
            }
        }

        triggerWasPressed = triggerPressed;
    }


    // =========================================================
    // 🔥 待機炎の音を開始
    // =========================================================

    private void StartIdleFlameSounds()
    {
        StopIdleFlameSounds();

        foreach (AudioClip clip in idleFlameSounds)
        {
            if (clip == null)
                continue;

            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = idleFlameVolume;
            source.loop = true;
            source.playOnAwake = false;

            // 3Dサウンド
            source.spatialBlend = 1f;

            source.Play();

            idleAudioSources.Add(source);
        }
    }


    // =========================================================
    // 🔥 待機炎の音を停止
    // =========================================================

    private void StopIdleFlameSounds()
    {
        foreach (AudioSource source in idleAudioSources)
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source);
            }
        }

        idleAudioSources.Clear();
    }


    // =========================================================
    // 🔫 発射音
    // =========================================================

    private void PlayShootSounds()
    {
        if (shootSounds == null)
            return;

        foreach (AudioClip clip in shootSounds)
        {
            if (clip == null)
                continue;

            AudioSource.PlayClipAtPoint(
                clip,
                firePoint != null
                    ? firePoint.position
                    : transform.position,
                shootSoundVolume
            );
        }
    }


    // =========================================================
    // 🔥 炎の球を発射
    // =========================================================

    private void ShootFireball()
    {
        lastAttackTime = Time.time;

        // 待機炎を消す
        if (handEffect != null)
        {
            handEffect.SetActive(false);
        }

        // 待機炎の音を止める
        StopIdleFlameSounds();


        // 発射位置チェック
        if (firePoint == null)
        {
            Debug.LogWarning(
                "FireballAttack：Fire Pointが設定されていません！"
            );
            return;
        }

        // Prefabチェック
        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "FireballAttack：Projectile Prefabが設定されていません！"
            );
            return;
        }


        // 🔫 発射音
        PlayShootSounds();


        // =====================================================
        // 炎の球を生成
        // =====================================================

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );


        // =====================================================
        // ★★★ プレイヤーとの衝突を完全に無視 ★★★
        // =====================================================

        Collider[] projectileColliders =
            projectile.GetComponentsInChildren<Collider>();

        Collider[] playerColliders =
            transform.root.GetComponentsInChildren<Collider>();


        foreach (Collider projectileCollider in projectileColliders)
        {
            if (projectileCollider == null)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                // 自分自身のColliderなら無視
                if (projectileCollider == playerCollider)
                    continue;

                Physics.IgnoreCollision(
                    projectileCollider,
                    playerCollider,
                    true
                );
            }
        }


        // =====================================================
        // Rigidbody
        // =====================================================

        Rigidbody rb =
            projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = firePoint.forward;

            rb.linearVelocity =
                direction * projectileSpeed;
        }
        else
        {
            Debug.LogWarning(
                "FireballAttack：ProjectileにRigidbodyがありません！"
            );
        }


        Debug.Log("🔥 右トリガーで炎の球を発射！");


        // =====================================================
        // 待機炎を再表示
        // =====================================================

        if (handEffectCoroutine != null)
        {
            StopCoroutine(handEffectCoroutine);
        }

        handEffectCoroutine =
            StartCoroutine(
                ShowHandEffectAfterDelay()
            );
    }


    // =========================================================
    // 待機炎を再表示
    // =========================================================

    private IEnumerator ShowHandEffectAfterDelay()
    {
        yield return new WaitForSeconds(
            handEffectReturnDelay
        );

        if (weaponSwitch != null &&
            weaponSwitch.CurrentWeapon == 3)
        {
            if (handEffect != null)
            {
                handEffect.SetActive(true);
            }

            // 待機炎の音も再開
            StartIdleFlameSounds();
        }

        handEffectCoroutine = null;
    }
}