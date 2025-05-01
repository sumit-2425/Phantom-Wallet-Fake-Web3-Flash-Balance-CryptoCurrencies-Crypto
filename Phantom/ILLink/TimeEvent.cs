using UnityEngine;
using System.Collections;

public class TimeEvent : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] monsters;              // มี 3 ตัว
    public float initialSpawnDelay = 60f;
    public float[] possibleActiveTimes = { 15f, 20f, 25f, 30f };

    // เก็บ reference ของตัวที่ถูกสุ่มใช้งานอยู่ตอนนี้
    private GameObject currentMonster;
    private MonsterAI currentMonsterAI;

    private void Start()
    {
        // ปิด Monster ทุกตัวตอนเริ่ม
        foreach (GameObject monster in monsters)
        {
            if (monster != null) monster.SetActive(false);
        }
        StartCoroutine(StartMonsterSpawn());
    }

    private IEnumerator StartMonsterSpawn()
    {
        // รอเวลาตั้งต้น
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            // (1) สุ่มเลือก 1 ตัวจาก monsters[]
            int randomIndex = Random.Range(0, monsters.Length);
            currentMonster = monsters[randomIndex];
            currentMonsterAI = currentMonster.GetComponent<MonsterAI>();

            // (2) เปิดใช้งานตัวที่สุ่มได้
            currentMonster.SetActive(true);
            yield return new WaitForSeconds(0.1f); // ดีเลย์เล็กน้อยกันบั๊ก Animator/Agent

            // เรียกให้ AI ทำงาน (เล่นเสียง spawn ฯลฯ)
            if (currentMonsterAI != null)
            {
                currentMonsterAI.ActivateMonster();
            }

            // (3) รอเวลาสุ่มที่จะให้มัน active
            float chosenActiveTime = possibleActiveTimes[Random.Range(0, possibleActiveTimes.Length)];
            yield return new WaitForSeconds(chosenActiveTime);

            // (4) รอจนกว่ามอนสเตอร์ที่กำลัง active อยู่จะเลิกโจมตี
            while (IsCurrentMonsterAttacking())
            {
                yield return null;
            }

            // ก่อนปิด monster ให้หยุดเสียงเดิน + เล่นเสียง vanish
            if (currentMonsterAI != null)
            {
                currentMonsterAI.StopWalkingSound();
                currentMonsterAI.PlayVanishSound();
            }

            // หน่วงเล็กน้อยให้เสียง vanish ได้เล่น
            yield return new WaitForSeconds(1f);

            // (5) ปิด Monster ที่สุ่มมา
            currentMonster.SetActive(false);

            // (6) รอช่วงที่ monster หายเป็นเวลาสุ่ม (15 - 30 วินาที)
            float randomInactiveDuration = Random.Range(15f, 30f);
            yield return new WaitForSeconds(randomInactiveDuration);

            // จากนั้นวน loop ไปสุ่มตัวใหม่
        }
    }

    // เช็คเฉพาะ monster ปัจจุบันว่ากำลังโจมตีอยู่หรือไม่
    private bool IsCurrentMonsterAttacking()
    {
        if (currentMonsterAI == null) return false;
        return currentMonsterAI.IsAttacking;
    }
}
