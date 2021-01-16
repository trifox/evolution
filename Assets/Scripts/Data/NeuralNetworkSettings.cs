using UnityEngine;
using System.Linq;
using Keiwando.JSON;

[SerializeField]

public struct NeuralNetworkLayerSettings : IJsonConvertible
{
    public int calculatedInputCount;
    public int calculatedOutputCount;
    public const int LAYER_DENSE = 1;
    public const int LAYER_CONVOLUTION = 2;
    public const int LAYER_MAX_POOL = 4;
    public const int LAYER_DROPOUT = 8;
    public const int LAYER_IDENTITY = 16;
    public const int LAYER_ZERO_PAD = 32; 
    public const int LAYER_DENSE_RECURRENT = 64;

    public const int O_BIASED = 1;
    public const int I_PLAIN = 0;
    public const int I_NORMALIZED = 1;


    public const int A_SIGMOID = 0;
    public const int A_TANH = 1;
    public const int A_RELU = 2;
    public const int A_LEAKYRELU = 3;
    public const int A_IDENTITY = 4;
    public int size; 
    public int kernelSize;
    public int type;// DENSE|CONVOLUTIONAL
    public int inputType;// NORMALIZED |
    public int outputType;// NORMALIZED |
    public int activationType; //SIGMOID|TANH|RELU|LEAKYRELU
    public float dropoutRate;
    public int poolSize; //SIGMOID|TANH|RELU|LEAKYRELU
    public int poolStride; //SIGMOID|TANH|RELU|LEAKYRELU
    public int convStride; //SIGMOID|TANH|RELU|LEAKYRELU
    public int padding; //SIGMOID|TANH|RELU|LEAKYRELU
    public int getOutputDimension(int inputLength)
    {
        var result = 0;
        switch (type)
        {
            case LAYER_CONVOLUTION:
                result = size * Mathf.CeilToInt((inputLength - kernelSize + 1) / (float)convStride);
                break;
            case LAYER_MAX_POOL:
                result = Mathf.CeilToInt((inputLength - poolSize + 1) / (float)poolStride);
                break;
            case LAYER_IDENTITY:
            case LAYER_DROPOUT:
                result = inputLength;
                break;
            case LAYER_ZERO_PAD:
                result = inputLength + padding * 2;
                break;

            case LAYER_DENSE:
            default:
                throw new System.Exception("INVALID QUERY FOR NOW");
        }
        //             Debug.LogWarning("xxxxxxxxxxresultxxxxxxxxxxxxxxxxxxxxx"+   (inputLength -kernelSize+1)/convStride )  ;
        //             Debug.LogWarning("xxxxxxxxxxresultxxxxxxxxxxxxxxxxxxxxx"+Mathf.Ceil(   (inputLength -kernelSize+1)/convStride ) );
        //             Debug.LogWarning("xxxxxxxxxxresult output conv stride/size"+convStride+"/"+kernelSize+'/'+convPad);
        //             Debug.LogWarning("xxxxxxxxxxxresult output pool size/stride"+poolSize+"/"+poolStride);
        // Debug.LogWarning("xxxxxxxxxxresult output size length/result"+inputLength+"/"+result);
        return result;
    }
    public int getInputSize(int inputNodeCount)
    {
        switch (type)
        {
            case LAYER_CONVOLUTION:
                return inputNodeCount;
            case LAYER_MAX_POOL:
                return inputNodeCount;
            case LAYER_IDENTITY:
            case LAYER_DROPOUT:
            case LAYER_ZERO_PAD:
                return inputNodeCount;
            case LAYER_DENSE:
            default:
                return size;
        }
    }
    // public int OutputDimension
    // {
    //     get
    //     {
    //         switch (type)
    //         {
    //             case LAYER_CONVOLUTION:
    //                 return size * kernelSize; 

