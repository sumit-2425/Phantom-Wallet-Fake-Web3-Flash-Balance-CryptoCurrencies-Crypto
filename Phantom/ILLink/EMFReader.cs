using UnityEngine;

public class EMFReader : MonoBehaviour
{
    public GameObject emfLevel5Prefab;  // Prefab ??? EMF RED (EMF5)
    public Transform itemHolder;        // ????????????? EMF (??????????)
    public AudioClip alertSound;        // ?? ??????????????
    private AudioSource audioSource;

    private bool isEMFCreated = false;  // ?????????? EMF5 ???????????????????
    private GameObject currentEMF;      // ?????????????? EMF ???????????????

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // ????? AudioSource ?????????
    }

    private void OnTriggerEnter(Collider other)
    {
        Room room = other.GetComponent<Room>();

        if (room != null && room.emfLevel == 5 && !isEMFCreated && IsHoldingEMF())
        {
            CreateEMF5();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Room room = other.GetComponent<Room>();
        if (room != null)
        {
            ResetEMF();
        }
    }

    // ? ??????????????????????? EMF ???????????
    private bool IsHoldingEMF()
    {
        if (itemHolder.childCount == 0) return false;

        foreach (Transform item in itemHolder)
        {
            if (item.name.Contains("EMF")) return true;
        }

        return false;
    }

    // ? ????? EMF RED (EMF5)
    private void CreateEMF5()
    {
        if (emfLevel5Prefab != null && IsHoldingEMF())
        {
            if (currentEMF != null)
            {
                Destroy(currentEMF);
            }

            currentEMF = Instantiate(emfLevel5Prefab, itemHolder.position, itemHolder.rotation);
            currentEMF.transform.SetParent(itemHolder);
            currentEMF.transform.localPosition = Vector3.zero;
            currentEMF.transform.localRotation = Quaternion.identity;

            // ? ?????????? EMF ??????????
            AdjustItemRotation(currentEMF);

            isEMFCreated = true;
            Debug.Log("EMF5 Created: " + currentEMF.name);
            PlaySound();
        }
    }

    // ? ?????? EMF ????????????
    public void ResetEMF()
    {
        if (currentEMF != null)
        {
            currentEMF.transform.SetParent(null);
            Destroy(currentEMF);
            currentEMF = null;
        }

        isEMFCreated = false;
        Debug.Log("EMF reset.");
    }

    // ? ?????????????????????????????? EMF5 ???????????? EMF
    public void OnItemChanged()
    {
        Debug.Log("OnItemChanged() called! Checking for EMF RED (Clone)");

        // ????? EMF RED (Clone) ?? ItemHolder ????????????
        foreach (Transform item in itemHolder)
        {
            if (item.name.Contains("EMF RED"))
            {
                Debug.Log("Destroying EMF RED (Clone): " + item.name);
                Destroy(item.gameObject);
            }
        }

        // ????????? currentEMF ????????
        currentEMF = null;
        isEMFCreated = false;
    }

    // ? ?????????????? EMF ??????????
    private void AdjustItemRotation(GameObject item)
    {
        if (item == null) return;
        item.transform.localRotation = Quaternion.Euler(0, -90, 0);
    }
    private void PlaySound()
    {
        if (alertSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(alertSound);
        }
    }
}
