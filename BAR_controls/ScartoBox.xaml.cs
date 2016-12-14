using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BAR_controls
{
    /// <summary>
    /// Logica di interazione per ScartoBox.xaml
    /// </summary>
    public partial class ScartoBox : UserControl
    {
        public bool _is_loaded = false;
        public ScartoBox()
        { 
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;
            FadeIn(border_main);
            //Zoom(border_main);
        }

        private void FadeIn(Border target)
        {
         

        }
        private void Zoom(Border target)
        {
            ScaleTransform trans = new ScaleTransform();
            target.RenderTransform = trans;
            // if you use the same animation for X & Y you don't need anim1, anim2 
            DoubleAnimation anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000));
            trans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            trans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

        }

        public static readonly DependencyProperty AltezzaProperty =
         DependencyProperty.Register("Altezza", typeof(string), typeof(ScartoBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty TipoScartoIDProperty =
         DependencyProperty.Register("TipoScartoID", typeof(string), typeof(ScartoBox));
 
        public static readonly DependencyProperty TipoScartoNameProperty =
         DependencyProperty.Register("TipoScartoName", typeof(string), typeof(ScartoBox));

        public static readonly DependencyProperty TipoScartoValueProperty =
         DependencyProperty.Register("TipoScartoValue", typeof(string), typeof(ScartoBox),
        new FrameworkPropertyMetadata(
            null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty GruppoColoreProperty =
         DependencyProperty.Register("GruppoColore", typeof(string), typeof(ScartoBox));

        public static readonly DependencyProperty BoxColoreProperty =
         DependencyProperty.Register("BoxColore", typeof(string), typeof(ScartoBox));

        private static readonly DependencyProperty ShowBoxProperty =
        DependencyProperty.Register("ShowBox", typeof(bool), typeof(ScartoBox));

        public string Altezza
        {
            get { return GetValue(AltezzaProperty).ToString(); }
            set { SetValue(AltezzaProperty, value.ToString()); }
        }

        public string TipoScartoName
        {
            get { return GetValue(TipoScartoNameProperty).ToString(); }
            set { SetValue(TipoScartoNameProperty, value.ToString()); }
        }
        public string TipoScartoValue
        {
            get { return GetValue(TipoScartoValueProperty).ToString(); }
            set { SetValue(TipoScartoValueProperty, value.ToString()); }
        }
        public string TipoScartoID
        {
            get { return GetValue(TipoScartoIDProperty).ToString(); }
            set { SetValue(TipoScartoIDProperty, value.ToString()); }
        }
        public string GruppoColore
        {
            get { return GetValue(GruppoColoreProperty).ToString(); }
            set { SetValue(GruppoColoreProperty, value.ToString()); }
        }
        public string BoxColore
        {
            get { return GetValue(BoxColoreProperty).ToString(); }
            set { SetValue(BoxColoreProperty, value.ToString()); }
        }
        public bool ShowBox
        {
            get { return (bool)GetValue(ShowBoxProperty); }
            set { SetValue(ShowBoxProperty, value); }
        }

        public event EventHandler ScartoBoxGotFocus;
        public event RoutedEventHandler ScartoBoxTextChanged;
        public event EventHandler ScartoBoxIncrease;
        public event EventHandler ScartoBoxDecrease;

        private void ScartoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ScartoBoxGotFocus(this, EventArgs.Empty);
        }

        private void ScartoTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_is_loaded)
            {
                ScartoBoxTextChanged(this, e);
            }
        }

        private void btn_incrementa_Click(object sender, RoutedEventArgs e)
        {
            ScartoBoxIncrease(this, e);
        }

        private void btn_decrementa_Click(object sender, RoutedEventArgs e)
        {
            ScartoBoxDecrease(this, e);
        }

        private void txt_tipo_scarto_value_Loaded(object sender, RoutedEventArgs e)
        {
            if (border_main.Background == Brushes.Blue || border_main.Background == Brushes.Red)
            {
                // cambiamo il colore del testo per renderlo leggibile
                txt_tipo_scarto_name.Foreground = Brushes.White; 
                txt_tipo_scarto_id.Foreground = Brushes.White;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            double _h;
            try
            {
                _h = double.Parse(Altezza); 

                _is_loaded = true;
                if (_h > 800.0)
                {
                    this.Height += 10;
                    txt_tipo_scarto_name.FontSize = 14;
                    grid_body.RowDefinitions[0].Height = new GridLength(60);
                    //this.Margin = new Thickness(4, 3, 3, 4);
                    this.Margin = new Thickness(10,10,10,10);
                }
            }
            catch (Exception ep) { btn_decrementa.Content = "err"; }
        }

    }
}
