using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeuralNetMineSweeper.NeuronClasses
{
    public class Mine
    {
        public Vector Postion;
        public bool IsSweped = false;
        public Mine(double width, double height)
        {
            Postion = new Vector(Ultility.RandomDouble() * width, Ultility.RandomDouble() * height);
        }
    }
}
