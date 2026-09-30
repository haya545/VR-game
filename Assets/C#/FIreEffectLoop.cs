using UnityEngine;
using System.Collections;

public class FireEffectLoop : MonoBehaviour
{
    [System.Serializable]
    public class EffectSetting
    {
        [Header("エフェクトPrefab")]
        public GameObject effectPrefab;

        [Header("繰り返し間隔（秒）")]
        public float interval = 1.0f;

        [Header("最初に出すまでの待ち時間")]
        public float startDelay = 0f;

        [Header("エフェクトを消すまでの時間")]
        public float lifetime = 1.0f;
    }

    [Header("ループさせるエフェクト")]
    public EffectSetting[] effects;

    private Coroutine[] routines;

    private void OnEnable()
    {
        StartEffects();
    }

    private void OnDisable()
    {
        StopEffects();
    }

    private void StartEffects()
    {
        if (effects == null)
        {
            return;
        }

        routines = new Coroutine[effects.Length];

        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].effectPrefab != null)
            {
                routines[i] = StartCoroutine(EffectLoop(effects[i]));
            }
        }
    }

    private void StopEffects()
    {
        if (routines == null)
        {
            return;
        }

        for (int i = 0; i < routines.Length; i++)
        {
            if (routines[i] != null)
            {
                StopCoroutine(routines[i]);
            }
        }

        routines = null;
    }

    private IEnumerator EffectLoop(EffectSetting setting)
    {
        if (setting.startDelay > 0)
        {
            yield return new WaitForSeconds(setting.startDelay);
        }

        while (true)
        {
            GameObject effect = Instantiate(
                setting.effectPrefab,
                transform.position,
                transform.rotation
            );

            // Firehandについて動くようにする
            effect.transform.SetParent(transform);

            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;

            if (setting.lifetime > 0)
            {
                Destroy(effect, setting.lifetime);
            }

            yield return new WaitForSeconds(setting.interval);
        }
    }
}