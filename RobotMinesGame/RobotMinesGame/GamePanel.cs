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
        private int gamePlayHeight;//height of game area in game units
        private int gamePlayWidth;//Width of game area in game units
        private List<GameImage> images = new List<GameImage>();

        public void NewGame(int gamePlayHeight, int gamePlayWidth, int numOfMines) {
            this.gamePlayHeight = gamePlayHeight;
            this.gamePlayWidth = gamePlayWidth;

            Image robotImage = new Image();
            Uri uri = new Uri("ms-appx:///Assets/robot.png", UriKind.RelativeOrAbsolute);
            robotImage.Source = new BitmapImage(uri);
            GameImage robot = new GameImage(robotImage, this, 10, 10, 100, 100);
            
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

                this.containerRightX = container.gamePlayHeight;
                this.containerBottomY = container.gamePlayWidth;
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