    //             case LAYER_DENSE:
    //             default:
    //                 return size;
    //         }
    //     }
    // }
    public NeuralNetworkLayerSettings(int size, int type, int inputtype, int outputType, int activation)
    {
        this.size = size;
        this.inputType = inputtype;
        this.activationType = activation;
        this.outputType = outputType;
        this.type = type;
        this.kernelSize = 3;
        this.convStride = 1;
        this.padding = 0;
        this.poolSize = 0;
        this.poolStride = 0;
        this.dropoutRate = 0.0f;
        this.calculatedInputCount = 0;
        this.calculatedOutputCount = 0; 


    }
    public static NeuralNetworkLayerSettings factorZeroPad(int amount = 10)
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(0, NeuralNetworkLayerSettings.LAYER_ZERO_PAD, I_NORMALIZED, 0, A_IDENTITY);
        result.padding = amount;
        return result;
    }

    public static NeuralNetworkLayerSettings factorDropout(float rate = 0.5f)
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(0, NeuralNetworkLayerSettings.LAYER_DROPOUT, I_NORMALIZED, 0, A_IDENTITY);
        result.dropoutRate = rate;
        return result;
    }

    public static NeuralNetworkLayerSettings factorRecurrent(int size ,int outputtype=I_NORMALIZED,int activation=A_TANH)
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(size, NeuralNetworkLayerSettings.LAYER_DENSE_RECURRENT, 0,outputtype,  A_IDENTITY);
      
        return result;
    }
    public static NeuralNetworkLayerSettings factorIdentity()
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(0, NeuralNetworkLayerSettings.LAYER_IDENTITY, 0, 0, A_IDENTITY);

        return result;
    }
    public static NeuralNetworkLayerSettings factorMaxPool1D(int size = 3, int stride = 1)
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(0, NeuralNetworkLayerSettings.LAYER_MAX_POOL, I_NORMALIZED, 0, A_IDENTITY);
        result.poolSize = size;
        result.poolStride = stride;
        return result;
    }
    public static NeuralNetworkLayerSettings factorConvolution1D(int nodes, int kernelsize = 3, int stride = 1, int pad = 0)
    {

        NeuralNetworkLayerSettings result = new NeuralNetworkLayerSettings(nodes, LAYER_CONVOLUTION, I_NORMALIZED, 0, A_TANH);
        result.convStride = stride;
        result.kernelSize = kernelsize;
        result.padding = pad;
        return result;
    }
    public JObject Encode()
    {
        JObject json = new JObject();
        json.Set("type", type);
        json.Set("poolSize", poolSize);
        json.Set("dropoutRate", dropoutRate);
        json.Set("poolStride", poolStride);
        json.Set("kernelSize", kernelSize);
        json.Set("convStride", convStride);
        json.Set("convPad", padding);
        json.Set("activationType", activationType);
        json.Set("inputType", inputType);
        json.Set("outputType", outputType);
        json.Set("size", size); 
        Debug.LogWarning("Encode settings" + json.ToString());
        return json;
    }

    public static NeuralNetworkLayerSettings Decode(JObject json)
    {
        try
        {
            Debug.LogWarning("Decode settings" + json.ToString());
            var result = new NeuralNetworkLayerSettings(
                    json["size"].ToInt(),
                    json["type"].ToInt(),
                    json["inputType"].ToInt(),
                    json["outputType"].ToInt(),
                    json["activationType"].ToInt()
           );

            result.dropoutRate = json["dropoutRate"].ToInt();
            result.kernelSize = json["kernelSize"].ToInt();
            result.poolSize = json["poolSize"].ToInt();
            result.poolStride = json["poolStride"].ToInt();
            result.convStride = json["convStride"].ToInt();
            result.padding = json["convPad"].ToInt(); 
            return result;
        }
        catch (System.Exception e)
        {
            return new NeuralNetworkLayerSettings(0, 0, 0, 0, 0);
        }
    }
}
public struct NeuralNetworkSettings : IJsonConvertible
{
    public static readonly int MAX_LAYERS = 200;
    public static readonly int MAX_NODES_PER_LAYER = 1000;

