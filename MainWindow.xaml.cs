using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button? submit1;

        private Button? clear;
        private object colorAnswer;
        private object colorQuestion;
        private NumPlace numPlace;

        public MainWindow()
        {
            InitializeComponent();

            string fullPath = Assembly.GetExecutingAssembly().Location;
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(fullPath);
            Title = info.ProductName + " " + info.FileVersion.ToString();

            colorQuestion = rdoQuestion.Foreground;
            colorAnswer= rdoAnswer.Foreground;

            numPlace = new NumPlace();
            numPlace.first();

            initView();

            var lst = numPlace.listGame();
            if (lst.Count > 0 )
            {
                lstHistory.ItemsSource = lst;
                lstHistory.SelectedIndex = lst.Count - 1;
            }
            I0.Content = "";
        }
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            numPlace.final();
        }

        private void initView(int index = -1)
        {
            Play play = numPlace.loadPlay(index);
            backImage.ImageSource = null;
            foreach (string a in Constants.CELLNAME)
            {
                object obj = FindName(a);
                Button btn = (Button)obj;
                btn.Content = " ";
            }
            if (play.squares.Count > 0)
            {
                foreach (var p in play.squares)
                {
                    object obj = FindName(String.Format("C{0}R{1}", p.col.ToString(), p.row.ToString()) );
                    Button btn = (Button)obj;
                    btn.Content = p.num.ToString();
                    btn.Foreground = p.cond == 0 ? (Brush)(colorQuestion) : (Brush)(colorAnswer);
                }
            }
        }

        //private void CreateControls()
        //{
        //    firstNameLabel = new();
        //    firstNameLabel.Content = "Enter your first name:";
        //    grid1.Children.Add(firstNameLabel);

        //    firstName = new TextBox();
        //    firstName.Margin = new Thickness(0, 5, 10, 5);
        //    Grid.SetColumn(firstName, 1);
        //    grid1.Children.Add(firstName);

        //    lastNameLabel = new Label();
        //    lastNameLabel.Content = "Enter your last name:";
        //    Grid.SetRow(lastNameLabel, 1);
        //    grid1.Children.Add(lastNameLabel);

        //    lastName = new TextBox();
        //    lastName.Margin = new Thickness(0, 5, 10, 5);
        //    Grid.SetColumn(lastName, 1);
        //    Grid.SetRow(lastName, 1);
        //    grid1.Children.Add(lastName);

        //    submit = new Button();
        //    submit.Content = "View message";
        //    Grid.SetRow(submit, 2);
        //    grid1.Children.Add(submit);

        //    clear = new Button();
        //    clear.Content = "Clear Name";
        //    //clear.Style = "{StaticResource GradButton}";
        //    Grid.SetRow(clear, 2);
        //    Grid.SetColumn(clear, 1);
        //    grid1.Children.Add(clear);
        //}
        //private void createButton()
        //{
        //    LinearGradientBrush buttonBrush = new LinearGradientBrush();
        //    buttonBrush.StartPoint = new Point(0, 0.5);
        //    buttonBrush.EndPoint = new Point(1, 0.5);
        //    buttonBrush.GradientStops.Add(new GradientStop(Colors.Blue, 0));
        //    buttonBrush.GradientStops.Add(new GradientStop(Colors.White, 0.9));

        //    submit1 = new Button();
        //    submit1.Content = "View message";
        //    Grid.SetRow(submit1, 3);
        //    grid1.Children.Add(submit1);

        //    submit1.Background = buttonBrush;
        //    submit1.FontSize = 14;
        //    submit1.FontWeight = FontWeights.Bold;
        //}

        private void btnGetImage(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePathToImageSource(openFileDialog.FileName);
            }
        }
        private void btnGetClipboard(object sender, EventArgs e)
        {
            var obj = (System.IO.MemoryStream)Clipboard.GetData("PNG");
            if (obj != null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = obj;
                bi.EndInit();
                bi.Freeze();
                backImage.ImageSource = cnv(bi);
            }
        }
        private void btnSaveAsGame(object sender, EventArgs e)
        {
            int idx0 = lstHistory.SelectedIndex;
            int idx1;
            if (idx0 == -1)
            {
                idx1 = lstHistory.Items.Count == 0 ? -1 : lstHistory.Items.Count - 1;
            }
            else
            {
                idx1 = idx0;
            }
            save(idx1);
        }
        private void btnSaveAddGame(object sender, EventArgs e)
        {
            save(-1);
        }
        private void save(int k)
        {
            Play play = new();
            foreach (var cn in Constants.CELLNAME)
            {
                Button btn = (Button)FindName(cn);
                Int32 n;
                if (Int32.TryParse(btn.Content.ToString(), out n))
                {
                    string[] name = btn.Name.Substring(1).Split("R");
                    var g = new Square();
                    g.col = Int32.Parse(name[0]);
                    g.row = Int32.Parse(name[1]);
                    g.num = n;
                    g.cond = btn.Foreground == (Brush)(colorQuestion) ? 0 : 1;
                    play.squares.Add(g);
                }
            }
            if (play.squares.Count > 0)
            {
                if( k == -1)
                {
                    play.today = DateTime.Now.ToString();
                }
                else
                {
                    play.today = lstHistory.SelectedItem.ToString();
                }
                numPlace.updatePlay(k, play);
            }
            if (k == -1)
            {
                initView();

                var lst = numPlace.listGame();
                if (lst.Count > 0)
                {
                    lstHistory.ItemsSource = lst;
                    lstHistory.SelectedIndex = lst.Count - 1;
                }
            }
        }
        private void btnDiagnose(object sender, EventArgs e)
        {
            int[,] board = new int[9, 9];

            foreach (var cn in Constants.CELLNAME)
            {
                object obj = FindName(cn);
                Button btn = (Button)obj;
                int n;
                if (Int32.TryParse(btn.Content.ToString(), out n))
                {
                    string[] name = cn.Substring(1).Split("R");
                    int col = Int32.Parse(name[0]);
                    int row = Int32.Parse(name[1]);
                    board[col - 1, row - 1] = n;
                    Debug.WriteLine(btn.Content.ToString() + " color:" + btn.Foreground.ToString() + " " + col + " " + row + " " + n);
                }
            }

            int diag = 0;
            for (int row = 1; row <= 9; row++)
            {
                Stack<int> hori = new Stack<int>();
                for (int col = 1; col <= 9; col++)
                {
                    int n = board[col - 1, row - 1];
                    if (n != 0)
                    {
                        if (hori.Contains(n))
                        {
                            diag = 1;   // 横に同一値
                            break;
                        }
                        hori.Push(n);
                    }
                }
                if (diag != 0) { break; }
                if (hori.Count != 9)
                {
                    diag = 2;   // 空がある
                    break;
                }
            }
            if (diag != 0)
            {
                MessageBox.Show("Diagnose " + diag);
                return;
            }
            for (int col = 1; col <= 9; col++)
            {
                Stack<int> vert = new Stack<int>();
                for (int row = 1; row <= 9; row++)
                {
                    int n = board[col - 1, row - 1];
                    if (vert.Contains(n))
                    {
                        diag = 11;   // 縦に同一値
                        break;
                    }
                    vert.Push(n);
                }
                if (diag != 0) { break; }
            }
            if (diag != 0)
            {
                MessageBox.Show("Diagnose " + diag);
                return;
            }
        }
        private void filePathToImageSource(string filePath)
        {
            try
            {
                var bi = new BitmapImage();
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    bi.BeginInit();

                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = fs;
                    bi.EndInit();
                }
                bi.Freeze();

                backImage.ImageSource = cnv(bi);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private BitmapSource cnv(BitmapImage bitmapimageOriginal)
        {
            // BitmapImageのPixelFormatをPbgra32に変換する
            FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bitmapimageOriginal, PixelFormats.Pbgra32, null, 0);

            // 画像の大きさに従った配列を作る
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] originalPixcels = new byte[width * height * 4];
            byte[] inversedPixcels = new byte[width * height * 4];

            // BitmapSourceから配列にコピー
            int stride = (width * bitmap.Format.BitsPerPixel + 7) / 8;
            bitmap.CopyPixels(originalPixcels, stride, 0);

            // 色を反転する
            for (int x = 0; x < originalPixcels.Length; x = x + 4)
            {
                inversedPixcels[x] = (byte)(255 - originalPixcels[x]);
                inversedPixcels[x + 1] = 255; // (byte)(255 - originalPixcels[x + 1]);
                inversedPixcels[x + 2] = (byte)(originalPixcels[x + 2]);
                inversedPixcels[x + 3] = originalPixcels[x + 3];
            }
            // 配列からBitmaopSourceを作る
            BitmapSource inversedBitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, inversedPixcels, stride);

            return inversedBitmap;
        }

        private void btnEraseImage(object sender, EventArgs e)
        {
            backImage.ImageSource = null;
        }
        private void btnResetItem(object sender, EventArgs e)
        {
            foreach (string a in Constants.CELLNAME)
            {
                object obj = FindName(a);
                Button btn = (Button)obj;
                if (btn.Foreground == (Brush)colorAnswer)
                {
                    btn.Content = " ";
                }
            }
        }
        private void btnEraseItem(object sender, EventArgs e)
        {
            foreach(string a in Constants.CELLNAME)
            {
                object obj = FindName(a);
                Button btn = (Button)obj;
                btn.Content = " ";
            }
        }
        private void btnNumberItem(object sender, EventArgs e)
        {
            Button lbl = (Button)sender;

            I0.Content = lbl.Content.ToString();
        }

        private void selectCellPreRBDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)FindName(((Button)sender).Name);
            btn.Content = "";
        }
        private void selectCellDown(object sender, EventArgs e)
        {
            string bn = ((Button)sender).Name;
            Button btn = (Button)FindName(bn);
            btn.Content = I0.Content;
            int n = int.Parse(I0.Content.ToString());
            bool qa = ((bool)rdoQuestion.IsChecked);
            btn.Foreground = qa ? (Brush)(colorQuestion) : (Brush)(colorAnswer);
            int idx = lstHistory.SelectedIndex;
            numPlace.updateCell(idx, bn, n, (qa ? 0 : 1));
        }
        private void selectCellWheel(object sender, MouseWheelEventArgs e)
        {
            string bn = ((Button)sender).Name;
            Button btn = (Button)FindName(bn);
            int n;
            if (!Int32.TryParse(btn.Content.ToString(), out n))
            {
                n = 0;
            }
            n += e.Delta / Math.Abs(e.Delta);
            if (n < 0)
            {
                n = 9;
            }
            else if (n > 9)
            {
                n = 0;
            }
            btn.Content = n == 0 ? " " : n.ToString();
            bool qa = ((bool)rdoQuestion.IsChecked);
            btn.Foreground = qa ? (Brush)(colorQuestion) : (Brush)(colorAnswer);

            int idx = lstHistory.SelectedIndex;
            numPlace.updateCell(idx, bn, n, (qa ? 0 : 1));
        }

        private void lstBoxSelect(object sender, EventArgs e)
        {
            int idx = (sender as ListBox).SelectedIndex;
            initView(idx);
        }
        private void deleteGame(object sender, EventArgs e)
        {
            int idx = lstHistory.SelectedIndex;
            if (idx >= 0)
            {
                sblbl1.Content = idx.ToString();
                numPlace.removePlay(idx);

                var lst = numPlace.listGame();
                lstHistory.ItemsSource = lst;
                lstHistory.SelectedIndex = lst.Count - 1;
            }
        }
        private void renameGame(object sender, EventArgs e)
        {
            int idx = lstHistory.SelectedIndex;
            if (idx >= 0)
            {
                string title = "名称変更";
                string td = numPlace.lookToday(idx);
                string cap = Microsoft.VisualBasic.Interaction.InputBox("名称", title, td);
                if (cap != "" && cap != td)
                {
                    numPlace.modifyCaption(idx, cap);

                    List<string> list = numPlace.listGame();
                    lstHistory.ItemsSource = null;
                    lstHistory.ItemsSource = list;
                    lstHistory.SelectedIndex = idx;
                }
            }
        }
    }
}
