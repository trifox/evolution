using UnityEngine;

namespace Keiwando.Evolution
{

    /// Get as far to the right as possible.

    public class LaserObjectiveTracker : ObjectiveTracker
    {

        private const float MAX_HEIGHT = 50f;

        private float maxHeightJumped = 0f;
        private float maxWeightedAverageHeight = 0f;
        /// <summary>
        /// The optimal distance a "perfect" creature could travel in 10 seconds.
        /// Quite arbitrarily chosen.
        /// </summary> 
	    private const int MAX_DISTANCE = 10;

        public override float EvaluateFitness(float simulationTime)
        {
            //Debug.Log("Hello");
            // return (creature.GetXPosition() - creature.InitialPosition.x) / (MAX_DISTANCE * simulationTime);
            return  FeedForwardNetwork.TanH(creature.GetXPosition() - creature.InitialPosition.x)  ;
        }

 
    }
}