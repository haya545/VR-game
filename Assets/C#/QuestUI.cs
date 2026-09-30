using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("会話UI")]
    public GameObject dialoguePanel;

    [Header("会話画像")]
    [Tooltip("会話画像を表示するUI Image")]
    public Image dialogueImage;

    [Header("選択肢UI")]
    public GameObject choicePanel;

    [Header("はいボタン")]
    public Button yesButton;

    [Header("ちょっと待ってボタン")]
    public Button waitButton;

    // =========================================================
    // VR照準
    // =========================================================

    [Header("VR右コントローラー")]
    [Tooltip("右コントローラー。空欄ならRightHandAnchorを自動検索")]
    public Transform rightControllerTransform;

    [Tooltip("照準の最大距離")]
    public float rayDistance = 20f;

    // =========================================================
    // ボタン見た目
    // =========================================================

    [Header("選択中ボタンの見た目")]
    [Tooltip("選択中のボタンを何倍にするか")]
    public float selectedButtonScale = 1.12f;

    [Tooltip("青い縁の太さ")]
    public float outlineSize = 12f;

    [Tooltip("選択中の青い縁の色")]
    public Color selectedOutlineColor =
        new Color(0.1f, 0.5f, 1f, 1f);

    // =========================================================
    // 会話UI位置
    // =========================================================

    [Header("会話UIの表示位置")]
    public float dialogueDistance = 1.5f;

    public float dialogueHeight = 1.5f;

    public float dialogueHorizontalOffset = 0f;

    // =========================================================
    // UIの向き
    // =========================================================

    [Header("UIの向き")]
    public bool facePlayer = true;

    // =========================================================
    // 内部変数
    // =========================================================

    private QuestDialogue[] currentDialogues;

    private QuestNPC currentNPC;

    private int currentIndex = 0;

    private bool talking = false;

    private Transform playerTransform;

    // 現在照準が当たっているボタン
    private Button pointedButton;

    // ボタン元サイズ
    private Vector3 yesOriginalScale;

    private Vector3 waitOriginalScale;

    // Outline
    private Outline yesOutline;

    private Outline waitOutline;

    // 選択肢表示中にZRを一度離したか
    private bool triggerReleased = false;


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        // -----------------------------------------------------
        // Player
        // -----------------------------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform =
                player.transform;

            Debug.Log(
                "✅ QuestUIがPlayerを取得：" +
                player.name
            );
        }
        else
        {
            Debug.LogWarning(
                "⚠ Tag = Player のオブジェクトが見つかりません"
            );
        }


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


        // -----------------------------------------------------
        // はいボタン
        // -----------------------------------------------------

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(
                OnYesButton
            );

            yesButton.onClick.AddListener(
                OnYesButton
            );

            yesOriginalScale =
                yesButton.transform.localScale;

            yesOutline =
                yesButton.GetComponent<Outline>();

            if (yesOutline == null)
            {
                yesOutline =
                    yesButton.gameObject
                        .AddComponent<Outline>();
            }

            yesOutline.enabled = false;

            yesOutline.effectColor =
                selectedOutlineColor;

            yesOutline.effectDistance =
                new Vector2(
                    outlineSize,
                    outlineSize
                );
        }


        // -----------------------------------------------------
        // ちょっと待って
        // -----------------------------------------------------

        if (waitButton != null)
        {
            waitButton.onClick.RemoveListener(
                OnWaitButton
            );

            waitButton.onClick.AddListener(
                OnWaitButton
            );

            waitOriginalScale =
                waitButton.transform.localScale;

            waitOutline =
                waitButton.GetComponent<Outline>();

            if (waitOutline == null)
            {
                waitOutline =
                    waitButton.gameObject
                        .AddComponent<Outline>();
            }

            waitOutline.enabled = false;

            waitOutline.effectColor =
                selectedOutlineColor;

            waitOutline.effectDistance =
                new Vector2(
                    outlineSize,
                    outlineSize
                );
        }


        // -----------------------------------------------------
        // 会話画像
        // -----------------------------------------------------

        SetupDialogueImage();


        Debug.Log(
            "✅ QuestUIが起動しました"
        );
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (!talking)
        {
            return;
        }

        if (
            choicePanel == null ||
            !choicePanel.activeSelf
        )
        {
            return;
        }

        UpdateButtonRay();

        UpdateTriggerInput();
    }


    // =========================================================
    // 右コントローラーの照準
    // =========================================================

    private void UpdateButtonRay()
    {
        if (rightControllerTransform == null)
        {
            return;
        }

        Ray ray =
            new Ray(
                rightControllerTransform.position,
                rightControllerTransform.forward
            );

        Button newButton =
            FindButtonByRay(ray);


        if (newButton != pointedButton)
        {
            pointedButton =
                newButton;

            UpdateChoiceVisual();
        }
    }


    // =========================================================
    // ZR入力
    // =========================================================

    private void UpdateTriggerInput()
    {
        bool trigger =
            OVRInput.Get(
                OVRInput.Button.SecondaryIndexTrigger
            );


        if (!trigger)
        {
            triggerReleased = true;
        }


        if (
            triggerReleased &&
            OVRInput.GetDown(
                OVRInput.Button.SecondaryIndexTrigger
            )
        )
        {
            Debug.Log(
                "🔫 QuestUI：右ZR入力を検出！"
            );

            triggerReleased = false;


            if (pointedButton == yesButton)
            {
                Debug.Log(
                    "🎯 ZR → 「はい」"
                );

                OnYesButton();
            }
            else if (pointedButton == waitButton)
            {
                Debug.Log(
                    "🎯 ZR → 「ちょっと待って」"
                );

                OnWaitButton();
            }
            else
            {
                Debug.Log(
                    "⚠ ZRを押しましたが、ボタンに照準がありません"
                );
            }
        }
    }


    // =========================================================
    // Rayでボタンを探す
    // =========================================================

    private Button FindButtonByRay(
        Ray ray
    )
    {
        Button[] buttons =
        {
            yesButton,
            waitButton
        };


        Button result = null;

        float closestDistance =
            float.MaxValue;


        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }


            if (!button.gameObject.activeInHierarchy)
            {
                continue;
            }


            RectTransform rect =
                button.GetComponent<RectTransform>();

            if (rect == null)
            {
                continue;
            }


            Plane plane =
                new Plane(
                    rect.forward,
                    rect.position
                );


            float distance;


            if (!plane.Raycast(
                ray,
                out distance
            ))
            {
                continue;
            }


            if (
                distance < 0f ||
                distance > rayDistance
            )
            {
                continue;
            }


            Vector3 hitPoint =
                ray.GetPoint(distance);


            Vector3 localPoint =
                rect.InverseTransformPoint(
                    hitPoint
                );


            Rect area =
                rect.rect;


            if (
                area.Contains(
                    new Vector2(
                        localPoint.x,
                        localPoint.y
                    )
                )
            )
            {
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
    // 選択中ボタンの見た目
    // =========================================================

    private void UpdateChoiceVisual()
    {
        if (yesButton != null)
        {
            if (pointedButton == yesButton)
            {
                yesButton.transform.localScale =
                    yesOriginalScale *
                    selectedButtonScale;
            }
            else
            {
                yesButton.transform.localScale =
                    yesOriginalScale;
            }
        }


        if (waitButton != null)
        {
            if (pointedButton == waitButton)
            {
                waitButton.transform.localScale =
                    waitOriginalScale *
                    selectedButtonScale;
            }
            else
            {
                waitButton.transform.localScale =
                    waitOriginalScale;
            }
        }


        if (yesOutline != null)
        {
            yesOutline.enabled =
                pointedButton == yesButton;

            yesOutline.effectColor =
                selectedOutlineColor;

            yesOutline.effectDistance =
                new Vector2(
                    outlineSize,
                    outlineSize
                );
        }


        if (waitOutline != null)
        {
            waitOutline.enabled =
                pointedButton == waitButton;

            waitOutline.effectColor =
                selectedOutlineColor;

            waitOutline.effectDistance =
                new Vector2(
                    outlineSize,
                    outlineSize
                );
        }
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
    // 会話画像設定
    // =========================================================

    private void SetupDialogueImage()
    {
        if (dialogueImage == null)
        {
            Debug.LogWarning(
                "⚠ Dialogue Imageが設定されていません！"
            );

            return;
        }


        dialogueImage.preserveAspect = true;
    }


    // =========================================================
    // 会話開始
    // =========================================================

    public void StartDialogue(
        QuestDialogue[] dialogues,
        QuestNPC npc,
        int startIndex = 0
    )
    {
        if (
            dialogues == null ||
            dialogues.Length == 0
        )
        {
            Debug.LogWarning(
                "⚠ 会話データがありません！"
            );

            return;
        }


        currentDialogues =
            dialogues;

        currentNPC =
            npc;

        currentIndex =
            startIndex;

        talking =
            true;


        Debug.Log(
            "💬 会話開始！"
        );


        if (currentNPC != null)
        {
            MoveDialoguePanelToNPC(
                currentNPC.transform
            );
        }


        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }


        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }


        ShowCurrentDialogue();
    }


    // =========================================================
    // NPCの前にUIを移動
    // =========================================================

    private void MoveDialoguePanelToNPC(
        Transform npc
    )
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning(
                "⚠ Dialogue Panelが設定されていません"
            );

            return;
        }


        if (npc == null)
        {
            return;
        }


        Vector3 forward =
            npc.forward;

        forward.y = 0f;


        if (
            forward.sqrMagnitude <
            0.001f
        )
        {
            forward =
                Vector3.forward;
        }


        forward.Normalize();


        Vector3 right =
            npc.right;

        right.y = 0f;


        if (
            right.sqrMagnitude <
            0.001f
        )
        {
            right =
                Vector3.right;
        }


        right.Normalize();


        Vector3 position =
            npc.position;


        position +=
            forward *
            dialogueDistance;


        position.y +=
            dialogueHeight;


        position +=
            right *
            dialogueHorizontalOffset;


        dialoguePanel.transform.position =
            position;


        if (
            facePlayer &&
            playerTransform != null
        )
        {
            Vector3 direction =
                playerTransform.position -
                dialoguePanel.transform.position;

            direction.y = 0f;


            if (
                direction.sqrMagnitude >
                0.001f
            )
            {
                direction.Normalize();


                dialoguePanel.transform.rotation =
                    Quaternion.LookRotation(
                        -direction
                    );
            }
        }


        Debug.Log(
            "📍 会話UIをNPCの前に移動：" +
            position
        );
    }


    // =========================================================
    // 現在の会話
    // =========================================================

    private void ShowCurrentDialogue()
    {
        if (
            currentDialogues == null ||
            currentIndex < 0 ||
            currentIndex >=
            currentDialogues.Length
        )
        {
            FinishDialogue();

            return;
        }


        QuestDialogue dialogue =
            currentDialogues[
                currentIndex
            ];


        // =====================================================
        // ★ 会話画像を表示
        // =====================================================

        if (dialogueImage != null)
        {
            dialogueImage.sprite =
                dialogue.dialogueImage;

            dialogueImage.preserveAspect =
                true;

            dialogueImage.enabled =
                dialogue.dialogueImage != null;
        }


        Debug.Log(
            "💬 会話 " +
            (currentIndex + 1) +
            "/" +
            currentDialogues.Length +
            "：" +
            dialogue.text
        );


        // =====================================================
        // 効果音
        // =====================================================

        PlaySounds(dialogue);

        PlayEffects(dialogue);
    }


    // =========================================================
    // 次の会話
    // =========================================================

    public void NextDialogue()
    {
        if (!talking)
        {
            return;
        }


        if (
            currentIndex >=
            currentDialogues.Length - 1
        )
        {
            if (currentNPC != null)
            {
                if (
                    !currentNPC.IsQuestCompleted()
                )
                {
                    if (
                        currentNPC.HasRequiredItem()
                    )
                    {
                        ShowChoice();

                        return;
                    }
                }
            }


            FinishDialogue();

            return;
        }


        currentIndex++;

        ShowCurrentDialogue();
    }


    // =========================================================
    // 選択肢表示
    // =========================================================

    private void ShowChoice()
    {
        Debug.Log(
            "❓ 素材を渡しますか？"
        );


        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }


        // -----------------------------------------------------
        // ★ 選択肢用画像
        // -----------------------------------------------------
        //
        // QuestDialogueの最後の画像をそのまま表示せず、
        // 別途設定したchoiceImageがあれば表示します。
        //

        if (dialogueImage != null)
        {
            dialogueImage.sprite = null;
            dialogueImage.enabled = false;
        }


        pointedButton = null;

        triggerReleased = false;

        UpdateChoiceVisual();


        Debug.Log(
            "🎯 選択画面表示完了"
        );
    }


    // =========================================================
    // はい
    // =========================================================

    private void OnYesButton()
    {
        Debug.Log(
            "👉 「はい」が押されました"
        );


        if (currentNPC == null)
        {
            Debug.LogWarning(
                "⚠ 現在のNPCがありません！"
            );

            return;
        }


        bool success =
            currentNPC.GiveRequiredItem();


        if (!success)
        {
            Debug.LogWarning(
                "⚠ 素材を渡せませんでした。"
            );

            return;
        }


        Debug.Log(
            "🎉 クエスト完了！"
        );


        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }


        FinishDialogue();
    }


    // =========================================================
    // ちょっと待って
    // =========================================================

    private void OnWaitButton()
    {
        Debug.Log(
            "👉 「ちょっと待って」が押されました"
        );


        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }


        FinishDialogue();
    }


    // =========================================================
    // 効果音
    // =========================================================

    private void PlaySounds(
        QuestDialogue dialogue
    )
    {
        if (dialogue == null)
        {
            return;
        }


        if (
            dialogue.soundEffects == null ||
            dialogue.soundEffects.Length == 0
        )
        {
            Debug.Log(
                "🔇 この会話には効果音が設定されていません"
            );

            return;
        }


        Vector3 soundPosition =
            transform.position;


        if (dialoguePanel != null)
        {
            soundPosition =
                dialoguePanel.transform.position;
        }


        foreach (
            AudioClip clip
            in dialogue.soundEffects
        )
        {
            if (clip == null)
            {
                continue;
            }


            Debug.Log(
                "🔊 会話効果音を再生：" +
                clip.name
            );


            AudioSource.PlayClipAtPoint(
                clip,
                soundPosition,
                1.0f
            );
        }
    }


    // =========================================================
    // エフェクト
    // =========================================================

    private void PlayEffects(
        QuestDialogue dialogue
    )
    {
        if (dialogue == null)
        {
            return;
        }


        if (
            dialogue.effects == null ||
            dialogue.effects.Length == 0
        )
        {
            Debug.Log(
                "✨ この会話にはエフェクトが設定されていません"
            );

            return;
        }


        Vector3 effectPosition =
            transform.position;


        if (dialoguePanel != null)
        {
            effectPosition =
                dialoguePanel.transform.position;
        }


        foreach (
            GameObject effect
            in dialogue.effects
        )
        {
            if (effect == null)
            {
                continue;
            }


            Debug.Log(
                "✨ 会話エフェクトを再生：" +
                effect.name
            );


            GameObject spawnedEffect =
                Instantiate(
                    effect,
                    effectPosition,
                    Quaternion.identity
                );


            ParticleSystem[] particles =
                spawnedEffect.GetComponentsInChildren<ParticleSystem>(
                    true
                );


            foreach (
                ParticleSystem particle
                in particles
            )
            {
                particle.Play();
            }
        }
    }


    // =========================================================
    // 会話終了
    // =========================================================

    public void FinishDialogue()
    {
        talking = false;


        currentDialogues = null;

        currentNPC = null;

        currentIndex = 0;


        pointedButton = null;

        triggerReleased = false;


        // -----------------------------------------------------
        // ボタンサイズを元に戻す
        // -----------------------------------------------------

        if (yesButton != null)
        {
            yesButton.transform.localScale =
                yesOriginalScale;
        }


        if (waitButton != null)
        {
            waitButton.transform.localScale =
                waitOriginalScale;
        }


        // -----------------------------------------------------
        // Outline
        // -----------------------------------------------------

        if (yesOutline != null)
        {
            yesOutline.enabled = false;
        }


        if (waitOutline != null)
        {
            waitOutline.enabled = false;
        }


        // -----------------------------------------------------
        // UI非表示
        // -----------------------------------------------------

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }


        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }


        // -----------------------------------------------------
        // 会話画像を消す
        // -----------------------------------------------------

        if (dialogueImage != null)
        {
            dialogueImage.sprite = null;
            dialogueImage.enabled = false;
        }


        Debug.Log(
            "🚪 会話終了"
        );
    }


    // =========================================================
    // 外部から会話終了
    // =========================================================

    public void HideDialogue()
    {
        FinishDialogue();
    }


    // =========================================================
    // 会話中か確認
    // =========================================================

    public bool IsTalking()
    {
        return talking;
    }


    // =========================================================
    // 選択画面が開いているか確認
    // =========================================================

    public bool IsChoiceOpen()
    {
        return talking &&
               choicePanel != null &&
               choicePanel.activeSelf;
    }
}