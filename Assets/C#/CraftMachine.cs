using UnityEngine;

public class CraftMachine : MonoBehaviour
{
    public int requiredWood = 3;

    public GameObject resultPrefab;

    public Transform spawnPoint;

    private int currentWood = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Wood"))
            return;

        currentWood++;

        Debug.Log(
            "木材投入 "
            + currentWood
            + "/"
            + requiredWood
        );

        Destroy(other.gameObject);

        if (currentWood >= requiredWood)
        {
            Craft();
        }
    }

    void Craft()
    {
        Instantiate(
            resultPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        Debug.Log("生成成功");

        currentWood = 0;
    }
}