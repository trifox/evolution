using System;
using System.Text;
using UnityEngine;
public class FeedForwardNetwork : IChromosomeEncodable<string>, IChromosomeEncodable<float[]>
{//WithInputNormalization
    private struct Constants
    {
        public static float MIN_WEIGHT = -3.0f;
        public static float MAX_WEIGHT = 3.0f;
    }

    public int NumberOfLayers { get { return layerDefinitions.Length; } }
    public int NumberOfInputs { get { return layerDefinitions[0].size; } }
    public int NumberOfOutputs { get { return layerDefinitions[layerDefinitions.Length - 1].size; } }

    public float[] Inputs { get; set; }

    public NeuralNetworkLayerSettings[] layerDefinitions;

    public float[][][][] weights;

    // Optimization
    // private float[][] tempResults;
    private float[][] memory;
    private StringBuilder builder;
    public int getLayerCount()
    {
        return layerDefinitions.Length;
    }

    public int getLayerSize(int i)
    {
        return layerDefinitions[i].size;
    }

    public float[] getWeight(int layer, int i)
    {
        // Debug.Log("Layer info" + layer + '-' + i);
        // Debug.Log("Layer info" + weights.Length);
        // Debug.Log("Layer info" + weights[layer].Length);
        // Debug.Log("Layer infoxx" + weights[layer][i].Length);
        // Debug.Log("Layer infoxx2" + String.Join("  ", weights[layer][i]));
        return weights[layer][0][i];
    }

    private FeedForwardNetwork(int inputCount, int outputCount, NeuralNetworkSettings settings)
    {
        // Setup layerSizes
        int layerCount = settings.NumberOfIntermediateLayers + 2;
        this.layerDefinitions = new NeuralNetworkLayerSettings[layerCount];

        // input layer 
        this.layerDefinitions[0] = new NeuralNetworkLayerSettings(
            inputCount,
         NeuralNetworkLayerSettings.LAYER_DENSE,
         NeuralNetworkLayerSettings.I_NORMALIZED,
         NeuralNetworkLayerSettings.O_BIASED,
          NeuralNetworkLayerSettings.A_TANH);

        // exit layer
        this.layerDefinitions[layerCount - 1] = new NeuralNetworkLayerSettings(
            outputCount, NeuralNetworkLayerSettings.LAYER_DENSE,
         NeuralNetworkLayerSettings.I_NORMALIZED,
         NeuralNetworkLayerSettings.O_BIASED,
          NeuralNetworkLayerSettings.A_SIGMOID);



        // Debug.Log("LayerHello there " + inputCount + "->hidden(" + layerCount + ")-> " + outputCount);
        // Debug.Log("RecommendationsLayerHello there " + inputCount + "->hidden(" + layerCount + ")-> " + outputCount);
        int connections = 0;
        int nodes = 0;
        int lastCount = 0;

        for (int i = 1; i < layerCount - 1; i++)
        {
            layerDefinitions[i] = settings.layerDefinition[i - 1];
        }
        Debug.Log("Stats Network");
        foreach (NeuralNetworkLayerSettings layerSize in this.layerDefinitions)
        {
            nodes += layerSize.size;
            connections += layerSize.size * lastCount;
            lastCount = layerSize.size;
        }
        Debug.Log("Stats Network nodeCount=" + nodes + "Connections: " + connections);
        Debug.Log("Hidden Layers should be no more than inputCount*2=" + (inputCount * 2));
        Debug.Log("Hidden Layers should be no more than (inputCount+outputCount)*2/3=" + (inputCount * (2.0 / 3.0) + outputCount));
        Debug.Log("Hidden Layers should be between  " + inputCount + " and " + outputCount);

        this.Inputs = new float[inputCount];
    }

    public FeedForwardNetwork(
        int inputCount,
        int outputCount,
        NeuralNetworkSettings settings,
        float[] weights
    ) : this(inputCount, outputCount, settings)
    {

        // Create weights arrays
        if (weights == null || weights.Length == 0)
        {
            SetupRandomWeights();
        }
        else
        {
            this.weights = WeightsFromFloatArray(weights);
        }

        // InitializeTempResults();
    }

    public FeedForwardNetwork(
        int inputCount,
        int outputCount,
        NeuralNetworkSettings settings,
        string encoded = ""
    ) : this(inputCount, outputCount, settings)
    {

        // Create weights arrays
        if (string.IsNullOrEmpty(encoded))
        {
            SetupRandomWeights();
        }
        else
        {
            // Decode Weights
        //    this.weights = WeightsFromBinaryString(encoded);
        }

        // InitializeTempResults();
    }

