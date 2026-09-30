using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// =========================================================
// 必要素材
// =========================================================

[System.Serializable]
public class MolecularMaterial
{
    [Header("必要な元素")]
    public string elementName;

    [Header("必要個数")]
    public int amount = 1;
}


// =========================================================
// レシピ
// =========================================================

[System.Serializable]
public class MolecularRecipe
{
    [Header("レシピ名")]
    public string recipeName;

    [Header("レシピ画像")]
    public Sprite recipeImage;

    [Header("必要素材")]
    public List<MolecularMaterial> materials =
        new List<MolecularMaterial>();

    [Header("完成する元素")]
    public string resultElementName;

    [Header("完成品の表示名")]
    public string resultDisplayName;

    [Header("完成品の個数")]
    public int resultAmount = 1;

    [Header("完成品アイコン")]
    public Sprite resultIcon;
}


// =========================================================
// 分子クラフトシステム
// =========================================================

public class MolecularCraftingSystem : MonoBehaviour
{
    // =========================================================
    // インベントリ
    // =========================================================

    [Header("インベントリ")]
    public InventoryManager inventoryManager;


    // =========================================================
    // 合成レシピ
    // =========================================================

    [Header("合成レシピ")]
    public List<MolecularRecipe> recipes =
        new List<MolecularRecipe>();


    // =========================================================
    // 基本合成時間
    // =========================================================

    [Header("基本合成時間")]
    [Tooltip("通常の合成にかかる時間")]
    public float synthesisTime = 2.0f;


    // =========================================================
    // 状態
    // =========================================================

    [Header("状態")]
    [SerializeField]
    private bool isCrafting = false;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager =
                FindFirstObjectByType<InventoryManager>();
        }


        if (inventoryManager == null)
        {
            Debug.LogError(
                "❌ MolecularCraftingSystem：InventoryManagerが見つかりません！"
            );
        }
        else
        {
            Debug.Log(
                "✅ MolecularCraftingSystem：InventoryManager接続成功"
            );
        }
    }


    // =========================================================
    // クラフト可能か確認
    // =========================================================

    public bool CanCraft(MolecularRecipe recipe)
    {
        if (recipe == null)
            return false;


        if (inventoryManager == null)
            return false;


        if (recipe.materials == null ||
            recipe.materials.Count == 0)
        {
            return false;
        }


        foreach (
            MolecularMaterial material
            in recipe.materials)
        {
            if (material == null)
                return false;


            if (string.IsNullOrWhiteSpace(
                material.elementName))
            {
                return false;
            }


            if (material.amount <= 0)
                return false;


            int currentAmount =
                inventoryManager.GetElementCount(
                    material.elementName
                );


            if (currentAmount <
                material.amount)
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // クラフト可能なレシピ一覧
    // =========================================================

    public List<MolecularRecipe>
        GetCraftableRecipes()
    {
        List<MolecularRecipe> result =
            new List<MolecularRecipe>();


        if (recipes == null)
            return result;


        foreach (
            MolecularRecipe recipe
            in recipes)
        {
            if (CanCraft(recipe))
            {
                result.Add(recipe);
            }
        }


        return result;
    }


    // =========================================================
    // 通常の合成開始
    // =========================================================

    public bool StartCrafting(
        MolecularRecipe recipe)
    {
        return StartCrafting(
            recipe,
            synthesisTime
        );
    }


    // =========================================================
    // 指定時間で合成開始
    // =========================================================

    public bool StartCrafting(
        MolecularRecipe recipe,
        float customSynthesisTime)
    {
        // -----------------------------------------
        // すでに合成中
        // -----------------------------------------

        if (isCrafting)
        {
            Debug.LogWarning(
                "⚠ 現在すでに合成中です。"
            );

            return false;
        }


        // -----------------------------------------
        // レシピ確認
        // -----------------------------------------

        if (recipe == null)
        {
            Debug.LogWarning(
                "⚠ 合成するレシピが指定されていません。"
            );

            return false;
        }


        // -----------------------------------------
        // 素材確認
        // -----------------------------------------

        if (!CanCraft(recipe))
        {
            Debug.LogWarning(
                "⚠ 必要な素材が足りません：" +
                recipe.recipeName
            );

            return false;
        }


        // -----------------------------------------
        // 合成時間を0未満にしない
        // -----------------------------------------

        customSynthesisTime =
            Mathf.Max(
                0f,
                customSynthesisTime
            );


        Debug.Log(
            "⚗️ 合成開始：" +
            recipe.recipeName +
            " / 合成時間：" +
            customSynthesisTime +
            "秒"
        );


        // -----------------------------------------
        // 合成開始
        // -----------------------------------------

        StartCoroutine(
            CraftCoroutine(
                recipe,
                customSynthesisTime
            )
        );


        return true;
    }


    // =========================================================
    // 合成コルーチン
    // =========================================================

    private IEnumerator CraftCoroutine(
        MolecularRecipe recipe,
        float customSynthesisTime)
    {
        isCrafting = true;


        Debug.Log(
            "⚗️ 合成中：" +
            recipe.recipeName
        );


        // -----------------------------------------
        // 合成演出が終わるまで待つ
        // -----------------------------------------

        yield return new WaitForSeconds(
            customSynthesisTime
        );


        // -----------------------------------------
        // 素材を消費
        // -----------------------------------------

        foreach (
            MolecularMaterial material
            in recipe.materials)
        {
            bool removed =
                inventoryManager.RemoveElement(
                    material.elementName,
                    material.amount
                );


            if (!removed)
            {
                Debug.LogError(
                    "❌ 合成中に素材の消費に失敗：" +
                    material.elementName
                );


                isCrafting = false;

                yield break;
            }
        }


        // -----------------------------------------
        // 完成品をインベントリへ追加
        // -----------------------------------------

        inventoryManager.AddElement(
            recipe.resultElementName,
            recipe.resultDisplayName,
            recipe.resultIcon,
            recipe.resultAmount
        );


        Debug.Log(
            "✅ 合成完了：" +
            recipe.resultDisplayName +
            " × " +
            recipe.resultAmount
        );


        // -----------------------------------------
        // 合成終了
        // -----------------------------------------

        isCrafting = false;
    }


    // =========================================================
    // 合成中か？
    // =========================================================

    public bool IsCrafting()
    {
        return isCrafting;
    }


    // =========================================================
    // レシピ数
    // =========================================================

    public int GetRecipeCount()
    {
        if (recipes == null)
            return 0;


        return recipes.Count;
    }


    // =========================================================
    // レシピ取得
    // =========================================================

    public MolecularRecipe GetRecipe(
        int index)
    {
        if (recipes == null)
            return null;


        if (index < 0 ||
            index >= recipes.Count)
        {
            return null;
        }


        return recipes[index];
    }
}