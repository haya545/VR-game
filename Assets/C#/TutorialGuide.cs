using UnityEngine;
using UnityEngine.UI;

public class TutorialGuide : MonoBehaviour
{
    // =========================================================
    // 説明ページ画像
    // =========================================================

    [Header("説明ページ画像")]
    public Sprite[] pages;

    [Header("ページ表示")]
    public Image pageImage;


    // =========================================================
    // ボタン
    // =========================================================

    [Header("ボタン")]
    public GameObject backButton;
    public GameObject nextButton;

    [Header("ボタンホバー")]
    public TutorialButtonHover backButtonHover;
    public TutorialButtonHover nextButtonHover;


    // =========================================================
    // VR照準
    // =========================================================

    [Header("VR右コントローラー")]
    [Tooltip("空欄ならRightHandAnchorを自動検索")]
    public Transform rightControllerTransform;

    [Header("照準距離")]
    public float rayDistance = 20f;


    // =========================================================
    // レーザー
    // =========================================================

    [Header("レーザー")]
    [Tooltip("RightHandAnchorの子にあるLine Renderer")]
    public LineRenderer laser;


    // =========================================================
    // ページ移動効果音
    // =========================================================

    [Header("ページ移動効果音")]
    public AudioClip[] pageSounds;

    [Header("効果音音量")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;


    // =========================================================
    // ページ移動エフェクト
    // =========================================================

    [Header("ページ移動エフェクト")]
    public GameObject[] pageEffects;

    [Header("エフェクトを表示する時間")]
    public float effectDuration = 1f;


    // =========================================================
    // 内部変数
    // =========================================================

    private int currentPage = 0;

    // 現在照準しているボタン
    private GameObject pointedButton;

    // 現在ホバーしているTutorialButtonHover
    private TutorialButtonHover pointedHover;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // 右コントローラー
        // -----------------------------------------------------

        if (rightControllerTransform == null)
        {
            rightControllerTransform =
                FindRightController();
        }

        if (rightControllerTransform != null)
        {
            Debug.Log(
                "🎯 TutorialGuide：右コントローラー：" +
                rightControllerTransform.name
            );
        }
        else
        {
            Debug.LogError(
                "❌ TutorialGuide：RightHandAnchorが見つかりません！"
            );
        }


        // -----------------------------------------------------
        // ページ
        // -----------------------------------------------------

        currentPage = 0;

        ShowPage();


        // -----------------------------------------------------
        // レーザー
        // -----------------------------------------------------

        if (laser != null)
        {
            laser.enabled = true;

            // 2点のレーザー
            laser.positionCount = 2;
        }
        else
        {
            Debug.LogWarning(
                "⚠ TutorialGuide：Laserが設定されていません！"
            );
        }
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        // -----------------------------------------
        // レーザーと照準
        // -----------------------------------------

        UpdateButtonRay();


        // -----------------------------------------
        // 右コントローラーのトリガー
        // -----------------------------------------

        if (
            OVRInput.GetDown(
                OVRInput.Button.SecondaryIndexTrigger
            )
        )
        {
            PressCurrentButton();
        }
    }


    // =========================================================
    // 右コントローラー照準
    // =========================================================

