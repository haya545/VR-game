using UnityEngine;
using System.Collections;

public class AxeHit : MonoBehaviour
{
    [Header("連続ヒット防止の時間（秒）")]
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;


    // =========================================================
    // 木の効果音
    // =========================================================

    [Header("木を叩いた音")]
    public AudioClip treeHitSound;

    [Header("木を叩いた音量")]
    [Range(0f, 1f)]
    public float treeHitVolume = 1f;


    // =========================================================
    // 振動設定
    // =========================================================

    [Header("=== 振動（ハプティクス）の設定 ===")]

    [Tooltip("斧を持っている手を指定")]
    public OVRInput.Controller targetController =
        OVRInput.Controller.RTouch;

    [Range(0f, 1f)]
    public float vibrationFrequency = 0.5f;

    [Range(0f, 1f)]
    public float vibrationAmplitude = 0.8f;

    public float vibrationDuration = 0.15f;


    // =========================================================
    // 当たり判定
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        // =====================================================
        // クールダウン
        // =====================================================

        if (Time.time - lastHitTime < hitCooldown)
        {
            return;
        }


        // =====================================================
        // 🪓 敵
        // =====================================================

        EnemyController enemy =
            other.GetComponent<EnemyController>();

        if (enemy == null)
        {
            enemy =
                other.GetComponentInParent<EnemyController>();
        }

        if (enemy != null)
        {
            Debug.Log(
                "🪓 敵に斧攻撃！ / " +
                enemy.gameObject.name
            );


            // =================================================
            // 実際に斧が当たった場所
            // =================================================

            Vector3 hitPosition =
                other.ClosestPoint(transform.position);


            // =================================================
            // 敵にダメージ
            // =================================================

            enemy.TakeAxeDamage(hitPosition);


            lastHitTime = Time.time;


            // 振動
            TriggerVibration();

            return;
        }


        // =====================================================
        // ⛏️ 鉱石
        // =====================================================

        Ore ore =
            other.GetComponent<Ore>();

        if (ore == null)
        {
            ore =
                other.GetComponentInParent<Ore>();
        }

        if (ore != null)
        {
            Debug.Log("★★ 鉱石を発見！ ★★");

            ore.HitOre();

            lastHitTime = Time.time;

            TriggerVibration();

            return;
        }


        // =====================================================
        // 🌲 木
        // =====================================================

        TreeFall tree =
            other.GetComponent<TreeFall>();

        if (tree == null)
        {
            tree =
                other.GetComponentInParent<TreeFall>();
        }

        if (tree != null)
        {
            Debug.Log("★★ 木を発見！ ★★");


            // 木を叩いた音
            if (treeHitSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    treeHitSound,
                    transform.position,
                    treeHitVolume
                );
            }


            tree.HitTree();

            lastHitTime = Time.time;

            TriggerVibration();

            return;
        }
    }


    // =========================================================
    // 振動
    // =========================================================

    private void TriggerVibration()
    {
        StopAllCoroutines();

        StartCoroutine(
            VibrateCoroutine()
        );
    }


    // =========================================================
    // 振動コルーチン
    // =========================================================

    private IEnumerator VibrateCoroutine()
    {
        OVRInput.SetControllerVibration(
            vibrationFrequency,
            vibrationAmplitude,
            targetController
        );

        yield return new WaitForSeconds(
            vibrationDuration
        );

        OVRInput.SetControllerVibration(
            0,
            0,
            targetController
        );
    }
}