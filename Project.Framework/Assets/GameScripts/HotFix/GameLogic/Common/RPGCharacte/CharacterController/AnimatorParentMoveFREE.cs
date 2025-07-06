using UnityEngine;

namespace GameLogic
{
	public class AnimatorParentMoveFREE:MonoBehaviour
	{
		[HideInInspector] public Animator anim;
		[HideInInspector] public RPGCharacterMovementControllerFREE rpgCharacterMovementController;

		void OnAnimatorMove()
		{
			if(!rpgCharacterMovementController.canMove)
			{
				transform.parent.rotation = anim.rootRotation;
				transform.parent.position += anim.deltaPosition;
			}
		}
	}
}
