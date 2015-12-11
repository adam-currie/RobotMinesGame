using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RobotMinesGame {

    public sealed partial class GetNamePopup : UserControl {
        public event EventHandler<string> GotInput;

        public GetNamePopup() {
            this.InitializeComponent();

            okButton.Click += OkClicked;
        }

        private void OkClicked(object sender, RoutedEventArgs e) {
            if(nameTextBox.Text != null) {
                GotInput(sender, nameTextBox.Text);
            }
        }

        private void nameTextBox_KeyDown(object sender, KeyRoutedEventArgs e) {
            if(e.Key == Windows.System.VirtualKey.Enter) {
                if(nameTextBox.Text != null) {
                    GotInput(sender, nameTextBox.Text);
                }
            }
        }
    }
}
