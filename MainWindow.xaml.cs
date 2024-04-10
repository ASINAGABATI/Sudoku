using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Sudoku
{
    enum GAME_TIMER
    {
        STOPPING, TIMEKEEPING, POUSE
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button? submit1;

        private Button? clear;
        private Brush colorAnswer;
        private Brush colorQuestion;
        private NumPlace numPlace;

        readonly Stopwatch stopwatch = new Stopwatch();
        readonly DispatcherTimer timer = new DispatcherTimer();
        GAME_TIMER gt = GAME_TIMER.STOPPING;

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

            timer.Interval = new TimeSpan(0, 0, 0, 1, 0); // 1秒
            timer.Tick += new EventHandler(TimerMethod);
            timer.Start();
        }
        

        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            numPlace.final();
        }

        private void initView(int index = -1)
        {
            Play play = numPlace.loadPlay(index);
            backImage.ImageSource = null;
            board.enableBackimage(false);

            board.ClearButton();
            if (play.squares.Count > 0)
            {
                foreach (var p in play.squares)
                {
                    board.setButtonProp(p.row, p.col, p.num > 0 ? p.num.ToString() : "", p.cond == 0 ? (Brush)(colorQuestion) : (Brush)(colorAnswer));
                }
            }
        }


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
                board.enableBackimage();
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
        private void btnNewGame(object sender, EventArgs e)
        {
            board.ClearButton();

            backImage.ImageSource = null;
            board.enableBackimage();
            Play play = new();
            play.today = DateTime.Now.ToString();
            numPlace.updatePlay(-1, play);

            var lst = numPlace.listGame();
            lstHistory.ItemsSource = lst;
            lstHistory.SelectedIndex = lst.Count - 1;
        }
        private void save(int k)
        {
            Play play = new();
            List<Kifu> ban = board.getBanmen(colorAnswer);
            if (ban.Count > 0)
            {
                foreach(Kifu kifu in ban)
                {
                    play.squares.Add( new Square() {row = kifu.row, col = kifu.col, num = Math.Abs(kifu.num), cond = kifu.num > 0 ? 1 : 0 } );
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
            }
            numPlace.updatePlay(k, play);
            sblbl1.Content = " update " + play.today;

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
        private async void btnAnswer(object sender, RoutedEventArgs e)
        {
            var dlg = new DialogProgress();
            dlg.banmen = board.getBanmen(colorAnswer);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (dlg.result == 1)
            {
                foreach (var item in dlg.resultList)
                {
                    board.setButtonProp(item.row + 1, 9 - item.col, item.num.ToString(), (Brush)(colorAnswer) );
                    int idx = lstHistory.SelectedIndex;
                    numPlace.updateCell(idx, item.row + 1, 9 - item.col, item.num, 1);
                }

            }
        }
        private void btnDiagnose(object sender, RoutedEventArgs e)
        {
            string[] dlgmess = new string[] { "横に同じ値", "未回答がある", "縦に同一値" };

            int[,] ban = new int[9, 9];

            List<Kifu> bamen = board.getBanmen(colorAnswer);
            if (bamen.Count > 0)
            {
                foreach (Kifu kifu in bamen)
                {
                    ban[kifu.col - 1, kifu.row - 1] = kifu.num;
                }
            }

            int diag = 0;
            for (int row = 1; row <= 9; row++)
            {
                Stack<int> hori = new Stack<int>();
                for (int col = 1; col <= 9; col++)
                {
                    int n = ban[col - 1, row - 1];
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
                MessageBox.Show(dlgmess[diag - 1]);
                return;
            }
            for (int col = 1; col <= 9; col++)
            {
                Stack<int> vert = new Stack<int>();
                for (int row = 1; row <= 9; row++)
                {
                    int n = ban[col - 1, row - 1];
                    if (vert.Contains(n))
                    {
                        diag = 3;   // 縦に同一値
                        break;
                    }
                    vert.Push(n);
                }
                if (diag != 0) { break; }
            }
            if (diag != 0)
            {
                MessageBox.Show("Diagnose " + dlgmess[diag - 1]);
            }
            else
            {
                sblbl1.Content = "正解";
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
                
                board.enableBackimage();
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
        private BitmapSource backColor(int w, int h)
        {
            byte[] inversedPixcels = new byte[w * h * 4];
            int stride = w * 32 / 8;
            for (int x = 0; x < w*h*4; x = x + 4)
            {
                inversedPixcels[x] = (byte)(255);
                inversedPixcels[x + 1] = 128;
                inversedPixcels[x + 2] = 0;
                inversedPixcels[x + 3] = 0;
            }
            BitmapSource inversedBitmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Pbgra32, null, inversedPixcels, stride);

            return inversedBitmap;
        }

        private void btnEraseImage(object sender, EventArgs e)
        {
            backImage.ImageSource = null;
            board.enableBackimage(false);
        }
        private void btnResetItem(object sender, EventArgs e)
        {
            board.ClearButton(colorAnswer);
        }
        private void btnEraseItem(object sender, EventArgs e)
        {
            board.ClearButton();
        }
        private void btnNumberItem(object sender, EventArgs e)
        {
            Button lbl = (Button)sender;

            I0.Content = lbl.Content.ToString();
            I0.Foreground = (bool)rdoQuestion.IsChecked ? (Brush)colorQuestion : (Brush)colorAnswer;
        }
        private void rdoI0Click(object sender, EventArgs e)
        {
            string bn = ((RadioButton)sender).Name;
            RadioButton btn = (RadioButton)FindName(bn);

            bool qa = (bool)rdoQuestion.IsChecked;
            I0.Foreground = qa ? (Brush)(colorQuestion) : (Brush)(colorAnswer);
        }

        private void lstBoxSelect(object sender, EventArgs e)
        {
            int idx = ((ListBox)sender).SelectedIndex;
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

        private void TimerMethod(object sender, EventArgs e)
        {
            var result = stopwatch.Elapsed;
            sblbltimer.Content = result.Minutes.ToString("00") + "m " + result.Seconds.ToString("00") + "s";
        }
        private void sbbtn_start_Click(object sender, RoutedEventArgs e)
        {
            switch(gt)
            {
                case GAME_TIMER.STOPPING:
                    stopwatch.Start();
                    gt = GAME_TIMER.TIMEKEEPING;
                    timer.Start();
                    break;
                case GAME_TIMER.TIMEKEEPING:
                    stopwatch.Stop();
                    gt = GAME_TIMER.POUSE;
                    timer.Stop();
                    break;
                case GAME_TIMER.POUSE:
                    stopwatch.Start();
                    gt = GAME_TIMER.TIMEKEEPING;
                    timer.Start();
                    break;
            }
            sblbl1.Content = "●" + gt.ToString();
        }
        private void sbbtn_reset_Click(object sender, RoutedEventArgs e)
        {
            switch (gt)
            {
                case GAME_TIMER.STOPPING:
                    break;
                case GAME_TIMER.TIMEKEEPING:
                    stopwatch.Restart();
                    timer.Start();
                    break;
                case GAME_TIMER.POUSE:
                    timer.Stop();
                    sblbltimer.Content = "00 min 00 sec";
                    stopwatch.Reset();
                    gt = GAME_TIMER.STOPPING;
                    break;
            }
            sblbl1.Content = "■" + gt.ToString();
        }

        private void board_ButtonClick(int row, int col)
        {
            sblbl2.Content = "click event row:" + row.ToString() + " ,col:" + col.ToString();
        }

        private void board_uc9x9MouseWheelEvent(int row, int col, string content, int delta, bool leftButton, bool rightButton, bool middleButton)
        {
            sblbl1.Content = "mouse wheel event Delta:" + delta.ToString() + " ,lBtn:" + leftButton.ToString() +
                " ,rBtn:" + rightButton.ToString() + " ,mBtn:" + middleButton.ToString();

            int n;
            if (!Int32.TryParse(content, out n))
            {
                n = 0;
            }
            n += delta / Math.Abs(delta);
            if (n < 0)
            {
                n = 9;
            }
            else if (n > 9)
            {
                n = 0;
            }
            bool qa = (bool)(rdoQuestion.IsChecked);
            Brush brush = qa ? (Brush)(colorQuestion) : (Brush)(colorAnswer);
            board.setButtonProp(row, col, n.ToString(), brush);

            int idx = lstHistory.SelectedIndex;
            numPlace.updateCell(idx, row, col, n, (qa ? 0 : 1));
        }

        private void board_uc9x9PreviewMouseEvent(int row, int col, int count, bool left, bool right, bool middle)
        {
            sblbl1.Content = "preview mouse row:" + row.ToString() + " col:" + col.ToString() + " ,Count:" + count.ToString() + " ,lBtn:" + left.ToString() +
                " ,rBtn:" + right.ToString() + " ,mBtn:" + middle.ToString();

            if (count > 0)
            {
                string s = Keyboard.IsKeyDown(Key.LeftShift) ? "Key.LeftShift" : "";
                if (Keyboard.IsKeyDown(Key.RightShift)) s += " Key.RightShift";
                if (Keyboard.IsKeyDown(Key.LeftAlt)) s += " Key.LeftAlt";
                if (Keyboard.IsKeyDown(Key.RightAlt)) s += " Key.RightAlt";
                sblbl2.Content = s;

                if (count != 1) return;

                if (left && !(Keyboard.IsKeyDown(Key.LeftShift)))
                {
                    board.setButtonProp(row, col, I0.Content.ToString());
                    int n;
                    if (int.TryParse(I0.Content.ToString(), out n))
                    {
                        bool qa = (bool)(rdoQuestion.IsChecked);
                        board.setButtonProp(row, col, null, qa ? (Brush)(colorQuestion) : (Brush)(colorAnswer));
                        int idx = lstHistory.SelectedIndex;
                        numPlace.updateCell(idx, row, col, n, (qa ? 0 : 1));
                    }
                }
                if (right)
                {
                    board.setButtonProp(row, col, "");
                }
                if (Keyboard.IsKeyDown(Key.LeftShift) || middle)
                {
                    Tuple<string, Brush> r = board.getButtonProp(row, col);
                    I0.Content = r.Item1;
                }
            }
        }
    }
}
