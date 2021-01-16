using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution {

	public class ClimbingBrain : Brain {

		public const int NUMBER_OF_INPUTS = 7;
		public override int NumberOfInputs => NUMBER_OF_INPUTS+7;

		/*Inputs:
		* 
		* - distance from ground
		* - dx velocity
		* - dy velocity
		* - rotational velocity
		* - number of points touching ground
		* - creature rotation
		*/
		protected override void UpdateInputs (){

			// distance from ground
			Network.Inputs[0] = creature.DistanceFromGround();
			// horizontal velocity
			Vector3 velocity = creature.GetVelocity();
			Network.Inputs[1] = velocity.x;
			// vertical velocity
			Network.Inputs[2] = velocity.y;
			// rotational velocity
			Network.Inputs[3] = creature.GetAngularVelocity().z;
			// number of points touching ground
			Network.Inputs[4] = creature.GetNumberOfPointsTouchingGround();
			// Creature rotation
			Network.Inputs[5] = creature.GetRotation();

			     Network.Inputs[6] = Mathf.Sin(Time.time * Mathf.PI * 2);
            int index = 0;
            Vector3 centerOfMass = Vector3.zero;
            foreach (Joint joint in creature.joints)
            {
                centerOfMass += joint.center;

            }
            centerOfMass /= creature.joints.Count;

            foreach (Joint joint in creature.joints)
            {
                // Debug.Log("Joint" + (joint.center - velocity).magnitude);
                // Debug.Log("Joint" + (joint.center - velocity).magnitude);
                // Debug.Log("Joint" + (joint.center - velocity).magnitude);
                // Debug.Log("Joint" + (joint.center - velocity).magnitude);

                Network.Inputs[NUMBER_OF_INPUTS + index++] = (joint.center - centerOfMass).magnitude;
            }
		}
	}

}