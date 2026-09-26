using UnityEngine;
using System.Collections.Generic;

public class EnemyWander : MonoBehaviour
{
    // =========================================================
    // 徘徊設定
    // =========================================================

    [Header("=== 徘徊設定 ===")]

    [Tooltip("敵の移動速度")]
    public float moveSpeed = 1.5f;

    [Tooltip("敵が徘徊する最大距離")]
    public float wanderRadius = 5f;

    [Tooltip("歩き続ける時間")]
    public float walkTime = 3f;

    [Tooltip("立ち止まる時間")]
    public float idleTime = 2f;


    // =========================================================
    // 向き設定
    // =========================================================

    [Header("=== 向き設定 ===")]

    [Tooltip("敵が移動方向を向く速度")]
    public float rotationSpeed = 5f;


    // =========================================================
    // Animator
    // =========================================================

    [Header("=== Animator ===")]

    [Tooltip("ゴブリンのAnimator")]
    public Animator animator;

    [Tooltip("Animator内のIdleステート名")]
    public string idleStateName = "Idle";

    [Tooltip("Animator内のWalkステート名")]
    public string walkStateName = "Walk";


    // =========================================================
    // 足音
    // =========================================================

    [Header("=== 歩行時の足音 ===")]

    [Tooltip("歩行中に鳴らす足音")]
    public List<AudioClip> walkFootstepSounds =
        new List<AudioClip>();

    [Tooltip("足音の音量")]
    [Range(0f, 3f)]
    public float footstepVolume = 1f;

    [Tooltip("足音を鳴らす間隔")]
    public float footstepInterval = 0.5f;


    // =========================================================
    // 内部変数
    // =========================================================

    private Vector3 startPosition;

    private Vector3 targetPosition;

    private bool isWalking = false;

    private float stateTimer;

    private float footstepTimer;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        startPosition = transform.position;

        StartIdle();
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        if (isWalking)
        {
            Walk();
        }
        else
        {
            if (stateTimer <= 0f)
            {
                StartWalking();
            }
        }
    }


    // =========================================================
    // 歩く
    // =========================================================

    private void Walk()
    {
        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;


        // 目的地に到着
        if (direction.magnitude < 0.2f)
        {
            StartIdle();
            return;
        }


        direction.Normalize();


        // =====================================================
        // 実際の移動
        // =====================================================

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;


        // =====================================================
        // 移動方向を向く
        // =====================================================

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }


        // =====================================================
        // 足音
        // =====================================================

        UpdateFootsteps();
    }


    // =========================================================
    // 歩き始める
    // =========================================================

    private void StartWalking()
    {
        isWalking = true;

        stateTimer = walkTime;

        footstepTimer = 0f;


        // ランダムな目的地
        Vector2 randomCircle =
            Random.insideUnitCircle *
            wanderRadius;


        targetPosition =
            startPosition +
            new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            );


        // Walkモーション
        PlayWalkAnimation();


        Debug.Log(
            "👹 " +
            gameObject.name +
            " が歩き始めた！"
        );
    }


    // =========================================================
    // 止まる
    // =========================================================

    private void StartIdle()
    {
        isWalking = false;

        stateTimer = idleTime;

        footstepTimer = 0f;


        // Idleモーション
        PlayIdleAnimation();


        Debug.Log(
            "👹 " +
            gameObject.name +
            " が立ち止まった"
        );
    }


    // =========================================================
    // Walkモーション
    // =========================================================

    private void PlayWalkAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(walkStateName))
        {
            return;
        }


        animator.CrossFade(
            walkStateName,
            0.2f
        );
    }


    // =========================================================
    // Idleモーション
    // =========================================================

    private void PlayIdleAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(idleStateName))
        {
            return;
        }


        animator.CrossFade(
            idleStateName,
            0.2f
        );
    }


    // =========================================================
    // 足音
    // =========================================================

    private void UpdateFootsteps()
    {
        if (walkFootstepSounds.Count == 0)
        {
            return;
        }


        footstepTimer -=
            Time.deltaTime;


        if (footstepTimer <= 0f)
        {
            PlayRandomFootstep();

            footstepTimer =
                footstepInterval;
        }
    }


    // =========================================================
    // 足音再生
    // =========================================================

    private void PlayRandomFootstep()
    {
        AudioClip clip =
            walkFootstepSounds[
                Random.Range(
                    0,
                    walkFootstepSounds.Count
                )
            ];


        if (clip == null)
        {
            return;
        }


        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            footstepVolume
        );
    }
}