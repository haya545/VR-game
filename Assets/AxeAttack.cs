using UnityEngine;
using System.Collections;

public class AxeAttack : MonoBehaviour
{
    public Transform axe;

    public bool isAttacking = false;
    public bool hasHitTree = false;
    public bool hasHitOre = false;

    void Update()
    {
        // 右人差し指トリガーで斧を振る
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && !isAttacking)
        {
            StartCoroutine(SwingAxe());
        }
    }

    IEnumerator SwingAxe()
    {
        isAttacking = true;

        hasHitTree = false;
        hasHitOre = false;

        Quaternion startRot = axe.localRotation;
        Quaternion attackRot = Quaternion.Euler(60, -30, 0);

        float time = 0;

        // 振り下ろし
        while (time < 0.15f)
        {
            axe.localRotation = Quaternion.Slerp(startRot, attackRot, time / 0.15f);
            time += Time.deltaTime;
            yield return null;
        }

        time = 0;

        // 戻す
        while (time < 0.15f)
        {
            axe.localRotation = Quaternion.Slerp(attackRot, startRot, time / 0.15f);
            time += Time.deltaTime;
            yield return null;
        }

        axe.localRotation = startRot;
        isAttacking = false;
    }
}