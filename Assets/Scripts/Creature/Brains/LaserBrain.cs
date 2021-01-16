using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution
{

    public class LaserBrain : Brain
    {

        public const int NUMBER_OF_INPUTS = 8;
        public override int NumberOfInputs => NUMBER_OF_INPUTS + 11;

        // Update is called once per frame

        void Update()
        { }



        private const float MAX_HEIGHT = 20f;

        /*Inputs:
        * 
        * - distance from ground
        * - dx velocity
        * - dy velocity
        * - rotational velocity
        * - number of points touching ground
        * - Creature rotation
        */
        protected override void UpdateInputs()
        {

            // Debug.Log("LaserBrain");
            // Debug.Log("LaserBrain");
            // Debug.Log("LaserBrain");
            // Debug.Log("LaserBrain"); 

            //creature.Alive = Network.Inputs[6] != creature.GetHighestPoint().y;
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
            Network.Inputs[6] = creature.GetHighestPoint().y;
            Network.Inputs[7] = Mathf.Sin(Time.time * Mathf.PI * 2);
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