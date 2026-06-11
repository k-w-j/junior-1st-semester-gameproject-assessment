using Code.Blocks;
using TMPro;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Agent
{
    using Unity.MLAgents;
    public class TetrisAgent : Agent
    {
        [Header("References")]
        [SerializeField] private BlockSpawner blockSpawner;

        [SerializeField] private TextMeshProUGUI wer;

        [Header("Layers")]
        [SerializeField] private LayerMask whatIsFloor;
        [SerializeField] private LayerMask whatIsBlock;
        [SerializeField] private LayerMask whatIsWall;

        [Header("Move Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 8f;

        [Header("Rewards")]
        [SerializeField] private float hitPenalty = -1f;
        [SerializeField] private float surviveBlockReward = 0.2f;
        [SerializeField] private bool isAuto;

        private Rigidbody _rigidbody;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private bool _isGrounded;
        
        public override void Initialize()
        {
            base.Initialize();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        {
            if (blockSpawner != null)
            {
                blockSpawner.ResetEnvironment();
            }

            transform.SetPositionAndRotation(_startPosition, _startRotation);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _isGrounded = false;
        }
    
        public override void OnActionReceived(ActionBuffers actions)
        {
            int moveAction = actions.DiscreteActions[0]; // 0: stop, 1: left, 2: right
            int jumpAction = actions.DiscreteActions[1]; // 0: no jump, 1: jump

            Vector3 velocity = _rigidbody.velocity;

            if (moveAction == 1)
                velocity.x = -moveSpeed;
            else if (moveAction == 2)
                velocity.x = moveSpeed;
            else
            {
                velocity.x = 0f;
            }

            _rigidbody.velocity = new Vector3(velocity.x, velocity.y, 0f);

            if (jumpAction == 1 && _isGrounded)
            {
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                _isGrounded = false;
            }
            
            AddReward(0.001f); 
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = Input.GetKey(KeyCode.A) ? 1 :
                                 Input.GetKey(KeyCode.D) ? 2 : 0;
            discreteActions[1] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.position.x);
            sensor.AddObservation(_rigidbody.velocity.x);
            sensor.AddObservation(_rigidbody.velocity.y);
            sensor.AddObservation(_isGrounded ? 1f : 0f);

            Transform dangerBlock = GetNearestFallingBlock();
            if (dangerBlock != null)
            {
                Vector3 relativePos = dangerBlock.position - transform.position;
                sensor.AddObservation(relativePos.x);
                sensor.AddObservation(relativePos.y);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        private Transform GetNearestFallingBlock()
        {
            if (blockSpawner.SpawnedBlocks.Count == 0) return null;

            Transform nearest = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < blockSpawner.SpawnedBlocks.Count; i++)
            {
                Transform block = blockSpawner.SpawnedBlocks[i].transform;

                float vertical = block.position.y - transform.position.y;
                if (vertical < -1f) continue;

                float horizontal = Mathf.Abs(block.position.x - transform.position.x);
                float score = vertical + horizontal * 0.5f;

                if (score < bestScore)
                {
                    bestScore = score;
                    nearest = block;
                }
            }

            return nearest;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (((1 << other.gameObject.layer) & whatIsFloor) != 0)
            {
                _isGrounded = true;
                return;
            }

            if (((1 << other.gameObject.layer) & whatIsWall) != 0)
            {
                AddReward(-5f); 
            }

            if (((1 << other.gameObject.layer) & whatIsBlock) != 0)
            {
                Vector3 normal = other.contacts[0].normal;

                if (normal.y > 0.2f) 
                {
                    _isGrounded = true;
                    AddReward(5f);
                }
                else if (normal.y < -0.7f) 
                {
                    AddReward(-10.0f);
                    
                    if (isAuto)
                    {
                        EndEpisode();
                    }
                    else
                    {
                        other.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (((1 << other.gameObject.layer) & whatIsFloor) != 0)
            {
                _isGrounded = true;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (((1 << other.gameObject.layer) & whatIsFloor) != 0)
            {
                _isGrounded = false;
            }
        }
    }
}
