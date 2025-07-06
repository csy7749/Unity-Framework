using System.Collections;
using UnityEngine;

namespace GameLogic
{
    //Placeholder functions for Animation events.
    public class RPGCharacterAnimatorEventsFREE:MonoBehaviour
    {
		[HideInInspector] public RPGCharacterControllerFREE rpgCharacterController;

		public void Hit()
        {
        }

        public void Shoot()
        {
        }

        public void FootR()
        {
        }

        public void FootL()
        {
        }

        public void Land()
        {
        }

		public void WeaponSwitch()
		{
			if(rpgCharacterController.rpgCharacterWeaponControllerFREE != null)
			{
				rpgCharacterController.rpgCharacterWeaponControllerFREE.WeaponSwitch();
			}
		}
	}
}