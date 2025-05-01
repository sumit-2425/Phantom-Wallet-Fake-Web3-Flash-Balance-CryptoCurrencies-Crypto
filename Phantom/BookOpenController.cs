using UnityEngine;
using System.Collections;

public class BookOpenController : MonoBehaviour
{
    [Header("Prefab ที่จะใช้ตอนเปลี่ยนกลับเป็น Book (ปิด)")]
    public GameObject bookClosedPrefab;

    [Header("Prefab สำหรับ BookWriten")]
    public GameObject bookWritenPrefab;

    [Header("Audio Settings")]
    public AudioClip paperWriteClip;  // เสียงเขียนกระดาษ

    public GameObject SpawnClosedBook(Transform itemHolder)
    {
        // ทำลาย BookOpen ตัวนี้
        Destroy(gameObject);

        // สร้าง Book (ปิด) ในมือผู้เล่น
        GameObject closedBook = Instantiate(bookClosedPrefab, itemHolder);
        closedBook.name = "Book";
        closedBook.tag = "Untagged"; // ในมือ ไม่ต้องเป็น Pickup
        closedBook.transform.localPosition = Vector3.zero;
        closedBook.transform.localRotation = Quaternion.identity;

        return closedBook;
    }

    // ฟังก์ชันเริ่ม Timer เพื่อเปลี่ยนเป็น BookWriten
    public void StartBookWritenTimer()
    {
        StartCoroutine(BookWritenCoroutine());
    }

    private IEnumerator BookWritenCoroutine()
    {
        // รอเวลาสุ่มในช่วง 5-10 วินาที
        yield return new WaitForSeconds(Random.Range(5f, 10f));

        // สร้าง BookWriten ที่ตำแหน่งและการหมุนเดียวกับ BookOpen
        GameObject bookWriten = Instantiate(bookWritenPrefab, transform.position, transform.rotation);
        bookWriten.name = "BookWriten";
        // เปลี่ยน tag ให้ไม่สามารถหยิบได้ (ไม่ใช่ "Pickup")
        bookWriten.tag = "Untagged";

        // เล่นเสียงเขียนกระดาษที่ตำแหน่งนี้
        if (paperWriteClip != null)
        {
            AudioSource.PlayClipAtPoint(paperWriteClip, transform.position);
        }

        // ทำลาย BookOpen
        Destroy(gameObject);
    }
}
