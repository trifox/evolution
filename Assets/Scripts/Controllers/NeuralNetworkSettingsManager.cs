using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Keiwando.JSON;

public class NeuralNetworkSettingsManager : MonoBehaviour
{

    private static readonly string INPUT_FIELD_PREFAB_NAME = "Prefabs/Layer Input Field";

    [SerializeField] private InputField numberOfLayersInput;

    // [SerializeField] private VisualNeuralNetwork visualNetwork;
    [SerializeField] private VisualNeuralNetworkRenderer networkRenderer;
    [SerializeField] private RectTransform leftEdge;
    [SerializeField] private RectTransform rightEdge;
    [SerializeField] private RawImage image;
    [SerializeField] private RenderTexture renderTextureAA;
    // For platforms that don't support AA textures (WebGL 1.0 Safari)
    [SerializeField] private RenderTexture renderTextureNoAA;
    private RenderTexture renderTexture;

    private Dictionary<int, InputField> intermediateInputs = new Dictionary<int, InputField>();

    void Start()
    {

#if UNITY_WEBGL
		renderTexture = renderTextureNoAA;
#else
        renderTexture = renderTextureAA;
#endif
        image.texture = renderTexture;

        numberOfLayersInput.onEndEdit.AddListener(delegate (string arg0)
        {
            NumberOfLayersChanged(arg0);
        });

        Refresh();
    }

    private void Refresh()
    {

        foreach (var input in intermediateInputs.Values)
        {
            Destroy(input.gameObject);
        }
        intermediateInputs.Clear();

        var settings = GetNetworkSettings();

        numberOfLayersInput.text = (settings.NumberOfIntermediateLayers + 2).ToString();

        var left = leftEdge.transform.localPosition.x;
        var right = rightEdge.transform.localPosition.x;
        var rectWidth = right - left;
        var spacing = rectWidth / (settings.NumberOfIntermediateLayers + 1);

        // Create an input field for every intermediate layer
        for (int i = 0; i < settings.NumberOfIntermediateLayers; i++)
        {

            var x = left + (i + 1) * spacing;
            var position = new Vector3(x, 0, transform.position.z);
            var inputNodeCount = CreateInputField(position);
            var inputType = CreateInputField(position + new Vector3(0, -80, 0));
            var inputInput = CreateInputField(position + new Vector3(0, -40, 0));
            var inputOutput = CreateInputField(position + new Vector3(0, 40, 0));
            var inputActivation = CreateInputField(position + new Vector3(0, 80, 0));

            switch (settings.layerDefinition[i].type)
            {
                case NeuralNetworkLayerSettings.LAYER_DENSE:
                    inputType.text = "DENSE";
                    break;

                case NeuralNetworkLayerSettings.LAYER_CONVOLUTION:
                    inputType.text = "CONVOLUTION";
                    break;
                case NeuralNetworkLayerSettings.LAYER_MAX_POOL:
                    inputType.text = "MAX_POOL";
                    break;

                case NeuralNetworkLayerSettings.LAYER_DROPOUT:
                    inputType.text = "DROPOUT";
                    break;

                default:
                    inputType.text = "-";
                    break;
            }

            switch (settings.layerDefinition[i].inputType)
            {
                case NeuralNetworkLayerSettings.I_NORMALIZED:
                    inputInput.text = "NORMALIZED";
                    break;
                default:
                    inputInput.text = "-";
                    break;

            }
            //inputInput.text = settings.NodesPerIntermediateLayer[i].inputType == NeuralNetworkLayerSettings.I_NORMALIZED ? "NORMALIZED" : "PLAIN";
            switch (settings.layerDefinition[i].outputType)
            {
                case NeuralNetworkLayerSettings.O_BIASED:
                    inputOutput.text = "BIASED";
                    break;

                default:
                    inputOutput.text = "UNBIASED";
                    break;
            }

            //  inputOutput.text = settings.NodesPerIntermediateLayer[i].outputType.ToString();


            switch (settings.layerDefinition[i].activationType)
            {
                case NeuralNetworkLayerSettings.A_RELU:
                    inputActivation.text = "RELU";
                    break;

                case NeuralNetworkLayerSettings.A_TANH:
                    inputActivation.text = "TANH";
                    break;

                case NeuralNetworkLayerSettings.A_SIGMOID:
                    inputActivation.text = "SIGMOID";
                    break;

                case NeuralNetworkLayerSettings.A_LEAKYRELU:
                    inputActivation.text = "LEAKYRELU";
                    break;
                case NeuralNetworkLayerSettings.A_IDENTITY:
                    inputActivation.text = "IDENTITY";
                    break; 
                default:
                    inputActivation.text = "-";
                    break;

            }
            //inputActivation.text = settings.NodesPerIntermediateLayer[i].activationType.ToString();
            intermediateInputs.Add(i, inputNodeCount);
            var index = i;

            inputNodeCount.onEndEdit.AddListener(delegate (string arg0)
            {

                LayerSizeInputChanged(index, arg0);
            });

            inputNodeCount.text = settings.layerDefinition[i].size.ToString();
        }

        // visualNetwork.Refresh();
        networkRenderer.Render(settings, renderTexture);
    }

