using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


// =========================================================
// 合成中ステップ
// =========================================================

[System.Serializable]
public class SynthesisStep
{
    [Header("表示画像")]
    public Sprite image;

    [Header("表示時間（秒）")]
    [Min(0.1f)]
    public float displayTime = 1.5f;
}


// =========================================================
// 合成成功ステップ
// =========================================================

[System.Serializable]
public class SuccessStep
{
    [Header("成功画面の画像")]
    public Sprite image;

    [Header("表示時間（秒）")]
    [Min(0.1f)]
    public float displayTime = 2f;
}


public class MolecularCraftingUI : MonoBehaviour
{
    // =========================================================
    // UIパネル
    // =========================================================

    [Header("確認画面")]
    public GameObject confirmPanel;

    [Header("レシピ一覧画面")]
    public GameObject recipePanel;

    [Header("合成中・成功画面")]
    public GameObject synthesisPanel;


    // =========================================================
    // 確認ボタン
    // =========================================================

    [Header("確認ボタン")]
    public GameObject yesButton;
    public GameObject noButton;


    // =========================================================
    // 複数レシピ用Scroll View
    // =========================================================

    [Header("レシピScroll View")]

    [Tooltip("RecipeScrollView")]
    public ScrollRect recipeScrollRect;

    [Tooltip("RecipeScrollViewのContent")]
    public Transform recipeContent;

    [Tooltip("1個分のレシピボタンPrefab")]
    public GameObject recipeButtonPrefab;


    // =========================================================
    // スクロールバー操作
    // =========================================================

    [Header("スクロールバー操作")]

    [Tooltip("縦スクロールバー")]
    public Scrollbar recipeScrollbar;

    private RectTransform scrollbarRect;

    // ZR長押しでスクロールバーを掴んでいるか
    private bool isDraggingScrollbar = false;


    // =========================================================
    // ボタンホバー
    // =========================================================

    [Header("確認ボタン ホバー")]
    public TutorialButtonHover yesButtonHover;

    public TutorialButtonHover noButtonHover;


    // =========================================================
    // クラフトシステム
    // =========================================================

    [Header("クラフトシステム")]
    public MolecularCraftingSystem craftingSystem;


    // =========================================================
    // 合成演出
    // =========================================================

    [Header("合成演出")]

    [Tooltip("合成中に画像を表示するImage")]
    public Image synthesisImage;

    [Tooltip("合成中に順番に表示する画像")]
    public List<SynthesisStep> synthesisSteps =
        new List<SynthesisStep>();


    // =========================================================
    // 合成成功画面
    // =========================================================

    [Header("合成成功画面")]

    [Tooltip("成功画面に表示するImage")]
    public Image successImage;

    [Tooltip("成功画面を順番に表示する")]
    public List<SuccessStep> successSteps =
        new List<SuccessStep>();


    // =========================================================
    // 成功時エフェクト画像
    // =========================================================

    [Header("成功時エフェクト画像")]

    [Tooltip("成功画面の最初の1枚にだけ重ねて表示するエフェクト")]
    public Image successEffectImage;

    [Tooltip("成功時エフェクト画像")]
    public Sprite successEffectSprite;


    // =========================================================
    // UI切り替えSE
    // =========================================================

    [Header("UI切り替えSE")]

    [Tooltip("画面や合成画像が切り替わったときに鳴らす共通SE")]
    public AudioClip transitionSound;

    [Range(0f, 1f)]
    public float transitionSoundVolume = 1f;


    // =========================================================
    // 合成完了演出
    // =========================================================

    [Header("合成完了演出")]

    [Tooltip("合成完了時に鳴らす効果音")]
    public AudioClip completionSound;

    [Range(0f, 1f)]
    public float completionSoundVolume = 1f;


    // =========================================================
    // Prefabエフェクト
    // =========================================================

    [Header("Prefabエフェクト（必要なら使用）")]

    [Tooltip("合成完了時に生成するエフェクトPrefab")]
    public GameObject completionEffectPrefab;

    [Tooltip("エフェクトを表示する位置")]
    public Transform completionEffectPoint;

    [Tooltip("Prefabエフェクトを表示してから消すまでの時間")]
    public float completionEffectDuration = 2f;


