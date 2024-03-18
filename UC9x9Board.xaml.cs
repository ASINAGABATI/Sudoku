using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sudoku
{
    public class Kifu
    {
        public int row; // 段 上から 0,1,2,...8 (一,二,...,九)
        public int col; // 筋 左から 0,1,2,...8 (１,２,...,９)
        public int ope;

        public Kifu() { row = -1; col = -1; ope = -1; }
        public Kifu(int r, int c, int o) { row = r; col = c; ope = o; }
    }

    /// <summary>
    /// UC9x9Board.xaml の相互作用ロジック
    /// </summary>
    public partial class UC9x9Board : UserControl
    {
        private readonly string[] SUJI = ["9", "8", "7", "6", "5", "4", "3", "2", "1"];
        private readonly string[] DAN = ["九", "八", "七", "六", "五", "四", "三", "二", "一"];
        private System.Windows.Media.Color gridBackcolor0 = Colors.WhiteSmoke;
        private System.Windows.Media.Color gridBackcolor1 = Colors.Gainsboro;

        public Kifu Bante
        {
            get => (Kifu)GetValue(NextBante);
            set
            {
                SetValue(NextBante, value);

                OnPropertyChanged(value);
            }
        }
        public static readonly DependencyProperty NextBante =
            DependencyProperty.Register(nameof(Bante), typeof(Kifu), typeof(UC9x9Board), new PropertyMetadata(new Kifu(0, 0, 0)));

        protected void OnPropertyChanged(Kifu k)
        {
            int grow = k.row / 3;
            int gcol = k.col / 3;
            string gn = "Para" + grow.ToString() + gcol.ToString();
            Grid grid = (Grid)FindName(gn);

            string n = "b" + k.row.ToString() + "_" + k.col.ToString();
            Button btn = (Button)grid.Children[(k.row % 3) * 3 + k.col % 3];

            btn.Content = k.ope.ToString();
            var color = Colors.DarkBlue;
            btn.Foreground = new SolidColorBrush(color);
        }

        public UC9x9Board()
        {
            InitializeComponent();

            board.Background = new SolidColorBrush(Colors.Transparent);

            int cols = (int)Math.Ceiling(board.ColumnDefinitions.Count / 2.0);
            int rows = (int)Math.Ceiling(board.RowDefinitions.Count / 2.0);

            for (int grow = 0; grow < rows; grow++)
            {
                for (int gcol = 0; gcol < cols; gcol++)
                {
                    string gn = "Para" + grow.ToString() + gcol.ToString();
                    Grid grid = (Grid)FindName(gn);

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            Button btn = new Button();
                            string left = (grow * 3 + r).ToString();
                            string right = (gcol * 3 + c).ToString();
                            btn.Name = "b" + left + "_" + right;
                            btn.Content = left + "-" + right;
                            btn.SetValue(Grid.RowProperty, r);
                            btn.SetValue(Grid.ColumnProperty, c);
                            btn.FontSize = 26;
                            btn.Foreground = new SolidColorBrush( Colors.Blue);
                            btn.Background = new SolidColorBrush(Colors.Transparent);
                            btn.BorderBrush = new SolidColorBrush(Colors.LightSlateGray);
                            
                            btn.Click += (sender, e) => ButtonClickEvent(sender, e);
                            btn.PreviewMouseDown += (sender, e) => PreviewMouseDownEvent(sender, e);
                            btn.MouseWheel += (sender, e) => MouseWheelEvent(sender, e);

                            grid.Children.Add(btn);
                        }
                    }
                }
            }
        }

        private void Btn_MouseWheel1(object sender, MouseWheelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Btn_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ClearButton(Brush color=null)
        {
            for (int pr = 0; pr < 3; pr++)
            {
                for (int pc = 0; pc < 3; pc++)
                {
                    string gn = "Para" + pr.ToString() + pc.ToString();
                    Grid grid = (Grid)FindName(gn);
                    for (int gcr = 0; gcr < 3; gcr++)
                    {
                        for (int gcc = 0; gcc < 3; gcc++)
                        {
                            Button btn = (Button)grid.Children[(gcr % 3) * 3 + gcc % 3];
                            if (color != null)
                            {
                                if (btn.Foreground == color)
                                {
                                    btn.Content = "";
                                }
                            }
                            else
                            {
                                btn.Content = "";
                            }
                        }
                    }
                }
            }
        }
        public void setButtonProp(int row, int col, string? content, Brush? color = null)
        {
            int sr = row - 1;
            int sc = 9 - col;
            int grow = sr / 3;
            int gcol = sc / 3;
            string gn = "Para" + grow.ToString() + gcol.ToString();
            Grid grid = (Grid)FindName(gn);
            Button btn = (Button)grid.Children[(sr % 3) * 3 + sc % 3];
            if (content != null)
            {
                btn.Content = content;
            }
            if (color != null)
            {
                btn.Foreground = color;
            }
        }
        public void getButtonProp(int row, int col, ref string? content, ref Brush? color)
        {
            int sr = row - 1;
            int sc = 9 - col;
            int grow = sr / 3;
            int gcol = sc / 3;
            string gn = "Para" + grow.ToString() + gcol.ToString();
            Grid grid = (Grid)FindName(gn);
            Button btn = (Button)grid.Children[(sr % 3) * 3 + sc % 3];
            if (content != null) content = btn.Content.ToString();
            if (color != null) color = btn.Foreground;
        }
        public List<Kifu> getBanmen(Brush answerColor)
        {
            List<Kifu> all = new();
            for (int pr = 0; pr < 3; pr++)
            {
                for (int pc = 0; pc < 3; pc++)
                {
                    string gn = "Para" + pr.ToString() + pc.ToString();
                    Grid grid = (Grid)FindName(gn);
                    for (int gcr = 0; gcr < 3; gcr++)
                    {
                        for (int gcc = 0; gcc < 3; gcc++)
                        {
                            Button btn = (Button)grid.Children[(gcr % 3) * 3 + gcc % 3];
                            int n;
                            if (Int32.TryParse(btn.Content.ToString(), out n))
                            {
                                Kifu kifu = new Kifu();
                                kifu.row = pr * 3 + gcr + 1;
                                kifu.col = 9 - pc * 3 - gcc;
                                if (btn.Foreground != answerColor) n *= -1;
                                kifu.ope = n;
                                all.Add(kifu);
                            }
                        }
                    }
                }
            }
            return all;
        }

        public void enableBackimage(bool v = true)
        {
            for (int pr = 0; pr < 3; pr++)
            {
                for (int pc = 0; pc < 3; pc++)
                {
                    string gn = "Para" + pr.ToString() + pc.ToString();
                    Grid grid = (Grid)FindName(gn);
                    grid.Background = new SolidColorBrush(v ? Colors.Transparent : gridColor(pr, pc) );
                }
            }
        }
        private System.Windows.Media.Color gridColor(int r, int c)
        {
            System.Windows.Media.Color gc = (r + c) == 1 || (r + c) == 3  ? gridBackcolor0 : gridBackcolor1;

            return gc;
        }

        public delegate void ButtonClickEventHandler(int row, int col);
        public event ButtonClickEventHandler uc9x9ButtonClickEvent;

        public delegate void PreviewMouseEventHandler(int row, int col, int count, bool left, bool right, bool middle);
        public event PreviewMouseEventHandler uc9x9PreviewMouseEvent;

        public delegate void MouseWheelEventHandler(int row, int col, string content, int delta, bool leftButton, bool rightButton, bool middleButton);
        public event MouseWheelEventHandler uc9x9MouseWheelEvent;

        private void ButtonClickEvent(object sender, RoutedEventArgs e)
        {
            string s = ((Button)sender).Name.ToString().Substring(1);
            string[] w = s.Split('_');
            int row = 1 + Int32.Parse(w[0]);
            int col = 9 - Int32.Parse(w[1]);
            uc9x9ButtonClickEvent?.Invoke(row, col);
        }

        private void PreviewMouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            string s = ((Button)sender).Name.ToString().Substring(1);
            string[] w = s.Split('_');
            int row = 1 + Int32.Parse(w[0]);
            int col = 9 - Int32.Parse(w[1]);
            int count = e.ClickCount;
            bool leftButtonPressed = e.LeftButton == MouseButtonState.Pressed;
            bool rightButtonPressed = e.RightButton == MouseButtonState.Pressed;
            bool middleButtonPressed = e.MiddleButton == MouseButtonState.Pressed;
            uc9x9PreviewMouseEvent?.Invoke(row, col, count, leftButtonPressed, rightButtonPressed, middleButtonPressed);
        }
        private void MouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            Button btn = (Button)sender;
            string s = btn.Name.ToString().Substring(1);
            string[] w = s.Split('_');
            int row = 1 + Int32.Parse(w[0]);
            int col = 9 - Int32.Parse(w[1]);
            string content = btn.Content.ToString();
            int delta = e.Delta;
            bool leftButtonPressed = e.LeftButton == MouseButtonState.Pressed;
            bool rightButtonPressed = e.RightButton == MouseButtonState.Pressed;
            bool middleButtonPressed = e.MiddleButton == MouseButtonState.Pressed;

            uc9x9MouseWheelEvent?.Invoke(row, col, content, delta, leftButtonPressed, rightButtonPressed, middleButtonPressed);
        }
    }
}
