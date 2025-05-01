using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioFader : MonoBehaviour
{
    // อ้างอิง AudioSource ที่ต้องการลดเสียง
    public AudioSource audioSource;
    // UI Image ที่ครอบคลุมหน้าจอสำหรับทำ fade effect (ควรเป็นสีดำ)
    public Image fadeImage;
    // ระยะเวลาที่ต้องการให้ fade (ทั้ง fade out และ fade in)
    public float fadeDuration = 4f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // ฟังก์ชันเรียกใช้เพื่อ fade out ทั้งเสียงและหน้าจอ แล้วโหลดซีน "main house map"
    public void FadeOutAndLoadMainHouseMap()
    {
        StartCoroutine(FadeTransitionCoroutine("main house map"));
    }

    // เพิ่มฟังก์ชันสำหรับ fade out แล้วโหลดซีน "Menu"
    public void FadeOutAndLoadMenu()
    {
        StartCoroutine(FadeTransitionCoroutine("Menu"));
    }

    private IEnumerator FadeTransitionCoroutine(string sceneName)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        // ตรวจสอบให้แน่ใจว่า fadeImage เริ่มที่โปร่งใส
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // Fade out audio และเพิ่มความมืดให้กับหน้าจอ
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeDuration);

            // ลดระดับเสียง
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);

            // เพิ่มความมืดให้กับหน้าจอ
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = t;
                fadeImage.color = c;
            }
            yield return null;
        }
        // ยืนยันระดับเสียงและความมืด
        audioSource.volume = 0f;
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }

        // โหลดซีนใหม่
        SceneManager.LoadScene(sceneName);
        yield return null;

        // รอสักครู่หลังโหลดซีน
        yield return new WaitForSeconds(0.1f);

        // Fade in ซีนใหม่ (ลดความมืดจากดำไปโปร่งใส)
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeDuration);
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 1f - t;
                fadeImage.color = c;
            }
            yield return null;
        }
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }
}
