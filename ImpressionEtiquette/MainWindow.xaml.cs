using ImpressionEtiquetteDepot.Core;
using ImpressionEtiquetteDepot.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace ImpressionEtiquetteDepot
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<Article> articles { get; set; }
        ObservableCollection<Article> articles { get; set; }
        bool Checked = false;
        public MainWindow()
        {
            try
            {
                Properties.Settings.Default.Reload();
                InitializeComponent();
                InitializeCombo();


                #region Image setting
                // Charger une image depuis un chemin d'accès relatif à l'emplacement de l'application
                string imagePath = "Images/GearSetting.ico"; // Chemin d'accès relatif à l'emplacement de l'application
                BitmapImage bitmapImageIcone = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                // Utiliser l'image chargée dans votre interface utilisateur
                Image image = new Image
                {
                    Source = bitmapImageIcone
                };
                // Ajouter l'image à votre interface utilisateur (par exemple, à un conteneur Grid)
                SettingButton.Content = image;
                #endregion

                if (Qte1 != null)
                {
                    Qte1.IsChecked = Properties.Settings.Default.Qte1;
                }
                if (EmplacementPrincipal != null)
                {
                    EmplacementPrincipal.IsChecked = Properties.Settings.Default.EmplPrincipal;
                }
            }
            catch (Exception ex)
            {
                Core.Log.WriteLog(ex.ToString());
            }
        }

        private void InitializeCombo()
        {

            ComboFournisseur.ItemsSource = Fournisseur.List();
            ComboFournisseur.SelectedItem = ComboFournisseur.Items[0];

            ComboFamille.ItemsSource = Famille.List();
            ComboFamille.SelectedItem = ComboFamille.Items[0];

            DepotCombo.ItemsSource = Depot.List();
            if (Properties.Settings.Default.IdDepotUsed != 0)
            {
                foreach (Depot depotItem in DepotCombo.Items)
                {
                    // Assurez-vous que DepotCombo.Items contient des objets avec une propriété DeNo
                    // Remplacez "LeTypeDeVotreObjet" par le type réel de vos objets dans DepotCombo.Items
                    if (depotItem is Depot depotObjet)
                    {
                        if (depotObjet.DeNo == Properties.Settings.Default.IdDepotUsed)
                        {
                            DepotCombo.SelectedItem = depotObjet;
                            break;  // Une fois que vous avez trouvé la correspondance, vous pouvez sortir de la boucle
                        }
                    }
                }
            }
        }

        private void Recherche_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        public void Load()
        {
            if(DepotCombo.SelectedItem != null)
            {
                Depot depot = (Depot)DepotCombo.SelectedItem;
                Emplacement empl = (Emplacement)EmplCombo.SelectedItem;
                Fournisseur fournisseur = (Fournisseur)ComboFournisseur.SelectedItem;
                Famille famille = (Famille) ComboFamille.SelectedItem;

                if(empl.DpNo == 0)
                {
                    articles = new ObservableCollection<Article>(Article.SearchSage((bool)Qte1.IsChecked,depot.DeNo, fournisseur.CtNum,famille.CodeFamille));
                }
                else if (Properties.Settings.Default.DataEasyLogistic)
                {
                    articles = new ObservableCollection<Article>(Article.SearchEasyEmpl((bool)Qte1.IsChecked, depot.DeNo, empl.DpNo, fournisseur.CtNum,famille.CodeFamille));
                }
                else if(Properties.Settings.Default.EmplPrincipal)
                {
                    articles = new ObservableCollection<Article>(Article.SearchSageEmplPrincipal((bool)Qte1.IsChecked, depot.DeNo, empl.DpNo, fournisseur.CtNum, famille.CodeFamille));
                }
                else
                {
                    articles = new ObservableCollection<Article>(Article.SearchSageEmpl((bool)Qte1.IsChecked, depot.DeNo, empl.DpNo, fournisseur.CtNum, famille.CodeFamille));
                }

                ChangeQte();
                DatagridArt.ItemsSource = articles;
            }
            else
            {
                System.Windows.MessageBox.Show("Veuillez choisir un depot","Information",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }
        private void DepotCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepotCombo.SelectedItem != null)
            {
                EmplCombo.IsEnabled = true;
                Depot depot = (Depot)DepotCombo.SelectedItem;
                EmplCombo.ItemsSource = depot.Emplacements;
                EmplCombo.SelectedItem = depot.Emplacements.FirstOrDefault();

                Properties.Settings.Default.IdDepotUsed = depot.DeNo;
                Properties.Settings.Default.Save();
            }
            else
            {
                EmplCombo.IsEnabled = false;
            }
        }

        private void Qte1_Checked(object sender, RoutedEventArgs e)
        {
            if (Qte1.IsChecked == true)
            {
                Properties.Settings.Default.Qte1 = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.Qte1 = false;
                Properties.Settings.Default.Save();
            }

            //Properties.Settings.Default.DataEasyLogistic = true;
            ChangeQte();
        }
        private void EmplacementPrincipal_Checked(object sender, RoutedEventArgs e)
        {
            if (EmplacementPrincipal.IsChecked == true)
            {
                Properties.Settings.Default.EmplPrincipal = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.EmplPrincipal = false;
                Properties.Settings.Default.Save();
            }

        }

        private void ChangeQte()
        {
            if (Qte1.IsChecked == true && articles != null)
            {
                foreach (Article article in articles)
                {
                    article.QteImp = 1;
                }
            }
            else if (articles != null)
            {

                foreach (Article article in articles)
                {
                    article.QteImp = article.Qte;
                }
            }
            DatagridArt.ItemsSource = "";
            DatagridArt.ItemsSource = articles;
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            Checked = !Checked;
            if (articles != null)
            {
                foreach (Article article in articles)
                {
                    article.isChecked = Checked;
                }
                DatagridArt.ItemsSource = "";
                DatagridArt.ItemsSource = articles;
            }
        }
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            Setting setting = new Setting();
            setting.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            setting.ShowDialog();
        }


        private void ImpressionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Etiquettes", $"{Properties.Settings.Default.Etiquette}.prn");
                foreach (Article article in articles.Where(a => a.isChecked == true))
                {        
                    Print.NewPrintArticle(path, article);
                }
            }
            catch(Exception ex) { Core.Log.WriteLog(ex.ToString()); }
        }

        #region Gestion click droit
        private void DataGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement source && source.DataContext is Article item)
            {

                DataGridRow selectedRow = (DataGridRow)DatagridArt.ItemContainerGenerator.ContainerFromItem(item);
                if (selectedRow != null)
                {
                    selectedRow.IsSelected = true;
                }


                // Créez le menu contextuel
                System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();

                // Ajoutez les options au menu contextuel
                System.Windows.Controls.MenuItem copierRefItem = new System.Windows.Controls.MenuItem { Header = $"Copier : {item.Ref}" };
                copierRefItem.Click += (s, args) => CopierRef_Click(item.Ref);

                System.Windows.Controls.MenuItem copierTout = new System.Windows.Controls.MenuItem { Header = "Copier Selections" };
                copierTout.Click += (s, args) => CopierTout_Click();


                contextMenu.Items.Add(copierRefItem);
                contextMenu.Items.Add(copierTout);

                // Affichez le menu contextuel
                contextMenu.IsOpen = true;

                // Empêchez l'événement de se propager à d'autres éléments (par exemple, la cellule de la colonne)
                e.Handled = true;
            }
        }
        private void CopierRef_Click(string item)
        {
            // Copiez la propriété "Ref" dans le presse-papiers
            System.Windows.Clipboard.SetText(item);
        }
        private void CopierTout_Click()
        {
            StringBuilder clipboardContent = new StringBuilder();

            foreach (Article selectedItem in DatagridArt.SelectedItems)
            {
                // Copiez les propriétés de chaque élément sélectionné
                clipboardContent.AppendLine($"Ref: {selectedItem.Ref}");
            }

            // Copiez le contenu combiné dans le presse-papiers
            System.Windows.Clipboard.SetText(clipboardContent.ToString());
        }
        #endregion

    }
}
