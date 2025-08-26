using UnityEngine;
using System.Collections;

public class TurretController : MonoBehaviour
{
    // 타겟팅 및 회전 관련 변수
    public Transform lookAtObj; // 회전할 오브젝트
    public string targetTag = "Enemy";
    public float rotationSpeed = 5f;
    public float range = 10f;
    public Transform target; // 현재 타겟
    private float homeY; // 초기 Y축 회전값

    // 발사 관련 변수
    public GameObject projectilePrefab;
    public Transform shootElement; // 발사 위치
    public float fireRate = 1f;
    private bool isShooting; // 발사 중인지 확인

    // 추가 기능 관련 변수
    public bool isCatcherType = false; // Catcher 타입 여부
    public Animator anim; // 애니메이터 컴포넌트
    public int damage = 10;
    
    // 코루틴 딜레이 계산용
    private float shootDelay;
    
    void Start()
    {
        // 초기 설정
        if (lookAtObj != null)
        {
            homeY = lookAtObj.localRotation.eulerAngles.y;
        }
        if (isCatcherType && anim == null)
        {
            anim = GetComponent<Animator>();
        }
        shootDelay = 1f / fireRate;
    }

    void Update()
    {
        // 1. 타겟 탐색
        FindTarget();

        // 2. 타겟이 있을 경우
        if (target != null)
        {
            AimAtTarget();
            
            // 발사 로직
            if (!isCatcherType && !isShooting)
            {
                StartCoroutine(ShootCoroutine());
            }
            else if (isCatcherType && !isShooting)
            {
                // Catcher 타입은 애니메이션으로 공격
                StartCoroutine(CatcherAttackCoroutine());
            }
        }
        // 3. 타겟이 없을 경우
        else
        {
            ReturnToHomeRotation();
        }
    }

    void FindTarget()
    {
        // 범위 내 가장 가까운 타겟 찾기
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(targetTag))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hitCollider.transform;
                }
            }
        }
        target = closestTarget;
    }

    void AimAtTarget()
    {
        // 타겟을 향해 회전
        Vector3 direction = target.position - lookAtObj.position;
        direction.y = 0; // Y축은 고정
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    void ReturnToHomeRotation()
    {
        // 초기 방향으로 회전
        Quaternion home = Quaternion.Euler(lookAtObj.localRotation.eulerAngles.x, homeY, lookAtObj.localRotation.eulerAngles.z);
        lookAtObj.rotation = Quaternion.Slerp(lookAtObj.rotation, home, Time.deltaTime * rotationSpeed);
    }

    // 일반 발사 코루틴
    IEnumerator ShootCoroutine()
    {
        isShooting = true;
        
        while(target != null)
        {
            isShooting = true;
         while(target != null)
        {
             Debug.Log("총알 발사! 타겟: " + target.name);
            // ... 총알 생성 코드
            yield return new WaitForSeconds(shootDelay);
        }
             isShooting = false;

            // 총알 생성
            if (projectilePrefab != null && shootElement != null)
            {
                GameObject newBullet = Instantiate(projectilePrefab, shootElement.position, shootElement.rotation);

                // 생성된 총알의 Projectile 스크립트에 타겟과 데미지 정보 전달
                var bulletScript = newBullet.GetComponent<Projectile>();
                if (bulletScript != null)
                {
                    bulletScript.target = target;
                    bulletScript.damage = damage;
                }
            }
            
            yield return new WaitForSeconds(shootDelay);
        }
        
        isShooting = false;
    }

    // Catcher 타입 공격 코루틴
    IEnumerator CatcherAttackCoroutine()
    {
        isShooting = true;
        
        while (target != null)
        {
            if (anim != null)
            {
                anim.SetBool("Attack", true);
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // 애니메이션 재생 시간만큼 기다림
                anim.SetBool("Attack", false);
            }
            
            // 공격 애니메이션이 끝나면 데미지 입히기
            if (target != null)
            {
                var enemyHp = target.GetComponent<EnemyHp>();
                if (enemyHp != null)
                {
                    enemyHp.Dmg(damage);
                }
            }
            
            yield return new WaitForSeconds(shootDelay);
        }

        isShooting = false;
        // 타겟이 사라지면 초기 애니메이션으로 돌아가기
        if (anim != null)
        {
            anim.SetBool("Attack", false);
            anim.SetBool("T_pose", true);
        }
    }
    
    // 개발 편의를 위한 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}