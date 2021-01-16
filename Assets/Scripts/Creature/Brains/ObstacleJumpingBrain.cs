using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution
{

    public class ObstacleJumpingBrain : Brain
    {

        public const int MAX_NUMBER_OF_INPUT_JOINTS = 9;
        public const int NUMBER_OF_INPUTS = 9;
        public override int NumberOfInputs => NUMBER_OF_INPUTS + MAX_NUMBER_OF_INPUT_JOINTS;

        // private GameObject obstacle = null;
        private GameObject obstacleLeft = null;
        private GameObject obstacleRight = null;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            FindObstacleIfNeeded();
        }

        /*Inputs:
		* 
		* - distance from ground
		* - dx velocity
		* - dy velocity
		* - rotational velocity
		* - number of points touching ground
		* - creature rotation
		* - distance from obstacle
		*/
        protected override void UpdateInputs()
        {
            float cPosX = creature.GetXPosition();
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
            // distance from obstacle
            Network.Inputs[6] = obstacleLeft == null ? 0 : (cPosX - obstacleLeft.transform.position.x);
            Network.Inputs[7] = obstacleRight == null ? 0 : (obstacleRight.transform.position.x - cPosX);
            Network.Inputs[8] = Mathf.Sin(Time.time * Mathf.PI * 2*2);
            // Network.Inputs[5] = creature.GetDistanceFromObstacle(this.obstacle);
            // Network.Inputs[6] = creature.GetDistanceFromObstacle(this.obstacleLeft);
            // Network.Inputs[7] = creature.GetDistanceFromObstacle(this.obstacleRight);
            // Network.Inputs[7] = creature.GetDistanceFromObstacle(this.obstacleRight);
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
                //Debug.Log("Joints joint" + creature.joints.Count + "' index '" + index);
                // if (NUMBER_OF_INPUTS + index < NUMBER_OF_INPUTS + MAX_NUMBER_OF_INPUT_JOINTS)
                // {
                    Network.Inputs[MAX_NUMBER_OF_INPUT_JOINTS + index++] = (joint.center - centerOfMass).magnitude;
                // }
            }

        }

        private void FindObstacleIfNeeded()
        {
            // if (obstacleLeft != null) return;
            // if (obstacleRight != null) return;
            int playbackCreatureLayer = LayerMask.NameToLayer("PlaybackCreature");
            int dynamicForegroundLayer = LayerMask.NameToLayer("DynamicForeground");
            int playbackDynamicForegroundLayer = LayerMask.NameToLayer("PlaybackDynamicForeground");
            var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            float minDistance = float.MaxValue;
            float minDistanceRight = float.MaxValue;
            float minDistanceLeft = float.MaxValue;
            float cPosX = creature.GetXPosition();
            // this.obstacle = null;
            this.obstacleLeft = null;
            this.obstacleRight = null;
            // Debug.LogWarning("FOUND OBNSTACLES creature xpos:" + this.creature.GetXPosition() + " obstacles:" + obstacles.Length);
            foreach (var obstacle in obstacles)
            {
                // Debug.LogWarning("FOUND OBNSTACLESxxx" + obstacle.transform.position);
                bool correctObstacle = false;

                if (!obstacle.activeSelf) continue;
                if (this.gameObject.layer == playbackCreatureLayer)
                {
                    correctObstacle = obstacle.layer == playbackDynamicForegroundLayer;
                }
                else
                {
                    correctObstacle = obstacle.layer == dynamicForegroundLayer;
                }
                if (correctObstacle)
                {
                    //Debug.LogWarning("Obstacle found" + obstacle.transform.position.x);
                    //Debug.LogWarning("Obstacle distance is " + creature.GetDistanceFromObstacle(obstacle));
                    if ((cPosX > obstacle.transform.position.x) && creature.GetDistanceFromObstacle(obstacle) < minDistanceRight)
                    {
                        //  Debug.LogWarning("xxxxFOUND CLOSER" + obstacle.transform.position.x);
                        this.obstacleRight = obstacle;
                        minDistance = creature.GetDistanceFromObstacle(obstacle);
                    }

                    if ((cPosX < obstacle.transform.position.x) && creature.GetDistanceFromObstacle(obstacle) < minDistanceLeft)

                    {
                        this.obstacleLeft = obstacle;
                        minDistanceLeft = creature.GetDistanceFromObstacle(obstacle);

                    }
                    // break;
                }
            }
        }
    }

}