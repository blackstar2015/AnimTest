using UnityEngine;

public class ApplyRootMotion : StateMachineBehaviour
{
    [SerializeField] private bool _applyRootMotion = false;
    [SerializeField] private float _rootMotionRotationTime = 0.2f;
    [SerializeField] private float _meleeResetTime = 0f;
    [SerializeField] private bool _canMove = true;
    [SerializeField] private bool _canShoot = true;
    [SerializeField] private bool _canMelee = true;
    [SerializeField] private int _visibleWeaponIndex = 0;

    private CustomCharacterMovement _movement;
    private CustomPlayerController _controller;
    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    // similar to start
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // we can get components on character GameObject like normal
        _movement = animator.GetComponent<CustomCharacterMovement>();
        _controller = animator.GetComponent<CustomPlayerController>();

        // set movement states
        _movement.CanMove = _canMove;
        animator.applyRootMotion = _applyRootMotion;

        // set control options
        _controller.CanShoot = _canShoot;
        _controller.CanMelee = _canMelee;
        //_controller.LookInCameraDirection = false;
        // show correct weapon if component in use
        // if(animator.TryGetComponent(out WeaponMeshController meshController))
        // {
        //     meshController.SetVisible(_visibleWeaponIndex);
        // }
    }

    // update
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // reset Melee trigger at start of animation
        if(_meleeResetTime > 0f && stateInfo.normalizedTime < _meleeResetTime)
        {
            animator.ResetTrigger("Unarmed");                  
        }
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // stop early if root motion disabled
        if (!_applyRootMotion) return;

        // manually re-activate root motion
        animator.ApplyBuiltinRootMotion();

        // manually control player rotation during root motion
        if (stateInfo.normalizedTime > _rootMotionRotationTime) return;
        Quaternion aimRotation = Quaternion.LookRotation(_movement.LookDirection);
        Quaternion rotation = Quaternion.Lerp(animator.transform.rotation, aimRotation, Time.deltaTime * 15f);
        animator.transform.rotation = rotation;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _controller.LookInCameraDirection = true;
        animator.applyRootMotion = false;
    }
}
