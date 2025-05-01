using UnityEngine;

public class BookController : MonoBehaviour
{
    [Header("Prefab ที่จะใช้ตอนเปลี่ยนเป็น BookOpen")]
    public GameObject bookOpenPrefab;

    public void SpawnOpenBook(Vector3 dropPosition, Quaternion dropRotation)
    {
        // ทำลาย Book (ปิด) ตัวนี้ก่อน
        Destroy(gameObject);

        // สร้าง BookOpen ลงพื้น
        GameObject openObj = Instantiate(bookOpenPrefab, dropPosition, Quaternion.Euler(270, 0, 90));
        openObj.name = "BookOpen";
        openObj.tag = "Pickup";  // ให้รองรับการหยิบได้

        // เช็คเงื่อนไข: player drop Book ใน Room ที่เกิดเหตุการณ์
        // และ Ghost ที่สุ่มมามี Evidence GhostWriting
        GhostEventManager gem = FindObjectOfType<GhostEventManager>();
        if (gem != null && gem.ghostRoom != null)
        {
            Collider roomCollider = gem.ghostRoom.GetComponent<Collider>();
            if (roomCollider != null && roomCollider.bounds.Contains(dropPosition)
                && gem.chosenGhost != null && gem.chosenGhost.requiredEvidences.Contains(EvidenceType.GhostWriting))
            {
                // ถ้าเงื่อนไขครบ ให้เรียก Timer ใน BookOpenController เพื่อเปลี่ยนเป็น BookWriten หลังจาก 3 วินาที
                BookOpenController boc = openObj.GetComponent<BookOpenController>();
                if (boc != null)
                {
                    boc.StartBookWritenTimer();
                }
            }
        }
    }
}
