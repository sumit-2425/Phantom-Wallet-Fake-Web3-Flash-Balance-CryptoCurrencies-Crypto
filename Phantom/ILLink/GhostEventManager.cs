using System.Collections.Generic;
using UnityEngine;

// ข้อมูลของผี 1 ตัว
[System.Serializable]
public class GhostData
{
    public string ghostName;
    public List<EvidenceType> requiredEvidences;
    // เช่น [EMF5, FreezingTemp, SpiritBox] สำหรับผีบางตัว
}

// ตัวอย่างการใช้งานร่วมกับ GhostEventManager
public class GhostEventManager : MonoBehaviour
{
    public List<Room> rooms;
    public List<GhostData> ghostDatas;

    public Room ghostRoom;
    public GhostData chosenGhost;

    private void Start()
    {
        PickGhostRoom();
        chosenGhost = PickRandomGhost();

        if (ghostRoom != null && chosenGhost != null)
        {
            // เอา Evidence จาก chosenGhost มาตั้งค่าในห้อง
            SetupEvidenceInRoom(ghostRoom, chosenGhost.requiredEvidences);
        }
    }

    private void PickGhostRoom()
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning("No rooms assigned in GhostEventManager!");
            return;
        }
        int randIndex = Random.Range(0, rooms.Count);
        ghostRoom = rooms[randIndex];
        Debug.Log("Ghost room is: " + ghostRoom.roomName);
    }

    private GhostData PickRandomGhost()
    {
        if (ghostDatas == null || ghostDatas.Count == 0)
        {
            Debug.LogWarning("No ghostDatas assigned!");
            return null;
        }
        int randIndex = Random.Range(0, ghostDatas.Count);
        var g = ghostDatas[randIndex];
        Debug.Log("Chosen Ghost: " + g.ghostName);
        return g;
    }

    private void SetupEvidenceInRoom(Room room, List<EvidenceType> evidences)
    {
        room.ResetEvidence();
        foreach (var evi in evidences)
        {
            switch (evi)
            {
                case EvidenceType.EMF5:
                    room.emfLevel = 5;
                    break;
                case EvidenceType.FreezingTemp:
                    room.temperature = -10f;
                    break;
                case EvidenceType.SpiritBox:
                    room.canUseSpiritBox = true;
                    break;
                case EvidenceType.GhostWriting:
                    room.canGhostWriting = true;
                    break;
                case EvidenceType.Fingerprint:
                    room.canFingerprint = true;
                    break;
            }
        }
    }
}
