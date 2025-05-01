using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GhostFilter : MonoBehaviour
{
    [Header("Evidence Toggles")]
    [SerializeField] private Toggle emfToggle;
    [SerializeField] private Toggle uvToggle;
    [SerializeField] private Toggle ghostBookToggle;
    [SerializeField] private Toggle spiritBoxToggle;
    [SerializeField] private Toggle thermometerToggle;
    [SerializeField] private BloodPanelFlicker bloodPanelFlicker;

    // โครงสร้าง UI ของผีแต่ละตัว
    [System.Serializable]
    public class GhostUI
    {
        public string ghostName;
        public Toggle ghostToggle;   // Toggle ให้ติ๊กเลือกผี
        public Graphic ghostLabel;   // สำหรับปรับความโปร่งใส
        public List<EvidenceType> requiredEvidences;
    }

    [Header("Ghosts Info (UI)")]
    [SerializeField] private GhostUI[] ghosts;

    [Header("Audio (Submit)")]
    [SerializeField] private AudioSource submitAudioSource;
    [SerializeField] private AudioClip heartBeatClip;

    [Header("Reference to GhostEventManager")]
    [SerializeField] private GhostEventManager ghostEventManager;

    // เก็บชื่อผีที่ผู้เล่นเลือกจากสมุด
    private string currentSelectedGhost = null;

    // *** ตัวแปรเก็บผลการทาย ***
    public bool isGhostGuessCorrect { get; private set; } = false;

    private void Start()
    {
        // ตั้งค่า Evidence Toggles เริ่มต้น
        emfToggle.isOn = false;
        uvToggle.isOn = false;
        ghostBookToggle.isOn = false;
        spiritBoxToggle.isOn = false;
        thermometerToggle.isOn = false;

        // ตั้งค่า Ghost Toggles
        foreach (var ghost in ghosts)
        {
            ghost.ghostToggle.isOn = false;
            ghost.ghostToggle.onValueChanged.AddListener(
                (isOn) => OnGhostToggleChanged(ghost, isOn)
            );
        }

        // เมื่อ Evidence เปลี่ยน, เรียก UpdateGhosts
        emfToggle.onValueChanged.AddListener((val) => UpdateGhosts());
        uvToggle.onValueChanged.AddListener((val) => UpdateGhosts());
        ghostBookToggle.onValueChanged.AddListener((val) => UpdateGhosts());
        spiritBoxToggle.onValueChanged.AddListener((val) => UpdateGhosts());
        thermometerToggle.onValueChanged.AddListener((val) => UpdateGhosts());

        UpdateGhosts();
    }

    private void OnGhostToggleChanged(GhostUI changedGhost, bool isOn)
    {
        if (isOn)
        {
            // ยกเลิกการเลือกผีตัวอื่น
            foreach (var ghost in ghosts)
            {
                if (ghost != changedGhost)
                {
                    ghost.ghostToggle.isOn = false;
                }
            }
            currentSelectedGhost = changedGhost.ghostName;
        }
        else
        {
            // ถ้า uncheck ผีที่เลือกอยู่ => เคลียร์
            if (currentSelectedGhost == changedGhost.ghostName)
            {
                currentSelectedGhost = null;
            }
        }
    }

    private void UpdateGhosts()
    {
        // เก็บ EvidenceType ที่ผู้เล่นติ๊ก
        List<EvidenceType> currentEvidence = new List<EvidenceType>();

        if (emfToggle.isOn) currentEvidence.Add(EvidenceType.EMF5);
        if (uvToggle.isOn) currentEvidence.Add(EvidenceType.Fingerprint);
        if (ghostBookToggle.isOn) currentEvidence.Add(EvidenceType.GhostWriting);
        if (spiritBoxToggle.isOn) currentEvidence.Add(EvidenceType.SpiritBox);
        if (thermometerToggle.isOn) currentEvidence.Add(EvidenceType.FreezingTemp);

        // ถ้าไม่มี Evidence เลย -> ผีทุกตัวยังเป็นไปได้
        if (currentEvidence.Count == 0)
        {
            foreach (var ghost in ghosts) SetGhostAlpha(ghost, 1f);
            return;
        }

        // กรองผีแบบ AND
        foreach (var ghost in ghosts)
        {
            bool isPossible = true;
            foreach (var ev in currentEvidence)
            {
                if (!ghost.requiredEvidences.Contains(ev))
                {
                    isPossible = false;
                    break;
                }
            }
            SetGhostAlpha(ghost, isPossible ? 1f : 0.4f);
        }
    }

    private void SetGhostAlpha(GhostUI ghost, float alpha)
    {
        if (ghost.ghostToggle && ghost.ghostToggle.targetGraphic)
        {
            var c = ghost.ghostToggle.targetGraphic.color;
            c.a = alpha;
            ghost.ghostToggle.targetGraphic.color = c;
        }
        if (ghost.ghostLabel)
        {
            var c = ghost.ghostLabel.color;
            c.a = alpha;
            ghost.ghostLabel.color = c;
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับปุ่ม Submit
    /// </summary>
    public void OnSubmitGhost()
    {
        if (string.IsNullOrEmpty(currentSelectedGhost))
        {
            Debug.Log("ยังไม่ได้เลือกผี");
            return;
        }

        // เล่นเสียง heartbeat แบบวน
        if (submitAudioSource != null && heartBeatClip != null)
        {
            submitAudioSource.clip = heartBeatClip;
            submitAudioSource.loop = true;
            submitAudioSource.Play();
        }

        // ถ้ามี GhostEventManager และผีที่สุ่ม (chosenGhost)
        if (ghostEventManager != null && ghostEventManager.chosenGhost != null)
        {
            string realGhostName = ghostEventManager.chosenGhost.ghostName;

            if (currentSelectedGhost == realGhostName)
            {
                isGhostGuessCorrect = true;
                Debug.Log($"เลือกถูก! ผีคือ: {realGhostName}");
            }
            else
            {
                isGhostGuessCorrect = false;
                Debug.Log($"เลือกผิด! ผีจริงคือ: {realGhostName} แต่ผู้เล่นเลือก: {currentSelectedGhost}");
            }
        }
        else
        {
            // ถ้าไม่มี GhostEventManager หรือ chosenGhost
            isGhostGuessCorrect = false;
            Debug.LogWarning("GhostEventManager หรือ chosenGhost ยังไม่ได้ตั้งค่า!");
        }

        // เริ่มกระพริบ Panel สีขอบเลือด
        if (bloodPanelFlicker != null)
        {
            bloodPanelFlicker.StartFlicker();
        }

        // ตัวอย่าง: ปิดสมุดหรือทำอย่างอื่น
        BookMover.instance.ForceCloseAndLockBook();
    }

    // เมธอดสำหรับหยุดเสียง heartbeat (เรียกใช้เมื่อ lever ถูกใช้)
    public void StopHeartbeat()
    {
        if (submitAudioSource != null)
        {
            submitAudioSource.Stop();
            submitAudioSource.loop = false;
        }
    }
}
