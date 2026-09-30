using UnityEngine;

public class ZoomCamera : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 2f;

    [Header("終了距離")]
    public float stopDistance = 1f;

    [Header("対象")]
    public Transform target;

    private bool isMoving = true;

    void Update()
    {
        if (!isMoving) return;

        // 前方向へ移動
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // ターゲットが設定されている場合
        if (target != null)
        {
            float distance = Vector3.Distance(
                transform.position,
                target.position
            );

            if (distance <= stopDistance)
            {
                isMoving = false;
            }
        }
    }
}