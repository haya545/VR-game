using UnityEngine;

[System.Serializable]
public class QuestDialogue
{
    [Header("表示する文章")]
    [TextArea(2, 5)]
    public string text;

    [Header("この文章で鳴らす効果音")]
    public AudioClip[] soundEffects;

    [Header("この文章で再生するエフェクト")]
    public GameObject[] effects;
}