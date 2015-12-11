using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace RobotMinesGame {

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private DisplayOrientations prevOrientation;

        //game settings
        private const int MINE_GRID_SIZE = 15;
        private int numMines = 25;
        private int minesDestroyed;
        private int secondsElapsed;
        private Point robotPos;
        private bool gameDone = false;

        public GamePage() {
            this.InitializeComponent();

            gamePanel.GameTimerTick += GameTimeChanged;
            gamePanel.RobotMoved += RobotMovedHandler;
            gamePanel.MineDestroyed += MineDestroyedHandler;
            minesRemainingText.Text = "Mines Remaining: " + numMines;

            Loaded += NewGame;
        }

        private void NewGame(object sender, RoutedEventArgs e) {
            try {
                secondsElapsed = (int)localSettings.Values["time"];
                minesDestroyed = (int)localSettings.Values["minesDestroyed"];
                string robotPosStr = (string)localSettings.Values["robotPos"];
                string[] pos = robotPosStr.Split(',');
                robotPos = new Point(double.Parse(pos[0]), double.Parse(pos[1]));

                string mineStr = (string)localSettings.Values["minePositions"];
                string[] minePointStings = mineStr.Split('|');

                List<Point> mines = new List<Point>();
                foreach(string s in minePointStings) {
                    string[] minePointStr = s.Split(',');
                    if(minePointStr.Length != 2) {
                        break;
                    }
                    mines.Add(new Point(double.Parse(minePointStr[0]), double.Parse(minePointStr[1])));
                }

                numMines -= minesDestroyed;
                minesRemainingText.Text = "Mines Remaining: " + numMines;
                minesDestroyedText.Text = "Mines Destroyed: " + minesDestroyed;
                gamePanel.StartGame(MINE_GRID_SIZE, mines, robotPos, true);

                gamePanel.Paused = true;
                pauseResumeButton.Icon = new SymbolIcon(Symbol.Play);
            } catch(NullReferenceException) {
                minesDestroyed = 0;
                secondsElapsed = 0;
                robotPos = new Point(0, 0);
                gamePanel.StartGame(MINE_GRID_SIZE, numMines, false);//if any of the settings are null, start new game
            }
        }

        private void MineDestroyedHandler(object sender, EventArgs e) {
            numMines--;
            minesDestroyed++;
            minesRemainingText.Text = "Mines Remaining: " + numMines;
            minesDestroyedText.Text = "Mines Destroyed: " + minesDestroyed;

            //game won
            if(numMines <= 0) {
                gamePanel.Paused = true;
                gameDone = true;
                namePopup.IsOpen = true;
                ((GetNamePopup)(namePopup.Child)).GotInput += (popupSender, name) => {
                    namePopup.IsOpen = false;
                    Frame.Navigate(typeof(LeaderboardPage), new LeaderboardScore(name, secondsElapsed));
                };
            }
        }

        private void RobotMovedHandler(object sender, Point robotPos) {
            this.robotPos = robotPos;
            robotXText.Text = "Robot X: " + robotPos.X;
            robotYText.Text = "Robot Y: " + robotPos.Y;
        }

        private void Suspending(object sender, SuspendingEventArgs e) {
            SaveGameState();
        }

        private void SaveGameState() {
            if(gameDone) {
                localSettings.Values["time"] = null;
                localSettings.Values["robotPos"] = null;
                localSettings.Values["minesDestroyed"] = null;
                localSettings.Values["minePositions"] = null;
            } else {
                localSettings.Values["time"] = secondsElapsed;
                localSettings.Values["robotPos"] = robotPos.X + "," + robotPos.Y;
                localSettings.Values["minesDestroyed"] = minesDestroyed;

                string minePositions = "";
                foreach(Point minePos in gamePanel.MinePositions) {
                    minePositions += minePos.X + "," + minePos.Y + "|";
                }
                localSettings.Values["minePositions"] = minePositions;
            }
        }

        private void GameTimeChanged(object sender, object e) {
            secondsElapsed++;
            timeText.Text = "Time elapsed: " + secondsElapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            //save old orientation preference and set to landscape only(reverted in OnNavigateFrom)
            prevOrientation = DisplayInformation.AutoRotationPreferences;
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            //listen for app suspending on this page only(removed in OnNavigateFrom)
            Application.Current.Suspending += new SuspendingEventHandler(Suspending);
        }
        
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            //restore old orientation preference
            DisplayInformation.AutoRotationPreferences = prevOrientation;

            //listen for app suspending on this page only(added in OnNavigateTo)
            Application.Current.Suspending -= new SuspendingEventHandler(Suspending);

            SaveGameState();
        }

        private void gamePageGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            double boundsHeight = gamePageGrid.ActualHeight - (gamePanel.Margin.Top + gamePanel.Margin.Bottom);
            double boundsWidth = gamePageGrid.ColumnDefinitions[1].ActualWidth - (gamePanel.Margin.Left + gamePanel.Margin.Right);

            if(boundsHeight > boundsWidth) {
                gamePanel.Height = boundsWidth;
                gamePanel.Width = boundsWidth;
            } else {
                gamePanel.Height = boundsHeight;
                gamePanel.Width = boundsHeight;
            }
        }

        private void leaderboardButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(LeaderboardPage));
        }

        private void pauseResumeButton_Click(object sender, RoutedEventArgs e) {
            if(gamePanel.Paused) {
                gamePanel.Paused = false;
                pauseResumeButton.Icon = new SymbolIcon(Symbol.Pause);
            } else {
                gamePanel.Paused = true;
                pauseResumeButton.Icon = new SymbolIcon(Symbol.Play);
            }
        }
    }

}
