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

namespace KinectV2MascotFace.Controls
{
    /// <summary>
    /// FaceSelectorControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FaceSelectorControl : UserControl
    {
        private RadioButton checkedRadio;
        public event RoutedEventHandler SetFace;

        public FaceSelectorControl()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // チェックされているRadioの情報を取得
            checkedRadio = sender as RadioButton;
            // けってい！ボタンを押せるようにする
            SetButton.IsEnabled = true;
            // 顔を変更する
            if (SetFace != null)
            {
                SetFace.Invoke(sender, e);
            }
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetFace != null)
            {
                SetFace.Invoke(sender, e);
            }
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        public Mascot GetSelectedFace()
        {
            Mascot selectedMascot = Mascot.Claudia;
            switch (checkedRadio.Name.ToString())
            {
                case "Anzu":
                    selectedMascot = Mascot.Anzu;
                    break;
                case "Conoha":
                    selectedMascot = Mascot.Conoha;
                    break;
                case "Claudia":
                    selectedMascot = Mascot.Claudia;
                    break;
                case "Pronama":
                    selectedMascot = Mascot.Pronama;
                    break;
                case "Query":
                    selectedMascot = Mascot.Query;
                    break;
                case "Unity":
                    selectedMascot = Mascot.Unity;
                    break;
                default:
                    selectedMascot = Mascot.Unknown;
                    break;
            }
            return selectedMascot;
        }
    }
}