    // =========================================================
    // VR右コントローラー
    // =========================================================

    [Header("VR右コントローラー")]

    [Tooltip("空欄ならRightHandAnchorを自動検索")]
    public Transform rightControllerTransform;


    // =========================================================
    // 照準距離
    // =========================================================

    [Header("照準距離")]
    public float rayDistance = 20f;


    // =========================================================
    // レーザー
    // =========================================================

    [Header("レーザー")]

    public LineRenderer laser;


    // =========================================================
    // 内部状態
    // =========================================================

    private GameObject pointedButton;

    private MolecularRecipe currentRecipe;

    private bool isSynthesisPlaying = false;

    private List<GameObject> spawnedRecipeButtons =
        new List<GameObject>();


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        Debug.Log("⚗️ MolecularCraftingUI 起動");


        // -----------------------------------------
        // 右コントローラー
        // -----------------------------------------

        if (rightControllerTransform == null)
            rightControllerTransform =
                FindRightController();


        if (rightControllerTransform != null)
        {
            Debug.Log(
                "🎯 右コントローラー：" +
                rightControllerTransform.name
            );
        }
        else
        {
            Debug.LogError(
                "❌ RightHandAnchorが見つかりません！"
            );
        }


        // -----------------------------------------
        // CraftingSystem
        // -----------------------------------------

        if (craftingSystem == null)
        {
            craftingSystem =
                FindFirstObjectByType<MolecularCraftingSystem>();
        }


        if (craftingSystem != null)
        {
            Debug.Log(
                "✅ MolecularCraftingSystem接続成功"
            );
        }
        else
        {
            Debug.LogError(
                "❌ MolecularCraftingSystemが見つかりません！"
            );
        }


        // -----------------------------------------
        // Scrollbar
        // -----------------------------------------

        if (recipeScrollbar != null)
        {
            scrollbarRect =
                recipeScrollbar.GetComponent<RectTransform>();

            Debug.Log("✅ Recipe Scrollbar接続成功");
        }
        else
        {
            Debug.LogWarning(
                "⚠️ Recipe Scrollbarが設定されていません！"
            );
        }


        // -----------------------------------------
        // 初期状態
        // -----------------------------------------

        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        if (recipePanel != null)
            recipePanel.SetActive(false);

        if (synthesisPanel != null)
            synthesisPanel.SetActive(false);


        // -----------------------------------------
        // 合成画像
        // -----------------------------------------

        if (synthesisImage != null)
        {
            synthesisImage.sprite = null;
            synthesisImage.enabled = false;
        }


