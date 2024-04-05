using UnityEngine;
using Unity.Sentis.Layers;
using Unity.Sentis;
using System;

public class ClassifyHandwrittenDigit : MonoBehaviour
{
    [SerializeField] private Texture2D inputTexture;
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private float[] results;
    [SerializeField] private FingerDrawing fingerDrawing;

    private Model runtimeModel;
    private IWorker worker;
    private TensorFloat inputTensor;
    
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);

        // * add Solfmax Layer
        string softmaxOutputName = "Softmax_Output";
        runtimeModel.AddLayer(new Softmax(softmaxOutputName, runtimeModel.outputs[0]));
        runtimeModel.outputs[0] = softmaxOutputName; 

        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
    }

    public void ExecuteModel(Texture2D inputTexture)
    {
        inputTensor?.Dispose();
        inputTensor = TextureConverter.ToTensor(inputTexture, 28, 28, 1);

        worker.Execute(inputTensor);

        TensorFloat outputTensor = worker.PeekOutput() as TensorFloat;
        outputTensor.MakeReadable();
        results = outputTensor.ToReadOnlyArray();
        outputTensor.Dispose();

        fingerDrawing.ClearTexture();
    }
}
