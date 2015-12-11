using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace RobotMinesGame {

    class GamePanel : Panel{
        private const int GAME_PLAY_SIZE = 1000;//width and height of game area in game units
        private const int ROBOT_SIZE = 100;
        private const float SPEED_FACTOR = 2;

        private List<GameImage> mines = new List<GameImage>();
        private Inclinometer inclinometer = null;
        private DispatcherTimer timer = null;
        private GameImage robot;
        private bool paused = true;

        public List<Point> MinePositions {
            get{
                List<Point> minePositions = new List<Point>();
                foreach(GameImage mine in mines) {
                    minePositions.Add(new Point(mine.X, mine.Y));
                }
                return minePositions;
            }
        }

        public event EventHandler<object> GameTimerTick;
        public event EventHandler<Point> RobotMoved;
        public event EventHandler MineDestroyed;

        public GamePanel() : base() {
            inclinometer = Inclinometer.GetDefault();
        }

        private async void Tilted(Inclinometer sender, InclinometerReadingChangedEventArgs args) {
            int xChange = (int)((args.Reading.PitchDegrees*SPEED_FACTOR)+.5);
            int yChange = (int)((args.Reading.RollDegrees*-SPEED_FACTOR)+.5);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                bool robotMoved = false;
                
                //X
                int newX = robot.X+xChange;
                if(newX > (int)(GAME_PLAY_SIZE-robot.Height)) {
                    newX = (int)(GAME_PLAY_SIZE-robot.Height);
                } else if(newX < 0) {
                    newX = 0;
                }
                if(newX != robot.X) {
                    robot.X = newX;
                    robotMoved = true;
                }

                //Y
                int newY = robot.Y+yChange;
                if(newY > (int)(GAME_PLAY_SIZE-robot.Width)) {
                    newY = (int)(GAME_PLAY_SIZE-robot.Width);
                } else if(newY < 0) {
                    newY = 0;
                }
                if(newY != robot.Y) {
                    robot.Y = newY;
                    robotMoved = true;
                }

                if(robotMoved) {
                    //check for collision with mines
                    List<GameImage> minesToRemove = new List<GameImage>();
                    foreach(GameImage mine in mines) {
                        if(Math.Abs((robot.X+(robot.Width/2)) - (mine.X+(mine.Width/2))) < ((mine.Width+robot.Width)/2)) {
                            if(Math.Abs((robot.Y+(robot.Height/2)) - (mine.Y+(mine.Height/2))) < ((mine.Height+robot.Height)/2)) {
                                minesToRemove.Add(mine);
                            }
                        }
                    }

                    //remove colliding mines
                    foreach(GameImage mineToRemove in minesToRemove) {
                        this.Children.Remove(mineToRemove.img);
                        mines.Remove(mineToRemove);
                        MineDestroyed(this, EventArgs.Empty);
                    }

                    RobotMoved(this, new Point( robot.X, robot.Y));
                }

            });

        }

        public void StartGame(int mineGridSize, int numOfMines, bool startPaused) {
            List<Point> minePoints = new List<Point>();

            bool[,] grid = new bool[mineGridSize, mineGridSize];//true when occupied
            Random r = new Random();
            for(int i = 0; i < numOfMines; i++) {
                Point p = new Point();

                do {
                    p.X = r.Next(0, mineGridSize);
                    p.Y = r.Next(0, mineGridSize);
                } while(grid[(int)p.X, (int)p.Y]);//loop until untaken
                grid[(int)p.X, (int)p.Y] = true;

                p.X = p.X*(GAME_PLAY_SIZE/mineGridSize);
                p.Y = p.Y*(GAME_PLAY_SIZE/mineGridSize);

                minePoints.Add(p);
            }

            StartGame(mineGridSize, minePoints, new Point(0, 0), startPaused);
        }

        public void StartGame(int mineGridSize, List<Point> minePoints, Point robotPos, bool startPaused) {
            //remove any old stuff
            Children.Clear();
            mines.Clear();

            Uri mineUri = new Uri("ms-appx:///Assets/landmine.png", UriKind.RelativeOrAbsolute);
            foreach(Point minePoint in minePoints) {
                Image mineImage = new Image();
                mineImage.Source = new BitmapImage(mineUri);
                GameImage mine = new GameImage(mineImage, this);
                mine.X = (int)minePoint.X;
                mine.Y = (int)minePoint.Y;
                mine.Width = GAME_PLAY_SIZE/mineGridSize;
                mine.Height = GAME_PLAY_SIZE/mineGridSize;

                Children.Add(mine.img);
                mines.Add(mine);
            }

            Image robotImage = new Image();
            Uri robotUri = new Uri("ms-appx:///Assets/robot.png", UriKind.RelativeOrAbsolute);
            robotImage.Source = new BitmapImage(robotUri);
            robot = new GameImage(robotImage, this);
            robot.X = (int)robotPos.X;
            robot.Y = (int)robotPos.Y;
            robot.Width = ROBOT_SIZE;
            robot.Height = ROBOT_SIZE;

            Children.Add(robot.img);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            Paused = startPaused;
        }

        public bool Paused {
            get {
                return paused;
            }
            set{
                if(paused == true && value == false) {
                    paused = false;
                    timer.Tick += GameTimerTick;
                    inclinometer.ReadingChanged += Tilted;
                }else if(paused == false && value == true) {
                    paused = true;
                    timer.Tick -= GameTimerTick;
                    inclinometer.ReadingChanged -= Tilted;
                }
            }
        }

        class GameImage {
            public Image img;
            private Panel container;
            private double height;
            private double width;
            private int x;
            private int y;
            private int containerRightX;
            private int containerBottomY;

            public GameImage(Image img, GamePanel container) {
                this.img = img;
                this.container = container;
                this.container.SizeChanged += ContainerSizeChanged;
                this.containerRightX = GamePanel.GAME_PLAY_SIZE;
                this.containerBottomY = GamePanel.GAME_PLAY_SIZE;
            }

            public double Width{
                get{
                    return width;
                }
                set{
                    width = value;
                    img.Width = width*(container.ActualWidth/containerRightX);
                }
            }

            public double Height{
                get{
                    return height;
                }
                set{
                    height = value;
                    img.Height = height*(container.ActualHeight/containerBottomY);
                }
            }

            public int X{
                get{
                    return x;
                }
                set{
                    x = value;
                    Thickness m = img.Margin;
                    m.Left = x*(container.ActualWidth/containerRightX);
                    img.Margin = m;
                }
            }

            public int Y{
                get{
                    return y;
                }
                set{
                    y = value;
                    Thickness m = img.Margin;
                    m.Top = y*(container.ActualHeight/containerBottomY);
                    img.Margin = m;
                }
            }

            public void ContainerSizeChanged(object sender, SizeChangedEventArgs e) {
                //re-evaluate all values
                Width = Width;
                Height = Height;
                X = X;
                Y = Y;
            }

        }

    }

}
