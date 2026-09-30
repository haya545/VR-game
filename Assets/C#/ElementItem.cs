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

    [Header("取得音")]
    public AudioClip pickupSound;

    [Tooltip("取得音の音量")]
    [Range(0f, 1f)]
    public float pickupVolume = 1.0f;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        // Player以外は無視
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 二重取得防止
        if (collected)
        {
            return;
        }

        collected = true;

        // =====================================================
        // インベントリへ追加
        // =====================================================

        InventoryManager inventoryManager =
            FindFirstObjectByType<InventoryManager>();

        if (inventoryManager != null)
        {
            inventoryManager.AddElement(
                elementName,
                displayName,
                icon,
                amount
            );

            Debug.Log(
                "🎒 元素を取得：" +
                displayName +
                " × " +
                amount
            );
        }
        else
        {
            Debug.LogError(
                "❌ InventoryManagerが見つかりません！"
            );
        }

        // =====================================================
        // 取得音を鳴らす
        // =====================================================

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position,
                pickupVolume
            );

            Debug.Log(
                "🔊 元素取得音：" +
                pickupSound.name
            );
        }

        // =====================================================
        // 元素オブジェクトを削除
        // =====================================================

        Destroy(gameObject);
    }
}