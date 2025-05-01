using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Pickup : MonoBehaviour
{
    public float interactionDistance = 3f;
    public Transform itemHolder;
    public List<GameObject> itemPrefabs;
    public TextMeshProUGUI pickupMessage;
    public TextMeshProUGUI interactionText;
    private GameObject heldItem;
    private Dictionary<string, GameObject> itemMap;
    private List<GameObject> inventory = new List<GameObject>();
    private int selectedItemIndex = 0;
    private float dropForwardDistance = 2f;
    public AudioSource audioSource;
    public AudioClip clickSound;

    // Ê¶Ò¹Ðä¿©ÒÂ
    private bool isFlashlightOn = false;

    private void Start()
    {
        itemMap = new Dictionary<string, GameObject>();
        foreach (GameObject item in itemPrefabs)
        {
            itemMap[item.name] = item;
        }

        if (pickupMessage != null)
            pickupMessage.gameObject.SetActive(false);

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // --- Raycast à´ÔÁ à¾×èÍµÃÇ¨¨ÑºËÂÔº/ÇÒ§¢Í§ ---
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Pickup") && hitObject.activeInHierarchy)
            {
                if (inventory.Count + (heldItem == null ? 0 : 1) < 3)
                {
                    string itemName = hitObject.name.Replace("(Clone)", "");
                    ShowInteractionText("Press 'F' to Pick Up (" + itemName + ")");

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        PickupItem(hitObject);
                    }
                }
                else
                {
                    ShowInteractionText("Inventory Full!");
                }
            }
            else if (heldItem != null)
            {
                string heldItemName = heldItem.name.Replace("(Clone)", "");
                ShowInteractionText("Press 'F' to Drop (" + heldItemName + ")");

                if (Input.GetKeyDown(KeyCode.F))
                {
                    DropItem();
                }
            }
            else
            {
                HideInteractionText();
            }
        }
        else
        {
            HideInteractionText();
        }

        // --- ÊÅÑºäÍà·çÁ´éÇÂ ScrollWheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelectItem(1);
        }
        else if (scroll < 0f)
        {
            SelectItem(-1);
        }

        // ========== ตรวจจับคลิกขวาเพื่อเปิด/ปิดไฟ (สำหรับ flashlight และ UV flashlight) ==========
        if (heldItem != null)
        {
            string checkName = heldItem.name.ToLower();
            if (checkName.Contains("flashlight"))
            {
                if (Input.GetMouseButtonDown(1)) // คลิกขวา
                {
                    isFlashlightOn = !isFlashlightOn;

                    // เล่นเสียงคลิกเมื่อเปิด/ปิดไฟ
                    if (audioSource != null && clickSound != null)
                    {
                        audioSource.PlayOneShot(clickSound);
                    }

                    // เลือกชื่อ child ของ Light ที่จะควบคุม: UV_Flashlight สำหรับ UV flashlight, ไม่เช่นนั้นใช้ Light_Flashlight
                    string lightChildName = checkName.Contains("uv") ? "UV_Flashlight" : "Light_Flashlight";
                    Transform lightTransform = heldItem.transform.Find(lightChildName);
                    if (lightTransform != null)
                    {
                        Light lightComp = lightTransform.GetComponent<Light>();
                        if (lightComp != null)
                        {
                            lightComp.enabled = isFlashlightOn;
                        }
                    }
                }
            }
        }
    }

    void PickupItem(GameObject itemObj)
    {
        BookOpenController bookOpenController = itemObj.GetComponent<BookOpenController>();
        if (bookOpenController != null)
        {
            // ถ้ามือถืออยู่แล้ว ให้เก็บไอเท็มที่ถืออยู่ใน inventory ก่อน
            if (heldItem != null)
            {
                inventory.Add(heldItem);
                heldItem.SetActive(false);
            }

            // เปลี่ยนจาก BookOpen ให้เป็น Book (ปิด)
            heldItem = bookOpenController.SpawnClosedBook(itemHolder);
            AdjustItemRotation(heldItem);
            return;
        }

        // ¶éÒÁ×ÍàÃÒ¶×ÍÍÐäÃÍÂÙè ãËéà¡çºà¢éÒ¤ÅÑ§ inventory ¡èÍ¹
        if (heldItem != null)
        {
            inventory.Add(heldItem);
            heldItem.SetActive(false);
        }

        // à»ÅÕèÂ¹ itemObj ÁÒà»ç¹ heldItem
        heldItem = itemObj;

        // àÍÒª×èÍ (Clone) ÍÍ¡
        string itemName = heldItem.name.Replace("(Clone)", "");
        heldItem.name = itemName;

        // ¨Ñ´ parent ãËé item ÍÂÙèã¹Á×Í
        heldItem.transform.SetParent(itemHolder);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        AdjustItemRotation(heldItem);

        heldItem.SetActive(true);
        heldItem.tag = "Untagged";

        // === ถ้าเป็นไฟฉายหรือ UV ไฟฉาย ให้เปิด GameObject ของ Light component ขึ้นมา (แต่ปิด Light component ไว้ก่อน) ===
        if (itemName.ToLower().Contains("flashlight"))
        {
            isFlashlightOn = false;
            string lightChildName = itemName.ToLower().Contains("uv") ? "UV_Flashlight" : "Light_Flashlight";
            Transform lightTransform = heldItem.transform.Find(lightChildName);
            if (lightTransform != null)
            {
                lightTransform.gameObject.SetActive(true);
                Light lightComp = lightTransform.GetComponent<Light>();
                if (lightComp != null)
                {
                    lightComp.enabled = false;
                }
            }
        }
    }

    void DropItem()
    {
        if (heldItem == null) return;

        // กำหนดตำแหน่งและการหมุนที่จะ Drop ไอเท็ม
        Vector3 dropPos = transform.position + transform.forward * dropForwardDistance;
        Quaternion dropRot = Quaternion.identity;

        // ตรวจสอบว่าไอเท็มที่ถืออยู่เป็น Book (ปิด) หรือไม่
        BookController bookController = heldItem.GetComponent<BookController>();
        if (bookController != null)
        {
            // ถ้าเป็น Book ให้เรียกเมธอด SpawnOpenBook() เพื่อ Destroy Book และ Instantiate BookOpen ลงบนพื้น
            bookController.SpawnOpenBook(dropPos, dropRot);
            heldItem = null;
            return;
        }

        // »Å´ parent ÍÍ¡
        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPos;
        heldItem.transform.rotation = Quaternion.identity;

        heldItem.tag = "Pickup";
        heldItem.SetActive(true);

        // ถ้าเป็นไฟฉายหรือ UV ไฟฉาย ให้ปิดไฟทั้งหมดก่อนทิ้ง
        string itemName = heldItem.name.ToLower();
        if (itemName.Contains("flashlight"))
        {
            isFlashlightOn = false;
            string lightChildName = itemName.Contains("uv") ? "UV_Flashlight" : "Light_Flashlight";
            Transform lightTransform = heldItem.transform.Find(lightChildName);
            if (lightTransform != null)
            {
                Light lightComp = lightTransform.GetComponent<Light>();
                if (lightComp != null)
                {
                    lightComp.enabled = false;
                }
                lightTransform.gameObject.SetActive(false);
            }
        }

        EMFReader emfReader = FindObjectOfType<EMFReader>();
        if (emfReader != null)
        {
            emfReader.OnItemChanged();
        }
        ThermometerReader thermoReader = FindObjectOfType<ThermometerReader>();
        if (thermoReader != null)
        {
            thermoReader.OnItemChanged();
        }
        SpiritBox spiritBox = FindObjectOfType<SpiritBox>();
        if (spiritBox != null)
        {
            spiritBox.ResetSpiritBox();  // รีเซ็ต SpiritBox เมื่อไอเทมถูกเปลี่ยน
        }

        heldItem = null;
    }

    void SelectItem(int direction)
    {
        if (inventory.Count > 0)
        {
            // à¡çº¢Í§ã¹Á×ÍãÊè¤ÅÑ§
            if (heldItem != null)
            {
                inventory.Add(heldItem);
                heldItem.SetActive(false);
            }

            // »ÃÑº index
            selectedItemIndex = (selectedItemIndex + direction) % inventory.Count;
            if (selectedItemIndex < 0)
                selectedItemIndex = inventory.Count - 1;

            // ´Ö§äÍà·çÁ¨Ò¡ inventory ÁÒäÇéã¹Á×Í
            heldItem = inventory[selectedItemIndex];
            inventory.RemoveAt(selectedItemIndex);

            heldItem.transform.SetParent(itemHolder);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;
            heldItem.SetActive(true);
            heldItem.tag = "Untagged";

            AdjustItemRotation(heldItem);

            if (heldItem.name.ToLower().Contains("spiritbox"))
            {
                // เริ่มการใช้งาน Spirit Box ใหม่
                SpiritBox spiritBox = heldItem.GetComponent<SpiritBox>();
                if (spiritBox != null)
                {
                    // รีเซ็ตและเริ่มต้นการใช้งาน Spirit Box
                    spiritBox.ResetSpiritBox();  // รีเซ็ตเมื่อไอเทมถูกเลือก
                }
            }

            EMFReader emfReader = FindObjectOfType<EMFReader>();
            if (emfReader != null)
            {
                emfReader.OnItemChanged();
            }
            ThermometerReader thermoReader = FindObjectOfType<ThermometerReader>();
            if (thermoReader != null)
            {
                thermoReader.OnItemChanged();
            }

            // ถ้าเป็นไฟฉายหรือ UV ไฟฉาย ให้เปิด GameObject ของ Light component ขึ้นมา (แต่ปิด Light component ไว้ก่อน)
            string itemName = heldItem.name.ToLower();
            if (itemName.Contains("flashlight"))
            {
                isFlashlightOn = false;
                string lightChildName = itemName.Contains("uv") ? "UV_Flashlight" : "Light_Flashlight";
                Transform lightTransform = heldItem.transform.Find(lightChildName);
                if (lightTransform != null)
                {
                    lightTransform.gameObject.SetActive(true);
                    Light lightComp = lightTransform.GetComponent<Light>();
                    if (lightComp != null)
                    {
                        lightComp.enabled = false;
                    }
                }
            }
        }
    }

    void AdjustItemRotation(GameObject item)
    {
        if (item == null) return;

        if (item.name == "UV flashlight")
        {
            item.transform.localRotation = Quaternion.Euler(180, 180, 0);
        }
        else if (item.name == "Thermometer")
        {
            item.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }
        else if (item.name == "Book")
        {
            item.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (item.name == "Crusifix")
        {
            item.transform.localRotation = Quaternion.Euler(270, 90, 0);
        }
        else if (item.name == "EMF")
        {
            item.transform.localRotation = Quaternion.Euler(0, -90, 0);
        }
        else if (item.name == "flashlight")
        {
            item.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
        else if (item.name == "spiritbox")
        {
            item.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    void ShowInteractionText(string msg)
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = msg;
        }
    }

    void HideInteractionText()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void ShowPickupMessage(string msg)
    {
        if (pickupMessage != null)
        {
            pickupMessage.gameObject.SetActive(true);
            pickupMessage.text = msg;
            CancelInvoke(nameof(HidePickupMessage));
            Invoke(nameof(HidePickupMessage), 2f);
        }
    }

    void HidePickupMessage()
    {
        if (pickupMessage != null)
            pickupMessage.gameObject.SetActive(false);
    }
}
