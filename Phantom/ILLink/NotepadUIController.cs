using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotepadUIController : MonoBehaviour
{
    [Header("Notepad UI")]
    [SerializeField] private RectTransform notepadRect;
    [SerializeField] private Image notepadImage;          // ภาพที่จะแสดงรูป Notepad
    [SerializeField] private Text contentText;            // เนื้อหาใน Notepad (ถ้าไม่ใช้ก็ลบออกได้)

    [Header("Notepad Move Settings")]
    [SerializeField] private Vector2 offScreenPos = new Vector2(0f, -500f);
    [SerializeField] private Vector2 centerPos = new Vector2(0f, 0f);
    [SerializeField] private float moveTime = 1f;

    [Header("Overlay UI (Dim Panel)")]
    [SerializeField] private Image overlayImage;           // Panel ดำที่ไว้จางพื้นหลัง
    [SerializeField] private float overlayFadeTime = 0.5f;
    [SerializeField] private float overlayAlphaTarget = 0.5f; // ความเข้มของพื้นหลังเวลาขึ้น

    [Header("Audio Settings")]
    [SerializeField] private AudioSource notepadAudioSource;  // AudioSource สำหรับเล่นเสียง
    [SerializeField] private AudioClip paperOpenClip;         // เสียงตอนเปิด
    [SerializeField] private AudioClip paperCloseClip;        // เสียงตอนปิด

    // ==============================
    // ตัวแปรภายใน
    private bool isVisible = false;   // ตอนนี้เปิดอยู่ไหม
    private bool isMoving = false;    // กำลังเลื่อน UI อยู่หรือเปล่า

    // ให้สคริปต์อื่นมาอ่านได้ว่าเปิดหรือปิด
    public bool IsVisible { get { return isVisible; } }


    void Start()
    {
        // เริ่มเกมให้ซ่อน Notepad
        if (notepadRect != null)
            notepadRect.gameObject.SetActive(false);

        // ซ่อน Overlay
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
            overlayImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// เมธอดสำหรับเปิด Notepad โดยรับข้อมูลจากวัตถุ 3D (Notepad3DObject) มาใส่ให้ภาพ/เนื้อหา
    /// </summary>
    /// <param name="data">Notepad3DObject ของเล่มที่ต้องการเปิด</param>
    public void OpenNotepad(Notepad3DObject data)
    {
        // ถ้ากำลังเคลื่อนที่หรือเปิดอยู่แล้ว ให้ return
        if (isMoving || isVisible) return;

        // 1) ตั้งค่า Sprite / ข้อความให้ตรงตามเล่มที่เพิ่งหยิบ
        if (notepadImage != null && data.notepadSprite != null)
        {
            notepadImage.sprite = data.notepadSprite;
        }
        if (contentText != null)
        {
            contentText.text = data.notepadContent;
        }

        // 2) เล่นเสียงเปิด (ถ้ามี)
        if (notepadAudioSource != null && paperOpenClip != null)
        {
            notepadAudioSource.PlayOneShot(paperOpenClip);
        }

        // 3) แสดงตัว Notepad บน Canvas
        notepadRect.gameObject.SetActive(true);

        // 4) เปิด Overlay + ทำ Fade-in
        if (overlayImage != null)
        {
            overlayImage.gameObject.SetActive(true);
            StartCoroutine(FadeOverlay(true));
        }

        // 5) เลื่อน Notepad จากจอด้านล่างมาที่กลางจอ
        StartCoroutine(MoveNotepad(offScreenPos, centerPos, true));
    }

    /// <summary>
    /// เมธอดปิด Notepad
    /// </summary>
    public void CloseNotepad()
    {
        // ถ้ากำลังเคลื่อนที่หรือมันปิดอยู่แล้ว ให้ return
        if (isMoving || !isVisible) return;

        // เล่นเสียงปิด (ถ้ามี)
        if (notepadAudioSource != null && paperCloseClip != null)
        {
            notepadAudioSource.PlayOneShot(paperCloseClip);
        }

        // Fade-out overlay
        if (overlayImage != null)
        {
            StartCoroutine(FadeOverlay(false));
        }

        // เลื่อน Notepad จากกลางจอกลับลงล่าง
        StartCoroutine(MoveNotepad(centerPos, offScreenPos, false));
    }

    /// <summary>
    /// Coroutine เลื่อน Notepad UI จาก startPos -> endPos
    /// </summary>
    private IEnumerator MoveNotepad(Vector2 startPos, Vector2 endPos, bool openState)
    {
        isMoving = true;
        notepadRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveTime);
            notepadRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // เมื่อเลื่อนเสร็จ
        isMoving = false;
        isVisible = openState;

        // ถ้าปิดแล้ว ให้ซ่อน GameObject
        if (!openState)
        {
            notepadRect.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Coroutine ทำ Fade Overlay Panel
    /// fadeIn = true => จางเข้าจนถึง overlayAlphaTarget
    /// fadeIn = false => จางออกจนถึง 0
    /// </summary>
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

        // ถ้าจางออกจน 0 แล้ว ให้ปิด Overlay ไปเลย
        if (!fadeIn)
        {
            overlayImage.gameObject.SetActive(false);
        }
    }
}
