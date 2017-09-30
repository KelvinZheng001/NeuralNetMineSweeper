using NeuralNetMineSweeper.NeuronClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeuralNetMineSweeper.Controllers
{
    public class ControllerBase
    {
        public object lockThis = new object();

        public double TotalFitness;
        public int Epoch = 0;
        public int Frame = 0;

        //public ThreadResource Resource;


        public List<MineSweeper> Sweepers;
        public List<List<Mine>> Mines;
        int MineCount;
        public int[] SwepedMine = new int[GameManager.SweeperCount];


        public string FileName;

        public ControllerBase(int mineCount)
        {
            MineCount = mineCount;
            ResetMines();
        }

        public void ResetMines()
        {
            Mines = new List<List<Mine>>();
            for (int i = 0; i < GameManager.SweeperCount; i++)
            {
                Mines.Add(new List<Mine>());
                for (int j = 0; j < MineCount; j++)
                {
                    Mine newMine = new Mine(GameManager.areaWidth, GameManager.areaHeight);
                    if (GameManager.LineUpMines == 1)
                    {
                        newMine.Postion.X = j * (GameManager.areaWidth / MineCount);
                        newMine.Postion.Y = 100;
                    }
                    else if (GameManager.LineUpMines == 2)
                    {
                        int row = (int)(j / 14);
                        int column = (int)(j % 14);
                        newMine.Postion.X = column * (GameManager.areaWidth / 14);
                        newMine.Postion.Y = row * (GameManager.areaHeight / 14); ;
                    }

                    Mines[i].Add(newMine);
                    OnMineUpdated(j);
                }

            }
            SwepedMine = new int[GameManager.SweeperCount];
        }




        public bool HasMine(int sweeperIndex)
        {
            return SwepedMine[sweeperIndex] < MineCount;
        }

        public void SetSweped(int sweeperIndex, int mineIndex)
        {
            Mines[sweeperIndex][mineIndex].IsSweped = true;
            SwepedMine[sweeperIndex]++;
        }

        public void SetRandomMine(int sweeperIndex, int mineIndex)
        {
            Mines[sweeperIndex][mineIndex] = new Mine(GameManager.areaWidth, GameManager.areaHeight);
        }



        public event EventHandler UpdateFrame;
        protected void OnUpdateFrame()
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    lock (lockThis)
                    {
                        if (UpdateFrame != null)
                        {
                            UpdateFrame(this, null);
                        }
                    }
                }));
            }

        }



        public event EventHandler<int> MineUpdated;
        protected void OnMineUpdated(int mineIndex)
        {
            if (GameManager.Render == false) return;
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (MineUpdated != null)
                    {
                        MineUpdated(this, mineIndex);
                    }
                }));
            }
        }

    }
}
