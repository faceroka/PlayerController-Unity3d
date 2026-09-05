using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.0f;
    public float maxVerticalAngle = 80.0f;

    [Header("Head Bobbing")]
    public float bobFrequency = 10.0f;
    public float bobAmount = 0.05f;

    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private AudioSource audioSource;
    private Vector3 velocity;
    private float xRotation = 0.0f;

    private float defaultCameraY;
    private float timer = 0.0f;
    private float stepTimer = 0.0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (playerCamera != null)
        {
            defaultCameraY = playerCamera.localPosition.y;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleHeadBobAndAudio();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);

        if (playerCamera != null)
        {
            playerCamera.localPosition = new Vector3(
                playerCamera.localPosition.x,
                playerCamera.localPosition.y,
                playerCamera.localPosition.z
            );
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleHeadBobAndAudio()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f) && controller.isGrounded;

        if (isMoving)
        {
            timer += Time.deltaTime * bobFrequency;
            float newY = defaultCameraY + Mathf.Sin(timer) * bobAmount;

            if (playerCamera != null)
            {
                Vector3 currentPos = playerCamera.localPosition;
                playerCamera.localPosition = new Vector3(currentPos.x, newY, currentPos.z);
            }
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0.0f;
            }
        }
        else
        {
            timer = 0.0f;
            stepTimer = stepInterval;

            if (playerCamera != null)
            {
                Vector3 currentPos = playerCamera.localPosition;
                float resetY = Mathf.Lerp(currentPos.y, defaultCameraY, Time.deltaTime * 8.0f);
                playerCamera.localPosition = new Vector3(currentPos.x, resetY, currentPos.z);
            }
        }
    }

    void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0) return;
        int index = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[index]);
    }
}
