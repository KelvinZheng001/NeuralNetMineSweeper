using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NeuralNetMineSweeper.NeuronClasses
{
    [Serializable]
    public class MineSweeper
    {
        public static double dMaxTurnRate = 1;


        public NeuralNet _Brain = null;

        public double Fitness;



        public double LuckyFitness
        {
            get { return Fitness + 0.00001; }

        }
        // 它在世界坐标里的位置
        public Vector Position;

        // 扫雷机面对的方向

        public double directionAngle;
        public Vector directionVector;

        [XmlIgnore]
        public Mine Target;

        // 它的旋转(surprise surprise)
        double _RotationForce;

        double m_dSpeed;

        // 根据ANN保存输出
        double m_lTrack;
        double m_rTrack;

        public MineSweeper()
        {
        }

        public MineSweeper(NeuralNet brain)
        {
            ResetPhysicalProperties();
            _Brain = brain;
        }

        double MoveDistance = 0;
        double SumRotateAngle = 0;

        public void HitMine()
        {
            double a = 0.999;
            //List<double> list = new List<double>();
            //for (int i = 0; i < 20; i++)
            //{
            //    list.Add(Math.Pow(0.99233, i * 10));
            //}
            //double moveScore = 
            //Fitness += (Math.Pow(a, MoveDistance));
            Fitness += 1;
            double rotationScore = ((Math.Pow(0.99233, SumRotateAngle)) - 0.5) * 2;
            Fitness += rotationScore;

            //Fitness += 1;
            MoveDistance = 0;
            SumRotateAngle = 0;
            Target = null;
            Target = GetClosestMine(Mines);
        }

        /// <summary>
        /// Reset physical properties, not including brain
        /// </summary>
        public void ResetPhysicalProperties()
        {
            if (GameManager.RandomPosition)
            {
                Position.X = Ultility.RandomDouble() * GameManager.areaWidth;
                Position.Y = Ultility.RandomDouble() * GameManager.areaHeight;
            }
            else
            {
                Position.X = GameManager.areaWidth / 2;
                Position.Y = GameManager.areaHeight / 2;
            }

            if (GameManager.RandomDirection)
            {
                directionAngle = Ultility.RandomDouble() * 360;
                directionVector = Ultility.AngleToVector(directionAngle);
            }
            else
            {
                directionAngle = 0;
                directionVector = Ultility.AngleToVector(directionAngle);
            }

            Fitness = 0;
            MoveDistance = 0;
            SumRotateAngle = 0;

        }


        List<Mine> Mines = null;
        public void Update(List<Mine> mines)
        {
            Mines = mines;

            //计算从扫雷机到与其最接近的地雷（2个点）之间的向量
            if (GameManager.ShareMines == false)
            {
                if (Target == null || Target.IsSweped)
                {
                    Target = GetClosestMine(mines);
                }
            }
            else
            {
                Target = GetClosestMine(mines);
            }
            ThinkAndMove();
        }

        public void Update(Vector target)
        {
            if (Target == null)
            {
                Target = new Mine(GameManager.areaHeight, GameManager.areaHeight);
            }
            Target.Postion = target;
            ThinkAndMove();
        }

        void ThinkAndMove()
        {
            var closestMineNormalized = Position - Target.Postion;
            closestMineNormalized.Normalize();

            //这一向量用来存放神经网络所有的输入
            List<double> inputs = new List<double>();

            inputs.Add(closestMineNormalized.X);
            inputs.Add(closestMineNormalized.Y);
            directionVector = Ultility.AngleToVector(directionAngle);
            inputs.Add(directionVector.X);
            inputs.Add(directionVector.Y);
            //var temp = Position;
            //temp.Normalize();
            //inputs.Add(Position.X / GameManager.areaWidth);
            //inputs.Add(Position.Y / GameManager.areaHeight);

            _Brain.Update(inputs);

            // 把输出赋值到扫雷机的左、右轮轨
            m_lTrack = _Brain.Output[0];
            m_rTrack = _Brain.Output[1];

            //计算运动
            UpdateRatation();
            UpdatePosition();
        }

        void UpdateRatation()
        {
            // 计算驾驶的力
            _RotationForce = m_lTrack - m_rTrack;

            double rotateAngle = dMaxTurnRate * _RotationForce;

            // 进行左转或右转
            directionAngle += rotateAngle; //rotateMatrix.Transform(m_vLookAtAngle);

            SumRotateAngle += Math.Abs(rotateAngle);
        }


        void UpdatePosition()
        {
            m_dSpeed = (m_lTrack + m_rTrack);

            Matrix rotateMatrix = Matrix.Identity;
            rotateMatrix.Rotate(directionAngle);

            Vector direction = new Vector(1, 0);
            direction = rotateMatrix.Transform(direction);
            var moveVector = direction * m_dSpeed;

            Matrix moveMatrix = Matrix.Identity;
            moveMatrix.Translate(moveVector.X, moveVector.Y);
            //m_vPosition = moveMatrix.Transform(m_vPosition);


            if (GameManager.AllowCrossBorder)
            {
                Position += moveVector;
                MoveDistance += moveVector.Length;

                if (Position.X > GameManager.areaWidth) Position.X = 0;
                if (Position.X < 0) Position.X = GameManager.areaWidth;
                if (Position.Y > GameManager.areaHeight) Position.Y = 0;
                if (Position.Y < 0) Position.Y = GameManager.areaHeight;
            }
            else
            {
                var previousPosition = Position;
                Position += moveVector;
                if (Position.X > GameManager.areaWidth) Position.X = GameManager.areaWidth;
                if (Position.X < 0) Position.X = 0;
                if (Position.Y > GameManager.areaHeight) Position.Y = GameManager.areaHeight;
                if (Position.Y < 0) Position.Y = 0;
                moveVector = Position - previousPosition;
                MoveDistance += moveVector.Length;

            }
        }

        Mine GetClosestMine(List<Mine> mines)
        {
            mines = mines.Where(item => item.IsSweped == false).ToList();
            if (mines.Count == 0)
            {
                return null;
            }
            var closestMine = mines.OrderBy(item => (item.Postion - Position).Length).First();
            return closestMine;

        }

        public MineSweeper Clone()
        {
            var clonedBrain = this._Brain.Clone();
            MineSweeper clonedSweeper = new MineSweeper(clonedBrain);
            return clonedSweeper;
        }

        public void Mutate()
        {
            this._Brain.Mutate();

        }

    }
}

