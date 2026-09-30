using UnityEngine;

public class QuestNPC : MonoBehaviour
{
    [Header("クエスト設定")]
    public string requiredElement = "carbon";
    public int requiredAmount = 5;

    [Header("クエスト完了前の会話")]
    public QuestDialogue[] beforeQuestDialogues;

    [Header("クエスト完了後の会話")]
    public QuestDialogue[] afterQuestDialogues;

    [Header("会話開始距離")]
    [Tooltip("この距離以内にプレイヤーが入ると話しかけられます")]
    public float talkDistance = 3.0f;

    [Header("Quest UI")]
    public QuestUI questUI;

    [Header("Inventory")]
    public InventoryManager inventoryManager;

    private Transform playerTransform;
    private bool playerInRange = false;

    private bool questCompleted = false;

    // ZRの前回状態
    private bool previousTriggerState = false;

    private void Awake()
    {
        // =====================================================
        // QuestUIを自動取得
        // =====================================================

        if (questUI == null)
        {
            questUI = FindFirstObjectByType<QuestUI>();

            if (questUI != null)
            {
                Debug.Log(
                    "✅ QuestUIを自動取得しました：" +
                    questUI.gameObject.name
                );
            }
            else
            {
                Debug.LogError(
                    "❌ QuestUIがScene内に見つかりません！"
                );
            }
        }

        // =====================================================
        // InventoryManagerを自動取得
        // =====================================================

        if (inventoryManager == null)
        {
            inventoryManager =
                FindFirstObjectByType<InventoryManager>();

            if (inventoryManager != null)
            {
                Debug.Log(
                    "✅ InventoryManagerを自動取得しました：" +
                    inventoryManager.gameObject.name
                );
            }
            else
            {
                Debug.LogError(
                    "❌ InventoryManagerがScene内に見つかりません！"
                );
            }
        }

        // =====================================================
        // Playerを探す
        // =====================================================

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;

            Debug.Log(
                "✅ Playerを発見：" +
                player.name
            );
        }
        else
        {
            Debug.LogError(
                "❌ Tag = Player のオブジェクトが見つかりません！"
            );
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        // =====================================================
        // NPCとプレイヤーの距離
        // =====================================================

        Vector3 npcPosition = transform.position;
        Vector3 playerPosition = playerTransform.position;

        npcPosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                npcPosition,
                playerPosition
            );

        // =====================================================
        // 会話可能範囲
        // =====================================================

        if (distance <= talkDistance)
        {
            if (!playerInRange)
            {
                playerInRange = true;

                Debug.Log(
                    "🗣 村人に話しかけられます！" +
                    " 距離：" +
                    distance.ToString("F2") +
                    "m"
                );
            }
        }
        else
        {
            if (playerInRange)
            {
                playerInRange = false;

                Debug.Log(
                    "🚶 村人から離れました。" +
                    " 距離：" +
                    distance.ToString("F2") +
                    "m"
                );

                // 会話中なら終了
                if (questUI != null && questUI.IsTalking())
                {
                    questUI.HideDialogue();
                }
            }
        }

        // =====================================================
        // 選択画面が開いている間は
        // QuestNPC側ではZRを処理しない
        // =====================================================

        if (questUI != null && questUI.IsChoiceOpen())
        {
            // 現在のZR状態を記録
            // 選択画面が閉じた直後の誤入力も防ぐ
            previousTriggerState =
                OVRInput.Get(
                    OVRInput.Button.SecondaryIndexTrigger
                );

            return;
        }

        // =====================================================
        // 会話開始 / 次の文章
        // =====================================================

        if (playerInRange)
        {
            bool triggerNow =
                OVRInput.Get(
                    OVRInput.Button.SecondaryIndexTrigger
                );

            // 押した瞬間だけ検出
            bool triggerDown =
                triggerNow && !previousTriggerState;

            previousTriggerState = triggerNow;

            if (triggerDown)
            {
                Talk();
            }
        }
        else
        {
            // 範囲外でも状態だけ更新
            previousTriggerState =
                OVRInput.Get(
                    OVRInput.Button.SecondaryIndexTrigger
                );
        }
    }

    // =========================================================
    // 会話
    // =========================================================

    private void Talk()
    {
        if (questUI == null)
        {
            Debug.LogError(
                "❌ QuestUIが設定されていません！"
            );

            return;
        }

        // -----------------------------------------------------
        // 選択画面が開いている場合
        // -----------------------------------------------------

        if (questUI.IsChoiceOpen())
        {
            Debug.Log(
                "🎯 選択画面表示中のためQuestNPC側では処理しません"
            );

            return;
        }

        // -----------------------------------------------------
        // すでに会話中なら次の文章へ
        // -----------------------------------------------------

        if (questUI.IsTalking())
        {
            questUI.NextDialogue();
            return;
        }

        // -----------------------------------------------------
        // クエスト完了後
        // -----------------------------------------------------

        if (questCompleted)
        {
            if (
                afterQuestDialogues != null &&
                afterQuestDialogues.Length > 0
            )
            {
                Debug.Log(
                    "💬 クエスト完了後の会話を開始"
                );

                questUI.StartDialogue(
                    afterQuestDialogues,
                    null
                );
            }

            return;
        }

        // -----------------------------------------------------
        // クエスト完了前
        // -----------------------------------------------------

        if (
            beforeQuestDialogues != null &&
            beforeQuestDialogues.Length > 0
        )
        {
            Debug.Log(
                "💬 クエスト開始前の会話を開始"
            );

            questUI.StartDialogue(
                beforeQuestDialogues,
                this
            );
        }
        else
        {
            Debug.LogWarning(
                "⚠ クエスト開始前の会話が設定されていません！"
            );
        }
    }

    // =========================================================
    // 必要素材を持っているか
    // =========================================================

    public bool HasRequiredItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError(
                "❌ InventoryManagerがありません！"
            );

            return false;
        }

        int currentAmount =
            inventoryManager.GetElementCount(
                requiredElement
            );

        Debug.Log(
            "🔎 クエスト素材確認：" +
            requiredElement +
            " / 必要：" +
            requiredAmount +
            " / 所持：" +
            currentAmount
        );

        return currentAmount >= requiredAmount;
    }

    // =========================================================
    // 必要素材を渡す
    // =========================================================

    public bool GiveRequiredItem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError(
                "❌ InventoryManagerがありません！"
            );

            return false;
        }

        // 素材が足りるか確認
        if (!HasRequiredItem())
        {
            Debug.LogWarning(
                "⚠ クエスト素材が足りません！"
            );

            return false;
        }

        // 素材を消費
        bool removed =
            inventoryManager.RemoveElement(
                requiredElement,
                requiredAmount
            );

        if (!removed)
        {
            Debug.LogWarning(
                "❌ クエスト素材の消費に失敗しました。"
            );

            return false;
        }

        // クエスト完了
        questCompleted = true;

        Debug.Log(
            "🎉 クエスト完了！ " +
            requiredElement +
            " × " +
            requiredAmount
        );

        return true;
    }

    // =========================================================
    // クエスト完了状態
    // =========================================================

    public bool IsQuestCompleted()
    {
        return questCompleted;
    }
}