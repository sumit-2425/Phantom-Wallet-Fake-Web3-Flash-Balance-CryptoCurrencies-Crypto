using UnityEngine;

public class DoorAutoOpen : MonoBehaviour
{
    public string doorOpenAnimName = "DoorOpen";
    public string doorCloseAnimName = "DoorClose";

    public float openDistance = 2f;  // ระยะที่อนุญาตให้ประตูเปิด
    public AudioSource audioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    private Animator doorAnim;
    private bool isOpen = false;  // state ว่าประตูเปิดอยู่หรือปิด

    void Start()
    {
        doorAnim = GetComponent<Animator>();
        if (doorAnim == null)
        {
            Debug.LogError("No Animator component found on door!");
        }
    }

    void Update()
    {
        // ค้นหา Monster ทั้งหมดในฉากที่มี tag "monster"
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("monster");
        bool shouldOpen = false;

        // ตรวจสอบ Monster แต่ละตัว ว่ามีตัวไหนอยู่ใกล้ประตูหรือไม่
        foreach (GameObject monster in monsters)
        {
            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance <= openDistance)
            {
                shouldOpen = true;
                break;
            }
        }

        // ถ้ามี Monster อยู่ใกล้ประตู และประตูยังปิดอยู่ ให้เปิดประตู
        if (shouldOpen && !isOpen)
        {
            if (doorAnim.GetCurrentAnimatorStateInfo(0).IsName(doorCloseAnimName))
            {
                doorAnim.ResetTrigger("close");
                doorAnim.SetTrigger("open");
                isOpen = true;

                if (audioSource && doorOpenSound)
                {
                    audioSource.PlayOneShot(doorOpenSound);
                }
            }
        }
        // ถ้าไม่มี Monster อยู่ใกล้ประตู และประตูเปิดอยู่ ให้ปิดประตู
        else if (!shouldOpen && isOpen)
        {
            if (doorAnim.GetCurrentAnimatorStateInfo(0).IsName(doorOpenAnimName))
            {
                doorAnim.ResetTrigger("open");
                doorAnim.SetTrigger("close");
                isOpen = false;

                if (audioSource && doorCloseSound)
                {
                    audioSource.PlayOneShot(doorCloseSound);
                }
            }
        }
    }
}
