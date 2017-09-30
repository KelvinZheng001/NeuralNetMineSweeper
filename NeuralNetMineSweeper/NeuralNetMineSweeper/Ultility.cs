using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeuralNetMineSweeper
{
    public class Ultility
    {
        static object lockThis = new object();
        static Random Random = new Random();
        public static double RandomDouble()
        {
            lock (lockThis)
            {
                return Random.NextDouble();
            }
        }

        public static double RandomClamped()
        {
            lock (lockThis)
            {
                return Random.NextDouble() * 2 - 1;
            }
        }

        public static Vector AngleToVector(double angle)
        {
            lock (lockThis)
            {
                Vector vector = new Vector(0, 1);
                Matrix rotateMatrix = Matrix.Identity;
                rotateMatrix.Rotate(angle);
                vector = rotateMatrix.Transform(vector);
                return vector;
            }
        }

    }
}
