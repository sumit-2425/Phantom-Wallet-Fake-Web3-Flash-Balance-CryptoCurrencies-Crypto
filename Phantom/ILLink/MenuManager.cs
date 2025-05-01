using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public AudioFader audioFader;
    public AudioSource uiAudioSource;
    public AudioClip startButtonClip;

    // ใช้ pointer ที่นำเข้ามาจากภายนอก
    public Texture2D customCursorTexture;
    // ตั้ง hotSpot เป็น (0,0) ซึ่งหมายถึงมุมบนซ้ายของภาพ
    public Vector2 hotSpot = Vector2.zero;

    private bool isGameStarting = false;

    void Start()
    {
        // ตั้งค่า custom cursor โดยใช้ hotSpot ที่ (0, 0)
        Cursor.SetCursor(customCursorTexture, hotSpot, CursorMode.Auto);
    }

    public void PlayGame()
    {
        if (!isGameStarting)
        {
            isGameStarting = true;
            StartCoroutine(PlayStartSoundAndGo());
        }
    }

    private IEnumerator PlayStartSoundAndGo()
    {
        if (uiAudioSource != null && startButtonClip != null)
        {
            uiAudioSource.PlayOneShot(startButtonClip);
        }
        audioFader.FadeOutAndLoadMainHouseMap();
        yield break;
    }

    // ฟังก์ชันสำหรับปุ่ม Menu ที่เล่นเสียงแล้วค่อยเปลี่ยนซีน
    public void GoToMenu()
    {
        if (!isGameStarting)
        {
            isGameStarting = true;
            StartCoroutine(PlayMenuSoundAndGo());
        }
    }

    private IEnumerator PlayMenuSoundAndGo()
    {
        if (uiAudioSource != null && startButtonClip != null)
        {
            uiAudioSource.PlayOneShot(startButtonClip);
        }
        audioFader.FadeOutAndLoadMenu();
        yield break;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
