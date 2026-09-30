using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MolecularRecipeButton : MonoBehaviour
{
    [Header("レシピ画像")]
    public Image recipeImage;

    [Header("レシピ名")]
    public TMP_Text recipeNameText;

    [HideInInspector]
    public MolecularRecipe recipe;


    // =========================================================
    // レシピ情報を設定
    // =========================================================

    public void Setup(MolecularRecipe targetRecipe)
    {
        recipe = targetRecipe;

        if (recipe == null)
            return;


        // =====================================================
        // レシピ画像
        // =====================================================

        if (recipeImage != null)
        {
            recipeImage.sprite = recipe.recipeImage;

            recipeImage.enabled =
                recipe.recipeImage != null;
        }


        // =====================================================
        // レシピ名
        // =====================================================

        if (recipeNameText != null)
        {
            recipeNameText.text =
                recipe.recipeName;
        }
    }
}