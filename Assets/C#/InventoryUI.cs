using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("インベントリパネル")]
    public GameObject inventoryPanel;

    [Header("スロットを並べる場所")]
    public Transform slotParent;

    [Header("スロットPrefab")]
    public GameObject slotPrefab;

    [Header("スロット数")]
    public int slotCount = 16;


    [Header("VRカメラ")]
    public Transform centerEyeAnchor;

    [Header("表示位置")]
    public float distanceFromPlayer = 1.0f;

    [Tooltip("マイナスにすると左側")]
    public float horizontalOffset = -0.45f;

    [Tooltip("マイナスにすると下側")]
    public float verticalOffset = -0.35f;


    // 作成したスロット
    private InventorySlot[] slots;

    // インベントリ開閉状態
    private bool isInventoryOpen = false;


    // ========================================
    // 起動時
    // ========================================
    private void Start()
    {
        Debug.Log("🎒 InventoryUIが起動しました！");


        // ------------------------------------
        // VRカメラが設定されていなければ探す
        // ------------------------------------
        if (centerEyeAnchor == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                centerEyeAnchor = mainCamera.transform;

                Debug.Log(
                    "📷 Main CameraをVRカメラとして使用します"
                );
            }
            else
            {
                Debug.LogWarning(
                    "⚠️ Center Eye Anchorが設定されていません"
                );
            }
        }


        // ------------------------------------
        // 最初は非表示
        // ------------------------------------
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        else
        {
            Debug.LogError(
                "❌ Inventory Panelが設定されていません！"
            );
        }


        // ------------------------------------
        // スロット作成
        // ------------------------------------
        CreateSlots();
    }


    // ========================================
    // VR入力
    // ========================================
    private void Update()
    {
        // ------------------------------------
        // Xボタン
        // ------------------------------------
        if (OVRInput.GetDown(
            OVRInput.Button.Four))
        {
            Debug.Log(
                "🎮 Xボタンを検出！"
            );

            ToggleInventory();
        }
    }


    // ========================================
    // インベントリ開閉
    // ========================================
    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError(
                "❌ Inventory Panelが設定されていません！"
            );

            return;
        }


        // ====================================
        // 開く
        // ====================================
        if (!isInventoryOpen)
        {
            // ★開くたびに現在位置へ移動
            SetInventoryPosition();

            inventoryPanel.SetActive(true);

            isInventoryOpen = true;

            Debug.Log(
                "🎒 インベントリを開きました！"
            );
        }


        // ====================================
        // 閉じる
        // ====================================
        else
        {
            inventoryPanel.SetActive(false);

            isInventoryOpen = false;

            Debug.Log(
                "🎒 インベントリを閉じました！"
            );
        }
    }


    // ========================================
    // インベントリ位置設定
    // ========================================
    private void SetInventoryPosition()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogWarning(
                "⚠️ Center Eye Anchorがありません"
            );

            return;
        }


        // ====================================
        // 現在のHMD位置
        // ====================================
        Vector3 cameraPosition =
            centerEyeAnchor.position;


        // ====================================
        // 現在のHMDの向き
        // ====================================
        Vector3 forward =
            centerEyeAnchor.forward;

        Vector3 right =
            centerEyeAnchor.right;

        Vector3 up =
            centerEyeAnchor.up;


        // ====================================
        // 左下の位置を計算
        // ====================================
        Vector3 targetPosition =
            cameraPosition
            + forward * distanceFromPlayer
            + right * horizontalOffset
            + up * verticalOffset;


        // ====================================
        // Canvasをワールド空間にする
        // ====================================
        transform.SetParent(
            null,
            true
        );


        // ====================================
        // 位置を更新
        // ====================================
        transform.position =
            targetPosition;


        // ====================================
        // プレイヤーの方向を向く
        // ====================================
        transform.rotation =
            Quaternion.LookRotation(
                transform.position -
                cameraPosition
            );


        Debug.Log(
            "📍 インベントリ位置を更新しました"
        );
    }


    // ========================================
    // スロット作成
    // ========================================
    private void CreateSlots()
    {
        if (slotParent == null)
        {
            Debug.LogError(
                "❌ Slot Parentが設定されていません！"
            );

            return;
        }

        if (slotPrefab == null)
        {
            Debug.LogError(
                "❌ Slot Prefabが設定されていません！"
            );

            return;
        }


        // ------------------------------------
        // 既存スロットを削除
        // ------------------------------------
        for (
            int i = slotParent.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                slotParent.GetChild(i).gameObject
            );
        }


        // ------------------------------------
        // スロットを作成
        // ------------------------------------
        for (
            int i = 0;
            i < slotCount;
            i++
        )
        {
            GameObject newSlot =
                Instantiate(
                    slotPrefab,
                    slotParent
                );

            InventorySlot slot =
                newSlot.GetComponent<InventorySlot>();

            if (slot != null)
            {
                slot.ClearSlot();
            }
        }


        // ------------------------------------
        // 作成したスロットを取得
        // ------------------------------------
        slots =
            slotParent.GetComponentsInChildren<
                InventorySlot
            >();


        Debug.Log(
            "🎒 インベントリスロットを " +
            slots.Length +
            " 個作成しました"
        );
    }


    // ========================================
    // インベントリ表示更新
    // ========================================
    public void UpdateInventory(
        Dictionary<string, InventoryElement> inventory
    )
    {
        if (slots == null)
        {
            Debug.LogWarning(
                "⚠️ InventorySlotがまだ作成されていません"
            );

            return;
        }


        // ------------------------------------
        // 全スロットを空にする
        // ------------------------------------
        foreach (
            InventorySlot slot
            in slots
        )
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
        }


        // ------------------------------------
        // 元素を表示
        // ------------------------------------
        int index = 0;


        foreach (
            KeyValuePair<
                string,
                InventoryElement
            > item in inventory
        )
        {
            if (index >= slots.Length)
            {
                break;
            }


            InventoryElement element =
                item.Value;


            slots[index].SetSlot(
                element.displayName,
                element.amount,
                element.icon
            );


            index++;
        }


        Debug.Log(
            "🎒 インベントリUIを更新しました。" +
            " 所持元素数：" +
            inventory.Count
        );
    }
}