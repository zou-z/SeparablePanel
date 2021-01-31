using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WPF_Control
{
    internal class SeparablePanel : DockPanel
    {
        private class Indicator : Border
        {
            private readonly LinearGradientBrush[] brushes = new LinearGradientBrush[4];
            private Dock insertMode;

            public Dock InsertMode
            {
                get { return insertMode; }
                set
                {
                    insertMode = value;
                    switch (insertMode)
                    {
                        case Dock.Left: Background = brushes[0]; break;
                        case Dock.Top: Background = brushes[1]; break;
                        case Dock.Right: Background = brushes[2]; break;
                        case Dock.Bottom: Background = brushes[3]; break;
                    }
                    DockPanel.SetDock(this, insertMode);
                }
            }

            public Indicator()
            {
                for (int i = 0; i < brushes.Length; ++i)
                {
                    brushes[i] = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1 - (i % 2), i % 2)
                    };
                    brushes[i].GradientStops.Add(new GradientStop
                    {
                        Offset = i < 2 ? 0 : 1,
                        Color = Color.FromArgb(255, 0, 191, 255),
                    });
                    brushes[i].GradientStops.Add(new GradientStop
                    {
                        Offset = i < 2 ? 1 : 0,
                        Color = Color.FromArgb(0, 0, 191, 255),
                    });
                }
                SnapsToDevicePixels = true;
            }
        }
        private readonly Indicator indicator;
        private const double range = 10; // 生效范围（像素）

        public SeparablePanel()
        {
            indicator = new Indicator();
            Loaded += SeparablePanel_Loaded;
        }

        private void SeparablePanel_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SeparablePanel_Loaded;
            foreach (UIElement ui in Children)
            {
                if (ui is SeparablePanelItem item)
                {
                    item.MoveOut += Item_MoveOut;
                    item.Move += Item_Move;
                    item.MoveIn += Item_MoveIn;
                }
            }
        }

        private void Item_MoveOut(SeparablePanelItem sender)
        {
            Children.Remove(sender);
        }

        private bool Item_Move(SeparablePanelItem sender, bool is_enter)
        {
            if (Children.Contains(indicator))
            {
                Point p = Mouse.GetPosition(indicator);
                if (indicator.InsertMode == Dock.Left && (p.X < 0 || range <= p.X))
                {
                    Children.Remove(indicator);
                }
                else if (indicator.InsertMode == Dock.Right && (p.X < indicator.ActualWidth - range || indicator.ActualWidth <= p.X))
                {
                    Children.Remove(indicator);
                }
                else if (indicator.InsertMode == Dock.Top && (p.Y < 0 || range <= p.Y))
                {
                    Children.Remove(indicator);
                }
                else if (indicator.InsertMode == Dock.Bottom && (p.Y < indicator.ActualHeight - range || indicator.ActualHeight <= p.Y))
                {
                    Children.Remove(indicator);
                }
                else
                {
                    return true;
                }
            }
            if (!is_enter)
            {
                return false;
            }
            #region 判断鼠标在哪个子面板中
            for (int i = 0; i < Children.Count; ++i)
            {
                FrameworkElement item = Children[i] as FrameworkElement;
                if (item.Visibility != Visibility.Visible)
                    continue;
                bool isLast = i == Children.Count - 1;
                if (sender.Orientation == Orientation.Vertical)
                {
                    Dock dock = DockPanel.GetDock(item);
                    Point p = Mouse.GetPosition(item);
                    if (p.Y < 0 || item.ActualHeight <= p.Y)
                        continue;
                    indicator.Width = sender.ActualWidth;
                    indicator.Height = double.NaN;
                    if (0 <= p.X && p.X < range)
                    {
                        if (dock == Dock.Right)
                        {
                            indicator.InsertMode = isLast ? Dock.Left : Dock.Right;
                            Children.Insert(Children.IndexOf(item) + (isLast ? 0 : 1), indicator);
                        }
                        else
                        {
                            indicator.InsertMode = Dock.Left;
                            Children.Insert(Children.IndexOf(item), indicator);
                        }
                        return true;
                    }
                    else if (item.ActualWidth - range <= p.X && p.X < item.ActualWidth)
                    {
                        if (dock == Dock.Left)
                        {
                            indicator.InsertMode = isLast ? Dock.Right : Dock.Left;
                            Children.Insert(Children.IndexOf(item) + (isLast ? 0 : 1), indicator);
                        }
                        else
                        {
                            indicator.InsertMode = Dock.Right;
                            Children.Insert(Children.IndexOf(item), indicator);
                        }
                        return true;
                    }
                }
                else if (sender.Orientation == Orientation.Horizontal)
                {
                    Dock dock = DockPanel.GetDock(item);
                    Point p = Mouse.GetPosition(item);
                    if (p.X < 0 || item.ActualWidth <= p.X)
                        continue;
                    indicator.Height = sender.ActualHeight;
                    indicator.Width = double.NaN;
                    if (0 <= p.Y && p.Y < range)
                    {
                        if (dock == Dock.Bottom)
                        {
                            indicator.InsertMode = isLast ? Dock.Top : Dock.Bottom;
                            Children.Insert(Children.IndexOf(item) + (isLast ? 0 : 1), indicator);
                        }
                        else
                        {
                            indicator.InsertMode = Dock.Top;
                            Children.Insert(Children.IndexOf(item), indicator);
                        }
                        return true;
                    }
                    else if (item.ActualHeight - range <= p.Y && p.Y < item.ActualHeight)
                    {
                        if (dock == Dock.Top)
                        {
                            indicator.InsertMode = isLast ? Dock.Bottom : Dock.Top;
                            Children.Insert(Children.IndexOf(item) + (isLast ? 0 : 1), indicator);
                        }
                        else
                        {
                            indicator.InsertMode = Dock.Bottom;
                            Children.Insert(Children.IndexOf(item), indicator);
                        }
                        return true;
                    }
                }
            }
            return false;
            #endregion
        }

        private void Item_MoveIn(SeparablePanelItem sender, int index = 0)
        {
            if (Children.Contains(indicator))
            {
                DockPanel.SetDock(sender, indicator.InsertMode);
                Children.Insert(Children.IndexOf(indicator), sender);
                Children.Remove(indicator);
            }
            else
            {
                Children.Insert(index, sender);
            }
        }
    }

    internal class SeparablePanelItem : StackPanel
    {
        private bool isShowHeader = true;
        /// <summary>
        /// 是否显示头部
        /// </summary>
        public bool IsShowHeader
        {
            get { return isShowHeader; }
            set
            {
                isShowHeader = value;
                header.Visibility = isShowHeader ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool isHide = false;
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHide
        {
            get { return isHide; }
            set
            {
                isHide = value;
                if (isHide)
                {
                    childWindow.Hide();
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (separatingFlag == 2 || separatingFlag == 3)
                    {
                        childWindow.Show();
                    }
                    Visibility = Visibility.Visible;
                }
            }
        }

        public delegate void MoveOutDelegate(SeparablePanelItem sender);
        public delegate bool MoveDelegate(SeparablePanelItem sender, bool is_enter);
        public delegate void MoveInDelegate(SeparablePanelItem sender, int index = 0);
        public event MoveOutDelegate MoveOut;
        public event MoveDelegate Move;
        public event MoveInDelegate MoveIn;

        private readonly Border header; // 头部控件
        private SeparablePanel parent; // 父面板
        private readonly Window childWindow; // 子窗口
        private Window parentWindow; // 父窗口
        private Point mp; // 鼠标相对于此控件的位置
        private bool isPressed = false; // 鼠标是否按下
        private byte separatingFlag = 0; // 标记

        public SeparablePanelItem()
        {
            header = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
            };
            childWindow = new Window
            {
                Title = "",
                WindowStyle = WindowStyle.None,
                ResizeMode=ResizeMode.NoResize,
                AllowsTransparency = true,
                ShowInTaskbar = false,
                Background = Brushes.Transparent
            };
            Loaded += SeparablePanelItem_Loaded;
        }

        private void SeparablePanelItem_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SeparablePanelItem_Loaded;
            header.Width = Orientation == Orientation.Vertical ? double.NaN : 1;
            header.Height = Orientation == Orientation.Vertical ? 1 : double.NaN;
            header.MouseEnter += Header_MouseEnter;
            header.MouseLeave += Header_MouseLeave;
            Children.Insert(0, header);
            parentWindow = Window.GetWindow(this);
            childWindow.Owner = parentWindow;
            parent = Parent as SeparablePanel;
            header.MouseMove += Header_MouseMove;
            childWindow.MouseLeftButtonUp += Header_MouseLeftButtonUp;
            header.MouseLeftButtonDown += Header_MouseLeftButtonDown;
        }

        private void Header_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(0),
                To = 8,
            };
            header.BeginAnimation(Orientation == Orientation.Vertical ? Border.HeightProperty : Border.WidthProperty, da, HandoffBehavior.SnapshotAndReplace);
        }

        private void Header_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromMilliseconds(2000),
                Duration = TimeSpan.FromMilliseconds(100),
                To = 1,
            };
            header.BeginAnimation(Orientation == Orientation.Vertical ? Border.HeightProperty : Border.WidthProperty, da, HandoffBehavior.SnapshotAndReplace);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPressed = true;
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPressed = false;
            if (childWindow.Opacity >= 1)
                return;
            (childWindow.Content as Border).Child = null;
            childWindow.Content = null;
            childWindow.LocationChanged -= ChildWindow_LocationChanged;
            childWindow.Hide();
            childWindow.Opacity = 1;
            MoveIn(this);
            separatingFlag = 0;
            parentWindow.Focus();
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPressed)
            {
                return;
            }
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                isPressed = false;
                return;
            }
            if (separatingFlag == 0)
            {
                separatingFlag = 1;
                Point p = Mouse.GetPosition(parentWindow);
                mp = Mouse.GetPosition(this);
                MoveOut(this);
                childWindow.Width = ActualWidth + 16;
                childWindow.Height = ActualHeight + 16;
                childWindow.Left = (parentWindow.WindowState == WindowState.Maximized ? -8 : parentWindow.Left) + p.X - mp.X;
                childWindow.Top = (parentWindow.WindowState == WindowState.Maximized ? -8 : parentWindow.Top) + 23 + p.Y - mp.Y;
                childWindow.Content = new Border
                {
                    Margin = new Thickness(8),
                    Background = Brushes.Transparent,
                    Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(0, 0, 0),
                        ShadowDepth = 0,
                        BlurRadius = 12,
                        Opacity = 0.3,
                    },
                    Child = this,
                };
                childWindow.Show();
                childWindow.DragMove();
                separatingFlag = 2;
            }
            else if (separatingFlag == 2)
            {
                childWindow.LocationChanged += ChildWindow_LocationChanged;
                separatingFlag = 3;
            }
            else if (separatingFlag == 3)
            {
                childWindow.DragMove();
            }
        }

        private void ChildWindow_LocationChanged(object sender, EventArgs e)
        {
            Point p = Mouse.GetPosition(parent);
            bool is_enter = false;
            if (0 <= p.X && p.X < parent.ActualWidth && 0 <= p.Y && p.Y < parent.ActualHeight)
            {
                is_enter = true;
            }
            bool b = Move(this, is_enter);
            childWindow.Opacity = b == true ? 0.4 : 1.0;
        }

        /// <summary>
        /// 返回父面板
        /// </summary>
        /// <param name="index">
        /// 在父面板中的索引
        /// 可插入的范围为:[0,parentControl.Children.Count]，但是为了不让其成为最后一个元素而充满父面板剩余空间，所以允许范围为:[0,parentControl.Children.Count-1]
        /// </param>
        /// <returns>true:返回成功,false:返回失败，面板已经在父窗体中或者index超出范围</returns>
        public bool Back(int index)
        {
            if (childWindow.Content == null)
                return false;
            else if (index < 0 || (index > 0 && parent.Children.Count <= index))
                return false;
            isPressed = false;
            childWindow.Content = null;
            childWindow.LocationChanged -= ChildWindow_LocationChanged;
            childWindow.Hide();
            childWindow.Opacity = 1;
            MoveIn(this, index);
            separatingFlag = 0;
            parentWindow.Focus();
            return true;
        }
    }
}
