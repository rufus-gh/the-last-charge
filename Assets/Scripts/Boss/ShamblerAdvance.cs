using UnityEngine;




public class ShamblerAdvance : MonoBehaviour
{
    public Transform target;
    public float speed = 1.6f;
    public float stopDistance = 1.2f;

    Animator animator;

    static readonly int SpeedId = Animator.StringToHash("Speed");

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        ZombieVoice.Add(gameObject, 1f, 0.55f);
        
        if (animator != null) animator.Play(0, 0, Mathf.Repeat(transform.position.x * 0.37f + transform.position.z * 0.21f, 1f));
    }

    void Update()
    {
        if (target == null) return;
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float distance = to.magnitude;
        if (distance > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 4f * Time.deltaTime);
        bool moving = distance > stopDistance;
        if (moving) transform.position += to.normalized * speed * Time.deltaTime;

        if (animator != null) animator.SetFloat(SpeedId, moving ? speed : 0f, 0.15f, Time.deltaTime);
    }
}
