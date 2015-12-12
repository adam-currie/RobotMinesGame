using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RobotMinesGame {

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LeaderboardPage : Page {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public LeaderboardPage() {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if(e.Parameter != null) {
                LeaderboardScore score = (LeaderboardScore)e.Parameter;
                localSettings.Values["leaderboard"] += score.Name + '\n' + score.Time + '|';
            }

            if(localSettings.Values["leaderboard"] != null) {
                List<LeaderboardScore> scores = new List<LeaderboardScore>();

                string leaderboardStr = (string)localSettings.Values["leaderboard"];
                string[] scoreStrings = leaderboardStr.Split('|');

                foreach(string scoreStr in scoreStrings) {
                    string[] values = scoreStr.Split('\n');
                    if(values.Length != 2) {
                        break;
                    }

                    string name = values[0];
                    int time = int.Parse(values[1]);

                    //go throught scores until new score's time is greater than score n's time, insert after score n
                    int i = 0;
                    while(i < scores.Count) {
                        if(time > scores[i].Time) {
                            break;
                        }
                        i++;
                    }
                    scores.Insert(i, new LeaderboardScore(name, time));
                }

                foreach(LeaderboardScore score in scores) {
                    TextBlock nameText = new TextBlock();
                    nameText.Text = score.Name;
                    namePanel.Children.Add(nameText);

                    TextBlock timeText = new TextBlock();
                    timeText.Text = score.Time.ToString();
                    timePanel.Children.Add(timeText);
                }
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            if(rootFrame != null && rootFrame.CanGoBack) {
                rootFrame.GoBack();
            }
        }
    }


    public class LeaderboardScore{
        private string name;
        private int time;

        public LeaderboardScore(string name, int time) {
            this.name = name;
            this.time = time;
        }

        public string Name{
            get { return name; }
        }
        public int Time{
            get { return time; }
        }

    }

}
