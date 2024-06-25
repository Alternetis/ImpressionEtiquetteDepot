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
using System.Configuration;
using ImpressionEtiquetteDepot.Properties;
using ImpressionEtiquetteDepot.Core;
using System.Data.SqlClient;

namespace ImpressionEtiquetteDepot
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
        public Configuration()
        {
            InitializeComponent();
            if (Properties.Settings.Default.OuvrirConfig)
            {
                ReOuverture.IsChecked = Properties.Settings.Default.OuvrirConfig;
                EmplacementLogistic.IsChecked = Properties.Settings.Default.DataEasyLogistic;

                string tarifSpCalcul = Properties.Settings.Default.TarifSp_Calcul;

                // Découper la chaîne en utilisant le caractère ';'
                string[] tarifCalcule = tarifSpCalcul.Split(';');

                if(tarifCalcule.Length == 2)
                {
                    SpCoef.Text = tarifCalcule[1];
                    SpCategorie.Text = tarifCalcule[0];
                }

                SageConnexion.Text = Properties.Settings.Default.SageConnection;
                EasyConnexion.Text = Properties.Settings.Default.EasyLogisticConnection;

            }
            else
            {
                OpenMain();
            }
        }

        private void OpenMain()
        {
            MainWindow main = new MainWindow();
            main.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Close();
            main.Show();

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigManager.SetSetting("OuvrirConfig", ReOuverture.IsChecked.ToString());
                ConfigManager.SetSetting("DataEasyLogistic", EmplacementLogistic.IsChecked.ToString());
                ConfigManager.SetSetting("TarifSp_Calcul", $"{SpCategorie.Text};{SpCoef.Text}");

                ConfigManager.SetSetting("SageConnection", $"{SageConnexion.Text}");
                ConfigManager.SetSetting("EasyLogisticConnection", $"{EasyConnexion.Text}");

                OpenMain();
            }
            catch (Exception ex)
            {
                Core.Log.WriteLog(ex.ToString());
            }
        }

        private void SageConnexionTest_Click(object sender, RoutedEventArgs e)
        {
            //LoadingScreen loadingScreen = new LoadingScreen("Test connexion SQL en cours...");
            //loadingScreen.SetIndeterminate();
            //loadingScreen.Show();
            if (TestSqlConnexion(SageConnexion.Text))
            {
                //loadingScreen.Close();
                MessageBox.Show("Connexion réussis", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //loadingScreen.Close();
                MessageBox.Show("Connexion échoué", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EasyConnexionTest_Click(object sender, RoutedEventArgs e)
        {
            if (TestSqlConnexion(EasyConnexion.Text))
            {
                MessageBox.Show("Connexion réussis", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Connexion échoué", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TestSqlConnexion(string connexion)
        {
            // Créer une connexion
            using (SqlConnection connection = new SqlConnection(connexion))
            {
                try
                {
                    // Ouvrir la connexion
                    connection.Open();

                    return true;
                }
                catch (Exception ex)
                {
                    // Afficher un message d'erreur en cas d'échec de la connexion
                    return false;
                }
            }
        }
    }
}
