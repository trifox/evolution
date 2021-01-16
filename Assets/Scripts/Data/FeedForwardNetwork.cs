using System;
using System.Text;
using UnityEngine;
public class FeedForwardNetworkOrig : IChromosomeEncodable<string>, IChromosomeEncodable<float[]>
{

    private struct Constants
    {
        public static float MIN_WEIGHT = -3.0f;
        public static float MAX_WEIGHT = 3.0f;
    }

    public int NumberOfLayers { get { return layerSizes.Length; } }
    public int NumberOfInputs { get { return layerSizes[0]; } }
    public int NumberOfOutputs { get { return layerSizes[layerSizes.Length - 1]; } }

    public float[] Inputs { get; set; }

    private int[] layerSizes;

    public float[][][] weights;

    // Optimization
    private float[][] tempResults;
    private StringBuilder builder;
    public int getLayerCount()
    {
        return layerSizes.Length;
    }

    public int getLayerSize(int i)
    {
        return layerSizes[i];
    }

    public float[] getWeight(int layer, int i)
    {
        // Debug.Log("Layer info" + layer + '-' + i);
        // Debug.Log("Layer info" + weights.Length);
        // Debug.Log("Layer info" + weights[layer].Length);
        // Debug.Log("Layer infoxx" + weights[layer][i].Length);
        // Debug.Log("Layer infoxx2" + String.Join("  ", weights[layer][i]));
        return weights[layer][i];
    }

    private FeedForwardNetworkOrig(int inputCount, int outputCount, NeuralNetworkSettings settings)
    {
        // Setup layerSizes
        int layerCount = settings.NumberOfIntermediateLayers + 2;
        this.layerSizes = new int[layerCount];
        this.layerSizes[0] = inputCount;
        this.layerSizes[layerCount - 1] = outputCount;
        // Debug.Log("LayerHello there " + inputCount + "->hidden(" + layerCount + ")-> " + outputCount);
        // Debug.Log("RecommendationsLayerHello there " + inputCount + "->hidden(" + layerCount + ")-> " + outputCount);
        int connections = 0;
        int nodes = 0;
        int lastCount = 0;

        for (int i = 1; i < layerCount - 1; i++)
        {

            layerSizes[i] = settings.layerDefinition[i - 1].size;
        }
        Debug.Log("Stats Network");
        foreach (int layerSize in this.layerSizes)
        {
            nodes += layerSize;
            connections += layerSize * lastCount;
            lastCount = layerSize;
        }
        Debug.Log("Stats Network nodeCount=" + nodes + "Connections: " + connections);
        Debug.Log("Hidden Layers should be no more than inputCount*2=" + (inputCount * 2));
        Debug.Log("Hidden Layers should be no more than (inputCount+outputCount)*2/3=" + (inputCount * (2.0 / 3.0) + outputCount));
        Debug.Log("Hidden Layers should be between  " + inputCount + " and " + outputCount);

        this.Inputs = new float[inputCount];
    }

    public FeedForwardNetworkOrig(
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

        InitializeTempResults();
    }

    public FeedForwardNetworkOrig(
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
            this.weights = WeightsFromBinaryString(encoded);
        }

