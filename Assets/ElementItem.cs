using UnityEngine;

public class ElementItem : MonoBehaviour
{
    [Header("元素")]
    public string elementName = "Carbon";

    [Header("表示名")]
    public string displayName = "炭素";

    [Header("アイコン")]
    public Sprite icon;

    [Header("入手個数")]
    public int amount = 1;

    private bool collected = false;


    // ========================================
    // Triggerで触れた場合
    // ========================================
    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }


    // ========================================
    // Collider同士がぶつかった場合
    // ========================================
    private void OnCollisionEnter(Collision collision)
    {
        TryCollect(collision.collider);
    }


    // ========================================
    // 元素取得
    // ========================================
    private void TryCollect(Collider other)
    {
        // すでに取得済みなら何もしない
        if (collected)
        {
            return;
        }


        // ========================================
        // プレイヤーか確認
        // ========================================

        bool isPlayer = false;

        // 自分自身
        if (other.CompareTag("Player"))
        {
            isPlayer = true;
        }

        // 親オブジェクトがPlayer
        if (other.GetComponentInParent<CharacterController>() != null)
        {
            isPlayer = true;
        }

        // 親にPlayerタグがある場合
        Transform parent = other.transform;

        while (parent != null)
        {
            if (parent.CompareTag("Player"))
            {
                isPlayer = true;
                break;
            }

            parent = parent.parent;
        }


        // プレイヤー以外なら終了
        if (!isPlayer)
        {
            return;
        }


        // ========================================
        // InventoryManagerを探す
        // ========================================

        InventoryManager inventory =
            FindFirstObjectByType<InventoryManager>();


        if (inventory == null)
        {
            Debug.LogError(
                "❌ InventoryManagerがシーンにありません！"
            );

            return;
        }


        // ========================================
        // インベントリに追加
        // ========================================

        inventory.AddElement(
            elementName,
            displayName,
            icon,
            amount
        );


        Debug.Log(
            "✅ 元素を取得！：" +
            displayName +
            " × " +
            amount
        );


        // 取得済みにする
        collected = true;


        // 元素オブジェクトを消す
        Destroy(gameObject);
    }
}