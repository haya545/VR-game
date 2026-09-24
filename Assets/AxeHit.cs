using UnityEngine;

public class AxeHit : MonoBehaviour
{
    [Header("連続ヒット防止の時間（秒）")]
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    [Header("木を叩いた音")]
    public AudioClip treeHitSound;

    [Header("木を叩いた音量")]
    [Range(0f, 1f)]
    public float treeHitVolume = 1f;


    private void OnTriggerEnter(Collider other)
    {
        // =========================
        // クールダウン
        // =========================

        if (Time.time - lastHitTime < hitCooldown)
        {
            return;
        }


        // =========================
        // 鉱石
        // =========================

        Ore ore = other.GetComponent<Ore>();

        if (ore == null)
        {
            ore = other.GetComponentInParent<Ore>();
        }

        if (ore != null)
        {
            Debug.Log("★★ 鉱石を発見！ ★★");

            ore.HitOre();

            lastHitTime = Time.time;

            return;
        }


        // =========================
        // 木
        // =========================

        TreeFall tree = other.GetComponent<TreeFall>();

        if (tree == null)
        {
            tree = other.GetComponentInParent<TreeFall>();
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

            return;
        }
    }
}