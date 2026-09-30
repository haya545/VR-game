using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonHover : MonoBehaviour
{
    [Header("ホバー時の拡大率")]
    public float hoverScale = 1.15f;

    [Header("アウトライン")]
    public Outline outline;

    [Header("アウトラインの太さ")]
    public Vector2 outlineDistance = new Vector2(5f, 5f);

    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = transform.localScale;

        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        if (outline != null)
        {
            outline.enabled = false;
            outline.effectDistance = outlineDistance;
        }
    }

    public void SetHover(bool hover)
    {
        if (hover)
        {
            transform.localScale = normalScale * hoverScale;

            if (outline != null)
            {
                outline.enabled = true;
            }
        }
        else
        {
            transform.localScale = normalScale;

            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }
}