    public InputField CreateInputField(Vector3 position)
    {

        var input = ((GameObject)Instantiate(Resources.Load(INPUT_FIELD_PREFAB_NAME))).GetComponent<InputField>();

        //input.transform.SetParent(transform);
        input.transform.SetParent(transform, false);
        input.transform.localPosition = position;

        return input;
    }

    /// <summary>
    /// The input for the number of nodes for an intermediate changed.
    /// Index specifies the index of the hidden layer that was edited.
    /// </summary>
    public void LayerSizeInputChanged(int index, string value)
    {

        var settings = GetNetworkSettings();
        var num = Mathf.Clamp(int.Parse(value), 1, NeuralNetworkSettings.MAX_NODES_PER_LAYER);

        if (num == settings.layerDefinition[index].size) return;

        settings.layerDefinition[index].size = num;
        SaveNewSettings(settings);

        Refresh();
    }

    /// <summary>
    /// The number of layers should be changed.
    /// </summary>
    /// <param name="value">Value.</param>
    public void NumberOfLayersChanged(string value)
    {

        int num = int.Parse(value);

        num = Mathf.Clamp(num, 3, NeuralNetworkSettings.MAX_LAYERS);

        numberOfLayersInput.text = num.ToString();

        var settings = GetNetworkSettings();

        var oldNumber = settings.NumberOfIntermediateLayers + 2;

        if (num != oldNumber)
        {
            // Number was changed
            var layerSizes = new List<NeuralNetworkLayerSettings>(settings.layerDefinition);

            if (num > oldNumber)
            {
                // Duplicate the last layer
                for (int i = 0; i < num - oldNumber; i++)
                    layerSizes.Add(layerSizes[layerSizes.Count - 1]);
            }
            else
            {
                for (int i = 0; i < oldNumber - num; i++)
                    layerSizes.RemoveAt(layerSizes.Count - 1);
            }

            SaveNewSettings(layerSizes.ToArray());
        }

        Refresh();
    }

    private void SaveNewSettings(NeuralNetworkLayerSettings[] nodesPerIntermediateLayer)
    {

        var settings = new NeuralNetworkSettings();
        settings.layerDefinition = nodesPerIntermediateLayer;

        SaveNewSettings(settings);
    }

    private void SaveNewSettings(NeuralNetworkSettings settings)
    {
        EditorStateManager.NetworkSettings = settings;
    }

    public void Reset()
    {

        var settings = NeuralNetworkSettings.Default;
        SaveNewSettings(settings);
        Refresh();
    }

    public static NeuralNetworkSettings GetNetworkSettings()
    {

        return EditorStateManager.NetworkSettings;
    }

}
