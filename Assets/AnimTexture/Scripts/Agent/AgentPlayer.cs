namespace AnimTexture
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;

    public class AgentPlayer : MonoBehaviour
    {
        public NavMeshAgent agent;
        public Animator animator;
        public float moveSpeed = 4;

        int ID_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();

            //BaseComponentSystem.GetInstance<AgentPlayerSystem>().Add(this);
        }

        // Update is called once per frame
        public void Update()
        {
            ClickMove();
            //InputMove();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                animator.SetTrigger("IsDie");
            }

            if (Input.GetKeyDown(KeyCode.Z))
                animator.SetTrigger("IsAttack");

            animator.SetFloat(ID_MOVE_SPEED, agent.velocity.magnitude);
        }

        void ClickMove()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    agent.destination = hit.point;
                }
            }
        }

        void InputMove()
        {
            var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            dir *= moveSpeed;
            if (dir.magnitude > 0.05f)
            {
                agent.velocity = dir;
            }
            Debug.DrawRay(transform.position, agent.velocity);
        }
    }
}