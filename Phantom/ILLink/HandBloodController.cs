using UnityEngine;

public class HandBloodController : MonoBehaviour
{
    private Renderer[] allRenderers;

    // ค่าปัจจุบันและเป้าหมายของ Alpha
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    // ปรับได้ตามต้องการ (ยิ่งมาก ยิ่งเฟดช้า)
    private float fadeSpeed = 10f;

    void Awake()
    {
        allRenderers = GetComponentsInChildren<Renderer>();

        // เริ่มต้นที่ Alpha = 0 (มองไม่เห็น)
        SetAlpha(0f);
        // ปิด Renderer ทันที (ไม่ต้อง Render ถ้า alpha = 0)
        EnableRenderers(false);
    }

    void Update()
    {
        // ไล่ค่า currentAlpha ไปยัง targetAlpha อย่างนุ่มนวล
        if (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(
                currentAlpha,
                targetAlpha,
                fadeSpeed * Time.deltaTime
            );
            SetAlpha(currentAlpha);
        }
        else
        {
            // ถ้าใกล้เป้าหมายแล้ว ให้บังคับเป็นค่าเป๊ะ
            currentAlpha = targetAlpha;
            SetAlpha(currentAlpha);

            // ถ้า alpha == 0 => ปิด renderer
            if (Mathf.Approximately(currentAlpha, 0f))
            {
                EnableRenderers(false);
            }
        }
    }

    /// <summary>
    /// เรียกเมื่อ “โดนไฟ” => visible = true => targetAlpha = 1
    ///        เมื่อ “ไม่โดนไฟ” => visible = false => targetAlpha = 0
    /// </summary>
    public void SetVisible(bool visible)
    {
        targetAlpha = visible ? 1f : 0f;

        // ถ้าจะให้แสดง ก็เปิด renderer ทันที เพื่อให้เริ่มเฟดจาก 0 -> 1 ได้
        if (visible)
        {
            EnableRenderers(true);
        }
    }

    /// <summary>
    /// เซ็ต alpha ให้กับ material ของ renderer ทั้งหมด
    /// หมายเหตุ: ต้องแน่ใจว่า Shader ที่ใช้มี property ให้ปรับ alpha 
    ///           เช่น "_BaseColor" หรือ "_Color" (แล้วแต่ Shader)
    ///           ถ้าเป็น URP มักจะใช้ "_BaseColor"
    /// </summary>
    private void SetAlpha(float alpha)
    {
        foreach (var rend in allRenderers)
        {
            // ตัวอย่างนี้เช็ค "_BaseColor" ถ้าใช้ Standard Pipeline อาจเป็น "_Color"
            if (rend.material.HasProperty("_BaseColor"))
            {
                Color c = rend.material.GetColor("_BaseColor");
                c.a = alpha;
                rend.material.SetColor("_BaseColor", c);
            }
            // ถ้า Shader มีชื่อ property อื่น ก็เปลี่ยนตามนั้น
        }
    }

    private void EnableRenderers(bool enable)
    {
        foreach (var rend in allRenderers)
        {
            rend.enabled = enable;
        }
    }
}