    // private void InitializeTempResults()
    // {
    //     // this.tempResults = new float[layerDefinitions.Length][];
    //     // for (int i = 0; i < layerDefinitions.Length - 1; i++)
    //     // {
    //     //     // Debug.Log("XXXx" + i + " ' layers:' " + layerSizes.Length);
    //     //     // Debug.Log("XXXq" + i + ' ' + weights[i]);
    //     //     // Debug.Log("XXXw" + i + ' ' + weights[i].Length);
    //     //     // Debug.Log("XXXa" + i + ' ' + weights[i][0].Length);
    //     //     if (weights[i].Length > 0)
    //     //     {
    //     //         int cols = weights[i][0].Length;
    //     //         this.tempResults[i] = new float[cols];
    //     //     }
    //     //     else
    //     //     {

    //     //     }
    //     // }
    // }


    /** how is convolution implemented ?

    to match node count equals output in convolution the process is:

    1st convolute inputs
    2nd max pool to number of output

    so, the convolution is always combined with a max pooling of number of nodes

    **/

    private void SetupRandomWeights()
    {

        this.weights = new float[layerDefinitions.Length + 2][][][];
        memory=new float[layerDefinitions.Length][];
        for (int i = 0; i < layerDefinitions.Length - 1; i++)
        {
          //  Debug.Log("Creating layer"+i+"' type'"+layerDefinitions[i].type+" LAYER NODE COUNT IS"+layerDefinitions[i].size);
            switch (layerDefinitions[i].type)

            {
                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    {
                        weights[i] = new float[1][][];
                        weights[i][0] = MatrixUtils.CreateRandomMatrix2D(
                     layerDefinitions[i].size, layerDefinitions[i].kernelSize + 1 /* the +1 is for the bias */,
                    1f, 1f
                 );
                        break;
                    }
                case NeuralNetworkLayerSettings.LAYER_DROPOUT:
                case NeuralNetworkLayerSettings.LAYER_IDENTITY:
                case NeuralNetworkLayerSettings.LAYER_MAX_POOL:
                case NeuralNetworkLayerSettings.LAYER_ZERO_PAD:
                    {
                        weights[i] = new float[0][][];
                        break;
                    }
                case NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT:  
{
    // init memory for this layer, a memory entry for each node in the layer
    //memory[i]=new float[layerDefinitions[i].memorySize*layerDefinitions[i].size];
 int outputSize=layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size);
                        weights[i] = new float[4][][];
                        // 0 = input matrix
                        // 1 = memory matrix
                        // 2 = output matrix
                        // 3 = initial memory vector

                        // input weight matrix input*input
                        weights[i][0] = MatrixUtils.CreateRandomMatrix2D(
                        layerDefinitions[i].size,  layerDefinitions[i].size,
                        Constants.MIN_WEIGHT, Constants.MAX_WEIGHT                    );
                        // memory weight matrix memory*memory 
                        weights[i][1] = MatrixUtils.CreateRandomMatrix2D(
                        outputSize, outputSize,
                        Constants.MIN_WEIGHT, Constants.MAX_WEIGHT                    );
                        // output matrix (memory+input)*outputsize
                        weights[i][2] = MatrixUtils.CreateRandomMatrix2D(
                        layerDefinitions[i].size+outputSize, layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size),
                        Constants.MIN_WEIGHT, Constants.MAX_WEIGHT                    );
                        // initial memory
                        weights[i][3] = MatrixUtils.CreateRandomMatrix2D(
                       1, outputSize,
                        Constants.MIN_WEIGHT, Constants.MAX_WEIGHT                    );
                        break;

}
                    // for rnns we need to encode 3 matrices into the weights
                    // used for
                    // w1 = matrix incoming 
                    // w2 = matrix memory
                    // w3 = matrix outgoing 
                    // tbd unclear how to organize multi dimensional weights, plan would be to extend dimension of weights :D
 
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    {
                        weights[i] = new float[1][][];
                        weights[i][0] = MatrixUtils.CreateRandomMatrix2D(
                        layerDefinitions[i].size, layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size),
                        Constants.MIN_WEIGHT, Constants.MAX_WEIGHT
                    );
                    
            //Debug.Log(i+"DENSE LAYER "+weights[i][0].Length+'/'+weights[i][0][0].Length);
                                            break;
                    }
            }
        }
        // put in the last layer the weights matrix/ to reuse all of the weigths storing and restoring functionality
        // biases
        weights[layerDefinitions.Length] = new float[1][][];
        weights[layerDefinitions.Length][0] = new float[layerDefinitions.Length - 1][];
        for (int i = 0; i < layerDefinitions.Length - 1; i++)
        {
            switch (layerDefinitions[i].type)
            {
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                default:
                    weights[layerDefinitions.Length][0][i] = new float[layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size)];
                    for (int k = 0; k < weights[layerDefinitions.Length][0][i].Length; k++)
                    {
                        weights[layerDefinitions.Length][0][i][k] = UnityEngine.Random.Range(-1f, 1f);
                    }
                    break;
            }

        }

        // put in the  layer the input weight normalization 
        weights[layerDefinitions.Length + 1] = new float[1][][];
        weights[layerDefinitions.Length + 1][0] = new float[layerDefinitions.Length][];

        for (int i = 0; i < layerDefinitions.Length; i++)
        {
            // encode a+b in an array of twice the length, to store learnable input normalization
            weights[layerDefinitions.Length + 1][0][i] = new float[4];

            weights[layerDefinitions.Length + 1][0][i][0] = 0;
            weights[layerDefinitions.Length + 1][0][i][1] = 1;
            weights[layerDefinitions.Length + 1][0][i][2] = 1;
            weights[layerDefinitions.Length + 1][0][i][3] = 0;

        }
        // put in layer type layer weight, 0..3 for the layer type 
        // 0-> layer type => dense|normalized| biased 
        // 1-> layer activation -> sigmoid|tanH|relu|leakyRelu 
        // weights[layerDefinitions.Length + 2] = new float[layerDefinitions.Length][];
        // for (int i = 0; i < layerDefinitions.Length; i++)
        // {
        //     // encode a+b in an array of twice the length, to store learnable input normalization
        //     weights[layerDefinitions.Length + 2][i] = new float[3];
        //     // layer config
        //     weights[layerDefinitions.Length + 2][i][0] = NeuralNetworkLayerSettings.I_NORMALIZED;//input
        //     weights[layerDefinitions.Length + 2][i][1] = NeuralNetworkLayerSettings.O_BIASED;// outputType
        //     weights[layerDefinitions.Length + 2][i][2] = NeuralNetworkLayerSettings.A_TANH; //activation

        // }
    }

    public   float[] RNN(int index,NeuralNetworkLayerSettings settings,float[][][] weights, float[] input, float[] result = null)
    {

          if (memory[index] == null)
        {
// Debug.Log("INITIALISING MEMORY1"+index); 
// Debug.Log("INITIALISING MEMORY1"+weights[3].Length); 
// Debug.Log("INITIALISING MEMORY1"+memory[index]); 
              memory[index] = weights[3][0]; 
// Debug.Log("INITIALISING MEMORY2"+memory[index].Length+'/'+index); 
        }
          if (result == null)
        {
            // initialise with start memory vektor
          
            result = new float[settings.getInputSize(settings.size)];
        }
// Debug.Log("HI THERE< INPUT LENGTH "+input.Length);
// Debug.Log("HI THERE< weigh0 LENGTH "+weights[0].Length+'/'+weights[0][0].Length); 
// Debug.Log("HI THERE< weigh1 LENGTH "+weights[1].Length+'/'+weights[1][0].Length); 
// Debug.Log("HI THERE< weigh2 LENGTH "+weights[2].Length+'/'+weights[2][0].Length); 
// Debug.Log("HI THERE< weigh3 LENGTH "+weights[3].Length+'/'+weights[3][0].Length); 
// Debug.Log("HI THERE< weigh4 LENGTH "+memory[index].Length+'/'+index); 

// muliply input
    float[]  inputW = MatrixUtils.MatrixProductTranspose(weights[0], input, null);

// Debug.Log("HI THERE1");

// muliply current memoryh
// Debug.Log("Multiplying memory with weightmatrix");
// Debug.Log("Multiplying memory with weightmatrix"+weights.Length);
// Debug.Log("Multiplying memory with weightmatrix"+weights[1].Length );
// Debug.Log("Multiplying memory with weightmatrix"+weights[1].Length+'x'+weights[1][0].Length);
// Debug.Log("Memory length is "+memory[index].Length);
    float[]  memoryW = MatrixUtils.MatrixProductTranspose(weights[1], memory[index], null);
// Debug.Log("HI THERE2");
// concatenate input and memoryW
float[] interm=new float[inputW.Length+memoryW.Length];
for(int i=0;i<inputW.Length; i++){
    interm[i]=inputW[i];
}
for(int i=0;i<memoryW.Length; i++){
    interm[inputW.Length+i]=memoryW[i];
}

// multiply interm now with output matrix1
   ApplyActivation(interm, TanH);
          result = MatrixUtils.MatrixProductTranspose(weights[2], interm, null);
// Debug.Log("HI THERE3"+String.Join(",",result));
          // store result in memory
          memory[index]=result;
        return result;
    }


    private float[] identity(NeuralNetworkLayerSettings settings, float[] input = null, float[] result = null)
    { 
      
        if (result == null)
        {
            result = new float[input.Length];
        }  
            for (int i = 0; i < input.Length; i++)
            {
              
            result[i] = input[i];

         }
        
        return result;
    }

    private float[] maxPool1D(NeuralNetworkLayerSettings settings, float[] input = null, float[] result = null)
    {
        int index = 0;
        int resultIndex = 0;
        if (result == null)
        {
            result = new float[settings.getOutputDimension(input.Length)];
        }
        do
        {

            float max = float.NegativeInfinity;
            
            for (int i = 0; i < settings.poolSize; i++)
            {
                if(index+i<input.Length){
                if (input[index + i] > max)
                {
                    max = input[index + i];
                }
            }
            }

            // Debug.Log("RESULT MAXPool"+resultIndex+"/"+result.Length);
            // Debug.Log("RESULT MAXPool"+index+"/"+input.Length);
            result[resultIndex++] = max;

            index += settings.poolStride;   
        } while (index < settings.getOutputDimension(input.Length)-settings.poolSize);
        return result;
    }
    private float[] dropout(NeuralNetworkLayerSettings settings, float[] input = null, float[] result = null)
    {
        if (result == null)
        {
            result = new float[settings.getOutputDimension(input.Length)];
        }
        for (int i = 0; i < result.Length; i++)
        {

            if (UnityEngine.Random.Range(0f, 1f) > settings.dropoutRate)
            {
                // Inputs not set to 0 are scaled up by 1/(1 - rate) such that the sum over all inputs is unchanged.
                result[i] = input[i]*(1/(1-settings.dropoutRate));
            }
            else
            {
                result[i] = 0;
            }

        }
        return result;
    }



    private float[] zeroPad(NeuralNetworkLayerSettings settings, float[] input = null, float[] result = null)
    {
        if (result == null)
        {
            result = new float[settings.getOutputDimension(input.Length)];
        }
        for (int i = 0; i < settings.padding; i++)
        {
            result[i] = 0;
            result[result.Length - i - 1] = 0;
        }
        for (int i = 0; i < input.Length; i++)
        {
            result[i + settings.padding-1] = input[i];
        }
        return result;
    }
    private float[] convolution1D(NeuralNetworkLayerSettings settings, float[][] kernel, float[] input = null, float[] result = null)
    {
        /** 
        create convolution for each node a kernel exists
        the output of each node is input convoluted and perhaps rescaled depending on paddzeroes
        and after that max pooling is used to create the output

        */
        // Debug.LogWarning("Convolution of input" + input.Length + " " + kernel.Length + "creating output of dimension" + settings.getOutputDimension(input.Length));
        int resultIndex = 0;

        result = new float[settings.getOutputDimension(input.Length)];

        for (int nodeLayer = 0; nodeLayer < settings.size; nodeLayer++)
        {
            int index = 0;
            do
            {
                float kernelised = 0;

                for (int i = 0; i < settings.kernelSize; i++)
                {
                    // Debug.LogWarning("KERNELa xxxx nodes count real - "+kernel.Length);
                    // Debug.LogWarning("KERNELa nodelayers - "+nodeLayer+"' index'"+index+'i'+i+" kernel size for node"+kernel[nodeLayer].Length+" INPUT LENGTH IS"+input.Length+"kernselize settings is "+settings.kernelSize);
                    // // Debug.LogWarning("KERNELa output index - "+index);
                    // // Debug.LogWarning("KERNEL akernel item - "+i);
                    // Debug.LogWarning("KERNELa kernelsize - "+kernel[nodeLayer].Length);
                    // Debug.LogWarning("KERNELa kernelvalue - "+kernel[nodeLayer][i]);
                    // Debug.LogWarning("----------------------");
                    kernelised += input[index + i] * kernel[nodeLayer][i];
                }
                // add bias from last element in weights array
                //                    Debug.LogWarning("---------------------- "+kernel[nodeLayer].Length+"i"+ input.Length+'r'+result.Length+' '+resultIndex+' '+kernel[nodeLayer][settings.kernelSize]);
                result[resultIndex] = kernelised + kernel[nodeLayer][settings.kernelSize];
                //nodeLayer++;
                resultIndex++;
                index += settings.convStride;
            } while (index < input.Length - settings.kernelSize);
        }
        // Debug.Log("'Result dimensions would be then'" + resultIndex);
        return result;


    }
    public float[] CalculateOutputs()
    {

        float[] result = Inputs;

        for (int i = 0; i < layerDefinitions.Length - 1; i++)
        {

            float[][][] layerWeights = weights[i];
            //      float[] layerBiases = biases[i];
           // float[] tempResultVec = tempResults[i];


            int weightType = layerDefinitions[i].type;
            int activationType = layerDefinitions[i].activationType;

            // fixed layer configs 
            int inputType = layerDefinitions[i].inputType;
            int outputType = layerDefinitions[i].outputType;
            // activationType = A_TANH;

            

            // Debug.Log("multiplying alayer step " + i + " input size"+result.Length);

            switch (weightType)
            {
                case NeuralNetworkLayerSettings.LAYER_IDENTITY:
                    //Debug.Log("DOING LAYER_DENSE" + layerWeights.Length + '/' + layerWeights[0].Length);
                    result = identity(layerDefinitions[i], result, null);
                    break;
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    // Debug.Log(i+"DOING LAYER_DENSE current v" +result.Length+'-'+ layerWeights[0].Length + '/' + layerWeights[0][0].Length);
                    // Debug.Log(i+"DOING LAYER_DENSE matrix size is "+layerWeights[0].Length+'/'+layerWeights[0][0].Length);
                    result = MatrixUtils.MatrixProductTranspose(layerWeights[0], result, null);
                    break;
                case NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT:
                   // Debug.Log("DOING LAYER_DENSE" + layerWeights.Length + '/' + layerWeights[0].Length);
                    result = RNN(i,layerDefinitions[i],layerWeights,result);
                    break;
                case NeuralNetworkLayerSettings.LAYER_MAX_POOL:
                    //Debug.Log("DOING LAYER_DENSE" + layerWeights.Length + '/' + layerWeights[0].Length);
                    result = maxPool1D(layerDefinitions[i], result, null);
                    break;

                case NeuralNetworkLayerSettings.LAYER_DROPOUT:
                    //Debug.Log("DOING LAYER_DENSE" + layerWeights.Length + '/' + layerWeights[0].Length);
                    result = dropout(layerDefinitions[i], result, null);
                    break;
                case NeuralNetworkLayerSettings.LAYER_ZERO_PAD:
                    //Debug.Log("DOING LAYER_DENSE" + layerWeights.Length + '/' + layerWeights[0].Length);
                    result = zeroPad(layerDefinitions[i], result, null);
                    break;
                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    result = convolution1D(layerDefinitions[i], layerWeights[0], result, null);
                    // perform 1d convolution
                    //   result = MatrixUtils.MatrixProductTranspose(layerWeights, result, tempResultVec);
                    break;
            }
            
              //Debug.Log("multiplying alayer step " + i + " output size"+result.Length);
 if ((inputType & NeuralNetworkLayerSettings.O_BIASED) > 0)
            {
                   switch (weightType)
          {

          case NeuralNetworkLayerSettings.LAYER_DENSE:
                // apply input normalization
                for (int k = 0; k < result.Length; k++)
                {
                    // Debug.Log("ik1 is" + i + " length of result- " + result.Length + ' ' + k);
                    // Debug.Log("ik2 is" + i + " length of bias1- " + weights[weights.Length - 1][i].Length);
                    // Debug.Log("ik3 is" + i + " length of this layer a" + layerSizes[i] + ' ' + k);
                    // if (k == 0 && i == 0)
                    // {
                    //     Debug.Log("weight is" + weights[weights.Length - 1][i][k]);
                    // }
                   
//                    result[k] = result[k]+weights[layerDefinitions.Length][0][i][k];

                }
                break;
            }
            }
if ((inputType & NeuralNetworkLayerSettings.I_NORMALIZED) > 0)
            {
                // apply input normalization
                for (int k = 0; k < result.Length; k++)
                {
                    // Debug.Log("ik1 is" + i + " length of result- " + result.Length + ' ' + k);
                    // Debug.Log("ik2 is" + i + " length of bias1- " + weights[weights.Length - 1][i].Length);
                    // Debug.Log("ik3 is" + i + " length of this layer a" + layerSizes[i] + ' ' + k);
                    // if (k == 0 && i == 0)
                    // {
                    //     Debug.Log("weight is" + weights[weights.Length - 1][i][k]);
                    // }
                    float mean = weights[layerDefinitions.Length + 1][0][i][0];
                    float div = weights[layerDefinitions.Length + 1][0][i][1];
                    float a = weights[layerDefinitions.Length + 1][0][i][2];
                    float b = weights[layerDefinitions.Length + 1][0][i][3];
                    result[k] = ((result[k] - mean) / div) * a + b;

                }
            }

            //Debug.Log("multiplying blayer step " + i + '-' + layerWeights.Length + "/" + layerWeights[0].Length + " result length is" + result.Length);
            // Debug.LogWarning("BIASS IS " + biases.Length);
            // Debug.LogWarning("Result IS " + result.Length);
            // Debug.LogWarning("BIASS IS " + k + '-' + biases[i].Length);
//           /*   if ((outputType & NeuralNetworkLayerSettings.O_BIASED) > 0)
//             {
//                 switch (weightType)
//                 {

//                     case NeuralNetworkLayerSettings.LAYER_DENSE:
// if(result.Length!=weights[layerDefinitions.Length][i].Length){
//     throw new ArgumentException("More fishi stuff going on here"+ weights[layerDefinitions.Length][i].Length + "length of result- " + result.Length + ' ' );
// }
//                         for (int k = 0; k < weights[layerDefinitions.Length][i].Length; k++)
//                         {
//                              //Debug.Log("ik1 is" + i + "/"+k+" length of result- " + result.Length + ' ' + k);
//                             // Debug.Log("ik2 is" + i + " length of bias1- " + weights[weights.Length - 1][i].Length);
//                             // Debug.Log("ik3 is" + i + " length of this layer a" + layerDefinitions[i].size + ' ' + k);
//                             // if (k == 0 && i == 0)
//                             // {
//                             //     Debug.Log("weight is" + weights[weights.Length - 1][i][k]);
//                             // }
//                             result[k] += weights[layerDefinitions.Length][i][k];

//                         }
//                         break;
//                 }
//             }
//  */

            if (i < weights.Length - 1)
            {
                switch (activationType)
                {
                    case NeuralNetworkLayerSettings.A_TANH:
                        ApplyActivation(result, TanH);
                        break;
                    case NeuralNetworkLayerSettings.A_SIGMOID:
                        ApplyActivation(result, Sigmoid);
                        break;
                    case NeuralNetworkLayerSettings.A_RELU:
                        ApplyActivation(result, ReLu);
                        break;
                    case NeuralNetworkLayerSettings.A_LEAKYRELU:

                        ApplyActivation(result, LeakyReLu);
                        break;
                    case NeuralNetworkLayerSettings.A_IDENTITY:
                        // result=result
                        break;


                }
            }
            else
            {
                // Last apply sigmoid
                ApplyOuterActivation(result);

            }
        }
        return result;
    }

    public string ToBinaryString()
    {

        if (builder == null)
            builder = new StringBuilder();
        else
            builder.Length = 0;

        if (NumberOfOutputs == 0) return "";

        for (int i = 0; i < weights.Length; i++)
        {
            ConversionUtils.MatrixToString(weights[0][i], builder);
        }


        return builder.ToString();
    }

    public float[] ToFloatArray()
    {

        int weightCount = GetTotalWeightCount();
        float[] allWeights = new float[weightCount];
        int weightIndex = 0;
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            switch (layerDefinitions[i].type)
            {
                case NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT:
                  { 

                        for (int weight = 0; weight < 4; weight++){
                        int rows = this.weights[i][weight].Length;
                        for (int row = 0; row < rows; row++)
                        {
                            int cols = weights[i][weight][row].Length;
                            for (int col = 0; col < cols; col++)
                            {
                                allWeights[weightIndex] = this.weights[i][weight][row][col];
                                weightIndex++;
                            }
                        }
                        }
         Debug.Log(i+"SAVE RECURRENT index is a " + weightIndex);
                        break;
                    }
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    {
                        int rows = this.weights[i][0].Length;

                        for (int row = 0; row < rows; row++)
                        {
                            int cols = weights[i][0][row].Length;
                            for (int col = 0; col < cols; col++)
                            {
                                allWeights[weightIndex] = this.weights[i][0][row][col];
                                weightIndex++;
                            }
                        }
         Debug.Log(i+"SAVE DENSE index is a " + weightIndex);
                        break;
                    }
                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    {
                        int rows = layerDefinitions[i].size;
                        int cols = layerDefinitions[i].kernelSize + 1;
                        for (int row = 0; row < rows; row++)
                        {
                            for (int col = 0; col < cols; col++)
                            {
                                allWeights[weightIndex] = this.weights[i][0][row][col];
                                weightIndex++;
                            }
                        }
         Debug.Log(i+"SAVE CONV index is a " + weightIndex);
                        break;
                    }
            }



            // for (int bi = 0; bi < biases[i].Length; bi++)
            // {
            //     allWeights[weightIndex] = this.biases[i][bi];
            //     weightIndex++;
            // }
        }
        //Debug.Log("Weight index is a " + weightIndex);

        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            if (layerDefinitions[row].type == NeuralNetworkLayerSettings.LAYER_DENSE)
            {
                for (int col = 0; col < weights[NumberOfLayers][0][row].Length; col++)
                {
                    //Debug.Log("Saving index" + weightIndex + "max floats:" + weightCount + "r " + row + '/' + col);
                    allWeights[weightIndex] = this.weights[NumberOfLayers][0][row][col];
                    weightIndex++;
                }
            }
        }

           //Debug.Log("Weight index is b " + weightIndex);

        // save normalisation
        for (int row = 0; row < NumberOfLayers; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                // Debug.Log(weightCount + "Saving index" + row + ' ' + weightIndex + "max floats:" + weightCount + "r " + row + '/' + col);
                allWeights[weightIndex] = this.weights[NumberOfLayers + 1][0][row][col];
                weightIndex++;
            }
        }
       // Debug.Log("Weight index is c " + weightIndex);


        // // save layertype configs 
        // for (int row = 0; row < NumberOfLayers; row++)
        // {
        //     for (int col = 0; col < 2; col++)
        //     {
        //         //Debug.Log("Saving layer type" + weightIndex + "max floats:" + weightCount + "r " + row + '/' + col);
        //         allWeights[weightIndex] = this.weights[NumberOfLayers + 2][row][col];
        //         weightIndex++;
        //     }
        // }


        if (weightCount != weightIndex)
        {
            throw new ArgumentException("Some fuckup goes on" + weightIndex + "!=" + weightCount);
        }
        return allWeights;
    }

    private int GetTotalWeightCount()
    {
        int totalWeightCount = 0;
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            switch (layerDefinitions[i].type)
            {
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    totalWeightCount += weights[i][0].Length * weights[i][0][0].Length;
                    break;
                case NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT:

                        for (int weight = 0; weight < 4; weight++){
                        int rows = this.weights[i][weight].Length;
                        for (int row = 0; row < rows; row++)
                        {
                            int cols = weights[i][weight][row].Length;
                            for (int col = 0; col < cols; col++)
                            {
                               totalWeightCount++;
                            }
                        }
                        }
                        break;                 
                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    totalWeightCount += weights[i][0].Length * weights[i][0][0].Length;
                    break;
            }
        }

        Debug.Log("Save Weight index is y " + totalWeightCount);
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            switch (layerDefinitions[i].type)
            {
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    totalWeightCount += weights[layerDefinitions.Length][0][i].Length;
                    break;

            }
            //  totalWeightCount += this.weights[NumberOfLayers][i].Length;

        }
        Debug.Log("Save Weight index is y " + totalWeightCount);
        // add one normalization layer, 4 elements, mean size, a and b, trainable config
        totalWeightCount += NumberOfLayers * 4;

        Debug.Log("Save Weight index is y " + totalWeightCount);
        //Debug.Log("Weight index is z " + totalWeightCount);
        // type config per layer trainable fonfig
        // totalWeightCount += NumberOfLayers * 2;
        return totalWeightCount;
    }

    public static float Sigmoid(float x) => ((float)(1.0f / (1f + Mathf.Exp(-x))));
    public static float BinaryStep(float x) => (x > 0 ? 0 : 1);
    public static float Softplus(float x) => (Mathf.Log(1 + Mathf.Exp(x)));
    public static float Binary(float x) => (x > 0 ? 0 : 1);

    public static float ReLu(float x) => (x > 0 ? x : 0);
    public static float LeakyReLu(float x) => (x > 0 ? x : x * .0001f);


    public static float TanH(float x) => ((float)System.Math.Tanh(x));


    public static void ApplyInnerActivation(float[] vector)
    {
        // ApplyTanH(vector);
        ApplyActivation(vector, ReLu);
    }
    public static void ApplyOuterActivation(float[] vector)
    {
        // ApplyTanH(vector);
        ApplyActivation(vector, TanH);
    }


    public delegate float PerformCalculation(float y);

    public static void ApplyActivation(float[] vector, PerformCalculation calc)
    {
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = calc(vector[i]);
        }
    }

    private float[][][] WeightsFromBinaryString(string encoded)
    {

        float[][][] matrices = new float[NumberOfLayers][][];
        int strIndex = 0;
        // split the cromosome into the required sizes and turn the substrings into weight matrices.
        for (int i = 0; i < NumberOfLayers; i++)
        {
            int rows = layerDefinitions[i].size;
            int cols = layerDefinitions[i + 1].size;
            int substrLength = rows * cols * 32;

            matrices[i] = ConversionUtils.BinaryStringToMatrix(rows, cols, encoded, strIndex);
            // Non Optimized equivalent calls
            // string substr = chromosome.Substring(strIndex, substrLength);
            //matrices[i] = MatrixFromString(rows, cols, substr); 

            strIndex += substrLength;
        }
        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            int substrLength = layerDefinitions[row + 1].size * 32;
            matrices[NumberOfLayers][row] = ConversionUtils.BinaryStringToFloatArray(encoded, strIndex, layerDefinitions[row + 1].size);
            strIndex += substrLength;

        }
        for (int row = 0; row < NumberOfLayers; row++)
        {
            matrices[NumberOfLayers + 1][row] = ConversionUtils.BinaryStringToFloatArray(encoded, strIndex, 4);
            strIndex += layerDefinitions[row].size;

        }
        for (int row = 0; row < NumberOfLayers; row++)
        {
            matrices[NumberOfLayers + 2][row] = ConversionUtils.BinaryStringToFloatArray(encoded, strIndex, 2);
            strIndex += layerDefinitions[row].size;

        }
        return matrices;
    }

    private float[][][][] WeightsFromFloatArray(float[] weights)
    {

        float[][][][] matrices = new float[NumberOfLayers + 2][][][];
        
        memory=new float[layerDefinitions.Length][];
        int weightIndex = 0;
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            var def = layerDefinitions[i];
            int rows = layerDefinitions[i].size;
            switch (def.type)
            {
                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    {

                        int cols = layerDefinitions[i].kernelSize + 1;
                        var matrix = new float[1][][];
                          matrix[0] = new float[rows][];
                        for (int row = 0; row < rows; row++)
                        {
                            matrix[0][row] = new float[cols];
                            for (int col = 0; col < cols; col++)
                            {
                                matrix[0][row][col] = weights[weightIndex];
                                weightIndex++;
                            }
                        }
                        matrices[i]= matrix;
                    }
                    break;
                case NeuralNetworkLayerSettings.LAYER_IDENTITY:
                case NeuralNetworkLayerSettings.LAYER_DROPOUT:
                case NeuralNetworkLayerSettings.LAYER_ZERO_PAD:
                case NeuralNetworkLayerSettings.LAYER_MAX_POOL:
                    {

                        matrices[i] = new float[0][][];
                    }
                    break;
                case NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT:
 
 

{
    // init memory for this layer, a memory entry for each node in the layer
    //memory[i]=new float[layerDefinitions[i].memorySize*layerDefinitions[i].size];
 int outputSize=layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size);
                      var matrix = new float[4][][];
                        // 0 = input matrix
                        // 1 = memory matrix
                        // 2 = output matrix
                        // 3 = initial memory vector 
      // weights1 the input weight
        matrix[0]=new float[layerDefinitions[i].size][];
        for(int l =0;l< matrix[0].Length;l++){
            matrix[0][l]=new float[layerDefinitions[i].size];
        for(int k =0;k< layerDefinitions[i].size;k++){
matrix[0][l][k]=weights[weightIndex++];
        }
        }

// weights for memory
        matrix[1]=new float[outputSize][];

        for(int l =0;l<matrix[1].Length;l++){
            matrix[1][l]=new float[outputSize];
        for(int k =0;k<  matrix[1][l].Length;k++){
matrix[1][l][k]=weights[weightIndex++];
        }
        }


        // matrix 2 the output weight
        matrix[2]=new float[layerDefinitions[i].size+outputSize][];

        for(int l =0;l< matrix[2].Length;l++){
            matrix[2][l]=new float[outputSize];
        for(int k =0;k<  matrix[2][l].Length;k++){
matrix[2][l][k]=weights[weightIndex++];
        }
        }


        // matrix 3 the initial mem
        matrix[3]=new float[1][];

        for(int l =0;l< matrix[3].Length;l++){
            matrix[3][l]=new float[outputSize];
        for(int k =0;k<  matrix[3][l].Length;k++){
matrix[3][l][k]=weights[weightIndex++];
        }
        
                        }
                            matrices[i] = matrix;
         Debug.Log("LOAD RECURRENT Load index is a " + weightIndex);
        break; 
        }
        
                    

    
                      
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    {
                        int cols = layerDefinitions[i + 1].getInputSize(layerDefinitions[i].size);
                        int weightsInMatrix = rows * cols;
                        var matrix = new float[1][][];
                        matrix[0] = new float[rows][];
                        for (int row = 0; row < rows; row++)
                        {
                            matrix[0][row] = new float[cols];
                            for (int col = 0; col < cols; col++)
                            {
                                matrix[0][row][col] = weights[weightIndex];
                                weightIndex++;
                            }
                        }
                        matrices[i] = matrix;
         Debug.Log("LOAD DENSE Load index is a " + weightIndex);
                    break;
                    }
            }
        }
        matrices[NumberOfLayers] = new float[1][][];
        matrices[NumberOfLayers][0] = new float[NumberOfLayers][];

         Debug.Log("Load index is a " + weightIndex);
        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            // Debug.Log("Loading from float biases row" + row);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row]);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row + 1]); 
            

            switch (layerDefinitions[row].type)
            {

                case NeuralNetworkLayerSettings.LAYER_DENSE:
                
                   
                    matrices[NumberOfLayers][0][row] = new float[layerDefinitions[row + 1].getInputSize(layerDefinitions[row].size)];
                    for (int c = 0; c <  matrices[NumberOfLayers][0][row].Length; c++)
                    {
                        matrices[NumberOfLayers][0][row][c] = weights[weightIndex];
                        weightIndex++;
                    }
                    break;
            }



        }
        matrices[NumberOfLayers + 1] = new float[1][][];
        matrices[NumberOfLayers + 1][0] = new float[NumberOfLayers][];

         Debug.Log("Load index is b " + weightIndex);
        for (int row = 0; row < NumberOfLayers; row++)
        {
            // Debug.Log("Loading from float biases row" + row);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row]);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row + 1]);
            matrices[NumberOfLayers + 1][0][row] = new float[4];
            for (int c = 0; c < 4; c++)
            {
                matrices[NumberOfLayers + 1][0][row][c] = weights[weightIndex];
                weightIndex++;
            }

        }

        Debug.Log("Load index is c " + weightIndex);
       

        if (weights.Length != weightIndex)
        {
            throw new ArgumentException("Some fuckup goes on" + weightIndex + "!=" + weights.Length);
        }
        return matrices;
    }

    string IChromosomeEncodable<string>.ToChromosome()
    {
        return ToBinaryString();
    }

    float[] IChromosomeEncodable<float[]>.ToChromosome()
    {
        return ToFloatArray();
    }
}