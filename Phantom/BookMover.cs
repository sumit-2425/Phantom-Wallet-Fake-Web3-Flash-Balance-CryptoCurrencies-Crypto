using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BookMover : MonoBehaviour
{
    public static BookMover instance;

    [Header("Book UI")]
    [SerializeField] private RectTransform bookRect;
    [SerializeField] private Vector2 offScreenPos = new Vector2(0f, -500f);
    [SerializeField] private Vector2 centerPos = new Vector2(0f, 0f);
    [SerializeField] private float moveTime = 1f;

    [Header("Overlay UI (Dim Panel)")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private float overlayFadeTime = 0.5f;
    [SerializeField] private float overlayAlphaTarget = 0.5f;

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D customCursorTexture;
    [SerializeField] private Vector2 cursorHotSpot = Vector2.zero;

    [Header("UI to display 'Run !'")]
    [SerializeField] private Text runMessageText;

    // == เพิ่มตัวแปร Audio สำหรับหนังสือ ==
    [Header("Audio Settings")]
    [SerializeField] private AudioSource bookAudioSource;  // ใช้เล่นเสียงในหนังสือ
    [SerializeField] private AudioClip pageOpenClip;       // เสียงเปิดกระดาษ
    [SerializeField] private AudioClip pageCloseClip;      // เสียงปิดกระดาษ
    [SerializeField] private AudioClip clickClip;          // เสียงคลิก

    private bool isMoving = false;
    private bool isVisible = false;
    private bool isLocked = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        bookRect.gameObject.SetActive(false);
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
            overlayImage.gameObject.SetActive(false);
        }

        if (runMessageText != null)
        {
            runMessageText.gameObject.SetActive(false);
        }

        // ซ่อนเมาส์
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isLocked) return; // ถ้าล็อคแล้ว ห้ามเปิด

        // --- เช็กว่าถ้า Notepad ยังเปิดอยู่ => ห้ามกด H เพื่อเปิด Book
        // วิธีอ้างถึง: สมมติคุณมี NotepadUIController เป็นตัวแปรหรือหาจาก FindObjectOfType<NotepadUIController>()
        NotepadUIController notepadUI = FindObjectOfType<NotepadUIController>();
        if (notepadUI != null && notepadUI.IsVisible)
        {
            // Notepad เปิดอยู่ => ทำ return ทันที
            return;
        }

        if (Input.GetKeyDown(KeyCode.H) && !isMoving)
        {
            if (!isVisible)
            {
                OpenBook();
            }
            else
            {
                CloseBook();
            }
        }
    }

    // ใต้ class BookMover {...}
    public bool IsVisible
    {
        get { return isVisible; }
    }

    public void OpenBook()
    {
        // เล่นเสียงเปิดกระดาษ
        if (bookAudioSource != null && pageOpenClip != null)
        {
            bookAudioSource.PlayOneShot(pageOpenClip);
        }

        // แสดงหนังสือ
        bookRect.gameObject.SetActive(true);

        if (overlayImage != null)
        {
            overlayImage.gameObject.SetActive(true);
            StartCoroutine(FadeOverlay(true));
        }
        StartCoroutine(MoveBook(offScreenPos, centerPos, true));

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (customCursorTexture != null)
        {
            Cursor.SetCursor(customCursorTexture, cursorHotSpot, CursorMode.Auto);
        }
    }

    public void CloseBook()
    {
        // เล่นเสียงปิดกระดาษ
        if (bookAudioSource != null && pageCloseClip != null)
        {
            bookAudioSource.PlayOneShot(pageCloseClip);
        }

        if (overlayImage != null)
        {
            StartCoroutine(FadeOverlay(false));
        }
        StartCoroutine(MoveBook(centerPos, offScreenPos, false));

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private IEnumerator MoveBook(Vector2 startPos, Vector2 endPos, bool becomeVisible)
    {
        isMoving = true;
        bookRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            bookRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        isMoving = false;
        isVisible = becomeVisible;

        if (!becomeVisible)
        {
            bookRect.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOverlay(bool fadeIn)
    {
        if (overlayImage == null) yield break;

        float startAlpha = overlayImage.color.a;
        float endAlpha = fadeIn ? overlayAlphaTarget : 0f;
        float elapsed = 0f;
        while (elapsed < overlayFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / overlayFadeTime);
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            Color c = overlayImage.color;
            c.a = newAlpha;
            overlayImage.color = c;
            yield return null;
        }

        if (!fadeIn)
        {
            overlayImage.gameObject.SetActive(false);
        }
    }

    // ---- เมธอด Force ปิดหนังสือ ----
    public void ForceCloseAndLockBook()
    {
        if (isVisible) CloseBook();
        isLocked = true;

        // ตัวอย่าง: โชว์ “Run !”
        if (runMessageText != null)
        {
            runMessageText.gameObject.SetActive(true);
            runMessageText.text = "Run !";
        }
    }

    // ==== เมธอดให้ UI Button เรียกเมื่อคลิกในหนังสือ ====
    public void OnClickInBook()
    {
        if (bookAudioSource != null && clickClip != null)
        {
            bookAudioSource.PlayOneShot(clickClip);
        }
    }
}