using NeuralNetMineSweeper.NeuronClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeuralNetMineSweeper.Controllers
{
    public class ThreadController : ControllerBase
    {


        Thread _Thread = null;

        bool StopFlag = false;

        public ThreadController(int mineCount)
            : base(mineCount)
        {

        }

        public void Stop()
        {
            StopFlag = true;
        }

        public void Start()
        {
            StopFlag = false;

            if (_Thread != null)
            {
                if (_Thread.IsAlive)
                    return;
            }

            _Thread = new Thread(() =>
            {
                GameLoop();
            });
            _Thread.IsBackground = true;
            _Thread.Start();
        }


        void GameLoop()
        {

            WaitForNextEpoch();

            while (StopFlag == false)
            {
                lock (lockThis)
                {
                    Frame++;
                    CheckMineHit();
                }
                OnUpdateFrame();

                if (Frame > GameManager.FramesPerEpoch)
                {
                    EndCurrentEpoch();
                    WaitForNextEpoch();
                }
                if (GameManager.SlowDown)
                {
                    Thread.Sleep(3);
                }

            }

        }//GameLoop


        void EndCurrentEpoch()
        {
            lock (lockThis)
            {
                TotalFitness = Sweepers.Sum(item => item.Fitness);
                EpochEndEvent.Set();//Notify current epoch ends
            }
        }

        void WaitForNextEpoch()
        {
            //_Resource.NextEpoch(); 
            NewEpochSweeperSetSetEvent.WaitOne();//Wait for next Epoch data to be set by external
            lock (lockThis)
            {
                Frame = 0;
                Epoch++;
                ResetMines();
            }
            for (int i = 0; i < GameManager.MineCount; i++)
            {
                OnMineUpdated(i);
            }
        }

        void CheckMineHit()
        {
            for (int sweeperIndex = 0; sweeperIndex < Sweepers.Count; sweeperIndex++)
            {
                var sweeper = Sweepers[sweeperIndex];


                int mineListIndex = 0;
                if (GameManager.ShareMines == false)
                    mineListIndex = sweeperIndex;

                if (HasMine(mineListIndex) == false)
                {
                    ResetMines();
                }


                //sweeper.Update();
                sweeper.Update(Mines[mineListIndex]);

                //Check mine hit
                for (int i = 0; i < GameManager.MineCount; i++)
                {
                    if (Mines[mineListIndex][i].IsSweped == false)
                    {
                        if ((sweeper.Position - Mines[mineListIndex][i].Postion).Length < 5.0)
                        {
                            if (GameManager.RegenerateMine)
                            {
                                SetRandomMine(mineListIndex, i);
                            }
                            else
                            {
                                SetSweped(mineListIndex, i);
                            }
                            if (mineListIndex == 0)
                            {
                                OnMineUpdated(i);
                            }
                            sweeper.HitMine();
                        }
                    }
                }



            }
        }




        public AutoResetEvent EpochEndEvent = new AutoResetEvent(false);

        public AutoResetEvent NewEpochSweeperSetSetEvent = new AutoResetEvent(false);


        public void SetSweepersForNextEpoch(List<MineSweeper> sweepers)
        {
            Sweepers = sweepers;

        }

    }
}
