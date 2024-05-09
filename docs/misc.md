
  - MainWindow.xaml

            <Grid Grid.Column="1">
                <Grid >
                    <Grid.Background>
                        <ImageBrush x:Name="backImage"  />
                    </Grid.Background>

                    <local:UC9x9Board x:Name="board" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Margin="2,2,2,2"
                                  uc9x9ButtonClickEvent="board_ButtonClick" 
                                  uc9x9MouseWheelEvent="board_uc9x9MouseWheelEvent"
                                  uc9x9PreviewMouseEvent="board_uc9x9PreviewMouseEvent"/>
                </Grid>

            </Grid>

  - MainWindow.xaml.cs

        private void board_ButtonClick(int row, int col)
        {
            sblbl2.Content = "click event row:" + row.ToString() + " ,col:" + col.ToString();
        }


  - UC9x9Board.xaml.cs

        public delegate void ButtonClickEventHandler(int row, int col); // デリゲート型　を定義
        public event ButtonClickEventHandler uc9x9ButtonClickEvent;     // デリゲート型　イベント名

        private void ButtonClickEvent(object sender, RoutedEventArgs e)
        {

            uc9x9ButtonClickEvent?.Invoke(row, col); // 別のクラス(MainWindow)にイベントを発生させる
        }

        btn.Click += (sender, e) => ButtonClickEvent(sender, e); //ボタンのクリックイベントを登録する


