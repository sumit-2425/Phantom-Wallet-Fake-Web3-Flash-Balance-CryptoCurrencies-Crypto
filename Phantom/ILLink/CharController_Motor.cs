using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharController_Motor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10.0f;
    public float sprintMultiplier = 1.5f;
    public float crouchSpeed = 5.0f;
    public float jumpForce = 5.0f;

    [Header("Camera Settings")]
    public float sensitivity = 30.0f;
    public float WaterHeight = 15.5f;
    public GameObject cam;
    public float zoomSpeed = 5.0f;
    private Camera mainCamera;
    private float defaultFOV;
    private float zoomedFOV;

    [Header("Crouch Settings")]
    public float crouchTransitionSpeed = 10.0f;
    private float originalHeight;
    private float crouchHeight = 1.0f;
    private bool isCrouching = false;

    [Header("Double Tap Run Settings")]
    private float lastForwardPressTime = 0f;
    private float doubleTapTime = 0.3f;
    private bool isSprinting = false;

    [Header("Exhaustion / Sprint Gauge Settings")]
    public float maxSprintDuration = 3f;
    private float sprintGauge;
    private bool isExhausted = false;
    public Slider sprintCooldownBar;

    [Header("Animation Settings")]
    private Animator animator;

    [Header("Other Settings")]
    public bool webGLRightClickRotation = true;
    public GameObject crosshair;
    public float maxVerticalAngle = 80f;

    [Header("Audio Settings")]
    public AudioSource walkingAudioSource;
    public AudioClip walkingSound;

    public AudioSource exhaustedAudioSource;
    public AudioClip exhaustedSound;

    // ====== ปรับเป็น PlayOneShot, ไม่ใช้ loop ======
    public AudioSource jumpAudioSource;
    public AudioClip jumpSound;

    public AudioSource runningAudioSource;
    public AudioClip runningSound;

    private CharacterController character;
    private float moveFB, moveLR;
    private float rotX, rotY;
    private float gravity = -9.8f;
    private float verticalRotation = 0f;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool isDead = false;

    [Header("Death / Third Person Camera Settings")]
    public Transform thirdPersonCamPos;
    public float cameraTransitionSpeed = 2f;

    private bool hasPlayedExhaustSound = false;

    public static bool canMove = true;

    public bool IsDead()
    {
        return isDead;
    }

    public void TriggerDeath()
    {
        if (!isDead)
        {
            isDead = true;
            animator.SetTrigger("Death");
            if (crosshair != null)
                crosshair.SetActive(false);

            // คืนค่าเมาส์ให้กลับมาแสดงและปลดล็อค
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // เรียก Coroutine เพื่อรอ 3 วินาทีแล้วเปลี่ยน Scene
            StartCoroutine(LoadDeadSceneAfterDelay());
        }
    }
    public void StopAllPlayerSounds()
    {
        if (walkingAudioSource != null && walkingAudioSource.isPlaying)
            walkingAudioSource.Stop();

        if (runningAudioSource != null && runningAudioSource.isPlaying)
            runningAudioSource.Stop();

        if (jumpAudioSource != null && jumpAudioSource.isPlaying)
            jumpAudioSource.Stop();

        if (exhaustedAudioSource != null && exhaustedAudioSource.isPlaying)
            exhaustedAudioSource.Stop();
    }
    private IEnumerator LoadDeadSceneAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Dead");
    }

    void Start()
    {
        character = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        mainCamera = cam.GetComponent<Camera>();
        defaultFOV = mainCamera.fieldOfView;
        zoomedFOV = defaultFOV * 1.3f;

        originalHeight = character.height;
        CharController_Motor.canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshair != null)
            crosshair.SetActive(true);

        if (Application.isEditor)
        {
            webGLRightClickRotation = false;
            sensitivity *= 1.5f;
        }

        sprintGauge = maxSprintDuration;
        if (sprintCooldownBar != null)
        {
            sprintCooldownBar.maxValue = maxSprintDuration;
            sprintCooldownBar.value = sprintGauge;
        }

        if (walkingAudioSource != null)
        {
            walkingAudioSource.loop = true;
        }
    }

    void Update()
    {
        if (isDead)
        {
            HandleDeathCamera();
            return;
        }

        isGrounded = character.isGrounded;
        float currentSpeed = speed;

        // หากถูกปิดการเคลื่อนไหว (เช่น หลังจากกด lever)
        if (!canMove)
        {
            // อนุญาตให้เลื่อนเมาส์หมุนกล้องได้ตามปกติ
            float rotX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            CameraRotation(cam, rotX, rotY);
            return;
        }

            // ===== Sprint (double tap) =====
            if (!isExhausted)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (Time.time - lastForwardPressTime < doubleTapTime && sprintGauge > 0f)
                {
                    isSprinting = true;
                }
                lastForwardPressTime = Time.time;
            }
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
        }

        // ===== Sprint gauge =====
        if (isSprinting && !isExhausted)
        {
            sprintGauge -= Time.deltaTime;
            if (sprintGauge <= 0f)
            {
                sprintGauge = 0f;
                isExhausted = true;
                isSprinting = false;
            }
        }
        else
        {
            sprintGauge += Time.deltaTime * (maxSprintDuration / 8f);
            if (sprintGauge >= maxSprintDuration)
            {
                sprintGauge = maxSprintDuration;
                isExhausted = false;
            }
        }
        sprintGauge = Mathf.Clamp(sprintGauge, 0f, maxSprintDuration);
        if (sprintCooldownBar != null)
        {
            sprintCooldownBar.value = sprintGauge;
        }

        // ===== เล่นเสียงเหนื่อยเมื่อสปรินต์หมด =====
        if (isExhausted && !hasPlayedExhaustSound)
        {
            exhaustedAudioSource.PlayOneShot(exhaustedSound);
            hasPlayedExhaustSound = true;
        }
        else if (!isExhausted)
        {
            hasPlayedExhaustSound = false;
        }

        // ===== ปรับความเร็ว / FOV =====
        if (isSprinting && !isExhausted)
        {
            currentSpeed *= sprintMultiplier;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomedFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        }

        // ===== Crouch =====
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
        character.height = Mathf.Lerp(character.height, isCrouching ? crouchHeight : originalHeight, Time.deltaTime * crouchTransitionSpeed);
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }

        // ===== เคลื่อนที่ =====
        moveFB = Input.GetAxis("Vertical") * currentSpeed;
        moveLR = Input.GetAxis("Horizontal") * currentSpeed;
        rotX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        rotY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        CheckForWaterHeight();

        Vector3 movement = new Vector3(moveLR, 0, moveFB);
        movement = transform.rotation * movement;
        character.Move(movement * Time.deltaTime);

        // ===== กระโดด (PlayOneShot) =====
        // Logic: ถ้ากด Space (ค้างหรือกดบ่อย) และเราติดพื้น + ความเร็วแนวตั้ง <= 0 => กระโดดใหม่
        // แล้วให้ PlayOneShot เพื่อเล่นเสียงจนจบ
        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrounded && playerVelocity.y <= 0f)
            {
                playerVelocity.y = jumpForce;

                // เล่นเสียงกระโดดแบบ OneShot (ไม่มี loop) => จบเอง
                if (jumpAudioSource != null && jumpSound != null)
                {
                    jumpAudioSource.PlayOneShot(jumpSound);
                }
            }
        }

        // แรงโน้มถ่วง
        playerVelocity.y += gravity * Time.deltaTime;
        character.Move(playerVelocity * Time.deltaTime);

        // หมุนกล้อง
        if (webGLRightClickRotation)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                CameraRotation(cam, rotX, rotY);
            }
        }
        else
        {
            CameraRotation(cam, rotX, rotY);
        }

        // ===== จัดการ Animation =====
        bool isMoving = (Mathf.Abs(moveFB) > 0.1f || Mathf.Abs(moveLR) > 0.1f);
        bool isJumping = !isGrounded;

        animator.SetBool("IsWalking", (isMoving && !isSprinting));
        animator.SetBool("IsRunning", (isMoving && isSprinting));
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsIdle", (!isMoving && !isJumping));

        // ===== เสียงเดิน / วิ่ง =====
        // เดิน
        if (isMoving && !isSprinting)
        {
            if (!walkingAudioSource.isPlaying)
            {
                walkingAudioSource.clip = walkingSound;
                walkingAudioSource.loop = true;
                walkingAudioSource.Play();
            }
            if (runningAudioSource.isPlaying)
            {
                runningAudioSource.Stop();
            }
        }
        else
        {
            if (walkingAudioSource.isPlaying)
            {
                walkingAudioSource.Stop();
            }
        }

        // วิ่ง
        if (isMoving && isSprinting && !isExhausted)
        {
            if (!runningAudioSource.isPlaying)
            {
                runningAudioSource.clip = runningSound;
                runningAudioSource.loop = true;
                runningAudioSource.Play();
            }
            if (walkingAudioSource.isPlaying)
            {
                walkingAudioSource.Stop();
            }
        }
        else
        {
            if (runningAudioSource.isPlaying)
            {
                runningAudioSource.Stop();
            }
        }
    }

    void CheckForWaterHeight()
    {
        if (transform.position.y < WaterHeight)
        {
            gravity = 0f;
        }
        else
        {
            gravity = -9.8f;
        }
    }

    void CameraRotation(GameObject cam, float rotX, float rotY)
    {
        transform.Rotate(0, rotX, 0);
        verticalRotation -= rotY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleDeathCamera()
    {
        if (thirdPersonCamPos != null)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                thirdPersonCamPos.position,
                Time.deltaTime * cameraTransitionSpeed
            );
            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                thirdPersonCamPos.rotation,
                Time.deltaTime * cameraTransitionSpeed
            );
        }
        else
        {
            Debug.LogWarning("No ThirdPersonCamPos assigned!");
        }
    }
}