    private void UpdateButtonRay()
    {
        if (rightControllerTransform == null)
        {
            return;
        }


        // -----------------------------------------------------
        // 右手からRayを飛ばす
        // -----------------------------------------------------

        Ray ray =
            new Ray(
                rightControllerTransform.position,
                rightControllerTransform.forward
            );


        // -----------------------------------------------------
        // 一番近いボタンを探す
        // -----------------------------------------------------

        GameObject newButton =
            FindButtonByRay(ray);


        // -----------------------------------------------------
        // レーザー終点
        // -----------------------------------------------------

        Vector3 laserStart =
            rightControllerTransform.position;

        Vector3 laserEnd =
            laserStart +
            rightControllerTransform.forward *
            rayDistance;


        // ボタンに当たっている場合
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


                if (
                    plane.Raycast(
                        ray,
                        out distance
                    )
                )
                {
                    if (
                        distance >= 0f &&
                        distance <= rayDistance
                    )
                    {
                        laserEnd =
                            ray.GetPoint(distance);
                    }
                }
            }
        }


        // -----------------------------------------------------
        // レーザーを常に表示
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // 照準先が変わった
        // -----------------------------------------------------

        if (newButton != pointedButton)
        {
            pointedButton =
                newButton;

            UpdateButtonVisual();
        }
    }


    // =========================================================
    // Rayでボタンを探す
    // QuestUIと同じ方式
    // =========================================================

    private GameObject FindButtonByRay(
        Ray ray
    )
    {
        GameObject[] buttons =
        {
            backButton,
            nextButton
        };


        GameObject result = null;

        float closestDistance =
            float.MaxValue;


        foreach (GameObject button in buttons)
        {
            if (button == null)
            {
                continue;
            }


            // 非表示ボタンは無視
            if (!button.activeInHierarchy)
            {
                continue;
            }


            RectTransform rect =
                button.GetComponent<RectTransform>();


            if (rect == null)
            {
                continue;
            }


            // -------------------------------------------------
            // ボタンの平面
            // -------------------------------------------------

            Plane plane =
                new Plane(
                    rect.forward,
                    rect.position
                );


            float distance;


            // Rayとボタン平面の交点
            if (
                !plane.Raycast(
                    ray,
                    out distance
                )
            )
            {
                continue;
            }


            // -------------------------------------------------
            // 距離チェック
            // -------------------------------------------------

            if (
                distance < 0f ||
                distance > rayDistance
            )
            {
                continue;
            }


            // -------------------------------------------------
            // Rayが当たった位置
            // -------------------------------------------------

            Vector3 hitPoint =
                ray.GetPoint(distance);


            // -------------------------------------------------
            // ボタンのローカル座標
            // -------------------------------------------------

            Vector3 localPoint =
                rect.InverseTransformPoint(
                    hitPoint
                );


            Rect area =
                rect.rect;


            // -------------------------------------------------
            // 本当にボタンの中か確認
            // -------------------------------------------------

            if (
                area.Contains(
                    new Vector2(
                        localPoint.x,
                        localPoint.y
                    )
                )
            )
            {
                // 一番近いボタンを採用
                if (
                    distance <
                    closestDistance
                )
                {
                    closestDistance =
                        distance;

                    result =
                        button;
                }
            }
        }


        return result;
    }


    // =========================================================
    // ボタンの見た目
    // =========================================================

    private void UpdateButtonVisual()
    {
        // -----------------------------------------------------
        // 一旦すべて解除
        // -----------------------------------------------------

        if (backButtonHover != null)
        {
            backButtonHover.SetHover(false);
        }

        if (nextButtonHover != null)
        {
            nextButtonHover.SetHover(false);
        }

        pointedHover = null;


        // -----------------------------------------------------
        // 何も狙っていない
        // -----------------------------------------------------

        if (pointedButton == null)
        {
            return;
        }


        // -----------------------------------------------------
        // 戻るボタン
        // -----------------------------------------------------

        if (pointedButton == backButton)
        {
            if (backButtonHover != null)
            {
                backButtonHover.SetHover(true);

                pointedHover =
                    backButtonHover;
            }

            return;
        }


        // -----------------------------------------------------
        // 次へボタン
        // -----------------------------------------------------

        if (pointedButton == nextButton)
        {
            if (nextButtonHover != null)
            {
                nextButtonHover.SetHover(true);

                pointedHover =
                    nextButtonHover;
            }

            return;
        }
    }


    // =========================================================
    // トリガーで現在のボタンを押す
    // =========================================================

    private void PressCurrentButton()
    {
        if (pointedButton == null)
        {
            Debug.Log(
                "⚠ TutorialGuide：ボタンに照準がありません"
            );

            return;
        }


        // -----------------------------------------------------
        // 次へ
        // -----------------------------------------------------

        if (pointedButton == nextButton)
        {
            Debug.Log(
                "🎯 TutorialGuide：次へボタンを選択"
            );

            NextPage();

            return;
        }


        // -----------------------------------------------------
        // 戻る
        // -----------------------------------------------------

        if (pointedButton == backButton)
        {
            Debug.Log(
                "🎯 TutorialGuide：戻るボタンを選択"
            );

            PreviousPage();

            return;
        }
    }


    // =========================================================
    // ページ表示
    // =========================================================

    private void ShowPage()
    {
        if (
            pages == null ||
            pages.Length == 0
        )
        {
            Debug.LogWarning(
                "⚠ TutorialGuide：説明画像が設定されていません。"
            );

            return;
        }


        // ページ番号を安全な範囲にする
        if (currentPage < 0)
        {
            currentPage = 0;
        }


        if (
            currentPage >=
            pages.Length
        )
        {
            currentPage =
                pages.Length - 1;
        }


        // -----------------------------------------------------
        // ページ画像
        // -----------------------------------------------------

        if (pageImage != null)
        {
            pageImage.sprite =
                pages[currentPage];

            pageImage.preserveAspect =
                true;
        }


        // -----------------------------------------------------
        // 戻るボタン
        // -----------------------------------------------------

        if (backButton != null)
        {
            backButton.SetActive(
                currentPage > 0
            );
        }


        // -----------------------------------------------------
        // 次へボタン
        // -----------------------------------------------------

        if (nextButton != null)
        {
            nextButton.SetActive(
                currentPage <
                pages.Length - 1
            );
        }


        // -----------------------------------------------------
        // 照準解除
        // -----------------------------------------------------

        pointedButton = null;

        UpdateButtonVisual();
    }


    // =========================================================
    // 次のページ
    // =========================================================

    public void NextPage()
    {
        if (
            pages == null ||
            pages.Length == 0
        )
        {
            return;
        }


        if (
            currentPage >=
            pages.Length - 1
        )
        {
            return;
        }


        currentPage++;


        PlayPageChange();


        ShowPage();
    }


    // =========================================================
    // 前のページ
    // =========================================================

    public void PreviousPage()
    {
        if (
            pages == null ||
            pages.Length == 0
        )
        {
            return;
        }


        if (currentPage <= 0)
        {
            return;
        }


        currentPage--;


        PlayPageChange();


        ShowPage();
    }


    // =========================================================
    // RightHandAnchor検索
    // =========================================================

    private Transform FindRightController()
    {
        GameObject[] objects =
            FindObjectsByType<GameObject>(
                FindObjectsSortMode.None
            );


        // RightHandAnchor
        foreach (GameObject obj in objects)
        {
            if (
                obj.name ==
                "RightHandAnchor"
            )
            {
                return obj.transform;
            }
        }


        // RightControllerAnchor
        foreach (GameObject obj in objects)
        {
            if (
                obj.name ==
                "RightControllerAnchor"
            )
            {
                return obj.transform;
            }
        }


        return null;
    }


    // =========================================================
    // ページ移動演出
    // =========================================================

    private void PlayPageChange()
    {
        // -----------------------------------------------------
        // 効果音
        // -----------------------------------------------------

        if (
            pageSounds != null &&
            currentPage >= 0 &&
            currentPage <
            pageSounds.Length &&
            pageSounds[currentPage] != null
        )
        {
            AudioSource.PlayClipAtPoint(
                pageSounds[currentPage],
                transform.position,
                soundVolume
            );
        }


        // -----------------------------------------------------
        // エフェクト
        // -----------------------------------------------------

        if (
            pageEffects != null &&
            currentPage >= 0 &&
            currentPage <
            pageEffects.Length &&
            pageEffects[currentPage] != null &&
            pageImage != null
        )
        {
            GameObject effect =
                Instantiate(
                    pageEffects[currentPage],
                    pageImage.transform.position,
                    pageImage.transform.rotation
                );


            Destroy(
                effect,
                effectDuration
            );
        }
    }
}