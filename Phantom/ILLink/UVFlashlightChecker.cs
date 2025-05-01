using UnityEngine;

public class UVFlashlightChecker : MonoBehaviour
{
    public Light uvLight;
    public float checkRadius = 5f;   // ระยะ Overlap
    public float spotAngle = 30f;    // มุมกรวยไฟ

    [SerializeField] private GhostEventManager ghostEventManager;

    private HandBloodController[] allBloods;

    void Start()
    {
        // ดึง HandBloodController ทั้งหมดในซีน
        allBloods = FindObjectsOfType<HandBloodController>();

        // เริ่มต้นให้ทั้งหมดจาง (alpha = 0)
        foreach (var b in allBloods)
        {
            b.SetVisible(false);
        }
    }

    void Update()
    {
        if (uvLight == null) return;

        // ถ้า UV ปิด => เซ็ตให้จางทั้งหมด
        if (!uvLight.enabled)
        {
            foreach (var b in allBloods)
            {
                b.SetVisible(false);
            }
            return;
        }

        // ป้องกัน null
        if (ghostEventManager == null || ghostEventManager.chosenGhost == null)
        {
            // ไม่รู้ว่าผีเป็นตัวไหน => ปิด HandBlood ไปเลย
            foreach (var b in allBloods)
            {
                b.SetVisible(false);
            }
            return;
        }

        // ถามว่า chosenGhost มี EvidenceType.Fingerprint ไหม
        bool hasFingerprint = ghostEventManager
            .chosenGhost
            .requiredEvidences
            .Contains(EvidenceType.Fingerprint);

        // ถ้าไม่มี Fingerprint => ไม่เห็นเลือดมือเลย
        if (!hasFingerprint)
        {
            foreach (var b in allBloods)
            {
                b.SetVisible(false);
            }
            return;
        }

        // ได้แล้วว่ามี Fingerprint => ต้องเช็กเพิ่มว่าห้องตรงกับ ghostRoom มั้ย
        Room theGhostRoom = ghostEventManager.ghostRoom;  // หรือใช้ getter

        // วนเช็ก HandBlood ทีละอัน
        foreach (var b in allBloods)
        {
            if (b == null)
            {
                continue;
            }

            // ========== เช็กว่ามันอยู่ห้องที่สุ่มได้หรือไม่ ==========
            // สมมติ HandBloodController อยู่เป็นลูก (child) ภายใต้ GameObject ของ Room
            // เราอาจ getComponentInParent<Room> ได้แบบนี้:
            Room bloodRoom = b.GetComponentInParent<Room>();
            // ถ้าไม่ใช่ห้องเดียวกับผี => ไม่ให้โผล่
            if (bloodRoom != theGhostRoom)
            {
                b.SetVisible(false);
                continue;
            }

            // ========== ส่วนเดิม: ถ้าอยู่ในห้องแล้ว => เช็กลำแสง UV ==========
            Vector3 dir = b.transform.position - transform.position;
            float dist = dir.magnitude;

            if (dist <= checkRadius)
            {
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= spotAngle / 2f)
                {
                    // โดนไฟ UV => Fade In
                    b.SetVisible(true);
                }
                else
                {
                    b.SetVisible(false);
                }
            }
            else
            {
                b.SetVisible(false);
            }
        }
    }
}
