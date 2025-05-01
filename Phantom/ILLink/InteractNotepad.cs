using TMPro;
using UnityEngine;
using UnityEngine.UI; // ถ้าใช้ TextMeshPro ให้เปลี่ยนเป็น using TMPro;

public class InteractNotepad : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Notepad UI")]
    public NotepadUIController notepadUI;
    public BookMover bookMover;

    [Header("UI Message")]
    public TextMeshProUGUI interactMessage;
    // หรือใช้ TextMeshProUGUI แทนได้ เช่น:
    // public TextMeshProUGUI interactMessage;

    void Update()
    {
        // ถ้า Notepad เปิดอยู่ => กด E ปิดได้เลย
        if (notepadUI.IsVisible)
        {
            // ซ่อนข้อความ (เผื่อค้างอยู่)
            if (interactMessage != null)
            {
                interactMessage.gameObject.SetActive(false);
            }

            if (Input.GetKeyDown(interactKey))
            {
                notepadUI.CloseNotepad();
            }
            return;
        }

        // ถ้า BookMover เปิดอยู่ => ห้ามเปิด Notepad
        if (bookMover != null && bookMover.IsVisible)
        {
            // ซ่อนข้อความ
            if (interactMessage != null)
            {
                interactMessage.gameObject.SetActive(false);
            }
            return;
        }

        // ยิง Ray เพื่อหา Notepad
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            if (hit.collider.CompareTag("Notepad"))
            {
                // หากเจอ Notepad => โชว์ข้อความ "Press E to read note"
                if (interactMessage != null)
                {
                    interactMessage.text = "Press 'E' to Read Note";
                    interactMessage.gameObject.SetActive(true);
                }

                // เมื่อผู้เล่นกด E
                if (Input.GetKeyDown(interactKey))
                {
                    var noteData = hit.collider.GetComponent<Notepad3DObject>();
                    if (noteData != null)
                    {
                        notepadUI.OpenNotepad(noteData);
                    }
                }
            }
            else
            {
                // ถ้า Raycast เจออย่างอื่น => ซ่อนข้อความ
                if (interactMessage != null)
                {
                    interactMessage.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // ถ้า Raycast ไม่โดนอะไร => ซ่อนข้อความ
            if (interactMessage != null)
            {
                interactMessage.gameObject.SetActive(false);
            }
        }
    }
}