        // -----------------------------------------
        // 成功画像
        // -----------------------------------------

        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }


        // -----------------------------------------
        // 成功エフェクト
        // -----------------------------------------

        if (successEffectImage != null)
        {
            successEffectImage.sprite = null;
            successEffectImage.enabled = false;
        }


        // -----------------------------------------
        // レーザー
        // -----------------------------------------

        if (laser != null)
        {
            laser.enabled = true;
            laser.positionCount = 2;
        }
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (isSynthesisPlaying)
            return;


        // =====================================================
        // スクロールバーを掴んでいる
        // =====================================================

        if (isDraggingScrollbar)
        {
            UpdateScrollbarDrag();


            // ZRを離したら掴み解除
            if (!OVRInput.Get(
                OVRInput.Button.SecondaryIndexTrigger))
            {
                StopScrollbarDrag();
            }


            return;
        }


        // =====================================================
        // 通常の照準
        // =====================================================

        UpdateButtonRay();


        // =====================================================
        // ZRを押した
        // =====================================================

        if (OVRInput.GetDown(
            OVRInput.Button.SecondaryIndexTrigger))
        {
            // -----------------------------------------
            // まずスクロールバーを掴めるか確認
            // -----------------------------------------

            if (TryStartScrollbarDrag())
            {
                return;
            }


            // -----------------------------------------
            // 通常のボタン
            // -----------------------------------------

            PressCurrentButton();
        }
    }


    // =========================================================
    // スクロールバーを掴む
    // =========================================================

    private bool TryStartScrollbarDrag()
    {
        if (recipeScrollbar == null)
            return false;


        if (recipePanel == null ||
            !recipePanel.activeInHierarchy)
        {
            return false;
        }


        if (rightControllerTransform == null)
            return false;


        if (scrollbarRect == null)
        {
            scrollbarRect =
                recipeScrollbar.GetComponent<RectTransform>();
        }


        if (scrollbarRect == null)
            return false;


        Ray ray = new Ray(
            rightControllerTransform.position,
            rightControllerTransform.forward
        );


        // -----------------------------------------
        // スクロールバーの平面
        // -----------------------------------------

        Plane plane =
            new Plane(
                scrollbarRect.forward,
                scrollbarRect.position
            );


        float distance;


        if (!plane.Raycast(
            ray,
            out distance))
        {
            return false;
        }


        if (distance < 0f ||
            distance > rayDistance)
        {
            return false;
        }


        // -----------------------------------------
        // レーザーが当たった場所
        // -----------------------------------------

        Vector3 hitPoint =
            ray.GetPoint(distance);


        Vector3 localPoint =
            scrollbarRect.InverseTransformPoint(
                hitPoint
            );


        // -----------------------------------------
        // スクロールバーの範囲内か
        // -----------------------------------------

        if (!scrollbarRect.rect.Contains(
            new Vector2(
                localPoint.x,
                localPoint.y
            )))
        {
            return false;
        }


        // -----------------------------------------
        // 掴む
        // -----------------------------------------

        isDraggingScrollbar = true;


        Debug.Log(
            "🖐 スクロールバーを掴みました"
        );


        // -----------------------------------------
        // 掴んだ位置へ移動
        // -----------------------------------------

        UpdateScrollbarValueFromWorldPoint(
            hitPoint
        );


        // -----------------------------------------
        // ボタンホバー解除
        // -----------------------------------------

        pointedButton = null;

        UpdateButtonVisual();


        return true;
    }


    // =========================================================
    // スクロールバーをドラッグ中
    // =========================================================

    private void UpdateScrollbarDrag()
    {
        if (recipeScrollbar == null)
            return;


        if (rightControllerTransform == null)
            return;


        if (scrollbarRect == null)
        {
            scrollbarRect =
                recipeScrollbar.GetComponent<RectTransform>();
        }


        Ray ray = new Ray(
            rightControllerTransform.position,
            rightControllerTransform.forward
        );


        Plane plane =
            new Plane(
                scrollbarRect.forward,
                scrollbarRect.position
            );


        float distance;


        if (!plane.Raycast(
            ray,
            out distance))
        {
            return;
        }


        if (distance < 0f ||
            distance > rayDistance)
        {
            return;
        }


        Vector3 hitPoint =
            ray.GetPoint(distance);


        // -----------------------------------------
        // スクロール位置変更
        // -----------------------------------------

        UpdateScrollbarValueFromWorldPoint(
            hitPoint
        );


        // -----------------------------------------
        // レーザーをスクロールバー位置まで表示
        // -----------------------------------------

        if (laser != null)
        {
            laser.enabled = true;

            laser.positionCount = 2;

            laser.SetPosition(
                0,
                rightControllerTransform.position
            );

            laser.SetPosition(
                1,
                hitPoint
            );
        }


        // ★ここではSEを鳴らさない
    }


    // =========================================================
    // スクロールバーの値を変更
    // =========================================================

    private void UpdateScrollbarValueFromWorldPoint(
        Vector3 worldPoint)
    {
        if (recipeScrollbar == null)
            return;


        if (scrollbarRect == null)
            return;


        Vector3 localPoint =
            scrollbarRect.InverseTransformPoint(
                worldPoint
            );


        Rect rect =
            scrollbarRect.rect;


        if (rect.height <= 0f)
            return;


        // -----------------------------------------
        // 下 = 0
        // 上 = 1
        // -----------------------------------------

        float normalized =
            Mathf.InverseLerp(
                rect.yMin,
                rect.yMax,
                localPoint.y
            );


        normalized =
            Mathf.Clamp01(normalized);


        recipeScrollbar.value =
            normalized;
    }


    // =========================================================
    // スクロールバーを離す
    // =========================================================

    private void StopScrollbarDrag()
    {
        isDraggingScrollbar = false;


        Debug.Log(
            "✋ スクロールバーを離しました"
        );


        // -----------------------------------------
        // ボタン照準を再計算
        // -----------------------------------------

        pointedButton = null;

        UpdateButtonRay();
    }


    // =========================================================
    // VR照準
    // =========================================================

    private void UpdateButtonRay()
    {
        if (rightControllerTransform == null)
            return;


        Ray ray = new Ray(
            rightControllerTransform.position,
            rightControllerTransform.forward
        );


        GameObject newButton =
            FindButtonByRay(ray);


        Vector3 laserStart =
            rightControllerTransform.position;


        Vector3 laserEnd =
            laserStart +
            rightControllerTransform.forward *
            rayDistance;


        if (newButton != null)
        {
            RectTransform rect =
                newButton.GetComponent<RectTransform>();


            if (rect != null)
            {
                Plane plane =
                    new Plane(
                        rect.forward,
                        rect.position
                    );


                float distance;


                if (plane.Raycast(
                    ray,
                    out distance))
                {
                    if (distance >= 0f &&
                        distance <= rayDistance)
                    {
                        laserEnd =
                            ray.GetPoint(distance);
                    }
                }
            }
        }


        // -----------------------------------------
        // レーザー
        // -----------------------------------------

        if (laser != null)
        {
            laser.enabled = true;

            laser.positionCount = 2;

            laser.SetPosition(
                0,
                laserStart
            );

            laser.SetPosition(
                1,
                laserEnd
            );
        }


        // -----------------------------------------
        // 照準対象変更
        // -----------------------------------------

        if (newButton != pointedButton)
        {
            pointedButton =
                newButton;

            UpdateButtonVisual();
        }
    }


    // =========================================================
    // ボタン検索
    // =========================================================

    private GameObject FindButtonByRay(Ray ray)
    {
        GameObject result = null;

        float closestDistance =
            float.MaxValue;


        // -----------------------------------------
        // はい
        // -----------------------------------------

        CheckButton(
            yesButton,
            ray,
            ref result,
            ref closestDistance
        );


        // -----------------------------------------
        // いいえ
        // -----------------------------------------

        CheckButton(
            noButton,
            ray,
            ref result,
            ref closestDistance
        );


        // -----------------------------------------
        // レシピボタン
        // -----------------------------------------

        for (
            int i = 0;
            i < spawnedRecipeButtons.Count;
            i++
        )
        {
            GameObject button =
                spawnedRecipeButtons[i];


            if (button == null)
                continue;


            CheckButton(
                button,
                ray,
                ref result,
                ref closestDistance
            );
        }


        return result;
    }


    // =========================================================
    // ボタン照準チェック
    // =========================================================

    private void CheckButton(
        GameObject button,
        Ray ray,
        ref GameObject result,
        ref float closestDistance
    )
    {
        if (button == null)
            return;


        if (!button.activeInHierarchy)
            return;


        RectTransform rect =
            button.GetComponent<RectTransform>();


        if (rect == null)
            return;


        Plane plane =
            new Plane(
                rect.forward,
                rect.position
            );


        float distance;


        if (!plane.Raycast(
            ray,
            out distance))
        {
            return;
        }


        if (distance < 0f ||
            distance > rayDistance)
        {
            return;
        }


        Vector3 hitPoint =
            ray.GetPoint(distance);


        Vector3 localPoint =
            rect.InverseTransformPoint(
                hitPoint
            );


        Rect area =
            rect.rect;


        if (!area.Contains(
            new Vector2(
                localPoint.x,
                localPoint.y
            )))
        {
            return;
        }


        if (distance < closestDistance)
        {
            closestDistance =
                distance;

            result =
                button;
        }
    }


    // =========================================================
    // ホバー表示
    // =========================================================

    private void UpdateButtonVisual()
    {
        // -----------------------------------------
        // 確認ボタン
        // -----------------------------------------

        if (yesButtonHover != null)
            yesButtonHover.SetHover(false);


        if (noButtonHover != null)
            noButtonHover.SetHover(false);


        // -----------------------------------------
        // 全レシピのホバー解除
        // -----------------------------------------

        for (
            int i = 0;
            i < spawnedRecipeButtons.Count;
            i++
        )
        {
            GameObject button =
                spawnedRecipeButtons[i];


            if (button == null)
                continue;


            TutorialButtonHover hover =
                button.GetComponent<
                    TutorialButtonHover
                >();


            if (hover != null)
                hover.SetHover(false);
        }


        // -----------------------------------------
        // 照準なし
        // -----------------------------------------

        if (pointedButton == null)
            return;


        // -----------------------------------------
        // はい
        // -----------------------------------------

        if (pointedButton == yesButton)
        {
            if (yesButtonHover != null)
                yesButtonHover.SetHover(true);

            return;
        }


        // -----------------------------------------
        // いいえ
        // -----------------------------------------

        if (pointedButton == noButton)
        {
            if (noButtonHover != null)
                noButtonHover.SetHover(true);

            return;
        }


        // -----------------------------------------
        // レシピ
        // -----------------------------------------

        TutorialButtonHover recipeHover =
            pointedButton.GetComponent<
                TutorialButtonHover
            >();


        if (recipeHover != null)
            recipeHover.SetHover(true);
    }


    // =========================================================
    // ボタン押下
    // =========================================================

    private void PressCurrentButton()
    {
        if (pointedButton == null)
        {
            Debug.Log(
                "⚠ ボタンに照準がありません"
            );

            return;
        }


        // -----------------------------------------
        // はい
        // -----------------------------------------

        if (pointedButton == yesButton)
        {
            OnYesButton();
            return;
        }


        // -----------------------------------------
        // いいえ
        // -----------------------------------------

        if (pointedButton == noButton)
        {
            OnNoButton();
            return;
        }


        // -----------------------------------------
        // レシピ
        // -----------------------------------------

        MolecularRecipeButton recipeButton =
            pointedButton.GetComponent<
                MolecularRecipeButton
            >();


        if (recipeButton != null)
        {
            SelectRecipe(
                recipeButton.recipe
            );
        }
    }


    // =========================================================
    // はい
    // =========================================================

    public void OnYesButton()
    {
        Debug.Log(
            "🟢 クラフト「はい」が押されました"
        );


        if (confirmPanel != null)
            confirmPanel.SetActive(false);


        if (recipePanel != null)
        {
            recipePanel.SetActive(true);


            // -----------------------------------------
            // レシピ一覧生成
            // -----------------------------------------

            DisplayCraftableRecipes();


            // -----------------------------------------
            // 画面切り替えSE
            // -----------------------------------------

            PlayTransitionSound();
        }


        pointedButton = null;

        UpdateButtonVisual();
    }


    // =========================================================
    // いいえ
    // =========================================================

    public void OnNoButton()
    {
        Debug.Log(
            "🔴 クラフト「いいえ」が押されました"
        );


        if (confirmPanel != null)
            confirmPanel.SetActive(false);


        pointedButton = null;

        UpdateButtonVisual();
    }


    // =========================================================
    // レシピ一覧生成
    // =========================================================

    private void DisplayCraftableRecipes()
    {
        if (craftingSystem == null)
        {
            Debug.LogError(
                "❌ MolecularCraftingSystemがありません"
            );

            return;
        }


        if (recipeContent == null)
        {
            Debug.LogError(
                "❌ Recipe Contentが設定されていません！"
            );

            return;
        }


        if (recipeButtonPrefab == null)
        {
            Debug.LogError(
                "❌ Recipe Button Prefabが設定されていません！"
            );

            return;
        }


        // -----------------------------------------
        // 古いボタンを削除
        // -----------------------------------------

        ClearRecipeButtons();


        // -----------------------------------------
        // クラフト可能レシピ取得
        // -----------------------------------------

        List<MolecularRecipe> recipes =
            craftingSystem.GetCraftableRecipes();


        if (recipes == null ||
            recipes.Count == 0)
        {
            Debug.Log(
                "⚠ 現在クラフト可能なレシピがありません"
            );

            return;
        }


        Debug.Log(
            "🧪 クラフト可能レシピ数：" +
            recipes.Count
        );


        // -----------------------------------------
        // レシピボタン生成
        // -----------------------------------------

        foreach (
            MolecularRecipe recipe
            in recipes)
        {
            if (recipe == null)
                continue;


            GameObject buttonObject =
                Instantiate(
                    recipeButtonPrefab,
                    recipeContent
                );


            MolecularRecipeButton recipeButton =
                buttonObject.GetComponent<
                    MolecularRecipeButton
                >();


            // 念のため子階層も検索
            if (recipeButton == null)
            {
                recipeButton =
                    buttonObject.GetComponentInChildren<
                        MolecularRecipeButton
                    >(true);
            }


            if (recipeButton == null)
            {
                Debug.LogError(
                    "❌ RecipeButtonPrefabに " +
                    "MolecularRecipeButton がありません！"
                );

                Destroy(buttonObject);

                continue;
            }


            recipeButton.Setup(recipe);


            spawnedRecipeButtons.Add(
                buttonObject
            );
        }


        // -----------------------------------------
        // Scroll位置を一番上へ
        // -----------------------------------------

        if (recipeScrollRect != null)
        {
            recipeScrollRect.verticalNormalizedPosition =
                1f;
        }
    }


    // =========================================================
    // レシピボタン削除
    // =========================================================

    private void ClearRecipeButtons()
    {
        for (
            int i = spawnedRecipeButtons.Count - 1;
            i >= 0;
            i--
        )
        {
            if (spawnedRecipeButtons[i] != null)
            {
                Destroy(
                    spawnedRecipeButtons[i]
                );
            }
        }


        spawnedRecipeButtons.Clear();


        pointedButton = null;
    }


    // =========================================================
    // レシピ選択
    // =========================================================

    private void SelectRecipe(
        MolecularRecipe recipe
    )
    {
        if (isSynthesisPlaying)
            return;


        if (recipe == null)
        {
            Debug.LogWarning(
                "⚠ レシピがありません"
            );

            return;
        }


        currentRecipe =
            recipe;


        Debug.Log(
            "🧪 レシピ選択：" +
            recipe.recipeName
        );


        // -----------------------------------------
        // 合成開始
        // -----------------------------------------

        StartCoroutine(
            PlaySynthesisSequence()
        );
    }


    // =========================================================
    // 合成演出
    // =========================================================

    private IEnumerator PlaySynthesisSequence()
    {
        isSynthesisPlaying = true;


        // -----------------------------------------
        // レシピ画面を閉じる
        // -----------------------------------------

        if (recipePanel != null)
            recipePanel.SetActive(false);


        // -----------------------------------------
        // 合成画面表示
        // -----------------------------------------

        if (synthesisPanel != null)
            synthesisPanel.SetActive(true);


        // ★画面切り替えSE
        PlayTransitionSound();


        pointedButton = null;

        UpdateButtonVisual();


        // -----------------------------------------
        // 成功画像リセット
        // -----------------------------------------

        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }


        if (successEffectImage != null)
        {
            successEffectImage.sprite = null;
            successEffectImage.enabled = false;
        }


        // -----------------------------------------
        // 合成時間
        // -----------------------------------------

        float totalSynthesisTime =
            GetTotalSynthesisTime();


        if (totalSynthesisTime <= 0f)
        {
            totalSynthesisTime = 2f;
        }


        // -----------------------------------------
        // 合成開始
        // -----------------------------------------

        bool started =
            craftingSystem.StartCrafting(
                currentRecipe,
                totalSynthesisTime
            );


        if (!started)
        {
            Debug.LogWarning(
                "⚠ 合成を開始できませんでした"
            );


            if (synthesisPanel != null)
                synthesisPanel.SetActive(false);


            isSynthesisPlaying = false;

            yield break;
        }


        // -----------------------------------------
        // 合成画像
        // -----------------------------------------

        if (synthesisSteps != null &&
            synthesisSteps.Count > 0)
        {
            foreach (
                SynthesisStep step
                in synthesisSteps)
            {
                if (step == null)
                    continue;


                if (synthesisImage != null)
                {
                    synthesisImage.sprite =
                        step.image;

                    synthesisImage.enabled =
                        step.image != null;
                }


                // ★画像切り替えSE
                PlayTransitionSound();


                Debug.Log(
                    "⚗️ 合成演出：" +
                    (step.image != null
                        ? step.image.name
                        : "画像なし")
                );


                yield return new WaitForSeconds(
                    Mathf.Max(
                        0.1f,
                        step.displayTime
                    )
                );
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }


        // -----------------------------------------
        // 合成画像を消す
        // -----------------------------------------

        if (synthesisImage != null)
        {
            synthesisImage.sprite = null;
            synthesisImage.enabled = false;
        }


        // -----------------------------------------
        // 成功演出
        // -----------------------------------------

        yield return StartCoroutine(
            PlaySuccessSequence()
        );


        // -----------------------------------------
        // 合成画面を閉じる
        // -----------------------------------------

        if (synthesisPanel != null)
            synthesisPanel.SetActive(false);


        // -----------------------------------------
        // 画像リセット
        // -----------------------------------------

        if (synthesisImage != null)
        {
            synthesisImage.sprite = null;
            synthesisImage.enabled = false;
        }


        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }


        if (successEffectImage != null)
        {
            successEffectImage.sprite = null;
            successEffectImage.enabled = false;
        }


        // -----------------------------------------
        // レシピ一覧へ戻る
        // -----------------------------------------

        if (recipePanel != null)
        {
            recipePanel.SetActive(true);


            // ★画面切り替えSE
            PlayTransitionSound();


            // 合成後の所持数を反映
            DisplayCraftableRecipes();
        }


        // -----------------------------------------
        // 状態リセット
        // -----------------------------------------

        isSynthesisPlaying = false;

        pointedButton = null;

        UpdateButtonVisual();


        Debug.Log(
            "✅ 合成完了 → レシピ一覧へ戻りました"
        );
    }


    // =========================================================
    // 成功演出
    // =========================================================

    private IEnumerator PlaySuccessSequence()
    {
        Debug.Log(
            "✨ 合成成功画面開始"
        );


        if (successSteps == null ||
            successSteps.Count == 0)
        {
            PlayCompletionSound();

            PlayCompletionEffect();

            yield return new WaitForSeconds(
                Mathf.Max(
                    0f,
                    completionEffectDuration
                )
            );

            yield break;
        }


        for (
            int i = 0;
            i < successSteps.Count;
            i++
        )
        {
            SuccessStep step =
                successSteps[i];


            if (step == null)
                continue;


            // -------------------------------------
            // 成功画像
            // -------------------------------------

            if (successImage != null)
            {
                successImage.sprite =
                    step.image;

                successImage.enabled =
                    step.image != null;
            }


            // ★成功画像切り替えSE
            PlayTransitionSound();


            // -------------------------------------
            // 最初だけ成功エフェクト
            // -------------------------------------

            if (i == 0)
            {
                ShowSuccessEffect();

                PlayCompletionSound();

                PlayCompletionEffect();
            }
            else
            {
                HideSuccessEffect();
            }


            yield return new WaitForSeconds(
                Mathf.Max(
                    0.1f,
                    step.displayTime
                )
            );
        }


        HideSuccessEffect();


        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }
    }


    // =========================================================
    // 成功エフェクト表示
    // =========================================================

    private void ShowSuccessEffect()
    {
        if (successEffectImage == null)
            return;


        if (successEffectSprite == null)
        {
            successEffectImage.enabled =
                false;

            return;
        }


        successEffectImage.sprite =
            successEffectSprite;

        successEffectImage.enabled =
            true;
    }


    // =========================================================
    // 成功エフェクト非表示
    // =========================================================

    private void HideSuccessEffect()
    {
        if (successEffectImage == null)
            return;


        successEffectImage.sprite =
            null;

        successEffectImage.enabled =
            false;
    }


    // =========================================================
    // 合成時間
    // =========================================================

    private float GetTotalSynthesisTime()
    {
        if (synthesisSteps == null ||
            synthesisSteps.Count == 0)
        {
            return 0f;
        }


        float total = 0f;


        foreach (
            SynthesisStep step
            in synthesisSteps)
        {
            if (step == null)
                continue;


            total +=
                Mathf.Max(
                    0.1f,
                    step.displayTime
                );
        }


        return total;
    }


    // =========================================================
    // UI切り替えSE
    // =========================================================

    private void PlayTransitionSound()
    {
        if (transitionSound == null)
            return;


        AudioSource.PlayClipAtPoint(
            transitionSound,
            transform.position,
            transitionSoundVolume
        );
    }


    // =========================================================
    // 合成完了SE
    // =========================================================

    private void PlayCompletionSound()
    {
        if (completionSound == null)
            return;


        AudioSource.PlayClipAtPoint(
            completionSound,
            transform.position,
            completionSoundVolume
        );
    }


    // =========================================================
    // 完成Prefabエフェクト
    // =========================================================

    private void PlayCompletionEffect()
    {
        if (completionEffectPrefab == null)
            return;


        Vector3 position;

        Quaternion rotation;


        if (completionEffectPoint != null)
        {
            position =
                completionEffectPoint.position;

            rotation =
                completionEffectPoint.rotation;
        }
        else if (synthesisPanel != null)
        {
            position =
                synthesisPanel.transform.position;

            rotation =
                synthesisPanel.transform.rotation;
        }
        else
        {
            position =
                transform.position;

            rotation =
                transform.rotation;
        }


        GameObject effect =
            Instantiate(
                completionEffectPrefab,
                position,
                rotation
            );


        if (completionEffectDuration > 0f)
        {
            Destroy(
                effect,
                completionEffectDuration
            );
        }
    }


    // =========================================================
    // 全パネル非表示
    // =========================================================

    public void HideAllPanels()
    {
        StopAllCoroutines();


        isSynthesisPlaying = false;

        isDraggingScrollbar = false;


        if (confirmPanel != null)
            confirmPanel.SetActive(false);


        if (recipePanel != null)
            recipePanel.SetActive(false);


        if (synthesisPanel != null)
            synthesisPanel.SetActive(false);


        if (synthesisImage != null)
        {
            synthesisImage.sprite = null;
            synthesisImage.enabled = false;
        }


        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }


        HideSuccessEffect();


        ClearRecipeButtons();


        pointedButton = null;

        UpdateButtonVisual();


        Debug.Log(
            "🔴 クラフトUIをすべて非表示"
        );
    }


    // =========================================================
    // 初期状態
    // =========================================================

    public void ResetCraftingUI()
    {
        StopAllCoroutines();


        isSynthesisPlaying = false;

        isDraggingScrollbar = false;

        currentRecipe = null;


        ClearRecipeButtons();


        if (confirmPanel != null)
            confirmPanel.SetActive(true);


        if (recipePanel != null)
            recipePanel.SetActive(false);


        if (synthesisPanel != null)
            synthesisPanel.SetActive(false);


        if (synthesisImage != null)
        {
            synthesisImage.sprite = null;
            synthesisImage.enabled = false;
        }


        if (successImage != null)
        {
            successImage.sprite = null;
            successImage.enabled = false;
        }


        HideSuccessEffect();


        if (recipeScrollRect != null)
        {
            recipeScrollRect.verticalNormalizedPosition =
                1f;
        }


        pointedButton = null;

        UpdateButtonVisual();


        Debug.Log(
            "🔄 MolecularCraftingUIを初期状態へ"
        );
    }


    // =========================================================
    // 右コントローラー検索
    // =========================================================

    private Transform FindRightController()
    {
        GameObject[] objects =
            FindObjectsByType<GameObject>(
                FindObjectsSortMode.None
            );


        foreach (GameObject obj in objects)
        {
            if (obj.name ==
                "RightHandAnchor")
            {
                return obj.transform;
            }
        }


        foreach (GameObject obj in objects)
        {
            if (obj.name ==
                "RightControllerAnchor")
            {
                return obj.transform;
            }
        }


        return null;
    }


    // =========================================================
    // 無効化
    // =========================================================

    private void OnDisable()
    {
        StopAllCoroutines();


        isSynthesisPlaying = false;

        isDraggingScrollbar = false;


        pointedButton = null;


        HideSuccessEffect();


        if (laser != null)
            laser.enabled = false;
    }
}