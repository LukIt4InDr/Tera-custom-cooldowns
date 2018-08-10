﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using TCC.Annotations;
using TCC.Data;
using TCC.ViewModels;
using TCC.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace TCC.Controls.ClassBars
{
    /// <summary>
    /// Logica di interazione per WarriorBar.xaml
    /// </summary>
    public partial class WarriorBar : INotifyPropertyChanged
    {
        public WarriorBar()
        {
            InitializeComponent();
        }

        private WarriorBarManager _dc;
        private DoubleAnimation _an;
        private DoubleAnimation _tc;
        private DoubleAnimation _tcCd;
        private DoubleAnimation _anCd;



        public bool WarningStance
        {
            get => _warningStance;
            set
            {
                if (_warningStance == value) return;
                _warningStance = value;
                NPC();
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _dc = DataContext as WarriorBarManager;
            _dc.TraverseCut.PropertyChanged += AnimateTraverseCut;
            _dc.TraverseCut.OnToZero += CooldownTraverseCut;
            _dc.EdgeCounter.PropertyChanged += EdgeCounter_PropertyChanged;

            _an = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _tc = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _tc.Completed += (_, __) => _tcAnimating = false;
            _tcCd = new DoubleAnimation(0, TimeSpan.FromMilliseconds(0));
            _anCd = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(0));
            _anCd.Completed += (o, args) => _cooldown = false;
            SessionManager.CurrentPlayer.PropertyChanged += CheckStanceWarning;
            _dc.Stance.PropertyChanged += CheckStanceWarning;
        }

        private void EdgeCounter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Counter.Val):
                    var rects = GetSortedEdge();
                    for (int i = 0; i < 10; i++)
                    {
                        if (i < _dc.EdgeCounter.Val)
                        {
                            rects[i].Opacity = 1;
                            (rects[i] as Rectangle).Fill = i < 8 ? i == 7 ? App.Current.FindResource("AquadraxColor") as SolidColorBrush :
                                                                            App.Current.FindResource("IgnidraxColor") as SolidColorBrush :
                                                                            App.Current.FindResource("HpColor") as SolidColorBrush;
                        }
                        else
                        {
                            rects[i].Opacity = 0.1;
                            (rects[i] as Rectangle).Fill = Brushes.White;
                        }
                    }
                    if (_dc.EdgeCounter.Val == 8 || _dc.EdgeCounter.Val == 10)
                    {
                        EdgeCounterBorder.BorderBrush = Brushes.White;
                        EdgeCounterBorder.Background = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
                        (EdgeCounterBorder.Effect as DropShadowEffect).Opacity = 1;
                        (EdgeCounterBorder.Effect as DropShadowEffect).Color = Colors.White;

                        if (_dc.EdgeCounter.Val == 10)
                        {
                            MainEdgeGrid.Effect =
                                new DropShadowEffect {BlurRadius = 15, ShadowDepth = 0, Color = Colors.White};
                        }
                    }
                    else
                    {
                        EdgeCounterBorder.Background = App.Current.FindResource("KrBgColor") as SolidColorBrush;
                        EdgeCounterBorder.BorderBrush = App.Current.FindResource("KrBorderColor") as SolidColorBrush;
                        (EdgeCounterBorder.Effect as DropShadowEffect).Opacity = 0;
                        MainEdgeGrid.Effect = App.Current.FindResource("DropShadow") as DropShadowEffect;

                    }
                    break;
                case "Maxed":
                    break;
            }
        }

        private void TranslateMovingTo(int edge)
        {
            var d = TimeSpan.FromMilliseconds(250);
            var e = new QuadraticEase();
            var rt = Moving.RenderTransform as TranslateTransform;
            var yan = new DoubleAnimation(-edge * 9, d) { EasingFunction = e };
            var xan = new DoubleAnimation(edge < 4 ? edge * 9 : (10 - edge - 2) * 9 - 4, d) { EasingFunction = e };
            rt.BeginAnimation(TranslateTransform.XProperty, xan);
            rt.BeginAnimation(TranslateTransform.YProperty, yan);
        }

        private List<UIElement> GetSortedEdge()
        {
            var ret = new List<UIElement>();
            for (int i = 4; i >= 0; i--)
            {
                ret.Add(Edge5to1.Children[i]);
            }
            for (int i = 4; i >= 0; i--)
            {
                ret.Add(Edge10to6.Children[i]);
            }
            return ret;
        }

        private void CheckStanceWarning(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Player.IsInCombat) &&
                e.PropertyName != nameof(StanceTracker<WarriorStance>.CurrentStance)) return;
            WarningStance = _dc.Stance.CurrentStance == WarriorStance.None && SessionManager.CurrentPlayer.IsInCombat;
        }

        private void CooldownTraverseCut(uint cd)
        {
            if (_tcAnimating)
            {
                Task.Delay(210).ContinueWith(t => CooldownTraverseCut(cd - 210));
                return;
            }
            //if (!_dc.TraverseCut.Maxed) return;
            Dispatcher.Invoke(() =>
            {
                _tcCd.From = _dc.TraverseCut.Factor * 359.9;
                _tcCd.Duration = TimeSpan.FromMilliseconds(cd);
                //TODO: TcGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _tcCd);
                TcArc.BeginAnimation(Arc.EndAngleProperty, _tcCd);
            });

        }

        private bool _cooldown;
        private void CooldownTempestAura(uint cd)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    _cooldown = true;
            //    _anCd.Duration = TimeSpan.FromMilliseconds(cd);
            //    TaGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _anCd);
            //});
        }

        private void AnimateTempestAura(object sender, PropertyChangedEventArgs e)
        {

            //if (e.PropertyName == nameof(StatTracker.Factor))
            //{
            //    if (_cooldown) return;
            //    _an.To = _dc.TempestAura.Factor;
            //    TaGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _an);
            //}
        }

        private bool _tcAnimating;
        private bool _warningStance;

        private void AnimateTraverseCut(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatTracker.Factor))
            {
                _tc.To = _dc.TraverseCut.Factor * 359.9;
                _tcAnimating = true;
                //TODO: TcGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _tc);
                TcArc.BeginAnimation(Arc.EndAngleProperty, _tc);

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NPC([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class WarriorStanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var s = (WarriorStance)value;
                switch (s)
                {
                    default:
                        return Application.Current.FindResource("KrBorderColor");
                    case WarriorStance.Assault:
                        return Application.Current.FindResource("AssaultStanceColor");
                    case WarriorStance.Defensive:
                        return Application.Current.FindResource("DefensiveStanceColor");
                }
            }
            else return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ArcherStanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var s = (ArcherStance)value;
                switch (s)
                {
                    default:
                        return Brushes.DimGray;
                    case ArcherStance.SniperEye:
                        return Application.Current.FindResource("SniperEyeColor");
                }
            }
            else return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
