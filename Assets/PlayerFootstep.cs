using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    [System.Serializable]
    public class FootstepSound
    {
        [Header("地面のTag")]
        public string groundTag;

        [Header("足音")]
        public AudioClip[] sounds;

        [Header("この地面の音量")]
        [Range(0f, 3f)]
        public float volume = 1.5f;
    }

    [Header("========== Audio設定 ==========")]
    public AudioSource audioSource;

    [Range(0f, 3f)]
    public float masterVolume = 2.0f;

    [Header("========== 足音設定 ==========")]
    public FootstepSound[] footstepSounds;

    [Tooltip("足音と足音の間隔")]
    public float stepInterval = 0.45f;

    [Header("========== 地面判定 ==========")]
    public float groundCheckDistance = 3.0f;

    [Header("========== 移動判定 ==========")]
    [Tooltip("これ以下の移動は『停止』として扱う")]
    public float minimumMoveDistance = 0.03f;

    [Header("========== デバッグ ==========")]
    public bool showDebugLog = true;
    public bool showMovementDebug = false;
    public bool showGroundDebug = false;
    public bool showPositionDebug = false;
    public bool showRayDebug = true;

    private float stepTimer = 0f;
    private Vector3 lastPosition;
    private float positionDebugTimer = 0f;

    private bool wasMoving = false;


    void Start()
    {
        lastPosition = transform.position;

        Debug.Log("👣 PlayerFootstep 起動！");
        Debug.Log("Player：" + gameObject.name);

        if (audioSource == null)
        {
            Debug.LogError(
                "❌ PlayerFootstep：Audio Sourceが設定されていません！"
            );
        }
    }


    void Update()
    {
        Vector3 currentPosition = transform.position;


        // ==================================================
        // X・Z方向の移動量を計算
        // ==================================================

        Vector3 horizontalMovement =
            new Vector3(
                currentPosition.x - lastPosition.x,
                0f,
                currentPosition.z - lastPosition.z
            );

        float moveDistance =
            horizontalMovement.magnitude;


        // ==================================================
        // ★ 微小な揺れを無視する
        // ==================================================

        bool isMoving =
            moveDistance >= minimumMoveDistance;


        // ==================================================
        // 地面判定
        // ==================================================

        RaycastHit hit;

        Vector3 rayStart =
            transform.position +
            Vector3.up * 0.2f;

        bool isGrounded =
            Physics.Raycast(
                rayStart,
                Vector3.down,
                out hit,
                groundCheckDistance
            );


        // ==================================================
        // ★ 停止した場合
        // ==================================================

        if (!isMoving)
        {
            // 足音タイマーをリセット
            stepTimer = 0f;

            // 移動状態を解除
            wasMoving = false;

            // ★ 現在再生中の足音も停止
            if (
                audioSource != null &&
                audioSource.isPlaying
            )
            {
                audioSource.Stop();

                if (showDebugLog)
                {
                    Debug.Log("⛔ プレイヤー停止 → 足音停止");
                }
            }

            // 現在位置を保存
            lastPosition = currentPosition;

            return;
        }


        // ==================================================
        // 移動開始
        // ==================================================

        if (!wasMoving)
        {
            stepTimer = 0f;
            wasMoving = true;

            if (showDebugLog)
            {
                Debug.Log("👣 プレイヤー移動開始！");
            }
        }


        // ==================================================
        // 足音処理
        // ==================================================

        if (isMoving && isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep(hit.collider);

                stepTimer =
                    stepInterval;
            }
        }


        // ==================================================
        // デバッグ
        // ==================================================

        if (showMovementDebug)
        {
            Debug.Log(
                "👣 移動中 / 移動量：" +
                moveDistance
            );
        }


        if (showGroundDebug)
        {
            if (isGrounded)
            {
                Debug.Log(
                    "🟢 地面：" +
                    hit.collider.gameObject.name +
                    " / Tag：" +
                    hit.collider.tag
                );
            }
            else
            {
                Debug.Log(
                    "🔴 地面なし"
                );
            }
        }


        if (showPositionDebug)
        {
            positionDebugTimer -=
                Time.deltaTime;

            if (positionDebugTimer <= 0f)
            {
                Debug.Log(
                    "📍 Player座標：" +
                    currentPosition +
                    " / 移動量：" +
                    moveDistance
                );

                positionDebugTimer = 1f;
            }
        }


        // ==================================================
        // 現在位置を保存
        // ==================================================

        lastPosition =
            currentPosition;
    }


    // ==================================================
    // 足音再生
    // ==================================================

    void PlayFootstep(Collider ground)
    {
        if (audioSource == null)
        {
            Debug.LogError(
                "❌ AudioSourceがありません！"
            );

            return;
        }


        if (ground == null)
        {
            return;
        }


        // ==================================================
        // 前の足音が鳴っている場合はスキップ
        // ==================================================

        if (audioSource.isPlaying)
        {
            return;
        }


        string groundTag =
            ground.tag;


        // ==================================================
        // 地面タイプ検索
        // ==================================================

        foreach (
            FootstepSound footstep
            in footstepSounds
        )
        {
            if (
                footstep.groundTag
                !=
                groundTag
            )
            {
                continue;
            }


            if (
                footstep.sounds == null ||
                footstep.sounds.Length == 0
            )
            {
                Debug.LogWarning(
                    "⚠ 足音が設定されていません！" +
                    " / Tag：" +
                    groundTag
                );

                return;
            }


            // ==================================================
            // ランダムな足音を選択
            // ==================================================

            int randomIndex =
                Random.Range(
                    0,
                    footstep.sounds.Length
                );


            AudioClip clip =
                footstep.sounds[randomIndex];


            if (clip == null)
            {
                return;
            }


            // ==================================================
            // 音量
            // ==================================================

            float finalVolume =
                masterVolume *
                footstep.volume;


            // ==================================================
            // 再生
            // ==================================================

            audioSource.clip =
                clip;

            audioSource.volume =
                finalVolume;

            audioSource.Play();


            if (showDebugLog)
            {
                Debug.Log(
                    "🔊 足音再生！" +
                    " / 地面：" +
                    groundTag +
                    " / 音：" +
                    clip.name +
                    " / 音量：" +
                    finalVolume
                );
            }


            return;
        }


        if (showDebugLog)
        {
            Debug.LogWarning(
                "⚠ 対応する足音がありません！" +
                " / Tag：" +
                groundTag
            );
        }
    }


    // ==================================================
    // Gizmo
    // ==================================================

    void OnDrawGizmosSelected()
    {
        if (!showRayDebug)
        {
            return;
        }


        Vector3 rayStart =
            transform.position +
            Vector3.up * 0.2f;


        Vector3 rayEnd =
            rayStart +
            Vector3.down *
            groundCheckDistance;


        Gizmos.color =
            Color.yellow;


        Gizmos.DrawLine(
            rayStart,
            rayEnd
        );


        Gizmos.DrawSphere(
            rayStart,
            0.05f
        );


        Gizmos.DrawSphere(
            rayEnd,
            0.05f
        );
    }
}