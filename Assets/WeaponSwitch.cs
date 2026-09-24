using UnityEngine;

public class WeaponSwitch : MonoBehaviour
{
    [Header("武器")]
    public GameObject axe;
    public GameObject gun;
    public GameObject fire;

    // 0 = 素手
    // 1 = 斧
    // 2 = 銃
    // 3 = 炎
    private int currentWeapon = 0;

    // 現在の武器を外部スクリプトから取得
    public int CurrentWeapon => currentWeapon;

    void Start()
    {
        SetWeapon(0);
    }

    void Update()
    {
        // 左コントローラー Yボタンで武器切り替え
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            currentWeapon++;

            if (currentWeapon > 3)
            {
                currentWeapon = 0;
            }

            SetWeapon(currentWeapon);
        }
    }

    void SetWeapon(int weapon)
    {
        // 全武器を非表示
        if (axe != null)
        {
            axe.SetActive(false);
        }

        if (gun != null)
        {
            gun.SetActive(false);
        }

        if (fire != null)
        {
            fire.SetActive(false);
        }

        // 選択した武器だけ表示
        switch (weapon)
        {
            case 0:
                Debug.Log("素手");
                break;

            case 1:
                if (axe != null)
                {
                    axe.SetActive(true);
                }

                Debug.Log("斧");
                break;

            case 2:
                if (gun != null)
                {
                    gun.SetActive(true);
                }

                Debug.Log("銃");
                break;

            case 3:
                if (fire != null)
                {
                    fire.SetActive(true);
                }

                Debug.Log("炎");
                break;
        }
    }
}