using UnityEngine;

public class WoodPickup : MonoBehaviour
{
    [Header("VR手元設定")]
    public Transform controllerTransform; // 右コントローラー (RightHandAnchor)
    public Transform holdPoint;            // オブジェクトを保持する手元位置
    public float grabDistance = 3f;

    private GameObject heldObject;
    private Rigidbody heldRB;
    private Collider heldCollider;
    private AxeToggle axeToggle;

    void Start()
    {
        axeToggle = GetComponent<AxeToggle>();

        // 安全対策：Inspectorで設定し忘れていた場合の警告と応急処置
        if (controllerTransform == null)
        {
            Debug.LogError("🚨WoodPickup: 'Controller Transform' が設定されていません！Inspectorを確認してください。");
            controllerTransform = this.transform;
        }
        if (holdPoint == null)
        {
            Debug.LogError("🚨WoodPickup: 'Hold Point' が設定されていません！Inspectorを確認してください。");
            holdPoint = this.transform;
        }
    }

    void Update()
    {
        if (axeToggle != null && axeToggle.axeEquipped) return;

        // 右コントローラーのグリップ（中指）ボタン、またはマウスの右クリック
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) || Input.GetMouseButtonDown(1))
        {
            if (heldObject == null) PickUp();
            else Drop();
        }

        if (heldObject != null && holdPoint != null)
        {
            heldObject.transform.position = holdPoint.position;
            heldObject.transform.rotation = holdPoint.rotation;
        }
    }

    void PickUp()
    {
        if (controllerTransform == null) return;

        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);

        // テスト用にRayを可視化
        Debug.DrawRay(ray.origin, ray.direction * grabDistance, Color.green, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.CompareTag("Wood"))
            {
                Debug.Log("🪵 木材を掴みました！: " + hit.collider.gameObject.name);
                heldObject = hit.collider.gameObject;
                heldRB = heldObject.GetComponent<Rigidbody>();
                heldCollider = heldObject.GetComponent<Collider>();

                if (heldRB != null) heldRB.isKinematic = true;
                if (heldCollider != null) heldCollider.enabled = false;
            }
        }
    }

    void Drop()
    {
        Debug.Log("🪵 木材を離しました！");
        if (heldRB != null) heldRB.isKinematic = false;
        if (heldCollider != null) heldCollider.enabled = true;

        heldObject = null;
        heldRB = null;
        heldCollider = null;
    }
}