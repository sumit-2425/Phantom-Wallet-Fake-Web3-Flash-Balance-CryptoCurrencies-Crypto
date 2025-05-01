using UnityEngine;

public class ChangeSceneManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    void Start()
    {
        // ??????? cursor ???????? cursor ?????
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ?? AudioSource ??????????????????? Inspector
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // ???????????????????????
        if (Input.GetMouseButtonDown(0) && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
