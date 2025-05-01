using UnityEngine;

public class ThermometerReader : MonoBehaviour
{
    public GameObject thermometerColdPrefab;  // Prefab ??? Thermometer ????????????
    public Transform itemHolder;              // ????????????? Thermometer (??????????)
    public AudioClip alertSound;              // ?? ??????????????
    private AudioSource audioSource;

    private bool isColdCreated = false;       // ?????????? Thermometer_Cold ???????????????????
    private GameObject currentThermometer;    // ?????????????? Thermometer ??????????

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); // ????? AudioSource ?????????
    }

    private void OnTriggerEnter(Collider other)
    {
        Room room = other.GetComponent<Room>();

        if (room != null && room.temperature <= -10 && !isColdCreated && IsHoldingThermometer())
        {
            ActivateColdThermometer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Room room = other.GetComponent<Room>();
        if (room != null)
        {
            ResetThermometer();
        }
    }

    // ? ???????????????????? Thermometer ???????????
    private bool IsHoldingThermometer()
    {
        if (itemHolder.childCount == 0) return false;

        foreach (Transform item in itemHolder)
        {
            if (item.name.Contains("Thermometer")) return true;
        }

        return false;
    }

    // ? ????? Thermometer_Cold
    private void ActivateColdThermometer()
    {
        if (thermometerColdPrefab != null && IsHoldingThermometer())
        {
            if (currentThermometer != null)
            {
                Destroy(currentThermometer);
            }

            currentThermometer = Instantiate(thermometerColdPrefab, itemHolder.position, itemHolder.rotation);
            currentThermometer.transform.SetParent(itemHolder);
            currentThermometer.transform.localPosition = Vector3.zero;
            currentThermometer.transform.localRotation = Quaternion.identity;

            AdjustItemRotation(currentThermometer);

            isColdCreated = true;
            Debug.Log("Cold Thermometer Activated: " + currentThermometer.name);
            PlaySound();
        }
    }

    // ? ?????? Thermometer ????????????
    public void ResetThermometer()
    {
        if (currentThermometer != null)
        {
            currentThermometer.transform.SetParent(null);
            Destroy(currentThermometer);
            currentThermometer = null;
        }

        isColdCreated = false;
        Debug.Log("Thermometer Reset to Normal.");
    }

    // ? ?????????????????????????????? Thermometer_Cold ???????????? Thermometer
    public void OnItemChanged()
    {
        Debug.Log("OnItemChanged() called! Checking for Thermometer_Cold");

        foreach (Transform item in itemHolder)
        {
            if (item.name.Contains("Thermometer RED"))
            {
                Debug.Log("Destroying Thermometer RED (Clone): " + item.name);
                Destroy(item.gameObject);
            }
        }

        currentThermometer = null;
        isColdCreated = false;
    }

    // ? ?????????????? Thermometer
    private void AdjustItemRotation(GameObject item)
    {
        if (item == null) return;
        item.transform.localRotation = Quaternion.Euler(0, 90, 0);
    }
    private void PlaySound()
    {
        if (alertSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(alertSound);
        }
    }
}
