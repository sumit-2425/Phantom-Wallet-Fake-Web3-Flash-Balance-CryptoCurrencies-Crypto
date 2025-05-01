using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public Transform player;
    public float chaseRange = 30f;
    public float attackRange = 1f;

    private NavMeshAgent agent;
    private Animator animator;
    private CharController_Motor playerMotor;
    private bool isActive = false;

    private enum MonsterState
    {
        Idle,
        Chase,
        Attack
    }
    private MonsterState currentState = MonsterState.Idle;

    [Header("Audio")]
    public AudioSource monsterAudioSource;
    public AudioClip monsterSpawnSound;
    public AudioClip monsterAttackSound;
    public AudioClip playerDeathSound;
    public AudioClip monsterVanishSound;
    public AudioClip monsterAlertSound;  // เสียงแจ้งเตือนเมื่อเข้าใกล้ player

    // สำหรับเสียงเดิน
    public AudioSource monsterWalkAudioSource;
    public AudioClip monsterWalkClip;
    public float maxVolumeDistance = 5f;
    public float minVolumeDistance = 30f;

    // flag เพื่อกันเล่นเสียงโจมตีซ้ำ
    private bool hasPlayedAttackSound = false;
    // flag สำหรับเสียงแจ้งเตือน (alert) ครั้งเดียวต่อการเกิดของ monster
    private bool hasPlayedAlertSound = false;

    void Start()
    {
        // สุ่มจุดเกิดครั้งแรก
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[randomIndex].position;
        }
        else
        {
            Debug.LogWarning("No spawn points assigned!");
        }

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player != null)
            playerMotor = player.GetComponent<CharController_Motor>();

        currentState = MonsterState.Idle;

        // ปิดการทำงานของ AI ชั่วคราว
        agent.enabled = false;
        animator.enabled = false;
    }

    void Update()
    {
        if (!isActive) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ตรวจสอบระยะห่างเพื่อเล่นเสียงแจ้งเตือนเพียงครั้งเดียว
        if (!hasPlayedAlertSound && distanceToPlayer <= 10f)
        {
            if (monsterAudioSource != null && monsterAlertSound != null)
            {
                monsterAudioSource.PlayOneShot(monsterAlertSound);
            }
            hasPlayedAlertSound = true;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                IdleState(distanceToPlayer);
                // หยุดเสียงเดินถ้าเป็น Idle
                StopWalkingSound();
                break;

            case MonsterState.Chase:
                ChaseState(distanceToPlayer);
                // เรียก HandleWalkingSound เพื่อเล่นและปรับ volume ตามระยะ
                HandleWalkingSound(distanceToPlayer);
                break;

            case MonsterState.Attack:
                AttackState(distanceToPlayer);
                // หยุดเสียงเดินเมื่อเข้าสู่ Attack
                StopWalkingSound();
                break;
        }
    }

    // เรียกใช้เมื่อเปิด (SetActive(true)) แล้วต้องการให้ AI พร้อมทำงาน
    public void ActivateMonster()
    {
        if (agent == null || animator == null)
        {
            InitializeMonster();
        }

        isActive = true;
        agent.enabled = true;
        animator.enabled = true;

        // เล่นเสียง spawn ทุกครั้งที่มอนสเตอร์ถูกเปิดใช้งาน
        if (monsterAudioSource != null && monsterSpawnSound != null)
        {
            monsterAudioSource.PlayOneShot(monsterSpawnSound);
        }

        currentState = MonsterState.Idle;
        hasPlayedAttackSound = false;
        hasPlayedAlertSound = false;  // รีเซ็ต flag เสียงแจ้งเตือนเมื่อ monster เกิดใหม่
    }

    void IdleState(float distance)
    {
        agent.isStopped = true;
        animator.SetBool("IsWalking", false);

        // รีเซ็ต flag ไม่ให้เล่นเสียงโจมตีซ้ำ
        hasPlayedAttackSound = false;

        if (distance <= chaseRange)
        {
            currentState = MonsterState.Chase;
        }
    }

    void ChaseState(float distance)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        animator.SetBool("IsWalking", true);

        hasPlayedAttackSound = false;

        if (distance <= attackRange)
        {
            currentState = MonsterState.Attack;
        }
        else if (distance > chaseRange)
        {
            currentState = MonsterState.Idle;
        }
    }

    void AttackState(float distance)
    {
        agent.isStopped = true;
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");
        animator.speed = 1.5f;

        // เล่นเสียงโจมตี (hit sound) เพียงครั้งเดียว
        if (!hasPlayedAttackSound && monsterAudioSource != null && monsterAttackSound != null)
        {
            monsterAudioSource.PlayOneShot(monsterAttackSound);
            hasPlayedAttackSound = true;
        }
        // ถ้าผู้เล่นยังไม่ตาย ให้จัดการตาย
        if (playerMotor != null && !playerMotor.IsDead())
        {
            playerMotor.StopAllPlayerSounds();  // เพิ่มมาบังคับหยุดเสียง
            playerMotor.TriggerDeath();         // ทำให้ Player ตาย
            StartCoroutine(PlayPlayerDeathSoundAfterDelay(2f));
        }
        // ถ้าผู้เล่นยังไม่ตาย ให้จัดการตาย
        if (playerMotor != null && !playerMotor.IsDead())
        {
            playerMotor.TriggerDeath();
            StartCoroutine(PlayPlayerDeathSoundAfterDelay(2f));
        }

        // ถ้าผู้เล่นหนีออกจากระยะโจมตี ให้กลับไป Chase
        if (distance > attackRange)
        {
            currentState = MonsterState.Chase;
        }
    }

    private IEnumerator PlayPlayerDeathSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (monsterAudioSource != null && playerDeathSound != null)
        {
            monsterAudioSource.PlayOneShot(playerDeathSound);
        }
    }

    // จัดการเสียงเดิน (เล่น/ปรับ volume ตามระยะ)
    private void HandleWalkingSound(float distanceToPlayer)
    {
        if (monsterWalkAudioSource != null && monsterWalkClip != null)
        {
            // ถ้ายังไม่ได้เล่นเสียงเดิน ให้เริ่มเล่น
            if (!monsterWalkAudioSource.isPlaying)
            {
                monsterWalkAudioSource.clip = monsterWalkClip;
                monsterWalkAudioSource.loop = true;
                monsterWalkAudioSource.Play();
            }

            // ปรับความดังตามระยะ
            float vol = 0f;
            if (distanceToPlayer <= maxVolumeDistance)
            {
                vol = 1f;
            }
            else if (distanceToPlayer >= minVolumeDistance)
            {
                vol = 0f;
            }
            else
            {
                // คำนวณสัดส่วนระหว่าง maxDistance ถึง minDistance
                float ratio = (distanceToPlayer - maxVolumeDistance) / (minVolumeDistance - maxVolumeDistance);
                vol = 1f - ratio;
            }
            monsterWalkAudioSource.volume = vol;
        }
    }

    // หยุดเสียงเดิน
    public void StopWalkingSound()
    {
        if (monsterWalkAudioSource != null && monsterWalkAudioSource.isPlaying)
        {
            monsterWalkAudioSource.Stop();
        }
    }

    // เมื่อ Monster ถูกสั่งปิด (SetActive(false)) หรือ Destroy
    void OnDisable()
    {
        StopWalkingSound();
    }

    // เมื่อ Monster ชนกับ Player ให้หยุดเสียงเดินทันที
    void OnCollisionEnter(Collision collision)
    {
        if (player != null && collision.gameObject == player.gameObject)
        {
            // หยุดเสียง Monster (ถ้ามี)
            StopWalkingSound();

            // หยุดเสียงทั้งหมดของ Player
            CharController_Motor playerMotor = player.GetComponent<CharController_Motor>();
            if (playerMotor != null)
            {
                playerMotor.StopAllPlayerSounds();
            }

            // แล้วจะสั่ง playerMotor.TriggerDeath() เลย หรือสั่งอย่างอื่นก็ได้
            // playerMotor.TriggerDeath();
        }
    }

    public void TeleportToRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[randomIndex].position;
        }
    }

    public void RespawnRandom()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            transform.position = spawnPoints[randomIndex].position;
        }
    }

    public bool IsAttacking
    {
        get { return currentState == MonsterState.Attack; }
    }

    // เล่นเสียงหายไป
    public void PlayVanishSound()
    {
        if (monsterAudioSource != null && monsterVanishSound != null)
        {
            monsterAudioSource.PlayOneShot(monsterVanishSound);
        }
    }

    void OnEnable()
    {
        if (agent == null || animator == null)
        {
            InitializeMonster();
        }
    }

    void InitializeMonster()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player != null)
        {
            playerMotor = player.GetComponent<CharController_Motor>();
        }
    }
}
