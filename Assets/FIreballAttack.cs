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

    // 動的追加をやめ、生成したAudioSourceを初期化時に保持して再利用する
    private List<AudioSource> idleAudioSources = new List<AudioSource>();

    // =========================================================
    // 🔫 発射音
    // =========================================================
    [Header("🔫 発射音")]
    public List<AudioClip> shootSounds = new List<AudioClip>();

    [Range(0f, 1f)]
    public float shootSoundVolume = 1f;

    // =========================================================
    // 📳 振動設定
    // =========================================================
    [Header("========== 振動設定 ==========")]
    [Header("発射時の振動時間（秒）")]
    public float shootVibrationDuration = 0.2f;
    [Header("発射時の振動周波数 (0～1)")]
    [Range(0f, 1f)] public float shootVibrationFrequency = 0.7f;
    [Header("発射時の振動強さ (0～1)")]
    [Range(0f, 1f)] public float shootVibrationAmplitude = 0.8f;

    // =========================================================
    // 内部変数
    // =========================================================
    private float lastAttackTime = -999f;
    private Coroutine handEffectCoroutine;
    private bool triggerWasPressed = false;

    private void Awake()
    {
        // 最初に必要な数だけAudioSourceを作っておき、使い回す
        InitializeIdleAudioSources();
    }

    private void InitializeIdleAudioSources()
    {
        foreach (AudioClip clip in idleFlameSounds)
        {
            if (clip == null) continue;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = idleFlameVolume;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 1f; // 3Dサウンド

            idleAudioSources.Add(source);
        }
    }

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

    private void OnDisable()
    {
        StopIdleFlameSounds();
    }

    private void Update()
    {
        if (weaponSwitch == null || weaponSwitch.CurrentWeapon != 3)
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
        foreach (AudioSource source in idleAudioSources)
        {
            if (source != null && !source.isPlaying)
            {
                source.volume = idleFlameVolume;
                source.Play();
            }
        }
    }

    // =========================================================
    // 🔥 待機炎の音を停止
    // =========================================================
    private void StopIdleFlameSounds()
    {
        foreach (AudioSource source in idleAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    // =========================================================
    // 🔫 発射音
    // =========================================================
    private void PlayShootSounds()
    {
        if (shootSounds == null) return;

        Vector3 playPos = firePoint != null ? firePoint.position : transform.position;

        foreach (AudioClip clip in shootSounds)
        {
            if (clip == null) continue;
            AudioSource.PlayClipAtPoint(clip, playPos, shootSoundVolume);
        }
    }

    // =========================================================
    // 📳 振動制御コルーチン
    // =========================================================
    private IEnumerator VibrateController(float duration, float frequency, float amplitude, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    // =========================================================
    // 🔥 炎の球を発射
    // =========================================================
    private void ShootFireball()
    {
        lastAttackTime = Time.time;

        // 発射時の振動
        StartCoroutine(VibrateController(shootVibrationDuration, shootVibrationFrequency, shootVibrationAmplitude, OVRInput.Controller.RTouch));

        // 待機炎・音を消す
        if (handEffect != null)
        {
            handEffect.SetActive(false);
        }
        StopIdleFlameSounds();

        // 発射位置・Prefabチェック
        if (firePoint == null || projectilePrefab == null)
        {
            Debug.LogWarning("FireballAttack：FirePoint または ProjectilePrefab が設定されていません！");
            return;
        }

        // 発射音
        PlayShootSounds();

        // 炎の球を生成
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 衝突無視設定
        Collider[] projectileColliders = projectile.GetComponentsInChildren<Collider>();
        Collider[] playerColliders = transform.root.GetComponentsInChildren<Collider>();

        foreach (Collider projectileCollider in projectileColliders)
        {
            if (projectileCollider == null) continue;
            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null || projectileCollider == playerCollider) continue;
                Physics.IgnoreCollision(projectileCollider, playerCollider, true);
            }
        }

        // 速度設定
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        // 待機炎の再表示タイマーリセット
        if (handEffectCoroutine != null)
        {
            StopCoroutine(handEffectCoroutine);
        }

        handEffectCoroutine = StartCoroutine(ShowHandEffectAfterDelay());
    }

    // =========================================================
    // 待機炎を再表示
    // =========================================================
    private IEnumerator ShowHandEffectAfterDelay()
    {
        yield return new WaitForSeconds(handEffectReturnDelay);

        if (weaponSwitch != null && weaponSwitch.CurrentWeapon == 3)
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