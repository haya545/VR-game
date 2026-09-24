using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("移動・回転設定")]
    public float speed = 3f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("スムーズ回転設定")]
    public float turnSpeed = 90f; // 1秒あたりの回転速度

    [Header("コンポーネント・参照")]
    public CharacterController controller;
    public Transform cameraTransform;

    private Vector3 velocity;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    void Update()
    {
        Move();
        RotatePlayer();
    }

    // =========================
    // 移動
    // =========================
    void Move()
    {
        // 左スティック
        Vector2 input =
            OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // カメラの向いている方向を取得
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // 移動方向
        Vector3 move = forward * input.y + right * input.x;

        // 移動
        controller.Move(move * speed * Time.deltaTime);

        // 接地
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Aボタンでジャンプ
        if (OVRInput.GetDown(OVRInput.Button.One) &&
            controller.isGrounded)
        {
            velocity.y =
                Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 重力
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    // =========================
    // スムーズ回転
    // =========================
    void RotatePlayer()
    {
        Vector2 rightStick =
            OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        // スティックの左右入力をそのまま回転速度にする
        float rotation = rightStick.x * turnSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, rotation);
    }
}