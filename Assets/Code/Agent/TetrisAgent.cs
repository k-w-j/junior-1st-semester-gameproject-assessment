using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Code.Agent
{
    public class TetrisAgent : Unity.MLAgents.Agent
    {
        [SerializeField] private LayerMask whatIsFloor;
        [SerializeField] private LayerMask whatIsBlock;
        private Rigidbody _rigidbody;
        private bool _isGrounded;

        protected override void Awake()
        {
            base.Awake();
            
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        {
            // 맵 초기화 (쌓인 블록 모두 삭제, 에이전트 위치 리셋)
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            int moveAction = actions.DiscreteActions[0];
            int jumpAction = actions.DiscreteActions[1];

            if (moveAction == 1)
            {
                _rigidbody.velocity = new Vector3(-5f, _rigidbody.velocity.y, 0);
            }
            else if (moveAction == 2)
            {
                _rigidbody.velocity = new Vector3(5f, _rigidbody.velocity.y, 0);
            }

            if (jumpAction == 1 && _isGrounded)
            {
                _rigidbody.AddForce(Vector3.up * 8f, ForceMode.Impulse);
                _isGrounded = false;
            }

            AddReward(0.001f);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (((1 << other.gameObject.layer) & whatIsFloor) != 0)
            {
                _isGrounded = true;
            }
            else if (((1 << other.gameObject.layer) & whatIsBlock) != 0)
            {
                Vector3 normal = other.contacts[0].normal;
            
                if (normal.y > 0.7f)
                {
                    _isGrounded = true;
                    AddReward(0.2f);
                }
                else
                {
                    SetReward(-1.0f);
                    EndEpisode();
                }
            }
        }
    }
}
