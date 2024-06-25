using ImpressionEtiquetteDepot.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Path = System.IO.Path;

namespace ImpressionEtiquetteDepot
{
    /// <summary>
    /// Logique d'interaction pour Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
            ObservableCollection<string> Imprimantes = new ObservableCollection<string>();
            ObservableCollection<string> Etiquettes  = new ObservableCollection<string>();

            string etiquettesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Etiquettes");

            // Vérifier si le répertoire existe, sinon le créer
            if (!Directory.Exists(etiquettesPath))
            {
                Directory.CreateDirectory(etiquettesPath);
            }


            foreach (string imprimante in PrinterSettings.InstalledPrinters)
            {
                Imprimantes.Add(imprimante);
            }
            foreach (string etiquette in Directory.EnumerateFiles(etiquettesPath).Select(e => Path.GetFileNameWithoutExtension(e)))
            {
                Etiquettes.Add(etiquette);
            }
            ComboEtiquette.ItemsSource = Etiquettes;
            ComboImprimante.ItemsSource = Imprimantes;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Imprimante))
            {
                ComboImprimante.SelectedItem = Properties.Settings.Default.Imprimante;
            }
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Etiquette))
            {
                ComboEtiquette.SelectedItem = Properties.Settings.Default.Etiquette;
            }
            //SelectedImprimante = Settings.Default.Imprimante;
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            if (ComboImprimante.SelectedItem != null && ComboEtiquette.SelectedItem != null)
            {
                Properties.Settings.Default.Imprimante = (string)ComboImprimante.SelectedItem;
                Properties.Settings.Default.Etiquette = (string)ComboEtiquette.SelectedItem;
                Properties.Settings.Default.Save();
                MessageBox.Show("Enregistrement reussis.","Information", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Veuillez choisir une imprimante et une etiquette.", "Information",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }
    }
}
