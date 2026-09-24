using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("元素アイコン")]
    public Image icon;

    [Header("表示名")]
    public TextMeshProUGUI elementNameText;

    [Header("個数")]
    public TextMeshProUGUI amountText;


    public void SetSlot(
        string elementName,
        int amount,
        Sprite elementIcon
    )
    {
        Debug.Log(
            "📋 SetSlot：" +
            elementName +
            " / 個数=" +
            amount
        );


        // =========================
        // 名前
        // =========================

        if (elementNameText != null)
        {
            elementNameText.text = elementName;
        }


        // =========================
        // 個数
        // =========================

        if (amountText != null)
        {
            amountText.text = "× " + amount;
        }


        // =========================
        // アイコン
        // =========================

        if (icon != null)
        {
            // アイコンがある場合
            if (elementIcon != null)
            {
                icon.sprite = elementIcon;
                icon.gameObject.SetActive(true);

                Debug.Log(
                    "🖼 アイコン設定：" +
                    elementIcon.name
                );
            }
            // アイコンがない場合
            else
            {
                icon.sprite = null;
                icon.gameObject.SetActive(false);

                Debug.Log(
                    "ℹ アイコン未設定：" +
                    elementName
                );
            }
        }
    }


    public void ClearSlot()
    {
        if (elementNameText != null)
        {
            elementNameText.text = "";
        }

        if (amountText != null)
        {
            amountText.text = "";
        }

        if (icon != null)
        {
            icon.sprite = null;
            icon.gameObject.SetActive(false);
        }
    }
}