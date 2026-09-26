using UnityEngine;
using System.Collections;

public class AxeAttack : MonoBehaviour
{
    public Transform axe;

    public bool isAttacking = false;
    public bool hasHitTree = false;
    public bool hasHitOre = false;

    [Header("========== 振動設定 ==========")]
    [Header("ヒット時の振動時間（秒）")]
    public float hitVibrationDuration = 0.15f;
    [Header("ヒット時の振動周波数 (0～1)")]
    [Range(0f, 1f)] public float hitVibrationFrequency = 0.8f;
    [Header("ヒット時の振動強さ (0～1)")]
    [Range(0f, 1f)] public float hitVibrationAmplitude = 1.0f;

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

    // ★ 木や鉱石に当たった時に外部や OnTriggerEnter 等から呼び出すメソッド
    public void OnHitTarget()
    {
        // 右手コントローラーを振動させる
        StartCoroutine(VibrateController(hitVibrationDuration, hitVibrationFrequency, hitVibrationAmplitude, OVRInput.Controller.RTouch));
    }

    // 振動制御コルーチン
    private IEnumerator VibrateController(float duration, float frequency, float amplitude, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(frequency, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
}