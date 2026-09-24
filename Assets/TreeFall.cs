using UnityEngine;
using System.Collections;

public class TreeFall : MonoBehaviour
{
    // =========================
    // 木の耐久
    // =========================

    public int treeHP = 3;


    // =========================
    // 木材ドロップ
    // =========================

    public GameObject woodPrefab;

    public int dropCount = 3;


    // =========================
    // 倒れ済み
    // =========================

    private bool isFallen = false;


    // =========================
    // 木が倒れる音
    // =========================

    [Header("木が倒れる音")]
    public AudioClip fallSound;

    [Header("木が倒れる音量")]
    [Range(0f, 1f)]
    public float fallVolume = 1f;


    // =========================
    // 斧が木に当たった
    // =========================

    public void HitTree()
    {
        if (isFallen)
            return;

        treeHP--;

        Debug.Log(
            "斧が木に命中！ 木HP : " +
            treeHP
        );

        if (treeHP <= 0)
        {
            StartCoroutine(FallTree());
        }
    }


    // =========================
    // 木を倒す
    // =========================

    IEnumerator FallTree()
    {
        isFallen = true;

        // =========================
        // 木が倒れ始める音
        // =========================

        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(
                fallSound,
                transform.position,
                fallVolume
            );
        }


        // =========================
        // 左右ランダム
        // =========================

        int direction =
            Random.value > 0.5f
            ? 1
            : -1;


        // =========================
        // 横に90度倒す
        // =========================

        float rotate = 0;

        while (rotate < 90f)
        {
            float speed =
                Mathf.Lerp(
                    20f,
                    120f,
                    rotate / 90f
                ) *
                Time.deltaTime;

            transform.Rotate(
                speed * direction,
                0,
                0
            );

            rotate += speed;

            yield return null;
        }


        // =========================
        // 少し前に回転
        // =========================

        float front = 0;

        while (front < 20f)
        {
            float speed =
                50f *
                Time.deltaTime;

            transform.Rotate(
                0,
                0,
                speed
            );

            front += speed;

            yield return null;
        }


        // =========================
        // 木材ドロップ
        // =========================

        DropWood();


        // 少し待つ

        yield return new WaitForSeconds(
            0.5f
        );


        // =========================
        // 木を削除
        // =========================

        Destroy(gameObject);
    }


    // =========================
    // 木材生成
    // =========================

    void DropWood()
    {
        Debug.Log(
            "薪スポーン基準位置: " +
            transform.position
        );

        for (
            int i = 0;
            i < dropCount;
            i++
        )
        {
            Vector3 pos =
                transform.position;

            pos += new Vector3(
                Random.Range(
                    -0.5f,
                    0.5f
                ),
                0.5f,
                Random.Range(
                    -0.5f,
                    0.5f
                )
            );

            GameObject wood =
                Instantiate(
                    woodPrefab,
                    pos,
                    Random.rotation
                );

            Rigidbody rb =
                wood.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 force =
                    new Vector3(
                        Random.Range(
                            -2f,
                            2f
                        ),
                        Random.Range(
                            2f,
                            4f
                        ),
                        Random.Range(
                            -2f,
                            2f
                        )
                    );

                rb.AddForce(
                    force,
                    ForceMode.Impulse
                );
            }
        }
    }
}