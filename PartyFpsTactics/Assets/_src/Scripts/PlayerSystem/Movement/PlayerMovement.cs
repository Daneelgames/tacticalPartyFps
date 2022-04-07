using System.Collections;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerMovement : MonoBehaviour
    {
        public bool narrativePlayer = false;
        [Header("Movement")]
        public LayerMask WalkableLayerMask;

        public Rigidbody rb;
        public float gravity = 5;
        public float jumpForce = 300;
        public float fallForceAcceleration = 20;
        public float fallDamageThreshold = 10;
        public int fallDamage = 100;
        
        public float walkSpeed = 5;
        public float runSpeed = 8;
        public float crouchSpeed = 2;
        public float crouchRunSpeed = 3.5f;
        public float acceleration = 1;
        public float groundCheckRadius = 0.25f;
        private Vector3 _targetVelocity;
        private Vector2 _movementInput;
        private Vector3 _moveVector;
        private Vector3 _prevVelocity;
        private Vector3 _resultVelocity;
        
        [SerializeField]
        private float _coyoteTimeMax = 0.5f;
        private float _coyoteTime = 0;

        [Header("Stamina")]
        public float stamina = 100;
        [HideInInspector]
        public float staminaMax = 100;
        [SerializeField] float runStaminaCost = 10;
        [SerializeField] float runCrouchStaminaCost = 5;
        [SerializeField] private float jumpStaminaCost = 10; 
        [SerializeField] private float idleStaminaRegen = 33;
        [SerializeField] private float moveStaminaRegen = 25; 
        
        [Header("Slopes")] 
        bool onSlope = false;
        private Vector3 slopeMoveDirection;
        private Vector3 slopeNormal;
        
        [SerializeField]
        private float _slopeRayHeight = 0.25f;
        
        [SerializeField]
        private float _slopeRayDistance = 0.5f;

        [Header("Crouching")] 
        public bool crouching = false;
        public CapsuleCollider topCollider;
        public CapsuleCollider bottomCollider;

        private Vector3 topColliderCenterStanding = new Vector3(0, 1.15f, 0);
        private float topColliderHeightStanding = 1.55731f;
        private Vector3 topColliderCenterCrouching = new Vector3(0, 0.3424235f, 0);
        private float topColliderHeightCrouching = 0.6848469f;
        
        private Vector3 bottomColliderCenterStanding = new Vector3(0, 0.5f, 0);
        private float bottomColliderHeightStanding = 1;
        private Vector3 bottomColliderCenterCrouching = new Vector3(0, 0.25f, 0);
        private float bottomColliderHeightCrouching = 0.5f;
        
        public Transform headTransform;
        
        private bool goingUpHill = false;
        public Transform rotator;
        public float rotatorSpeed = 10;
        public float minMaxRotatorAngle = 90;

        private bool _isDead = false;
        private bool canUseCoyoteTime = true;
        private float additinalFallForce;

        private GrindRail activeGrindRail;

        [ShowInInspector, ReadOnly]
        public MovementsState State { get; private set; } = new MovementsState();
        
        public Vector3 MoveVector => _moveVector;

        private float lastGroundedYPos = 0;

        private void Start()
        {
            staminaMax = stamina;
            SetCrouch(false);
        }

        private void Update()
        {
            if (_isDead)
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
                State.IsLeaning = false;
                return;
            }
        
            /*
            if (!LevelGenerator.Instance.levelIsReady)
                return;*/
            /*
            if (ProceduralCutscenesManager.Instance.InCutScene)
                return;*/

            HandleCrouch();
            HandleMovement();
            HandleStamina();
        }
        
        private void FixedUpdate()
        {
            if (_isDead)
                return;
            
            /*
            if (!LevelGenerator.Instance.levelIsReady)
                return;*/

            /*
            if (ProceduralCutscenesManager.Instance.InCutScene)
                return;*/
            
            GroundCheck();
            SlopeCheck();
            
            if (activeGrindRail == null)
                ApplyFreeMovement();
            else
                ApplyGrindRailMovement();
        }

        private float targetStaminaScaler = 1;
        void HandleStamina()
        {
            if (!State.IsRunning)
            {
                // MOVE
                targetStaminaScaler = idleStaminaRegen;
                if (State.IsMoving)
                    targetStaminaScaler = moveStaminaRegen;
                if (stamina < staminaMax)
                {
                    ChangeStamina(targetStaminaScaler * Time.deltaTime);
                }
                return;
            }
            
            // IF RUN
            if (!crouching)
                targetStaminaScaler = runStaminaCost;
            else
                targetStaminaScaler = runCrouchStaminaCost;
            ChangeStamina(-targetStaminaScaler * Time.deltaTime);
        }

        public void ChangeStamina(float offset)
        {
            stamina = Mathf.Clamp(stamina + offset, 0, staminaMax);
        }
        

        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                SetCrouch(!crouching);
        }
        
        private void SetCrouch(bool crouch)
        {
            if (narrativePlayer)
                return;
            
            if (crouching == crouch)
                return;

            if (!crouch)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out var hit, 1f, WalkableLayerMask))
                {
                    // found obstacle, can't stand
                    Debug.Log(hit.collider.name);
                    return;
                }
            }
            
            crouching = crouch;

            if (crouching)
            {
                topCollider.center =  topColliderCenterCrouching;
                topCollider.height = topColliderHeightCrouching;
                bottomCollider.center = bottomColliderCenterCrouching;
                bottomCollider.height = bottomColliderHeightCrouching;
            }
            else
            {
                topCollider.center =  topColliderCenterStanding;
                topCollider.height = topColliderHeightStanding;
                bottomCollider.center = bottomColliderCenterStanding;
                bottomCollider.height = bottomColliderHeightStanding;
            }
            
            Player.LookAround.SetCrouch(crouching);
        }
        
        private void HandleMovement()
        {
            float targetAngle;
            State.IsLeaning = true;
            
            if (Input.GetKey(KeyCode.D) && !Physics.CheckSphere(headTransform.position + headTransform.right * 1, 0.25f, WalkableLayerMask))
                targetAngle = -minMaxRotatorAngle;
            else if (Input.GetKey(KeyCode.A) && !Physics.CheckSphere(headTransform.position + headTransform.right * -1, 0.25f, WalkableLayerMask))
                targetAngle = minMaxRotatorAngle;
            else
            {
                targetAngle = 0;
                State.IsLeaning = false;
            }
            rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, targetAngle, rotatorSpeed * Time.deltaTime));

            int hor = (int)Input.GetAxisRaw("Horizontal");
            int vert = (int)Input.GetAxisRaw("Vertical");
        
            bool moveInFrame = hor != 0 || vert != 0;

            _movementInput = new Vector2(hor, vert);
            _moveVector = transform.right * _movementInput.x + transform.forward * _movementInput.y;
        
            _moveVector.Normalize();
        
            if (onSlope)
                _moveVector = Vector3.ProjectOnPlane(_moveVector, slopeNormal);
        
            // JUMP
            if (Input.GetKeyDown(KeyCode.Space) && (State.IsGrounded || _coyoteTime > 0))
            {
                SetGrindRail(null);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                StartCoroutine(CoyoteTimeCooldown());
                stamina = Mathf.Clamp(stamina - jumpStaminaCost, 0, staminaMax);
                _coyoteTime = 0;
            }
        
            float scaler = 1;
            
            if (stamina < 0.1f && State.IsGrounded)
                scaler = 0.33f;
            else if (Player.Interactor.carryingPortableRb)
                scaler = 0.5f;
            
            // RUNNING
            if (Input.GetKey(KeyCode.LeftShift))
            {
                State.IsRunning = moveInFrame;
                State.IsMoving = false;

                if (!crouching)
                    _targetVelocity = _moveVector * runSpeed * scaler;
                else
                    _targetVelocity = _moveVector * crouchRunSpeed * scaler;
            }
            else
            {
                State.IsMoving = moveInFrame;
                State.IsRunning = false;
            
                if (!crouching)
                    _targetVelocity = _moveVector * walkSpeed * scaler;
                else
                    _targetVelocity = _moveVector * crouchSpeed * scaler;
            }    
        
            if (goingUpHill)
                _targetVelocity += Vector3.up * 2;
        
            _resultVelocity = Vector3.Lerp(_prevVelocity, _targetVelocity, Time.deltaTime * acceleration);
            _prevVelocity = _resultVelocity;
        }


        private void SlopeCheck()
        {
            if (!State.IsGrounded)
            {
                onSlope = false;
                return;
            }
        
            if (Physics.Raycast(transform.position + Vector3.up * _slopeRayHeight, Vector3.down, out var hit, _slopeRayDistance, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.normal != Vector3.up)
                {
                    onSlope = true;
                    slopeNormal = hit.normal;
                }
                else
                    onSlope = false;
            }
            else
                onSlope = false;
        }

        private IEnumerator CoyoteTimeCooldown()
        {
            canUseCoyoteTime = false;
            yield return new WaitForSeconds(_coyoteTimeMax);
            
            canUseCoyoteTime = true;
        }

        private void GroundCheck()
        {
            if (Physics.CheckSphere(transform.position, groundCheckRadius, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (!State.IsGrounded)
                {
                    if (transform.position.y + fallDamageThreshold < lastGroundedYPos)
                    {
                        Player.Health.Damage(fallDamage, DamageSource.Environment);
                    }
                }
                
                
                lastGroundedYPos = transform.position.y;
                State.IsGrounded = true;
                additinalFallForce = 0;
                if (canUseCoyoteTime)
                    _coyoteTime = 0;
            }
            else
            {
                if (State.IsGrounded && canUseCoyoteTime)
                    _coyoteTime = _coyoteTimeMax;

                additinalFallForce += fallForceAcceleration * Time.deltaTime;
                State.IsGrounded = false;
                
                if (canUseCoyoteTime && _coyoteTime > 0)
                {
                    _coyoteTime -= Time.deltaTime;
                }
            }
        }
    
        private void ApplyFreeMovement()
        {
            float resultGravity = 0;
            if (!State.IsGrounded)
                resultGravity = gravity * additinalFallForce;
            else if (!onSlope)
                resultGravity = 1;

            rb.velocity = _resultVelocity + Vector3.down * resultGravity;
        }

        private void ApplyGrindRailMovement()
        {
            var targetTransform = activeGrindRail.GetTargetNode();
            if (targetTransform == null)
            {
                SetGrindRail(null);
                return;
            }
            
            rb.velocity = (targetTransform.position - transform.position).normalized * 10;
        }

        public void SetGrindRail(GrindRail rail)
        {
            activeGrindRail = rail;
        }
        
        
        public void Death(Transform killer = null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 1;
            rb.angularDrag = 10;
            _isDead = true;
        }
    }
}