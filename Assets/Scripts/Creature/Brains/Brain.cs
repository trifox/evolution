using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace Keiwando.Evolution
{

    [RequireComponent(typeof(Creature))]
    abstract public class Brain : MonoBehaviour
    { 
        public int creatureIndex = -1;
        public bool isTraining=false;
        public FeedForwardNetwork Network { get; protected set; }
        public bool drawNetwork = false;
        abstract public int NumberOfInputs { get; }
        virtual public int NumberOfOutputs => muscles.Length;

        private Muscle[] muscles;

        protected Creature creature;

        public virtual void Start()
        {
            this.creature = GetComponent<Creature>();
        }
        NeuralNetworkSettings settings;
        public void Init(NeuralNetworkSettings settings, Muscle[] muscles, float[] chromosome = null)
        {
            this.settings = settings;
            this.muscles = muscles;
            // if not in training mode, remove untrainable layers
            if(this.isTraining){
                //Debug.LogWarning("NORMAL TRAINING");
            this.Network = new FeedForwardNetwork(NumberOfInputs, NumberOfOutputs, settings, chromosome);
            }else{
               // Debug.LogWarning("JAWOLL UNTRAINED RESULT GENERATIOn");
                // remove untrainable layers, e.g. dropout
                List<NeuralNetworkLayerSettings> untrain=new List<NeuralNetworkLayerSettings>();
                foreach(var item in settings.layerDefinition){
                    if(item.type !=NeuralNetworkLayerSettings.LAYER_DROPOUT){
untrain.Add(item);
                    }else{
                        // replace with identity layers to keep layer count same
                        untrain.Add(NeuralNetworkLayerSettings.factorIdentity());
                    }
                }
                
            this.Network = new FeedForwardNetwork(NumberOfInputs, NumberOfOutputs, new NeuralNetworkSettings(untrain.ToArray()), chromosome);

            }

            //Debug.Log("Input and output signals are" + NumberOfInputs + ' ' + NumberOfOutputs);
        }
        int lastTime = 0;
        private bool jump = false;  // just an example
        void OnMouseDown()
        {
            print("Box Clicked!");
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                jump = true;


            int pos = (int)Input.mousePosition.x / 50;
            if (pos == creatureIndex)
            {
                print("Box Clicked!");
                Vector3 cpos = creature.joints[0].center;
                Debug.DrawLine(cpos, new Vector3(0, 0, 0), Color.red);
                Debug.DrawLine(new Vector3(cpos.x, 0, 0), new Vector3(cpos.x, 1000, 0), Color.gray);
                Debug.DrawLine(new Vector3(0, cpos.y, 0), new Vector3(10000, cpos.x, 0), Color.gray);
                Debug.DrawLine(new Vector3(0, pos, 0), new Vector3(10000, pos, 0), Color.green);
                if (Input.GetMouseButtonDown(1))
                {
                    Debug.Log("Killing creature" + creature);
                    creature.Alive = false;

                }

            }



        }
        // virtual public void onDrawGizmos()
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawSphere(this.transform.position, 100);
        // }



        virtual public void FixedUpdate()
        {


            {


                float[] outputs = new float[100];
                if (Network != null && creature.Alive)
                {
                    UpdateInputs();
                    outputs = Network.CalculateOutputs();
                    ApplyOutputs(outputs);

                }

                if (drawNetwork)
                {

                    for (int i = 0; i < Network.Inputs.Length; i++)
                    {

                        float inx = Network.Inputs[i];
                        //     float mean = Network.weights[Network.weights.Length - 1][0][0];
                        //     float div = Network.weights[Network.weights.Length - 1][0][1];
                        //     float a = Network.weights[Network.weights.Length - 1][0][2];
                        //     float b = Network.weights[Network.weights.Length - 1][0][3];
                        //    float normed = ((inx - mean) / div) * a + b;
                        Debug.DrawLine(new Vector3(0, i, 0), new Vector3(-inx, i, 0), Color.green, Time.fixedDeltaTime);
                    }
                    for (int i = 0; i < outputs.Length; i++)
                    {
                        Debug.DrawLine(new Vector3(0, i, 0), new Vector3(outputs[i] * 5.0f, i, 0), Color.red, Time.fixedDeltaTime);
                    }

                }
                // always draw a 5-unit colored line from the origin

                //Debug.Log("Rendering");
                //Debug.DrawLine(Vector3.zero, new Vector3(5, 10, 0), Color.red);
                float fact = 1f;
                float factx = 3f;

                if (drawNetwork && jump)
                {
                    int maxNodes = 0;
                    float[][] nextLayerInputs = new float[Network.getLayerCount()][];
                    for (int i = 0; i < Network.getLayerCount() - 1; i++)
                    {
                        if (Network.getLayerSize(i) > maxNodes)
                        {

                            maxNodes = Network.getLayerSize(i);

                        }
                    }
                    if (lastTime != Mathf.RoundToInt(Time.time))
                    {
                        lastTime = Mathf.RoundToInt(Time.time);


                        for (int i = 0; i < Network.getLayerCount() - 1; i++)
                        {

                            if(Network.layerDefinitions[i].type==NeuralNetworkLayerSettings.LAYER_DENSE){
                            // main loop each node layer

                            //Debug.DrawLine(new Vector3(i, 10, 0), new Vector3(i, Network.getLayerSize(i), 0), Color.red);

                            float lastSizeIn = 0;
                            float[] lastData = null;

                            nextLayerInputs[i + 1] = new float[Network.getLayerSize(i + 1)];
                            //Debug.LogWarning("Its comming like" + Network.getLayerSize(i + 1));
                            for (int l = 0; l < Network.getLayerSize(i + 1); l++)
                            {
                                for (int m = 0; m < Network.getLayerSize(i); m++)
                                {
                                    float[] data2 = Network.getWeight(i, m);

                                    //       nextLayerInputs[i + 1][l] += FeedForwardNetwork.Sigmoid(data2[l]);

                                }
                                nextLayerInputs[i + 1][l] /= Network.getLayerSize(i);
                                nextLayerInputs[i + 1][l] /= 2;
                            }
                            for (int k = 0; k < Network.getLayerSize(i); k++)
                            {

                                // this loop draws each node of this layer
                                float[] data = Network.getWeight(i, k);

                                float sizeoff = 0f; // the
                                for (int l = 0; l < data.Length; l++)
                                {
                                    float sigmoidValue = FeedForwardNetwork.Sigmoid(data[l]);
                                    sizeoff += sigmoidValue;
                                }
                                sizeoff /= data.Length;
                                sizeoff /= 2;
                                // if (i > 0)
                                // {
                                //     for (int l = 0; l < Network.getLayerSize(i - 1); l++)
                                //     {
                                //         float[] data2 = Network.getWeight(i - 1, l);
                                //         //  float[] data3 = Network.getWeight(i, l);

                                //         inValue += FeedForwardNetwork.Sigmoid(data2[k]);

                                //     }
                                //     inValue /= Network.getLayerSize(i - 1);
                                //     inValue /= 2;
                                // }



                                // count outgoing values
                                for (int l = 0; l < data.Length; l++)
                                {
                                    // float inValue = 0;
                                    // if (i > 0)
                                    // {
                                    //   /  try
                                    //   //  {
                                    //         inValue = nextLayerInputs[i][l];
                                    //    // }
                                    //     // catch (Exception e)
                                    //     // {
                                    //     //     Debug.LogWarning("Exception at " + i + '-' + k + '-' + l);
                                    //     // }
                                    // }
                                    // this loop effectively draws the current nodes in the layer and connects it to each of the output nodesList
                                    // float inValue = 0;
                                    // if (lastData != null)
                                    // {
                                    //     for (int r = 0; r < data.Length; r++)
                                    //     {

                                    //         inValue += FeedForwardNetwork.Sigmoid(lastData[l]);

                                    //     }

                                    // }
                                    float sigmoidValue = FeedForwardNetwork.Sigmoid(data[l]);
                                    int diff = maxNodes - Network.getLayerSize(i);
                                    int diff2 = maxNodes - Network.getLayerSize(i + 1);
                                    // Debug.Log("Layercount isa" + Network.weights[Network.getLayerCount()][i].Length);
                                    // Debug.Log("Layercount isb" + i);
                                    float bias = .25f * Network.weights[Network.getLayerCount()][0][i][l];


                                    Vector3 currNodePos = new Vector3(i * factx + sizeoff * factx, (k + diff / 2.0f) * fact, 0);
                                    Vector3 targetNodePos = new Vector3(factx + i * factx - nextLayerInputs[i + 1][l] * factx, bias * fact + (l + diff2 / 2.0f) * fact, 0);
                                    Vector3 targetNodePosxx = new Vector3(factx + i * factx, bias * fact + +(l + diff2 / 2.0f) * fact, 0);
                                    // Vector3 rowPos=new Vector3()
                                    Vector3 center = Vector3.Lerp(currNodePos, targetNodePos, 0.35f);
                                    Vector3 center2 = Vector3.Lerp(currNodePos, targetNodePos, .65f);


                                    // bias  
                                    //Debug.LogWarning("bias" + bias);
                                    // Debug.LogWarning("Its comming like2" + Network.weights[Network.getLayerCount() - 1].Length);
                                    // Debug.LogWarning("Its comming like3" + Network.weights[Network.getLayerCount() - 1][i + 1].Length);
                                    Debug.DrawLine(
                                       targetNodePos,
                                 new Vector3(targetNodePos.x, targetNodePos.y + bias * fact, targetNodePos.z), Color.magenta, 1.25f);
                                    // dividor
                                    Debug.DrawLine(
                                        new Vector3(i * factx, 0, 0),
                                    new Vector3(i * factx, maxNodes * fact, 0), Color.gray, 1.25f);
                                    // if (sigmoidValue >= 0)
                                    // {
                                    Debug.DrawLine(currNodePos, center, Color.Lerp(Color.red, Color.green, sigmoidValue), 1.25f);
                                    Debug.DrawLine(center2, targetNodePos, Color.Lerp(Color.red, Color.green, sigmoidValue), 1.25f);
                                    // }

                                    Debug.DrawLine(targetNodePos, targetNodePosxx, Color.red, 1.25f);


                                    if (i == Network.getLayerCount() - 2)
                                    {
                                        //  Debug.DrawLine(targetNodePos, targetNodePosxx, Color.cyan, 1.25f);
                                        Debug.DrawLine(targetNodePos, targetNodePosxx, Color.Lerp(Color.red, Color.green, outputs[l]), 1.25f);

                                    }
                                    if (i == 0)
                                    {
                                        Debug.DrawLine(
                                                            new Vector3(
                                                            i * factx,
                                                            (k + diff / 2.0f) * fact,
                                                             0),
                                                        new Vector3(
                                                            i * factx + sizeoff * factx,
                                                            (k + diff / 2.0f) * fact,
                                                              0), Color.Lerp(Color.red, Color.green, Network.Inputs[k]), 1.25f);

                                    }
                                    else
                                    {

                                        Debug.DrawLine(
                                            new Vector3(
                                            i * factx,
                                            (k + diff / 2.0f) * fact,
                                             0),
                                        new Vector3(
                                            i * factx + sizeoff * factx,
                                            (k + diff / 2.0f) * fact,
                                              0), Color.green, 1.25f);
                                    }


                                    // Debug.DrawLine(
                                    //     new Vector3(
                                    //         i * factx,
                                    //         (k + diff / 2.0f) * fact,
                                    //           0),
                                    //            new Vector3(
                                    //     i * factx - inValue * factx,
                                    //     (k + diff / 2.0f) * fact,
                                    //      0), Color.red, 1.25f);


                                }
                                lastSizeIn = sizeoff;
                                lastData = data;
                            }
                        }
                    }
                    }
                    // // draw end input markers
                    // for (int i = 0; i < Network.getLayerSize(Network.getLayerCount() - 1); i++)
                    // {
                    //     int diff = maxNodes - Network.getLayerSize(Network.getLayerCount() - 1);

                    //     Debug.DrawLine(new Vector3(
                    //                             Network.getLayerCount() * factx,
                    //                             (i + diff / 2.0f) * fact,
                    //                               0),
                    //                                new Vector3(
                    //                         i * factx - nextLayerInputs[Network.getLayerCount() - 1][i] * factx,
                    //                         (i + diff / 2.0f) * fact,
                    //                          0), Color.red, 1.25f);


                    // }
                }



            }
        }

        /// <summary>
        /// Load the Input values into the inputs vector.
        /// </summary>
        abstract protected void UpdateInputs();

        /// <summary>
        /// Takes the neural network outputs outputs and applies them to the 
        /// list of muscles. Calls the ApplyOutputToMuscle function for every output. 
        /// </summary>
        protected virtual void ApplyOutputs(float[] outputs)
        {
            if(outputs.Length>0){
                // network is initialised
Debug.Log("Output size is "+outputs.Length);
            for (int i = 0; i < muscles.Length; i++)
            {
                float output = float.IsNaN(outputs[i]) ? 0 : outputs[i];
                ApplyOutputToMuscle(output, muscles[i]);
            }
            }
        }

        /// <summary>
        /// Interprets the output and calls the respective function on the muscle.
        /// </summary>
        protected virtual void ApplyOutputToMuscle(float output, Muscle muscle)
        {

            // maps the output of the sigmoid function from [0, 1] to a range of [-1, 1]
            float percent = 2 * output - 1f;

            if (percent < 0)
                muscle.muscleAction = Muscle.MuscleAction.CONTRACT;
            else
                muscle.muscleAction = Muscle.MuscleAction.EXPAND;

            muscle.SetContractionForce(Math.Abs(percent));
        }

        // public abstract void EvaluateFitness();

        public string ToChromosomeString()
        {
            return Network.ToBinaryString();
        }


        private StringBuilder debugBuilder;
        private StringBuilder debugInputBuilder;
        protected virtual void DEBUG_PRINT_INPUTS()
        {

            // debugBuilder = new StringBuilder();
            if (debugInputBuilder == null) debugInputBuilder = new StringBuilder();
            var debugBuilder = debugInputBuilder;
            var inputs = Network.Inputs;

            debugBuilder.AppendLine("Distance from ground: " + inputs[0]);
            debugBuilder.AppendLine("Horiz vel: " + inputs[1]);
            debugBuilder.AppendLine("Vert vel: " + inputs[2]);
            debugBuilder.AppendLine("rot vel: " + inputs[3]);
            debugBuilder.AppendLine("points touchnig gr: " + inputs[4]);
            debugBuilder.AppendLine("rotation: " + inputs[5] + "\n");

            print(debugBuilder.ToString());
        }

        // protected virtual void DEBUG_PRINT_OUTPUTS() {

        // 	//var sBuilder = new StringBuilder();
        // 	if (debugBuilder == null) debugBuilder = new StringBuilder();

        // 	for (int i = 0; i < outputs[0].Length; i++) {
        // 		debugBuilder.AppendLine("Muscle " + (i+1) + " : " + outputs[0][i]);
        // 	}

        // 	print(debugBuilder.ToString());
        // }
    }
}