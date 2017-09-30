using NeuralNetMineSweeper.Controllers;
using NeuralNetMineSweeper.NeuronClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace NeuralNetMineSweeper
{
    public class GameManager
    {
        public static double areaWidth = 1900;
        public static double areaHeight = 900;

        public static bool SlowDown = false;
        public static bool AllowCrossBorder = false;
        public static bool RegenerateMine = false;
        public static bool RandomPosition = false;
        public static bool RandomDirection = false;
        public static bool Render = false;
        public static bool ShareMines = false;
        public static int LineUpMines = 0;

        public static double AverageFitness;


        public static int FramesPerEpoch = 5000;
        public int TotalThreads = 5;   //5
        public static int SweeperCount = 20;  //20
        public static int MineCount = 200;

        public static Vector MousePosition;

        public string FileName;
        public List<MineSweeper> Sweepers;
        public List<ThreadController> ThreadControllers = new List<ThreadController>();
        public GameModeController GameController;

        public void InitializeSweepers()
        {
            Sweepers = new List<MineSweeper>();
            for (int i = 0; i < SweeperCount; i++)
            {
                MineSweeper newSweeper = new MineSweeper(new NeuralNet(true));
                Sweepers.Add(newSweeper);
            }
        }

        public void SaveSweepers()
        {

            //NeuralNet net = new NeuralNet();
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(List<MineSweeper>));

                serializer.Serialize(fs, Sweepers);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void LoadSweepers()
        {
            try
            {
                FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                XmlSerializer xs = new XmlSerializer(typeof(List<MineSweeper>));
                Sweepers = (List<MineSweeper>)xs.Deserialize(fs);
                //Sweepers.ForEach(item => item.ResetPhysicalProperties());
            }
            catch
            {
                InitializeSweepers();
            }
        }

        Thread _EvolveThread;
        public void StartEvolveThread()
        {
            if (_EvolveThread != null)
            {
                if (_EvolveThread.IsAlive)
                    return;
            }

            _EvolveThread = new Thread(() =>
            {
                EvolveWorker();
            });
            _EvolveThread.IsBackground = true;
            _EvolveThread.Start();
        }


        void EvolveWorker()
        {
            var AllEpochEndEvents = new AutoResetEvent[TotalThreads];

            for (int i = 0; i < TotalThreads; i++)
            {
                AllEpochEndEvents[i] = ThreadControllers[i].EpochEndEvent;
            }

            while (true)
            {

                WaitHandle.WaitAll(AllEpochEndEvents);
                SelectNextEpochSweepersFromControllers();
                SetNextEpochSweepersForControllers();
                //GameManager.LineUpMines++;
                //if (GameManager.LineUpMines > 2)
                //{
                //    GameManager.LineUpMines = 0;
                //}


            }
        }



        //List<MineSweeper> TopSweepers = new List<MineSweeper>();
        void SelectNextEpochSweepersFromControllers()
        {
            //TopSweepers.ForEach(item => item.Fitness = item.Fitness * 0.95);
            //Put sweepers in all thread together
            List<MineSweeper> allSweepers = new List<MineSweeper>();
            foreach (var threadController in ThreadControllers)
            {
                lock (threadController.lockThis)
                {
                    allSweepers.AddRange(threadController.Sweepers);
                }
            }
            //allSweepers.AddRange(TopSweepers);
            //TopSweepers = allSweepers.OrderByDescending(item => item.Fitness).Take(3).ToList();

            Sweepers = allSweepers;
            AverageFitness = Sweepers.Average(item => item.Fitness);
            SaveSweepers();
        }

        public static DateTime EpochStartTime;
        public static TimeSpan ElaspedTime;

        void SetNextEpochSweepersForControllers()
        {

            //Select and mutate baby sweepers for each thread

            foreach (var threadController in ThreadControllers)
            {
                lock (threadController.lockThis)
                {
                    var parents = SelectParentSweepers(Sweepers, SweeperCount);
                    var babies = BreedBabySweepers(parents);
                    threadController.SetSweepersForNextEpoch(babies);
                }
            }

            foreach (var threadController in ThreadControllers)
            {
                lock (threadController.lockThis)
                {
                    threadController.NewEpochSweeperSetSetEvent.Set();
                }
            }
            ElaspedTime = DateTime.Now - EpochStartTime;
            EpochStartTime = DateTime.Now;

        }


        List<MineSweeper> SelectParentSweepers(List<MineSweeper> allSweepers, int selectCount)
        {
            List<MineSweeper> selectedSweepers = new List<MineSweeper>();
            for (int i = 0; i < selectCount; i++)
            {
                MineSweeper selected = RouletteWheelSelectSweeper(allSweepers);
                selectedSweepers.Add(selected);

            }
            return selectedSweepers;
        }

        List<MineSweeper> BreedBabySweepers(List<MineSweeper> parentSweepers)
        {
            List<MineSweeper> babySweepers = new List<MineSweeper>();
            foreach (var parent in parentSweepers)
            {
                var babySweeper = parent.Clone();
                babySweeper.Mutate();
                babySweepers.Add(babySweeper);
            }

            return babySweepers;
        }


        MineSweeper RouletteWheelSelectSweeper(List<MineSweeper> fromSweepers)
        {
            double totalLuckyFitness = fromSweepers.Sum(item => item.LuckyFitness);//add a small value to prevent all zero
            double fSlice = Ultility.RandomDouble() * totalLuckyFitness;
            double cfTotal = 0;
            foreach (var sweeper in fromSweepers)
            {
                cfTotal += (sweeper.LuckyFitness);
                if (cfTotal >= fSlice)
                {
                    return sweeper;
                }
            }
            throw new Exception("Not found");

            //   double fSlice = RandFloat()*m_dTotalFitnessScore;   

            //。。我们从零到整个适应分范围内随机选取了一实数fSlice 。我喜欢把此数看作整个适应性分数饼图中的一块，如早先在图3.4中所示。 ［但并不是其中一块，译注］   

            //   double cfTotal = O;   
            //   int SelectedGenome = 0;   
            //   for (int i=O; i<m_iPopSize; ++i)   
            //     {   
            //      cfTotal += m_vecGenomes[i].dFitness;   
            //      if (cfTotal > fSlice)   
            //       {   
            //         SelectedGenome = i;   
            //         break;   
            //       }   
            //     }   
            //     return m_vecGenomes[SelectedGenome];   
        }

        public void StartThreads()
        {
            for (int i = 0; i < TotalThreads; i++)
            {
                ThreadController controller = new ThreadController(MineCount);
                ThreadControllers.Add(controller);
                controller.Start();
            }
            SetNextEpochSweepersForControllers();
            StartEvolveThread();
        }


        public void StartPlayMode()
        {
            GameController = new GameModeController(Sweepers.Take(5).ToList());
            GameController.Start();
        }



        public void StopThreads()
        {
            GameController.Stop();
            foreach (var threadController in ThreadControllers)
            {
                threadController.Stop();
            }
            ThreadControllers.Clear();
            if (_EvolveThread != null)
            {
                _EvolveThread.Abort();
                _EvolveThread = null;
            }
        }

        public ThreadController SelectController(int index)
        {
            if (ThreadControllers.Count == 0)
                return null;
            else
            {
                return ThreadControllers[index];
            }
        }

    }
}
