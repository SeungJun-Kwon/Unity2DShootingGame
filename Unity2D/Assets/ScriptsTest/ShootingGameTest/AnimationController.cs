using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : StateMachineBehaviour
{
    PlayerController _playerController = null;

    enum Type { Instantiate, SetActive }

    [SerializeField] Type _type;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (_playerController == null)
            animator.gameObject.transform.root.gameObject.TryGetComponent(out _playerController);

        switch (_type)
        {
            case Type.Instantiate:
                Destroy(animator.gameObject);
                break;
            case Type.SetActive:
                _playerController.SetActiveObject(animator.gameObject.transform.name, false);
                break;
        }
    }
}
