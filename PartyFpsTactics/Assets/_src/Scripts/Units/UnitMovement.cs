using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace MrPink.Units
{
    public class UnitMovement : MonoBehaviour
    {
        // TODO вынести конфиги передвижения в SO
        
        [SerializeField]
        [Range(1, 100)]
        public float _moveSpeed = 2;
        
        [SerializeField]
        [Range(1,10)]
        private float _turnSpeed = 4;
        
        [Range(1,100)]
        private float _runSpeed = 4;

        [SerializeField]
        private float _stopDistanceFollow = 1.5f;
        
        [SerializeField]
        private float _stopDistanceMove = 0;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private NavMeshAgent _agent;

        [SerializeField, ChildGameObjectsOnly, Required]
        private HumanVisualController _selfVisualController;

        private Vector3 _currentTargetPosition;
        private Vector3 _currentVelocity;
        private Transform _lookTransform;

        private void Start()
        {
            // TODO не делать этого в старте
            _lookTransform = new GameObject(gameObject.name + "LookTransform").transform;
        }

        private void Update()
        {
            _currentVelocity = _agent.velocity;
            _selfVisualController.SetMovementVelocity(_currentVelocity);
            _lookTransform.transform.position = transform.position;
        }

        public void Death()
        {
            _agent.enabled = false;
            this.enabled = false;
        }

        public void Resurrect()
        {
            _agent.enabled = true;
            this.enabled = true;
        }

        public void Run()
        {
            _agent.speed = _runSpeed;
        }
        
        public void LookAt(Vector3 targetPosition)
        {
            _lookTransform.LookAt(targetPosition, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookTransform.rotation, Time.deltaTime * _turnSpeed);  
        }


        public IEnumerator FollowTarget(Transform target)
        {
            while (true)
            {
                if (!_agent || !_agent.enabled)
                    yield break;
            
                AgentSetPath(target.position, true);
            
                _currentTargetPosition = target.position;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        public IEnumerator MoveToPosition(Vector3 target)
        {
            AgentSetPath(target, false);
            _currentTargetPosition = target;
        
            while (Vector3.Distance(transform.position, target) > 1)
                yield return new WaitForSeconds(0.5f);
        }
        
        public void AgentSetPath(Vector3 target, bool isFollowing)
        {
            if (this.enabled == false || _agent.enabled == false)
                return;
            
            var path = new NavMeshPath();
        
            transform.position = SamplePos(transform.position);
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            _agent.speed = _moveSpeed;
            _agent.stoppingDistance = isFollowing ? _stopDistanceFollow : _stopDistanceMove;
            _agent.SetPath(path);
        }
    
        private Vector3 SamplePos(Vector3 startPos)
        {
            if (NavMesh.SamplePosition(startPos, out var hit, 10f, NavMesh.AllAreas))
                startPos = hit.position;

            return startPos;
        }
        
        private void OnDestroy()
        {
            if (_lookTransform != null)
                Destroy(_lookTransform.gameObject);
        }
        
        
        #if UNITY_EDITOR

        public void TransferData(UnitAi source)
        {
            _selfVisualController = source.humanVisualController;
        }
        
        #endif
    }
}