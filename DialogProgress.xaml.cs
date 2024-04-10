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
using System.Windows.Shapes;
using static Sudoku.Answer;

namespace Sudoku
{
    /// <summary>
    /// DialogProgress.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogProgress : Window
    {
        private int first = 0;
        public List<Kifu> banmen { get; set; }
        public int result { get; set; }
        public List<Kifu> resultList { get; }

        public DialogProgress()
        {
            InitializeComponent();

            Activated += (s, e) => {
                if (++first == 1)
                {
                    loadedDialog(s, e);
                }
            };

            resultList = new();
            int cols = (int)Math.Ceiling(masu9x9.ColumnDefinitions.Count / 2.0);
            int rows = (int)Math.Ceiling(masu9x9.RowDefinitions.Count / 2.0);

            for (int grow = 0; grow < rows; grow++)
            {
                for (int gcol = 0; gcol < cols; gcol++)
                {
                    string gn = "Para" + grow.ToString() + gcol.ToString();
                    Grid grid = (Grid)FindName(gn);
                    grid.ShowGridLines = true;

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            Label lbl = new Label();
                            lbl.Name = "L" + grow.ToString() + gcol.ToString() + "l" + r.ToString() + c.ToString();
                            lbl.HorizontalAlignment = HorizontalAlignment.Center;
                            lbl.VerticalAlignment = VerticalAlignment.Center;
                            lbl.Content = (1 + r * c).ToString(); // r.ToString() + " " + c.ToString();
                            lbl.FontSize = 20;
                            lbl.Foreground = Brushes.Blue;
                            lbl.Margin = new Thickness(0, 8, 20, 0);
                            lbl.SetValue(Grid.RowProperty, r);
                            lbl.SetValue(Grid.ColumnProperty, c);
                            int cnt1 = grid.Children.Count;
                            grid.Children.Add(lbl);
                            int cnt2 = grid.Children.Count;

                            //if (cnt2 == 2)
                            //{
                            //    Label lb1 = (Label)grid.Children[1];
                            //}

                            string[] candi = ["1", "2", "3", "4", "5", "6", "7", "8"];
                            HorizontalAlignment[] hori = [HorizontalAlignment.Center, HorizontalAlignment.Right];
                            int[] hori_idx = [0, 0, 0, 0, 1, 1, 1, 1];
                            VerticalAlignment[] vert = [VerticalAlignment.Top, VerticalAlignment.Center, VerticalAlignment.Bottom];
                            int[] vert_idx = [0, 0, 0, 0, 1, 1, 2, 2];
                            int[,] margin = {
                                    { -30,0,0,0 }, {-10,0,0,0 }, { 10,0,0,0 }, {30,0,0,0 },
                                    { 0, -7, 0, 0 }, {0, 10, 0, 0 },
                                    { 0, 0, 0, 0 }, { 0, 0, 10, 0}
                                };

                            for (int i = 0; i < 8; i++)
                            {
                                Label lbl1 = new Label();
                                lbl1.Name = "L" + grow.ToString() + gcol.ToString() + "s" + r.ToString() + c.ToString() + i.ToString();
                                lbl1.Content = candi[i];
                                lbl1.HorizontalAlignment = hori[hori_idx[i]];
                                lbl1.VerticalAlignment = vert[vert_idx[i]];
                                lbl1.FontSize = 8;
                                //lbl1.Foreground = Brushes.Blue;
                                lbl1.Margin = new Thickness(margin[i,0], margin[i,1], margin[i,2], margin[i,3]);
                                lbl1.SetValue(Grid.RowProperty, r);
                                lbl1.SetValue(Grid.ColumnProperty, c);
                                grid.Children.Add(lbl1);
                            }

                        }
                    }
                }
            }

        }
        private void loadedDialog(object s, EventArgs e)
        {
            int[,] cur_ans = new int[9, 9];
            int[,] current = new int[9, 9];
            foreach (var masu in banmen)
            {
                cur_ans[masu.row - 1, 9 - masu.col] = masu.num;
                current[masu.row - 1, 9 - masu.col] = Math.Abs(masu.num);
            }
            Answer sudoku = new(current);
            int zn = sudoku.Test();
            Ban9x9[,] ban = sudoku.getResult();

            banmen.Clear();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (ban[i, j].nn > 0)
                    {
                        Kifu k = new();
                        k.row = 1 + i;
                        k.col = 9 - j;
                        k.num = ban[i, j].nn;
                        banmen.Add(k);
                    }
                }
            }
            for (int grow = 0; grow < 3; grow++)
            {
                for (int gcol = 0; gcol < 3; gcol++)
                {
                    string gn = "Para" + grow.ToString() + gcol.ToString();
                    Grid grid = (Grid)FindName(gn);

                    for (int r = 0; r < 3; r++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            for (int x = 0; x < 9 * 9; x++)
                            {
                                Label lbl = (Label)grid.Children[x];
                                lbl.Content = "";
                            }
                        }
                    }
                }
            }
            foreach(var k in banmen)
            {
                int i0 = k.row - 1;
                int j0 = 9 - k.col;
                int pi = i0 / 3;
                int pj = j0 / 3;
                string gn = "Para" + pi.ToString() + pj.ToString();
                Grid grid = (Grid)FindName(gn);
                int ii = i0 % 3;
                int jj = j0 % 3;
                int idx = (ii * 3 + jj) * 9;
                Label lbl = (Label)grid.Children[idx];
                lbl.Content = k.num;
                int n = cur_ans[k.row - 1, 9 - k.col];
                if (n == 0)
                {
                    lbl.Foreground = Brushes.Green;
                    resultList.Add(new Kifu(k.row - 1, 9 - k.col, k.num));
                }
                else
                {
                    lbl.Foreground = n > 0 ? Brushes.Red : Brushes.Blue;
                }
                int ix = 0;
                foreach(var candi in ban[i0, j0].candidate)
                {
                    Label lbl2 = (Label)grid.Children[idx + ix++];
                    lbl2.Content = candi.ToString();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (ban[i, j].nn == 0)
                    {
                        int pi = i / 3;
                        int pj = j / 3;
                        string gn = "Para" + pi.ToString() + pj.ToString();
                        Grid grid = (Grid)FindName(gn);
                        int ii = i % 3;
                        int jj = j % 3;
                        int idx = (ii * 3 + jj) * 9;
                        foreach (var candi in ban[i, j].candidate)
                        {
                            Label lbl = (Label)grid.Children[++idx];
                            lbl.Content = candi.ToString();
                        }
                    }
                }
            }
        }

        private async void btnAcquire(object sender, RoutedEventArgs e)
        {
            result = 1;
            this.Close();
        }

        private async void btnCancel(object sender, RoutedEventArgs e)
        {
            result = 0;
            this.Close();
        }
    }
}