    //    public static NeuralNetworkSettings Default = new NeuralNetworkSettings(new int[] { 23, 23, 23, 23 });
    // basically an auto-encoder
    public static NeuralNetworkSettings Default = new NeuralNetworkSettings(
         new NeuralNetworkLayerSettings[] {
            //  new NeuralNetworkLayerSettings(10, NeuralNetworkLayerSettings.LAYER_CONVOLUTION, NeuralNetworkLayerSettings.I_NORMALIZED, 0, NeuralNetworkLayerSettings.A_TANH) ,
                // new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                //  new NeuralNetworkLayerSettings(10, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
            //    new NeuralNetworkLayerSettings(16, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                
                NeuralNetworkLayerSettings.factorZeroPad(2),
               NeuralNetworkLayerSettings.factorConvolution1D(6,3,1,0),
                NeuralNetworkLayerSettings.factorMaxPool1D(3,3),
                 NeuralNetworkLayerSettings.factorConvolution1D(1,3,2,0),
              NeuralNetworkLayerSettings.factorDropout(0.1f),
                 NeuralNetworkLayerSettings.factorConvolution1D(1,3,3,0),
               NeuralNetworkLayerSettings.factorRecurrent(6), 
               NeuralNetworkLayerSettings.factorRecurrent(4), 
               NeuralNetworkLayerSettings.factorRecurrent(3), 
               
                NeuralNetworkLayerSettings.factorDropout(0.1f), 
               NeuralNetworkLayerSettings.factorRecurrent(3),  
            //    NeuralNetworkLayerSettings.factorRecurrent(3),  
               NeuralNetworkLayerSettings.factorRecurrent(3),  
                // NeuralNetworkLayerSettings.factorMaxPool1D(10,10),      
               
                 //   new NeuralNetworkLayerSettings(16, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                //   new NeuralNetworkLayerSettings(7, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                //   new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                //   new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                //   new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
        //                         NeuralNetworkLayerSettings.factorDropout(0.1f), 
        //     new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                  
        //                     new NeuralNetworkLayerSettings(6, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
        //                     // NeuralNetworkLayerSettings.factorDropout(0.1f), 
          
        // //   NeuralNetworkLayerSettings.factorRecurrent(7,7),
        //    new NeuralNetworkLayerSettings(6  , NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
        //                     NeuralNetworkLayerSettings.factorDropout(0.1f), 
         
        //   new NeuralNetworkLayerSettings(3  , NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                    //        NeuralNetworkLayerSettings.factorDropout(0.1f), 
           
        //                     new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                      
        //   new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
        //   new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
        //   new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                  //   new NeuralNetworkLayerSettings(4, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // // new NeuralNetworkLayerSettings(4, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
              
                  
                //   new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(3, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                   //   NeuralNetworkLayerSettings.factorDropout(0.4f),
                  //   new NeuralNetworkLayerSettings(4, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_RELU),
                // new NeuralNetworkLayerSettings(4, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                  //    new NeuralNetworkLayerSettings(4, NeuralNetworkLayerSettings.LAYER_DENSE, NeuralNetworkLayerSettings.I_NORMALIZED, NeuralNetworkLayerSettings.O_BIASED, NeuralNetworkLayerSettings.A_TANH),
                });

    public int NumberOfIntermediateLayers
    {
        get { return layerDefinition.Length; }
    }

    public NeuralNetworkLayerSettings[] layerDefinition;

    public int[] getLayerSizes()
    {
        int[] result = new int[layerDefinition.Length];
        for (var i = 0; i < layerDefinition.Length; i++)
        {
            result[i] = layerDefinition[i].size;
        }
        return result;
    }

    public NeuralNetworkSettings(NeuralNetworkLayerSettings[] layerDefinition)
    {
        this.layerDefinition = layerDefinition;
    }

    #region Encode & Decode

    private static class CodingKey
    {
        public const string NodesPerIntermediateLayer = "NodesPerIntermediateLayer";
        public const string Type = "LayerType";
        public const string Input = "Input";
        public const string Activation = "Activation";
    }

    public JObject Encode()
    {

        JObject json = new JObject();
        JObject[] list = new JObject[layerDefinition.Length];
        for (int i = 0; i < layerDefinition.Length; i++)
        {
            list[i] = this.layerDefinition[i].Encode();
        }
        json[CodingKey.NodesPerIntermediateLayer] = list;
        Debug.LogWarning("Saving settings" + json.ToString());
        return json;
    }

    public static NeuralNetworkSettings Decode(JObject json)
    {

        Debug.LogWarning("Decode settings" + json.ToString());
        JToken[] nodesPerLayer = json[CodingKey.NodesPerIntermediateLayer].ToArray();
        NeuralNetworkLayerSettings[] layersettings = new NeuralNetworkLayerSettings[nodesPerLayer.Length];

        Debug.LogWarning("Decode settings2" + layersettings.ToString());
        for (int i = 0; i < nodesPerLayer.Length; i++)
        {
            Debug.LogWarning("Decode settings2" + layersettings.ToString());
            layersettings[i] = NeuralNetworkLayerSettings.Decode((JObject)nodesPerLayer[i]);
        }
        return new NeuralNetworkSettings(layersettings);
    }

    public static NeuralNetworkSettings Decode(string encoded)
    {

        if (string.IsNullOrEmpty(encoded))
        {
            return Default;
        }
        return Decode(JObject.Parse(encoded));

    }


    #endregion
}
