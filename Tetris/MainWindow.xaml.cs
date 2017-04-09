using Microsoft.Win32;
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
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Media;

namespace Tetris
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        const int HEIGHT = 15;            //长度
        int iPlay = 6;                      //玩家的操作
        Rectangle[] rect = new Rectangle[4];
        double leftValue = 15;                      //能够到达的左值
        double topValue = 450;                       //能够到达的底值
        double rightValue = 190;                   //能够到达的右值
        double left;                           //实际左值
        double top;                            //实际上值
        int iRan;                          //随机图形
        //Storyboard sb;              //创建游戏动画
        //DoubleAnimation[] da=new DoubleAnimation[4];    //创建动画
        Random ran = new Random();                     //随机画一个图形
        DispatcherTimer dt;             //游戏线程
        bool complete = false;                  //动画是否完成
        int brickNumber;                    //方块数
        int trans;                          //变形方案
        int current;                        //当前图案
        double time;                        //游戏时间
        int score;                          //分数
        int canGetScore;                        //能否得分
        SaveHistory sh = new SaveHistory();
        Color singleColor = Colors.Blue;
        Color borderColor = Colors.White;
        System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
        //读取资源中的音频文件
        //SoundPlayer sp=new SoundPlayer(Properties.Resources.ResourceManager.GetStream("tetris"));

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 画五个基本的图形
        public void drawShapes(int alternative)
        {
            if (complete)
            {
                if (gameover())
                {
                    sh.write("用时：" + labelTime.Content + "\t\t分数：" + labelScore.Content);
                    dt.IsEnabled = false;
                    //sp.Stop();
                    complete = false;
                    MessageBox.Show("游戏结束啦", "俄罗斯方块", MessageBoxButton.OK);
                    return;     //游戏结束
                }
                left = iPlay * HEIGHT;
                top = 0;
            }
            else
            {
                left = 60;
                top = 15;
                canvas.Children.Clear();
            }
            //画小方块
            for (int i = 0; i < 4; i++)
            {
                rect[i] = new Rectangle();
                rect[i].Height = HEIGHT;
                rect[i].Width = HEIGHT;
                if (cb.IsChecked==true)
                {
                    rect[i].Fill = new SolidColorBrush(singleColor);
                }
                else
                {
                    rect[i].Fill = new SolidColorBrush(Color.FromArgb(255, (byte)ran.Next(0, 255), (byte)ran.Next(0, 255), (byte)ran.Next(0, 255)));
                }
                if (db.IsChecked == true)
                {
                    rect[i].Stroke = new SolidColorBrush(borderColor);
                    rect[i].StrokeThickness = 1;
                }
            }
            //正方形
            if (alternative == 1)
            {
                Canvas.SetLeft(rect[0], left);
                Canvas.SetTop(rect[0], top);
                Canvas.SetLeft(rect[1], left + HEIGHT);
                Canvas.SetTop(rect[1], top);
                Canvas.SetLeft(rect[2], left);
                Canvas.SetTop(rect[2], top + HEIGHT);
                Canvas.SetLeft(rect[3], left + HEIGHT);
                Canvas.SetTop(rect[3], top + HEIGHT);

            }
            //Z形
            else if (alternative == 2)
            {
                Canvas.SetLeft(rect[0], left);
                Canvas.SetTop(rect[0], top);
                Canvas.SetLeft(rect[1], left + HEIGHT);
                Canvas.SetTop(rect[1], top);
                Canvas.SetLeft(rect[2], left + HEIGHT);
                Canvas.SetTop(rect[2], top + HEIGHT);
                Canvas.SetLeft(rect[3], left + HEIGHT * 2);
                Canvas.SetTop(rect[3], top + HEIGHT);
            }
            //凸形
            else if (alternative == 3)
            {
                Canvas.SetLeft(rect[0], left);
                Canvas.SetTop(rect[0], top);
                Canvas.SetLeft(rect[1], left - HEIGHT);
                Canvas.SetTop(rect[1], top + HEIGHT);
                Canvas.SetLeft(rect[2], left);
                Canvas.SetTop(rect[2], top + HEIGHT);
                Canvas.SetLeft(rect[3], left + HEIGHT);
                Canvas.SetTop(rect[3], top + HEIGHT);
            }
            //条形
            else if (alternative == 4)
            {
                Canvas.SetLeft(rect[0], left - HEIGHT);
                Canvas.SetTop(rect[0], top);
                Canvas.SetLeft(rect[1], left);
                Canvas.SetTop(rect[1], top);
                Canvas.SetLeft(rect[2], left + HEIGHT);
                Canvas.SetTop(rect[2], top);
                Canvas.SetLeft(rect[3], left + HEIGHT * 2);
                Canvas.SetTop(rect[3], top);
            }
            //L形
            else if (alternative == 5)
            {
                Canvas.SetLeft(rect[0], left);
                Canvas.SetTop(rect[0], top);
                Canvas.SetLeft(rect[1], left + HEIGHT);
                Canvas.SetTop(rect[1], top);
                Canvas.SetLeft(rect[2], left - HEIGHT);
                Canvas.SetTop(rect[2], top);
                Canvas.SetLeft(rect[3], left + HEIGHT);
                Canvas.SetTop(rect[3], top + HEIGHT);
            }
            //添加动画
            for (int i = 0; i < 4; i++)
            {
                if (complete)
                {
                    cgame.Children.Add(rect[i]);
                }
                else
                {
                    canvas.Children.Add(rect[i]);
                    if (brickNumber > 0)
                    {
                        rect[i] = cgame.Children[brickNumber - 1 - i] as Rectangle;
                    }
                }
                
            }
            complete = false;
        }
        #endregion

        #region 加载主窗体
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            first.Focus();
            dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds((6 - speedSlider.Value) * 40);
            dt.IsEnabled = false;
            dt.Tick += dt_Tick;
            speedSlider.ToolTip = speedSlider.Value;
            speedSlider.Focusable = false;
            iRan = ran.Next(1, 6);
            drawShapes(iRan);
            //this.KeyDown -= Window_KeyDown;
            TextBox tb = new TextBox();
            //屏蔽输入法
            InputMethod.SetIsInputMethodEnabled(this, false);
        }
        #endregion

        #region 判断游戏是否结束
        public bool gameover()
        {
            for (int i = 0; i < cgame.Children.Count; i++)
            {
                if (Canvas.GetTop(cgame.Children[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 处理键盘事件
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (upButton.IsEnabled && (e.Key == Key.Left || e.Key==Key.A))
            {
                operation(1);
            }
            else if (upButton.IsEnabled && (e.Key == Key.Right || e.Key == Key.D))
            {
                operation(2);
            }
            else if (upButton.IsEnabled && (e.Key == Key.Up || e.Key == Key.W))
            {
                operation(3);
            }
            else if (upButton.IsEnabled && (e.Key == Key.Down || e.Key == Key.S))
            {
                operation(4);
            }
            else if (e.Key == Key.M)
            {
                //开挂
                for (int i = 0; i < 4; i++)
                {
                    Canvas.SetTop(cgame.Children[brickNumber - 1 - i], Canvas.GetTop(cgame.Children[brickNumber - 1 - i]) - HEIGHT);
                    top -= HEIGHT;
                }
            }
            else if (e.Key == Key.N)
            {
                if (dt.IsEnabled)
                {
                    dt.IsEnabled = false;
                }
                else
                {
                    dt.IsEnabled = true;
                }
            }
            else if (e.Key == Key.Z)
            {
                stop();
            }
            else if (e.Key == Key.Q)
            {
                start_Click(sender, e);
            }
        }
        #endregion

        #region 处理玩家的操作
        public void operation(int way)
        {
            //到达最底
            for (int i = 0; i < 4; i++)
            {
                if (Canvas.GetTop(rect[i]) > (topValue - 10))
                {
                    return;
                }
            }
            //方向左
            if (way == 1 && canOperate(1))
            {
                for (int i = 0; i < 4; i++)
                {
                    //到达最左
                    if (Canvas.GetLeft(rect[i]) < leftValue)
                    {
                        return;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    //Canvas.SetLeft(cgame.Children[brickNumber - 1 - i], Canvas.GetLeft(cgame.Children[brickNumber - 1 - i]) - HEIGHT);
                    Canvas.SetLeft(rect[i], Canvas.GetLeft(rect[i]) - HEIGHT);
                    complete = false;
                }
            }
            //方向右
            else if (way == 2 && canOperate(2))
            {
                for (int i = 0; i < 4; i++)
                {
                    //到达最右
                    if (Canvas.GetLeft(rect[i]) > rightValue)
                    {
                        return;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    Canvas.SetLeft(rect[i], Canvas.GetLeft(rect[i]) + HEIGHT);
                    //Canvas.SetLeft(cgame.Children[brickNumber - 1 - i], Canvas.GetLeft(cgame.Children[brickNumber - 1 - i]) + HEIGHT);
                    complete = false;
                }
            }
            //方向上
            else if (way == 3 && canOperate(3))
            {
                left = Canvas.GetLeft(cgame.Children[brickNumber - 1]);
                for (int i = 0; i < 4; i++)
                {
                    left = Math.Min(left, Canvas.GetLeft(cgame.Children[brickNumber - 1 - i]));
                    top = Math.Max(top, Canvas.GetTop(cgame.Children[brickNumber - 1 - i]));
                }
                trans++;
                left = left > 165 ? 165 : left;
                left = left < 15 ? 15 : left;
                top = top > 420 ? 420 : top;
                if (current == 2)
                {
                    if (trans % 4 == 0)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left + HEIGHT);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left + HEIGHT * 2);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 4 == 1)
                    {
                        Canvas.SetLeft(rect[0], left + HEIGHT);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 4 == 2)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 4 == 3)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left + HEIGHT);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top - HEIGHT);
                        Canvas.SetLeft(rect[3], left + HEIGHT * 2);
                        Canvas.SetTop(rect[3], top - HEIGHT);
                    }
                }
                else if (current == 3)
                {
                    if (trans % 4 == 0)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left - HEIGHT);
                        Canvas.SetTop(rect[1], top + HEIGHT);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 4 == 1)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top + HEIGHT);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left);
                        Canvas.SetTop(rect[3], top + HEIGHT * 2);
                    }
                    else if (trans % 4 == 2)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left + HEIGHT);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT * 2);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 4 == 3)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top + HEIGHT);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT * 2);
                        Canvas.SetLeft(rect[3], left - HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                }
                else if (current == 5)
                {
                    if (trans % 6 == 0)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left + HEIGHT);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left - HEIGHT);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 6 == 1)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left + HEIGHT);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left - HEIGHT);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top - HEIGHT);
                    }
                    else if (trans % 6 == 2)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 6 == 3)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left - HEIGHT);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 6 == 4)
                    {
                        Canvas.SetLeft(rect[0], left - HEIGHT);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top - HEIGHT);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left);
                        Canvas.SetTop(rect[3], top + HEIGHT);
                    }
                    else if (trans % 6 == 5)
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left + HEIGHT);
                        Canvas.SetTop(rect[3], top - HEIGHT);
                    }
                }
                else if (current == 4)
                {
                    if (trans % 2 == 0)
                    {
                        Canvas.SetLeft(rect[0], left - HEIGHT);
                        Canvas.SetTop(rect[0], top);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left + HEIGHT);
                        Canvas.SetTop(rect[2], top);
                        Canvas.SetLeft(rect[3], left + HEIGHT * 2);
                        Canvas.SetTop(rect[3], top);
                    }
                    else
                    {
                        Canvas.SetLeft(rect[0], left);
                        Canvas.SetTop(rect[0], top - HEIGHT);
                        Canvas.SetLeft(rect[1], left);
                        Canvas.SetTop(rect[1], top);
                        Canvas.SetLeft(rect[2], left);
                        Canvas.SetTop(rect[2], top + HEIGHT);
                        Canvas.SetLeft(rect[3], left);
                        Canvas.SetTop(rect[3], top + HEIGHT * 2);
                    }
                }
            }
            else if (way == 4 && canOperate(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    Canvas.SetTop(cgame.Children[brickNumber - 1 - i], Canvas.GetTop(cgame.Children[brickNumber - 1 - i]) + HEIGHT);
                }
            }
        }
        #endregion

        #region 准备开始游戏
        private void start_Click(object sender, RoutedEventArgs e)
        {
            //sp.PlayLooping();
            if (dt.IsEnabled==false && cgame.Children.Count != 0)
            {
                dt.IsEnabled = true;
            }
            if (dt.IsEnabled && !gameover())
            {
                sh.write("用时：" + labelTime.Content + "\t\t分数：" + labelScore.Content);
                //if (goOn.Content.ToString() == "暂停")
                //{
                //    this.KeyDown -= Window_KeyDown;
                //}
            }
            goOn.IsEnabled = true;
            goOn.Content = "暂停";
            speedSlider.Value = 1;
            dt.IsEnabled = true;
            complete = true;
            cgame.Children.Clear();
            dt.Start();
            time = 0;
            score = 0;
            leftButton.IsEnabled = true;
            rightButton.IsEnabled = true;
            upButton.IsEnabled = true;
            downButton.IsEnabled = true;
            //this.KeyDown += Window_KeyDown;
        }
        #endregion

        #region 暂停按钮
        public void goOn_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }
        #endregion

        #region 处理方向按钮
        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton rb = sender as RepeatButton;
            if (rb.Content.ToString() == "Left")
            {
                operation(1);
            }
            else if (rb.Content.ToString() == "Right")
            {
                operation(2);
            }
            else if (rb.Content.ToString() == "Up")
            {
                operation(3);
            }
            else if (rb.Content.ToString() == "Down")
            {
                operation(4);
            }
        }
        #endregion

        #region 游戏运行中
        public void dt_Tick(object sender, EventArgs e)
        {
            time += dt.Interval.TotalMilliseconds/1000;
            labelTime.Content = time.ToString("#0.000") + " 秒";
            dt.Interval = TimeSpan.FromMilliseconds((6 - speedSlider.Value) * 40);
            if (complete)
            {
                calScore();
                drawShapes(iRan);
                current = iRan;
                brickNumber = cgame.Children.Count;
                trans = 0;
                iRan = ran.Next(1, 6);
                if (score > 500)
                {
                    if(score>1000 && speedSlider.Value < 4)
                    {
                        speedSlider.Value = 4;
                        if(score>2000 && speedSlider.Value < 5)
                        {
                            speedSlider.Value = 5;
                        }
                    }
                    if (speedSlider.Value < 3)
                    {
                        speedSlider.Value = 3;
                    }
                    iRan = iRan != 5 ? ran.Next(1, 6) : iRan;
                }
                else if (score > 100)
                {
                    if (speedSlider.Value < 2)
                    {
                        speedSlider.Value = 2;
                    }
                }
                else
                {
                    iRan = (iRan == 5) && (ran.Next(1, 6) > 4) ? 5 : iRan;
                } 
                drawShapes(iRan);
            }
            
            if (canOperate(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    Canvas.SetTop(cgame.Children[brickNumber - 1 - i], Canvas.GetTop(cgame.Children[brickNumber - 1 - i]) + HEIGHT);
                }
            }
            else
            {
                complete = true;
            }
        }
        #endregion

        #region 判断方向键是否起作用
        public bool canOperate(int play)
        {
            if (brickNumber < 6)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Canvas.GetTop(cgame.Children[brickNumber - 1 - i]) >= 450)
                    {
                        return false;
                    }
                }
                return true;
            }
            //左
            if (play == 1)
            {
                for (int i = 0; i < brickNumber - 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (Canvas.GetLeft(cgame.Children[i]) == (Canvas.GetLeft(cgame.Children[brickNumber - 1 - j]) - HEIGHT)
                            && Canvas.GetTop(cgame.Children[i]) == Canvas.GetTop(cgame.Children[brickNumber - 1 - j]))
                        {
                            return false;
                        }
                    }
                }
            }
            //右
            else if (play == 2)
            {
                for (int i = 0; i < brickNumber - 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (Canvas.GetLeft(cgame.Children[i]) == (Canvas.GetLeft(cgame.Children[brickNumber - 1 - j]) + HEIGHT)
                            && Canvas.GetTop(cgame.Children[i]) == Canvas.GetTop(cgame.Children[brickNumber - 1 - j]))
                        {
                            return false;
                        }
                    }
                }
            }
            //上下
            else if (play == 3)
            {
                double topSpan = 500;
                for (int i = 0; i < 4; i++)
                {
                    double top1 = 500;
                    if (Canvas.GetTop(cgame.Children[brickNumber - 1 - i]) >= 450)
                    {
                        return false;
                    }
                    for (int j = 0; j < brickNumber - 4; j++)
                    {
                        if (Canvas.GetLeft(cgame.Children[j]) == Canvas.GetLeft(cgame.Children[brickNumber - 1 - i])
                            && Canvas.GetTop(cgame.Children[j]) > Canvas.GetTop(cgame.Children[brickNumber - 1 - i]))
                        {
                            top1 = Math.Min(top1, Canvas.GetTop(cgame.Children[j]));
                        }
                    }
                    topSpan = Math.Min(topSpan, top1 - Canvas.GetTop(cgame.Children[brickNumber - 1 - i]));
                }
                if (topSpan < 20)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 读取历史记录
        private void recoder_Click(object sender, RoutedEventArgs e)
        {
            sh.read();
        }
        #endregion

        #region 计算分数
        public void calScore()
        {
            for(int i = 30; i > 0; i--)
            {
                canGetScore = 0;
                brickNumber = cgame.Children.Count;
                for(int j = 0; j < brickNumber; j++)
                {
                    if (Canvas.GetTop(cgame.Children[j]) == (HEIGHT * i))
                    {
                        canGetScore++;
                    }
                }
                if (canGetScore == 14)
                {
                    score++;
                    for (int j = 0; j < brickNumber; j++)
                    {
                        if (Canvas.GetTop(cgame.Children[j]) == HEIGHT * i)
                        {
                            cgame.Children.RemoveAt(j);
                            j--;
                            brickNumber--;
                        }
                    }
                    for (int j = 0; j < brickNumber; j++)
                    {
                        if (Canvas.GetTop(cgame.Children[j]) < HEIGHT * i)
                        {
                            Canvas.SetTop(cgame.Children[j], Canvas.GetTop(cgame.Children[j]) + HEIGHT);
                        }
                    }
                    i++;
                }
            }
            labelScore.Content = score*10 + " 分";
        }
        #endregion

        #region 关闭时写入记录
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (dt.IsEnabled == false && cgame.Children.Count != 0)
            {
                dt.IsEnabled = true;
            }
            if (dt.IsEnabled)
            {
                sh.write("用时：" + labelTime.Content + "\t\t分数：" + labelScore.Content);
            }
        }
        #endregion

        #region 速度改变
        private void speedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            speedSlider.ToolTip = speedSlider.Value.ToString("0.00");
        }
        #endregion

        #region 方块颜色
        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            if (leftButton.IsEnabled)
            {
                dt.Stop();
            }
            //System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.ShowDialog();
            if (cd.Color != null)
            {
                if((sender as CheckBox).Content.ToString() == "纯色")
                {
                    //system.drawing.color 与system.windows.media.color互转
                    singleColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                }
                else
                {
                    borderColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                }
            }
            if (leftButton.IsEnabled)
            {
                dt.Start();
            }
        }
        #endregion

        #region 窗体处于取消激活状态
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (goOn.Content.ToString() == "暂停")
            {
                stop();
            }
        }
        #endregion

        #region 暂停游戏方法
        public void stop()
        {
            if (goOn.Content.ToString() == "暂停")
            {
                dt.Stop();
                //sp.Stop();
                //this.KeyDown -= Window_KeyDown;
                leftButton.IsEnabled = false;
                rightButton.IsEnabled = false;
                upButton.IsEnabled = false;
                downButton.IsEnabled = false;
                goOn.Content = "继续";
            }
            else
            {
                dt.Start();
                //sp.PlayLooping();
                //this.KeyDown += Window_KeyDown;
                leftButton.IsEnabled = true;
                rightButton.IsEnabled = true;
                upButton.IsEnabled = true;
                downButton.IsEnabled = true;
                goOn.Content = "暂停";
            }
        }
        #endregion
    }
}