        InitializeTempResults();
    }

    private void InitializeTempResults()
    {
        this.tempResults = new float[weights.Length][];
        for (int i = 0; i < weights.Length; i++)
        {
            int cols = weights[i][0].Length;
            this.tempResults[i] = new float[cols];
        }
    }

    private void SetupRandomWeights()
    {
        this.weights = new float[layerSizes.Length][][];
        for (int i = 0; i < weights.Length - 1; i++)
        {

            this.weights[i] = MatrixUtils.CreateRandomMatrix2D(
                layerSizes[i], layerSizes[i + 1],
                Constants.MIN_WEIGHT, Constants.MAX_WEIGHT
            );
        }
        // put in the last layer the weights matrix/ to reuse all of the weigths storing and restoring functionality

        weights[layerSizes.Length - 1] = new float[layerSizes.Length - 1][];
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            weights[layerSizes.Length - 1][i] = new float[layerSizes[i + 1]];
            for (int k = 0; k < layerSizes[i + 1]; k++)
            {
                weights[layerSizes.Length - 1][i][k] = UnityEngine.Random.Range(-1f, 1f);
            }
        }
    }


    public float[] CalculateOutputs()
    {

        float[] result = Inputs;

        for (int i = 0; i < weights.Length - 1; i++)
        {

            float[][] layerWeights = weights[i];
            //      float[] layerBiases = biases[i];
            float[] tempResultVec = tempResults[i];
            //  for (int k = 0; k < weights[weights.Length - 1][i].Length; k++)
            //         {
            //             // Debug.Log("ik1 is" + i + " length of result- " + result.Length + ' ' + k);
            //             // Debug.Log("ik2 is" + i + " length of bias1- " + weights[weights.Length - 1][i].Length);
            //             // Debug.Log("ik3 is" + i + " length of this layer a" + layerSizes[i] + ' ' + k);
            //             // if (k == 0 && i == 0)
            //             // {
            //             //     Debug.Log("weight is" + weights[weights.Length - 1][i][k]);
            //             // }
            //             result[k] += weights[weights.Length - 1][i][k];

            //         }

            result = MatrixUtils.MatrixProductTranspose(layerWeights, result, tempResultVec);


            // Debug.LogWarning("BIASS IS " + biases.Length);
            // Debug.LogWarning("Result IS " + result.Length);
            // Debug.LogWarning("BIASS IS " + k + '-' + biases[i].Length);

            for (int k = 0; k < weights[weights.Length - 1][i].Length; k++)
            {
                // Debug.Log("ik1 is" + i + " length of result- " + result.Length + ' ' + k);
                // Debug.Log("ik2 is" + i + " length of bias1- " + weights[weights.Length - 1][i].Length);
                // Debug.Log("ik3 is" + i + " length of this layer a" + layerSizes[i] + ' ' + k);
                // if (k == 0 && i == 0)
                // {
                //     Debug.Log("weight is" + weights[weights.Length - 1][i][k]);
                // }
                result[k] += weights[weights.Length - 1][i][k];

            }


            if (i < weights.Length - 1)
            {
                //ApplyTanH(result);
                // ApplySigmoid(result);
                ApplyInnerActivation(result);
            }
            else
            {
                // Last apply sigmoid
                ApplySigmoid(result);

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
            ConversionUtils.MatrixToString(weights[i], builder);
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
            int rows = layerSizes[i];
            int cols = layerSizes[i + 1];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    allWeights[weightIndex] = this.weights[i][row][col];
                    weightIndex++;
                }
            }


            // for (int bi = 0; bi < biases[i].Length; bi++)
            // {
            //     allWeights[weightIndex] = this.biases[i][bi];
            //     weightIndex++;
            // }
        }
        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            for (int col = 0; col < weights[NumberOfLayers - 1][row].Length; col++)
            {
                //Debug.Log("Saving index" + weightIndex + "max floats:" + weightCount + "r " + row + '/' + col);
                allWeights[weightIndex] = this.weights[NumberOfLayers - 1][row][col];
                weightIndex++;
            }
        }


        return allWeights;
    }

    private int GetTotalWeightCount()
    {
        int totalWeightCount = 0;
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            totalWeightCount += this.weights[i].Length * this.weights[i][0].Length;
        }
        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            //  totalWeightCount += this.weights[NumberOfLayers][i].Length;
            totalWeightCount += layerSizes[i + 1];
        }
        return totalWeightCount;
    }

    public static float Sigmoid(float x) => ((float)(1.0f / (1f + Mathf.Exp(-x))));

    public static float ReLu(float x) => (x > 0 ? x : 0);
    public static float LeakyReLu(float x) => (x > 0 ? x : x * .0001f);


    public static float TanH(float x) => ((float)System.Math.Tanh(x));
    public static void ApplySigmoid(float[] vector)
    {
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = Sigmoid(vector[i]);
        }
    }
    public static void ApplyReLu(float[] vector)
    {
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = ReLu(vector[i]);
        }
    }

    public static void ApplyInnerActivation(float[] vector)
    {
        ApplyTanH(vector);
    }
    public static void ApplyTanH(float[] vector)
    {
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = TanH(vector[i]);
        }
    }

    private float[][][] WeightsFromBinaryString(string encoded)
    {

        float[][][] matrices = new float[NumberOfLayers][][];
        int strIndex = 0;
        // split the cromosome into the required sizes and turn the substrings into weight matrices.
        for (int i = 0; i < NumberOfLayers; i++)
        {
            int rows = layerSizes[i];
            int cols = layerSizes[i + 1];
            int substrLength = rows * cols * 32;

            matrices[i] = ConversionUtils.BinaryStringToMatrix(rows, cols, encoded, strIndex);
            // Non Optimized equivalent calls
            // string substr = chromosome.Substring(strIndex, substrLength);
            //matrices[i] = MatrixFromString(rows, cols, substr); 

            strIndex += substrLength;
        }
        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            matrices[NumberOfLayers][row] = ConversionUtils.BinaryStringToFloatArray(encoded, strIndex, layerSizes[row + 1]);
            strIndex += layerSizes[row];

        }
        return matrices;
    }

    private float[][][] WeightsFromFloatArray(float[] weights)
    {

        float[][][] matrices = new float[NumberOfLayers][][];
        int weightIndex = 0;

        for (int i = 0; i < NumberOfLayers - 1; i++)
        {
            int rows = layerSizes[i];
            int cols = layerSizes[i + 1];
            int weightsInMatrix = rows * cols;

            var matrix = new float[rows][];
            for (int row = 0; row < rows; row++)
            {
                matrix[row] = new float[cols];
                for (int col = 0; col < cols; col++)
                {
                    matrix[row][col] = weights[weightIndex];
                    weightIndex++;
                }
            }
            matrices[i] = matrix;
        }
        matrices[NumberOfLayers - 1] = new float[NumberOfLayers][];
        for (int row = 0; row < NumberOfLayers - 1; row++)
        {
            // Debug.Log("Loading from float biases row" + row);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row]);
            // Debug.Log("Loading from float biases lsize" + layerSizes[row + 1]);
            matrices[NumberOfLayers - 1][row] = new float[layerSizes[row + 1]];
            for (int c = 0; c < layerSizes[row + 1]; c++)
            {
                matrices[NumberOfLayers - 1][row][c] = weights[weightIndex];
                weightIndex++;
            }

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