using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Lever : MonoBehaviour
{
    public float interactionDistance = 3f;
    public GameObject leverText;
    public string leverPullAnimName;
    public string containerCloseAnimName;

    [Header("Container Settings")]
    public GameObject container;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip leverPullSound;
    public AudioClip containerCloseSound;
    public AudioClip truckStartSound;

    [Header("Reference to GhostFilter")]
    [SerializeField] private GhostFilter ghostFilter;

    [SerializeField] private BloodPanelFlicker bloodPanelFlicker;

    private bool leverPulled = false;

    // ===== เพิ่มตัวแปรอ้างอิงถึง Notepad กับ Book =====
    [Header("UI References")]
    public NotepadUIController notepadUI;
    public BookMover bookMover;

    private void Update()
    {
        // --- 1) ถ้า Notepad เปิดอยู่ หรือ BookMover เปิดอยู่ => ไม่อนุญาตให้ใช้ Lever ---
        if ((notepadUI != null && notepadUI.IsVisible) ||
            (bookMover != null && bookMover.IsVisible))
        {
            leverText.SetActive(false);
            return;
        }

        // --- 2) ยิง Ray เพื่อเช็กระยะการกด Lever ---
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.gameObject.CompareTag("lever"))
            {
                // แสดงข้อความบอกให้กด E
                leverText.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E) && !leverPulled)
                {
                    // ปิดการเคลื่อนไหวของตัวละคร
                    CharController_Motor.canMove = false;

                    // ถ้า BookMover มีอยู่ => บังคับปิดและล็อก
                    if (bookMover != null)
                    {
                        bookMover.ForceCloseAndLockBook();
                    }

                    Animator leverAnim = hit.collider.transform.root.GetComponent<Animator>();
                    if (leverAnim != null)
                    {
                        leverAnim.SetTrigger(leverPullAnimName);
                    }

                    if (audioSource && leverPullSound)
                    {
                        audioSource.PlayOneShot(leverPullSound);
                    }

                    if (container != null)
                    {
                        Animator containerAnim = container.GetComponent<Animator>();
                        if (containerAnim != null)
                        {
                            containerAnim.SetTrigger(containerCloseAnimName);
                        }

                        if (audioSource && containerCloseSound)
                        {
                            StartCoroutine(PlayContainerCloseAndTruckSequence());
                        }
                    }

                    leverPulled = true;
                }
            }
            else
            {
                leverText.SetActive(false);
            }
        }
        else
        {
            leverText.SetActive(false);
        }
    }

    private IEnumerator PlayContainerCloseAndTruckSequence()
    {
        audioSource.PlayOneShot(containerCloseSound);
        yield return new WaitForSeconds(containerCloseSound.length);

        if (truckStartSound)
        {
            audioSource.PlayOneShot(truckStartSound);
        }

        yield return new WaitForSeconds(5f);

        if (ghostFilter != null)
        {
            ghostFilter.StopHeartbeat();
        }

        if (bloodPanelFlicker != null)
        {
            bloodPanelFlicker.StopFlicker();
        }

        if (ghostFilter != null)
        {
            if (ghostFilter.isGhostGuessCorrect)
            {
                Debug.Log("Lever: ผลการทายถูก! เปลี่ยนซีนเป็น Win");
                SceneManager.LoadScene("Win");
            }
            else
            {
                Debug.Log("Lever: ผลการทายผิด! เปลี่ยนซีนเป็น Lose");
                SceneManager.LoadScene("Lose");
            }
        }
        else
        {
            Debug.LogWarning("Lever: ไม่พบการอ้างอิง GhostFilter");
            SceneManager.LoadScene("Lose");
        }
    }
}
