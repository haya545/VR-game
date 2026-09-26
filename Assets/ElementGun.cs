using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElementGun : MonoBehaviour
{
    // =========================================================
    // 元素データ
    // =========================================================
    [System.Serializable]
    public class AirElement
    {
        [Header("元素名")]
        public string elementName = "Nitrogen";

        [Header("表示名")]
        public string displayName = "窒素";

        [Header("ドロップするPrefab")]
        public GameObject elementPrefab;

        [Header("アイコン")]
        public Sprite icon;

        [Header("ドロップ確率（%）")]
        [Range(0f, 100f)]
        public float dropChance = 33.3f;
    }


    // =========================================================
    // 通常射撃
    // =========================================================
    [Header("========== 通常射撃 ==========")]

    [Header("射程距離")]
    public float range = 50f;


    // =========================================================
    // 発射エフェクト
    // =========================================================
    [Header("発射エフェクト")]
    public GameObject shootEffect;

    [Header("エフェクトを表示する場所")]
    public Transform effectPoint;

    [Header("エフェクトの表示時間")]
    public float effectLifetime = 1f;


    // =========================================================
    // レーザー
    // =========================================================
    [Header("レーザーポインター")]
    public LineRenderer laser;

    [Header("レーザーの太さ")]
    public float laserWidth = 0.01f;


    // =========================================================
    // 空中元素取得
    // =========================================================
    [Header("========== 空中元素取得 ==========")]

    [Header("最低上向き角度")]
    [Tooltip("水平から何度以上上を向いたら元素取得するか")]
    [Range(0f, 90f)]
    public float minUpAngle = 70f;

    [Header("最高上向き角度")]
    [Tooltip("水平から何度までを元素取得するか")]
    [Range(0f, 90f)]
    public float maxUpAngle = 90f;


    // =========================================================
    // ドロップ位置
    // =========================================================
    [Header("========== ドロップ位置 ==========")]

    [Header("前方距離")]
    [Tooltip("銃から前方に何m離れた場所に生成するか")]
    public float dropForwardDistance = 3f;

    [Header("ドロップ高さ")]
    [Tooltip("銃より何m上に生成するか")]
    public float dropHeight = 2f;


    // =========================================================
    // 元素
    // =========================================================
    [Header("========== ドロップ元素 ==========")]

    [Tooltip("＋ボタンから元素を追加できます")]
    public List<AirElement> airElements = new List<AirElement>();


    // =========================================================
    // ドロップエフェクト
    // =========================================================
    [Header("========== ドロップエフェクト ==========")]

    public GameObject dropEffect;

    [Header("エフェクト表示時間")]
    public float dropEffectLifetime = 2f;


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
    // 効果音
    // =========================================================
    [Header("========== 効果音 ==========")]

    [Header("🔫 トリガーを引いた音")]
    public AudioClip triggerSound;

    [Header("トリガー音の音量")]
    [Range(0f, 1f)]
    public float triggerSoundVolume = 1f;


    [Header("🎯 木などに命中した音")]
    public AudioClip hitSound;

    [Header("命中音の音量")]
    [Range(0f, 1f)]
    public float hitSoundVolume = 1f;


    [Header("☁️ 空中元素生成音")]
    public AudioClip dropSound;

    [Header("元素生成音の音量")]
    [Range(0f, 1f)]
    public float dropSoundVolume = 1f;


    // =========================================================
    // Start
    // =========================================================
    private void Start()
    {
        // レーザー設定
        if (laser != null)
        {
            laser.enabled = false;
            laser.startWidth = laserWidth;
            laser.endWidth = laserWidth;
        }
    }


    // =========================================================
    // Update
    // =========================================================
    private void Update()
    {
        // レーザーポインター
        UpdateLaser();

        // 右コントローラーの右トリガー
        if (OVRInput.GetDown(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch))
        {
            Shoot();
        }
    }


    // =========================================================
    // レーザーポインター
    // =========================================================
    private void UpdateLaser()
    {
        if (laser == null || effectPoint == null)
        {
            return;
        }

        laser.enabled = true;

        Vector3 startPosition = effectPoint.position;
        Vector3 direction = effectPoint.forward;
        Vector3 endPosition = startPosition + direction * range;

        RaycastHit hit;

        if (Physics.Raycast(
            startPosition,
            direction,
            out hit,
            range))
        {
            endPosition = hit.point;
        }

        laser.SetPosition(0, startPosition);
        laser.SetPosition(1, endPosition);
    }


    // =========================================================
    // 銃を撃つ
    // =========================================================
    private void Shoot()
    {
        Debug.Log("================================");
        Debug.Log("🔫 銃を撃った！");
        Debug.Log("================================");

        // ★ 発射時のコントローラー振動を発生させる
        StartCoroutine(VibrateController(shootVibrationDuration, shootVibrationFrequency, shootVibrationAmplitude, OVRInput.Controller.RTouch));

        // 🔊 トリガー音
        if (triggerSound != null)
        {
            AudioSource.PlayClipAtPoint(
                triggerSound,
                transform.position,
                triggerSoundVolume
            );
        }

        // 発射エフェクト
        PlayShootEffect();

        // Rayの発射位置
        Transform rayPoint =
            effectPoint != null
            ? effectPoint
            : transform;

        Vector3 direction = rayPoint.forward;

        // 上向き角度を計算
        float upAngle =
            Mathf.Asin(
                Mathf.Clamp(
                    direction.y,
                    -1f,
                    1f
                )
            ) * Mathf.Rad2Deg;

        Debug.Log("🔭 現在の上向き角度：" + upAngle.ToString("F1") + "°");
        Debug.Log("📐 判定範囲：" + minUpAngle + "° ～ " + maxUpAngle + "°");

        // 空中元素取得判定
        if (upAngle >= minUpAngle &&
            upAngle <= maxUpAngle)
        {
            Debug.Log("☁️ 空中元素取得の角度です！");
            TryDropAirElement(rayPoint);
        }
        else
        {
            Debug.Log("➡️ 空中元素取得の角度ではありません");
        }

        // 通常のRaycast
        Ray ray = new Ray(
            rayPoint.position,
            direction
        );

        Debug.DrawRay(
            ray.origin,
            ray.direction * range,
            Color.red,
            2f
        );

        RaycastHit hit;

        if (Physics.Raycast(
            ray,
            out hit,
            range))
        {
            Debug.Log("🎯 命中：" + hit.collider.gameObject.name);

            // 🎯 命中音
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    hitSound,
                    hit.point,
                    hitSoundVolume
                );
            }

            // ElementSourceを探す
            ElementSource elementSource =
                hit.collider.GetComponentInParent<ElementSource>();

            if (elementSource != null)
            {
                Debug.Log("✨ ElementSourceを発見！");
                elementSource.Convert(hit.point);
            }
            else
            {
                Debug.Log("⚠ 命中先にElementSourceはありません");
            }
        }
        else
        {
            Debug.Log("💨 何にも当たっていません");
        }
    }


    // =========================================================
    // 発射エフェクト
    // =========================================================
    private void PlayShootEffect()
    {
        if (shootEffect == null)
        {
            return;
        }

        Transform spawnPoint =
            effectPoint != null
            ? effectPoint
            : transform;

        GameObject effect =
            Instantiate(
                shootEffect,
                spawnPoint.position,
                spawnPoint.rotation
            );

        Destroy(
            effect,
            effectLifetime
        );

        Debug.Log("✨ 発射エフェクトを生成しました！");
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
    // 空中元素ドロップ判定
    // =========================================================
    private void TryDropAirElement(Transform rayPoint)
    {
        Debug.Log("🎲 空中元素の抽選開始");

        if (airElements == null || airElements.Count == 0)
        {
            Debug.LogError("❌ Air Elementsに元素が登録されていません！");
            return;
        }

        float totalChance = 0f;

        foreach (AirElement element in airElements)
        {
            if (element == null)
            {
                continue;
            }

            totalChance += Mathf.Max(0f, element.dropChance);

            Debug.Log(
                "🧪 " + element.displayName +
                " / " + element.dropChance +
                "% / Prefab：" +
                (element.elementPrefab != null ? element.elementPrefab.name : "未設定")
            );
        }

        Debug.Log("📊 合計確率：" + totalChance + "%");

        if (totalChance <= 0f)
        {
            Debug.LogError("❌ 合計ドロップ確率が0です！");
            return;
        }

        float randomValue = Random.Range(0f, totalChance);

        Debug.Log("🎲 抽選結果：" + randomValue.ToString("F2") + " / " + totalChance.ToString("F2"));

        float currentChance = 0f;

        foreach (AirElement element in airElements)
        {
            if (element == null)
            {
                continue;
            }

            currentChance += Mathf.Max(0f, element.dropChance);

            if (randomValue <= currentChance)
            {
                Debug.Log("🎯 当選：" + element.displayName);
                DropElement(element, rayPoint);
                return;
            }
        }

        Debug.LogWarning("⚠ 元素の抽選に失敗しました");
    }


    // =========================================================
    // 元素Prefabを生成
    // =========================================================
    private void DropElement(AirElement element, Transform rayPoint)
    {
        if (element.elementPrefab == null)
        {
            Debug.LogError("❌ " + element.displayName + " のPrefabが設定されていません！");
            return;
        }

        Vector3 forwardOffset = rayPoint.forward * dropForwardDistance;
        Vector3 heightOffset = Vector3.up * dropHeight;
        Vector3 spawnPosition = rayPoint.position + forwardOffset + heightOffset;

        Debug.Log("📍 元素生成位置：" + spawnPosition);

        GameObject droppedElement =
            Instantiate(
                element.elementPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Debug.Log("🧪 " + element.displayName + " ×1 を生成しました！");

        Rigidbody rb = droppedElement.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = true;

            // Unity 6対応
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Debug.Log("⬇ Rigidbodyの重力を有効にしました");
        }
        else
        {
            Debug.LogWarning("⚠ " + element.displayName + " PrefabにRigidbodyがありません！");
        }

        PlayDropEffect(spawnPosition);
        PlayDropSound(spawnPosition);
    }


    // =========================================================
    // ドロップエフェクト
    // =========================================================
    private void PlayDropEffect(Vector3 position)
    {
        if (dropEffect == null)
        {
            return;
        }

        GameObject effect =
            Instantiate(
                dropEffect,
                position,
                Quaternion.identity
            );

        Destroy(effect, dropEffectLifetime);

        Debug.Log("✨ ドロップエフェクト再生");
    }


    // =========================================================
    // ドロップ効果音
    // =========================================================
    private void PlayDropSound(Vector3 position)
    {
        if (dropSound == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            dropSound,
            position,
            dropSoundVolume
        );

        Debug.Log("🔊 ドロップ効果音再生");
    }
}