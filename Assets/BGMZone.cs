using UnityEngine;

public class BGMZone : MonoBehaviour
{
    [Header("このエリアで流すBGM")]
    public AudioClip bgm;

    [Header("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        // ① 侵入自体を検知できているか確認
        Debug.Log($"[BGMZone] 進入を検知: オブジェクト名='{other.name}', Tag='{other.tag}'");

        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning($"[BGMZone] タグが 'Player' ではないためスキップしました (検出タグ: '{other.tag}')");
            return;
        }

        if (BGMManager.Instance != null)
        {
            if (bgm == null)
            {
                Debug.LogError("[BGMZone] AudioClip (bgm) が Inspector で割り当てられていません！");
                return;
            }

            Debug.Log($"[BGMZone] '{bgm.name}' の再生リクエストを BGMManager に送信しました。");
            BGMManager.Instance.PlayBGM(bgm, volume);
        }
        else
        {
            Debug.LogError("[BGMZone] BGMManager.Instance が null です！シーン内に BGMManager が配置されているか確認してください。");
        }
    }
}