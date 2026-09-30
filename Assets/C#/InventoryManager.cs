using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryElement
{
    public string elementName;
    public string displayName;
    public int amount;
    public Sprite icon;
}

public class InventoryManager : MonoBehaviour
{
    private Dictionary<string, InventoryElement> inventory =
        new Dictionary<string, InventoryElement>();

    [Header("インベントリUI")]
    public InventoryUI inventoryUI;


    // =========================================================
    // 起動
    // =========================================================

    private void Start()
    {
        Debug.Log("🎒 InventoryManagerが起動しました！");

        if (inventoryUI == null)
        {
            Debug.LogError(
                "❌ Inventory UIが設定されていません！"
            );
        }
        else
        {
            Debug.Log(
                "✅ Inventory UIを発見：" +
                inventoryUI.gameObject.name
            );
        }

        UpdateUI();
    }


    // =========================================================
    // 元素を追加
    // =========================================================

    public void AddElement(
        string elementName,
        string displayName,
        Sprite icon,
        int amount = 1
    )
    {
        // -----------------------------------------------------
        // ElementNameを統一
        // iron / Iron / IRON → iron
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(elementName))
        {
            Debug.LogError(
                "❌ ElementNameが空です！：" +
                displayName
            );

            return;
        }

        elementName =
            elementName.Trim().ToLower();


        Debug.Log(
            "📦 元素追加処理：" +
            displayName +
            " × " +
            amount +
            " / ElementName=" +
            elementName
        );


        // -----------------------------------------------------
        // アイコン確認
        // -----------------------------------------------------

        if (icon != null)
        {
            Debug.Log(
                "🖼 アイコン受信成功：" +
                displayName +
                " → " +
                icon.name
            );
        }
        else
        {
            Debug.Log(
                "ℹ アイコン未設定：" +
                displayName +
                " / ElementName=" +
                elementName
            );
        }


        // -----------------------------------------------------
        // 既に持っている元素
        // -----------------------------------------------------

        if (inventory.ContainsKey(elementName))
        {
            InventoryElement existingElement =
                inventory[elementName];


            // 個数を追加
            existingElement.amount += amount;


            // 表示名を更新
            existingElement.displayName =
                displayName;


            // アイコンがある場合だけ更新
            //
            // NULLが渡されても、
            // 既存のアイコンは消さない
            //
            if (icon != null)
            {
                existingElement.icon =
                    icon;
            }


            Debug.Log(
                "🔄 既存元素を更新：" +
                displayName +
                " × " +
                existingElement.amount
            );
        }


        // -----------------------------------------------------
        // 初めて入手した元素
        // -----------------------------------------------------

        else
        {
            InventoryElement newElement =
                new InventoryElement();


            newElement.elementName =
                elementName;

            newElement.displayName =
                displayName;

            newElement.amount =
                amount;

            newElement.icon =
                icon;


            inventory.Add(
                elementName,
                newElement
            );


            Debug.Log(
                "🆕 新しい元素を登録：" +
                displayName +
                " × " +
                amount
            );
        }


        // -----------------------------------------------------
        // 保存されたアイコンを確認
        // -----------------------------------------------------

        if (inventory[elementName].icon != null)
        {
            Debug.Log(
                "💾 Inventoryに保存されたアイコン：" +
                inventory[elementName].icon.name
            );
        }
        else
        {
            Debug.Log(
                "ℹ Inventory内のアイコン未設定：" +
                displayName
            );
        }


        // -----------------------------------------------------
        // 現在の所持元素数
        // -----------------------------------------------------

        Debug.Log(
            "🎒 現在の所持元素種類数：" +
            inventory.Count
        );


        // -----------------------------------------------------
        // UI更新
        // -----------------------------------------------------

        UpdateUI();
    }


    // =========================================================
    // 元素を消費
    // =========================================================

    public bool RemoveElement(
        string elementName,
        int amount = 1
    )
    {
        if (string.IsNullOrWhiteSpace(elementName))
        {
            return false;
        }


        // ElementNameを統一
        elementName =
            elementName.Trim().ToLower();


        if (!inventory.ContainsKey(elementName))
        {
            Debug.LogWarning(
                "⚠ " +
                elementName +
                "を持っていません"
            );

            return false;
        }


        if (inventory[elementName].amount < amount)
        {
            Debug.LogWarning(
                "⚠ " +
                elementName +
                "が足りません"
            );

            return false;
        }


        inventory[elementName].amount -= amount;


        // 個数が0以下になったら削除
        if (inventory[elementName].amount <= 0)
        {
            inventory.Remove(elementName);
        }


        UpdateUI();

        return true;
    }


    // =========================================================
    // 所持数を取得
    // =========================================================

    public int GetElementCount(
        string elementName
    )
    {
        if (string.IsNullOrWhiteSpace(elementName))
        {
            return 0;
        }


        elementName =
            elementName.Trim().ToLower();


        if (inventory.ContainsKey(elementName))
        {
            return inventory[elementName].amount;
        }


        return 0;
    }


    // =========================================================
    // インベントリ取得
    // =========================================================

    public Dictionary<string, InventoryElement>
        GetInventory()
    {
        return inventory;
    }


    // =========================================================
    // UI更新
    // =========================================================

    private void UpdateUI()
    {
        if (inventoryUI == null)
        {
            Debug.LogError(
                "❌ UI更新失敗：Inventory UIが設定されていません！"
            );

            return;
        }


        Debug.Log(
            "🔄 InventoryUIを更新します。" +
            " 所持元素種類数：" +
            inventory.Count
        );


        inventoryUI.UpdateInventory(
            inventory
        );
    }
}