using UnityEngine;

public class MolecularCraftingTrigger : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("表示する確認画面")]
    public GameObject confirmPanel;

    [Header("レシピ一覧画面")]
    public GameObject recipePanel;

    [Header("合成中画面")]
    public GameObject synthesisPanel;

    [Header("クラフトUI全体")]
    public Transform craftingCanvas;

    [Header("クラフトUI制御")]
    public MolecularCraftingUI molecularCraftingUI;


    // =========================================================
    // VRカメラ
    // =========================================================

    [Header("VRカメラ")]
    public Transform centerEyeAnchor;


    // =========================================================
    // 表示位置
    // =========================================================

    [Header("クラフト台からの表示距離")]
    public float distanceFromMachine = 1.0f;

    [Tooltip("上方向へのオフセット")]
    public float verticalOffset = 0.5f;


    // =========================================================
    // ★ クラフトUIの角度
    // =========================================================

    [Header("クラフトUIの角度")]

    [Tooltip("プレイヤー方向を向いた後に追加する回転角度")]
    public Vector3 canvasRotationOffset = new Vector3(0, 180, 0);


    // =========================================================
    // プレイヤー
    // =========================================================

    [Header("プレイヤー")]
    public Transform player;

    [Header("表示距離")]
    public float interactionDistance = 3.0f;


    // =========================================================
    // 状態
    // =========================================================

    [Header("確認画面表示中")]
    public bool isShowing = false;

    // 現在の入場中にCanvas位置を設定済みか
    private bool hasFixedCanvasPosition = false;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // 最初は全部非表示
        // -----------------------------------------------------

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }

        if (recipePanel != null)
        {
            recipePanel.SetActive(false);
        }

        if (synthesisPanel != null)
        {
            synthesisPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // MolecularCraftingUIを自動検索
        // -----------------------------------------------------

        if (molecularCraftingUI == null)
        {
            molecularCraftingUI =
                GetComponentInChildren<MolecularCraftingUI>(true);
        }


        if (molecularCraftingUI != null)
        {
            Debug.Log(
                "⚗️ MolecularCraftingUIを発見：" +
                molecularCraftingUI.name
            );
        }
        else
        {
            Debug.LogWarning(
                "⚠️ MolecularCraftingUIが見つかりません。"
            );
        }


        // -----------------------------------------------------
        // VRカメラを探す
        // -----------------------------------------------------

        if (centerEyeAnchor == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                centerEyeAnchor =
                    mainCamera.transform;

                Debug.Log(
                    "📷 Main CameraをVRカメラとして使用します"
                );
            }
            else
            {
                Debug.LogError(
                    "❌ Main Cameraが見つかりません！"
                );
            }
        }


        // -----------------------------------------------------
        // Playerを探す
        // -----------------------------------------------------

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player =
                    playerObject.transform;

                Debug.Log(
                    "✅ Playerを発見：" +
                    player.name
                );
            }
            else
            {
                Debug.LogError(
                    "❌ Playerタグのオブジェクトが見つかりません！"
                );
            }
        }
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (player == null)
        {
            return;
        }


        // -----------------------------------------------------
        // クラフト台とプレイヤーの距離
        // -----------------------------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );


        // =====================================================
        // 範囲内
        // =====================================================

        if (distance <= interactionDistance)
        {
            ShowConfirmPanel();
        }


        // =====================================================
        // 範囲外
        // =====================================================

        else
        {
            HideCraftingUI();

            // ★ 次回近づいたときに
            //    Canvas位置を再計算する
            hasFixedCanvasPosition = false;
        }
    }


    // =========================================================
    // 確認画面を表示
    // =========================================================

    private void ShowConfirmPanel()
    {
        if (isShowing)
        {
            return;
        }


        if (confirmPanel == null)
        {
            Debug.LogError(
                "❌ ConfirmPanelが設定されていません！"
            );

            return;
        }


        // -----------------------------------------------------
        // 今回の接近でまだ位置を決めていない場合
        // -----------------------------------------------------

        if (!hasFixedCanvasPosition)
        {
            SetCraftingCanvasPosition();

            hasFixedCanvasPosition = true;
        }


        // -----------------------------------------------------
        // ★ 必ず初期状態に戻す
        // -----------------------------------------------------

        ResetAllCraftingPanels();


        // -----------------------------------------------------
        // UIを表示
        // -----------------------------------------------------

        confirmPanel.SetActive(true);

        isShowing = true;


        Debug.Log(
            "🟢 クラフト確認画面を表示しました！"
        );
    }


    // =========================================================
    // CraftingCanvasの位置・向きを設定
    // =========================================================

    private void SetCraftingCanvasPosition()
    {
        if (craftingCanvas == null)
        {
            Debug.LogError(
                "❌ CraftingCanvasが設定されていません！"
            );

            return;
        }


        if (centerEyeAnchor == null)
        {
            Debug.LogError(
                "❌ CenterEyeAnchorが設定されていません！"
            );

            return;
        }


        // =====================================================
        // クラフト台の位置
        // =====================================================

        Vector3 machinePosition =
            transform.position;


        // =====================================================
        // クラフト台 → プレイヤー方向
        // =====================================================

        Vector3 direction =
            centerEyeAnchor.position -
            machinePosition;


        // 上下方向は無視
        direction.y = 0;


        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }


        direction.Normalize();


        // =====================================================
        // Canvasの表示位置
        // =====================================================

        Vector3 targetPosition =
            machinePosition
            + direction * distanceFromMachine;


        // 上方向へ移動
        targetPosition +=
            Vector3.up * verticalOffset;


        // =====================================================
        // Canvasを独立させる
        // =====================================================

        craftingCanvas.SetParent(
            null,
            true
        );


        // =====================================================
        // Canvas位置
        // =====================================================

        craftingCanvas.position =
            targetPosition;


        // =====================================================
        // Canvasをプレイヤー方向へ向ける
        // =====================================================

        Vector3 lookDirection =
            centerEyeAnchor.position -
            craftingCanvas.position;


        lookDirection.y = 0;


        if (lookDirection.sqrMagnitude > 0.001f)
        {
            lookDirection.Normalize();


            // -------------------------------------------------
            // ★ プレイヤー方向を向く
            // -------------------------------------------------

            Quaternion lookRotation =
                Quaternion.LookRotation(
                    lookDirection,
                    Vector3.up
                );


            // -------------------------------------------------
            // ★ Inspectorで設定した角度を追加
            // -------------------------------------------------

            craftingCanvas.rotation =
                lookRotation
                * Quaternion.Euler(
                    canvasRotationOffset
                );
        }


        Debug.Log(
            "📍 CraftingCanvasの位置と向きを設定しました"
            + " / 追加角度：" +
            canvasRotationOffset
        );
    }


    // =========================================================
    // ★ 全クラフト画面を強制的にリセット
    // =========================================================

    private void ResetAllCraftingPanels()
    {
        // -----------------------------------------------------
        // 確認画面
        // -----------------------------------------------------

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // レシピ画面
        // -----------------------------------------------------

        if (recipePanel != null)
        {
            recipePanel.SetActive(false);
        }


        // -----------------------------------------------------
        // 合成画面
        // -----------------------------------------------------

        if (synthesisPanel != null)
        {
            synthesisPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // MolecularCraftingUI側もリセット
        // -----------------------------------------------------

        if (molecularCraftingUI != null)
        {
            molecularCraftingUI.ResetCraftingUI();
        }
    }


    // =========================================================
    // ★ クラフトUIを全部非表示
    // =========================================================

    public void HideCraftingUI()
    {
        // -----------------------------------------------------
        // 確認画面
        // -----------------------------------------------------

        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // レシピ画面
        // -----------------------------------------------------

        if (recipePanel != null)
        {
            recipePanel.SetActive(false);
        }


        // -----------------------------------------------------
        // 合成画面
        // -----------------------------------------------------

        if (synthesisPanel != null)
        {
            synthesisPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // MolecularCraftingUI側もリセット
        // -----------------------------------------------------

        if (molecularCraftingUI != null)
        {
            molecularCraftingUI.HideAllPanels();
        }


        // -----------------------------------------------------
        // 状態リセット
        // -----------------------------------------------------

        isShowing = false;


        Debug.Log(
            "🔴 クラフトUIをすべて非表示にしました"
        );
    }


    // =========================================================
    // 互換用
    // =========================================================

    public void HideConfirmPanel()
    {
        HideCraftingUI();
    }
}