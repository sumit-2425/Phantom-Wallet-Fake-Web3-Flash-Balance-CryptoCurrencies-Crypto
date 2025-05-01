using UnityEngine;
using TMPro; // ใช้ TextMeshPro

public class door : MonoBehaviour
{
    public float interactionDistance;
    public TextMeshProUGUI intText; // เปลี่ยนจาก GameObject เป็น TMP Text
    public string doorOpenAnimName, doorCloseAnimName;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    private void Start()
    {
        if (intText != null)
        {
            intText.gameObject.SetActive(false); // ปิดข้อความตอนเริ่มเกม
        }
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.gameObject.tag == "door")
            {
                GameObject doorParent = hit.collider.gameObject;
                Animator doorAnim = doorParent.GetComponentInParent<Animator>(); // หา Animator ใน Parent ใกล้ที่สุด

                if (intText != null)
                {
                    intText.text = "Press 'E' to Open/Close"; // แสดงข้อความ
                    intText.gameObject.SetActive(true);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (doorAnim.GetCurrentAnimatorStateInfo(0).IsName(doorOpenAnimName))
                    {
                        doorAnim.ResetTrigger("open");
                        doorAnim.SetTrigger("close");

                        // เล่นเสียงปิดประตู
                        if (audioSource && doorCloseSound)
                        {
                            audioSource.PlayOneShot(doorCloseSound);
                        }
                    }
                    else if (doorAnim.GetCurrentAnimatorStateInfo(0).IsName(doorCloseAnimName))
                    {
                        doorAnim.ResetTrigger("close");
                        doorAnim.SetTrigger("open");

                        // เล่นเสียงเปิดประตู
                        if (audioSource && doorOpenSound)
                        {
                            audioSource.PlayOneShot(doorOpenSound);
                        }
                    }
                }
            }
            else
            {
                if (intText != null)
                {
                    intText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (intText != null)
            {
                intText.gameObject.SetActive(false);
            }
        }
    }
}
