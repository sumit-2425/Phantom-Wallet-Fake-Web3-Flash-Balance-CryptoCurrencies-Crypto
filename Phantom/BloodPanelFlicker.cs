using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodPanelFlicker : MonoBehaviour
{
    [Header("Panel ที่ต้องการกระพริบ")]
    [SerializeField] private Image bloodPanel;

    [Header("ค่าความโปร่งใสต่ำสุด-สูงสุด")]
    [SerializeField] private float minAlpha = 0.5f; // เช่น 50%
    [SerializeField] private float maxAlpha = 0.7f; // เช่น 70%

    [Header("ความเร็วในการกระพริบ (ยิ่งมากยิ่งเร็ว)")]
    [SerializeField] private float flickerSpeed = 0.5f;

    private Coroutine flickerRoutine;

    /// <summary>
    /// เรียกใช้เมธอดนี้เพื่อเริ่มกระพริบ
    /// </summary>
    public void StartFlicker()
    {
        // ถ้ามีการกระพริบอยู่แล้ว ให้หยุดก่อน
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
        }
        // เริ่ม coroutine ใหม่
        flickerRoutine = StartCoroutine(FlickerLoop());
    }

    /// <summary>
    /// เรียกใช้เมธอดนี้เพื่อหยุดกระพริบ
    /// </summary>
    public void StopFlicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        // รีเซ็ต alpha = 0 (หรือจะตั้งเป็นค่าอื่น ๆ ก็ได้)
        if (bloodPanel != null)
        {
            Color c = bloodPanel.color;
            c.a = 0f;
            bloodPanel.color = c;
        }
    }

    private IEnumerator FlickerLoop()
    {
        // เริ่มต้นด้วย alpha กลาง ๆ
        float alpha = minAlpha;
        bool goingUp = true; // ไว้สลับทิศทางขึ้นลง

        while (true)
        {
            // ค่อย ๆ ปรับ alpha
            float step = flickerSpeed * Time.deltaTime;
            if (goingUp)
            {
                alpha += step;
                if (alpha >= maxAlpha)
                {
                    alpha = maxAlpha;
                    goingUp = false;
                }
            }
            else
            {
                alpha -= step;
                if (alpha <= minAlpha)
                {
                    alpha = minAlpha;
                    goingUp = true;
                }
            }

            // เซ็ตค่ากลับไปที่ Image
            if (bloodPanel != null)
            {
                Color c = bloodPanel.color;
                c.a = alpha;
                bloodPanel.color = c;
            }

            yield return null;
        }
    }
}
