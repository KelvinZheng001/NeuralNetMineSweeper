using NeuralNetMineSweeper.Controllers;
using NeuralNetMineSweeper.NeuronClasses;
using NeuralNetMineSweeper.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeuralNetMineSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        GameManager _GameManager;
        ControllerBase _RenderGameController = null;
        List<SweeperSprite> SweeperSprites = new List<SweeperSprite>();
        List<Rectangle> MineSprites = new List<Rectangle>();
        public MainWindow()
        {
            InitializeComponent();
            this.Width = GameManager.areaWidth + 30;
            this.Height = GameManager.areaHeight + 120;

            chkSlow.IsChecked = GameManager.SlowDown;
            chkRender.IsChecked = GameManager.Render;

            _GameManager = new GameManager();
            _GameManager.FileName = txtFileName.Text;
            _GameManager.LoadSweepers();

            for (int i = 0; i < _GameManager.TotalThreads; i++)
            {
                ComboSelectThread.Items.Add(i);
            }
            ComboSelectThread.SelectedIndex = 0;


            for (int i = 0; i < GameManager.SweeperCount; i++)
            {
                SweeperSprite sweeperSprite = new SweeperSprite();
                SweeperSprites.Add(sweeperSprite);
                mainCanvas.Children.Add(sweeperSprite);
            }


            for (int i = 0; i < GameManager.MineCount; i++)
            {
                Rectangle mineRect = new Rectangle();
                mineRect.Width = 4;
                mineRect.Height = 4;
                mineRect.Stroke = new SolidColorBrush(Colors.Black);
                MineSprites.Add(mineRect);
                mainCanvas.Children.Add(mineRect);

            }


        }


        void controller_UpdateFrame(object sender, EventArgs e)
        {
            UpdateFrame();
        }

        void UpdateFrame()
        {
            if (GameManager.Render)
            {
                if (GameManager.ShareMines)
                {
                    for (int i = 0; i < this._RenderGameController.Sweepers.Count; i++)
                    {
                        RenderSweeper(i);
                    }
                }
                else
                {
                    RenderSweeper(0);
                }
            }

            labelEpochDuration.Content = GameManager.ElaspedTime.TotalSeconds.ToString("f1");
            labelFrame.Content = _RenderGameController.Frame.ToString();
            labelTotalFitness.Content = _RenderGameController.TotalFitness.ToString("f2");
            labelAverageFitness.Content = GameManager.AverageFitness.ToString("f2");
        }

        void RenderMine(int mineIndex)
        {
            var mine = _RenderGameController.Mines[0][mineIndex];
            var mineRect = MineSprites[mineIndex];
            if (mine.IsSweped)
            {
                mineRect.Stroke = new SolidColorBrush(Colors.LightGreen);
            }
            else
            {
                mineRect.Stroke = new SolidColorBrush(Colors.Black);
            }
            TranslateTransform translateTransform = new TranslateTransform(mine.Postion.X - 2, mine.Postion.Y - 2);
            mineRect.RenderTransform = translateTransform;
        }

        void RenderSweeper(int index)
        {
            SweeperSprite sweeperSprite = SweeperSprites[index];
            MineSweeper sweeper = _RenderGameController.Sweepers[index];
            sweeperSprite.Render(sweeper);

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("Input file name");
                return;
            }
            _GameManager.FileName = txtFileName.Text;
            _GameManager.SaveSweepers();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFileName.Text))
            {
                MessageBox.Show("Input file name");
                return;
            }
            _GameManager.FileName = txtFileName.Text;
            _GameManager.LoadSweepers();
        }


        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _GameManager.StopThreads();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _GameManager.StopThreads();
            _GameManager.InitializeSweepers();

        }

        int ThreadIndex = 0;
        private void ComboSelectThread_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThreadIndex = (int)ComboSelectThread.SelectedItem;
            SelectController(_GameManager.SelectController(ThreadIndex));
        }

        void SelectController(ControllerBase controller)
        {
            if (_RenderGameController != null)
            {
                _RenderGameController.UpdateFrame -= controller_UpdateFrame;
                _RenderGameController.MineUpdated -= _RenderGameController_MineUpdated;
            }
            _RenderGameController = controller;
            if (_RenderGameController != null)
            {
                _RenderGameController.UpdateFrame += controller_UpdateFrame;
                _RenderGameController.MineUpdated += _RenderGameController_MineUpdated;
                //UpdateFrame();
            }
        }

        void _RenderGameController_MineUpdated(object sender, int e)
        {
            RenderMine(e);
        }

        private void chkSlow_Click(object sender, RoutedEventArgs e)
        {
            GameManager.SlowDown = chkSlow.IsChecked.Value;
        }

        private void chkRender_Click(object sender, RoutedEventArgs e)
        {
            GameManager.Render = chkRender.IsChecked.Value;
        }

        private void btnStartGame_Click(object sender, RoutedEventArgs e)
        {
            _GameManager.StartPlayMode();
            SelectController(_GameManager.GameController);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _GameManager.StartThreads();
            SelectController(_GameManager.SelectController(ThreadIndex));
        }



        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseP = Mouse.GetPosition(mainCanvas);
            GameManager.MousePosition.X = mouseP.X;
            GameManager.MousePosition.Y = mouseP.Y;

        }


    }
}
