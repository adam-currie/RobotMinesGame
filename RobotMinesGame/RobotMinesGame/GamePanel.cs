using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace RobotMinesGame {

    class GamePanel : Panel{
        private int gamePlaySize;//width and height of game area in game units
        private int robotSize = 100;
        private int gridRowsAndCols = 12;//width and height of grid
        private List<GameImage> images = new List<GameImage>();

        public void NewGame(int gamePlaySize, int numOfMines) {
            this.gamePlaySize = gamePlaySize;

            bool[,] grid = new bool[gridRowsAndCols, gridRowsAndCols];//true when occupied
            Uri mineUri = new Uri("ms-appx:///Assets/landmine.png", UriKind.RelativeOrAbsolute);
            Random r = new Random();
            for(int i = 0; i < numOfMines; i++) {
                int gridX, gridY;
                do {
                    gridX = r.Next(0, gridRowsAndCols);
                    gridY = r.Next(0, gridRowsAndCols);
                } while(grid[gridX,gridY]);//loop until untaken
                grid[gridX,gridY] = true;


                Image mineImage = new Image();
                mineImage.Source = new BitmapImage(mineUri);
                GameImage mine = new GameImage(
                    mineImage, 
                    this, 
                    gridX*(gamePlaySize/gridRowsAndCols),
                    gridY*(gamePlaySize/gridRowsAndCols),
                    gamePlaySize/gridRowsAndCols,
                    gamePlaySize/gridRowsAndCols
                );
                Children.Add(mine.img);
                images.Add(mine);
            }

            Image robotImage = new Image();
            Uri robotUri = new Uri("ms-appx:///Assets/robot.png", UriKind.RelativeOrAbsolute);
            robotImage.Source = new BitmapImage(robotUri);
            GameImage robot = new GameImage(robotImage, this, gamePlaySize/10, this.gamePlaySize/10, robotSize, robotSize);
            
            Children.Add(robot.img);
            images.Add(robot);

            //debug: just here to test movement
            robot.img.PointerPressed += (new PointerEventHandler(
                (object s, PointerRoutedEventArgs v) => {
                    robot.X = robot.X + 5;
                }
            ));
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

            public GameImage(Image img, GamePanel container, int x, int y, double width, double height) {
                this.img = img;
                this.container = container;
                this.container.SizeChanged += ContainerSizeChanged;

                this.containerRightX = container.gamePlaySize;
                this.containerBottomY = container.gamePlaySize;
                X = x;
                Y = y;
                Width = width;
                Height = height;
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
