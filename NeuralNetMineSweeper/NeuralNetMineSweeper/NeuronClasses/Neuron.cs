using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetMineSweeper.NeuronClasses
{
    [Serializable]
    public class Neuron
    {
        public static double Bias = -1;
        public static double ActivationResponse = 1;
        // 进入神经细胞的输入个数
        public int _InputCount;

        // 为每一输入提供的权重 + 为 Bias提供的权重
        public List<double> InputWeights;

        public double output;

        public Neuron()
        {
        }
        //构造函数
        public Neuron(int inputCount)
        {

            _InputCount = inputCount;
            // 我们要为偏移值也附加一个权重，因此输入数目上要 +1
            InputWeights = new List<double>();

            for (int i = 0; i < inputCount + 1; ++i)
            {
                // 把权重初始化为任意的值
                InputWeights.Add(Ultility.RandomClamped());
            }
        }

        public void Update(List<double> inputs)
        {
            if (inputs.Count != _InputCount)
            {
                throw new Exception("Input count mismatch");
            }

            double netinput = 0;
            for (int i = 0; i < _InputCount; i++)
            {
                netinput += InputWeights[i] * inputs[i];
            }

            //别忘记每个神经细胞的权重向量的最后一个权重实际是偏移值，这我们已经说明过了，我们总是将它设置成为 –1的。我已经在ini文件中包含了偏移值，你可以围绕它来做文章，考察它对你创建的网络的功能有什么影响。不过，这个值通常是不应该改变的。
            netinput += InputWeights[_InputCount] * Bias;

            output = Sigmoid(netinput, ActivationResponse);

            //                     CParams::dBias;
            //           double netinput = 0;

            //           int NumInputs = m_vecLayers[i].m_vecNeurons[j].m_NumInputs;

            //          // 对每一个权重
            //         for (int k=0; k<NumInputs-l; ++k)
            //          { 
            //             // 计算权重*输入的乘积的总和。
            //            netinput += m_vecLayers[i].m_vecNeurons[j].m_vecWeight[k] *
            //                     inputs[cWeight++];
            //          }

            //         // 加入偏移值
            //        netinput += m_vecLayers[i].m_vecNeurons[j].m_vecWeight[NumInputs-1] *
            //                     CParams::dBias;

            //   别忘记每个神经细胞的权重向量的最后一个权重实际是偏移值，这我们已经说明过了，我们总是将它设置成为 –1的。我已经在ini文件中包含了偏移值，你可以围绕它来做文章，考察它对你创建的网络的功能有什么影响。不过，这个值通常是不应该改变的。

            //     // 每一层的输出，当我们产生了它们后，我们就要将它们保存起来。但用Σ累加在一起的
            //     // 激励总值首先要通过S形函数的过滤，才能得到输出
            //outputs.push_back(); cWeight = 0:

        }

        public double Sigmoid(double netInput, double activationResponse)
        {
            return 1 / (1 + Math.Exp(netInput / activationResponse));
            //return 1 / (1 + Math.Exp(netInput / activationResponse)) ; //between 0 to 1
            //return 2 / (1 + Math.Exp(netInput / activationResponse)) - 1;  //between -1 to 1
        }


        public void ApplyWeights(List<double> weights)
        {
            if (InputWeights.Count != weights.Count)
            {
                throw new Exception("Count mismatch");
            }

            for (int i = 0; i < InputWeights.Count; i++)
            {
                InputWeights[i] = weights[i];
            }
        }
    }
}
