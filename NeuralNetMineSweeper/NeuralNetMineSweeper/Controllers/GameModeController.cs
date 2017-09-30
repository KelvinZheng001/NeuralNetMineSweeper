
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
    public class GameModeController : ControllerBase
    {

        Thread _Thread = null;
        bool StopFlag = false;

        public GameModeController(List<MineSweeper> sweepers)
            : base(0)
        {
            Sweepers = sweepers;
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
            while (StopFlag == false)
            {
                foreach (var sweeper in Sweepers)
                {
                    sweeper.Update(GameManager.MousePosition);
                }
                OnUpdateFrame();
                if (GameManager.SlowDown)
                {
                    Thread.Sleep(10);
                }
            }

        }//GameLoop

    }
}
