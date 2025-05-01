using UnityEngine;

public class Notepad3DObject : MonoBehaviour
{
    // Sprite ที่จะแสดงบน UI เวลาผู้เล่นเปิด Notepad นี้
    public Sprite notepadSprite;

    // ตัวอย่าง: ถ้ามีข้อความที่ต้องการแสดงใน Text
    [TextArea]
    public string notepadContent;

    // อาจมีข้อมูลอื่นตามต้องการ เช่น ชื่อหัวข้อ, เสียง ฯลฯ
}
