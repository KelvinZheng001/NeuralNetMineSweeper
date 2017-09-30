using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetMineSweeper.NeuronClasses
{
    [Serializable]
    public class NeuralNet
    {
        static double m_dMutationRate = 0.1;
        static double dMaxPerturbatlon = 0.1;

        public int InputCount;
        public int OutputCount;
        public int HiddenLayerCount;
        public int NeuronsPerHiddenLayer;

        public List<List<Neuron>> NeuronLayers = new List<List<Neuron>>();
        public List<double> Output;

        public NeuralNet()
        {


        }

        public NeuralNet(bool createNet)
            : this()
        {
            InputCount = 4;
            OutputCount = 2;
            HiddenLayerCount = 1;
            NeuronsPerHiddenLayer = 10;
            CreateNet();
        }

        public NeuralNet Clone()
        {
            NeuralNet cloned = new NeuralNet(true);
            for (int i = 0; i < NeuronLayers.Count; i++)
            {
                var neuronLayer = NeuronLayers[i];
                // copy weights to the cloned neural net.
                for (int j = 0; j < neuronLayer.Count; j++)
                {
                    cloned.NeuronLayers[i][j].ApplyWeights(NeuronLayers[i][j].InputWeights);
                }
            }
            return cloned;
        }


        List<Neuron> CreateNeuronLayer(int neuronCount, int inputCount)
        {
            List<Neuron> result = new List<Neuron>();

            for (int i = 0; i < neuronCount; i++)
            {
                Neuron neuron = new Neuron(inputCount);
                result.Add(neuron);
            }

            return result;
        }

        public void CreateNet()
        {
            // 创建网络的各个层
            for (int i = 0; i < HiddenLayerCount; i++)
            {
                if (i == 0)
                {
                    NeuronLayers.Add(CreateNeuronLayer(NeuronsPerHiddenLayer, InputCount));//neurons in first hidden layer is input count is the neuralNet's input count
                }
                else
                {
                    NeuronLayers.Add(CreateNeuronLayer(NeuronsPerHiddenLayer, NeuronsPerHiddenLayer)); //other layer's neuron input count is the count of the under layer.
                }
            }

            NeuronLayers.Add(CreateNeuronLayer(OutputCount, NeuronsPerHiddenLayer));
        }

        public void Update(List<double> inputs)
        {
            // 保存从每一层产生的输出
            List<double> outputs = null;


            // 首先检查输入的个数是否正确
            if (inputs.Count != InputCount)
            {
                // 如果不正确，就返回一个空向量
                throw new Exception("Input count mismatch");
            }

            // 对每一层,调用UPDATE
            for (int i = 0; i < NeuronLayers.Count; i++)
            {
                var neuronLayer = NeuronLayers[i];
                if (i > 0)
                {
                    inputs = outputs;
                }

                // 对每个神经细胞,求输入*对应权重乘积之总和。并将总和抛给S形函数,以计算输出
                for (int j = 0; j < neuronLayer.Count; j++)
                {
                    var newron = neuronLayer[j];
                    newron.Update(inputs);

                }

                outputs = neuronLayer.Select(neuron => neuron.output).ToList();
            }

            Output = outputs;

        }

        public void Mutate()
        {
            foreach (var layer in NeuronLayers)
            {
                foreach (var neuron in layer)
                {

                    for (int i = 0; i < neuron.InputWeights.Count; i++)
                    {
                        if (Ultility.RandomDouble() < m_dMutationRate)
                        {
                            neuron.InputWeights[i] += Ultility.RandomClamped() * dMaxPerturbatlon;
                        }
                    }


                    // 遍历权重向量，按突变率将每一个权重进行突变
                    //for (int i=0; i<chromo.size(); ++i)
                    // {
                    //    // 我们要骚扰这个权重吗？
                    //   if (RandFloat() < m_dMutationRate)
                    //    {
                    //      // 为权重增加或减小一个小的数量
                    //     chromo[i] += (RandomClamped() * CParams::dMaxPerturbatlon);
                    //    }
                    // }
                }
            }
        }

    }
}
