using ImpressionEtiquetteDepot.Properties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpressionEtiquetteDepot.Model
{
  
    public class Article
    {
        public bool isChecked { get; set; }
        public string Ref { get; set; }
        public string LotSerie { get; set; }
        public string CodeBarre { get; set; }
        public string Designation { get; set; }
        public double Qte { get; set; }
        public double QteImp { get; set; }

        public static List<Article> SearchSage(bool qte1, int deNo, string ctnum,string codeFamille)
        {
            List<Article> articles = new List<Article>();

            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();

                string query = "";
                string queryWhere = "";

                query += $"SELECT F_ARTICLE.AR_ref, AR_Design,AR_CodeBarre,AS_QteSto,F_LOTSERIE.LS_NoSerie,LS_QteRestant " +
                    $"FROM F_ARTICLE " +
                    $"INNER JOIN F_ARTSTOCK ON F_ARTSTOCK.AR_Ref = F_ARTICLE.AR_Ref " +
                    $"LEFT JOIN F_LOTSERIE ON F_LOTSERIE.AR_Ref = F_ARTICLE.AR_Ref ";

                if (ctnum != "Tous")
                {
                    query += $"INNER JOIN F_ARTFOURNISS ON F_ARTFOURNISS.AR_Ref = F_ARTICLE.AR_Ref ";
                    queryWhere = $" AND F_ARTFOURNISS.CT_Num = '{ctnum}'" +
                                 $" AND F_ARTFOURNISS.AF_Principal = 1 ";
                }
                if(codeFamille != "Tous")
                {
                    queryWhere += $" AND F_ARTICLE.FA_CodeFamille = '{codeFamille}' ";
                }

                query += $"WHERE F_ARTSTOCK.DE_No = {deNo} AND AS_QteSto > 0 AND ( LS_LotEpuise = 0 OR LS_LotEpuise IS NULL ) ";
      
                query += queryWhere;

                query += " ORDER BY  F_ARTICLE.AR_ref";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int qte = (int)(decimal)reader["AS_QteSto"];
                            bool lotserie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie"));

                            articles.Add(new Article
                            {
                                isChecked = false,
                                Ref = reader.IsDBNull(reader.GetOrdinal("AR_Ref")) ? "" : (string)reader["AR_Ref"],
                                CodeBarre = reader.IsDBNull(reader.GetOrdinal("AR_CodeBarre")) ? "" : (string)reader["AR_CodeBarre"],
                                LotSerie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie")) ? "" : (string)reader["LS_NoSerie"],
                                Qte = lotserie == false ? (int)(decimal)reader["LS_QteRestant"] : qte,
                                QteImp = lotserie == false ? qte1 ? 1 : (int)(decimal)reader["LS_QteRestant"] : qte ,
                                Designation = reader.IsDBNull(reader.GetOrdinal("AR_Design")) ? "" : (string)reader["AR_Design"],
                            });

                            
                        }
                    }
                }
            }

            return articles;
        }

        public static List<Article> SearchSageEmpl(bool qte1,int deNo, int dpNo,string ctnum, string codeFamille)
        {
            List<Article> articles = new List<Article>();

            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();

                string query = "";
                string queryWhere = "";

                query += $"SELECT F_ARTICLE.AR_ref, AR_Design,AR_CodeBarre,AE_QteSto,F_LOTSERIE.LS_NoSerie,LS_QteRestant " +
                    $"FROM F_ARTICLE " +
                    $"INNER JOIN F_ARTSTOCKEMPL on F_ARTSTOCKEMPL.AR_Ref = F_ARTICLE.AR_Ref " +
                    $"LEFT JOIN F_LOTSERIE ON F_LOTSERIE.AR_Ref = F_ARTICLE.AR_Ref ";

                if (ctnum != "Tous")
                {
                    query += $"INNER JOIN F_ARTFOURNISS ON F_ARTFOURNISS.AR_Ref = F_ARTICLE.AR_Ref ";
                    queryWhere += $" AND F_ARTFOURNISS.CT_Num = '{ctnum}' " +
                                  $" AND F_ARTFOURNISS.AF_Principal = 1 ";
                }
                if (codeFamille != "Tous")
                {
                    queryWhere += $" AND F_ARTICLE.FA_CodeFamille = '{codeFamille}' ";
                }

                query += $"WHERE F_ARTSTOCKEMPL.DE_No = {deNo} AND DP_No = {dpNo} AND AE_QteSto > 0  AND ( LS_LotEpuise = 0 OR LS_LotEpuise IS NULL ) ";
                
                query += queryWhere;

                query += " ORDER BY  F_ARTICLE.AR_ref";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int qte = (int)(decimal)reader["AE_QteSto"];
                            bool lotserie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie"));
                            articles.Add(new Article
                            {
                                isChecked = false,
                                Ref = reader.IsDBNull(reader.GetOrdinal("AR_Ref")) ? "" : (string)reader["AR_Ref"],
                                CodeBarre = reader.IsDBNull(reader.GetOrdinal("AR_CodeBarre")) ? "" : (string)reader["AR_CodeBarre"],
                                LotSerie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie")) ? "" : (string)reader["LS_NoSerie"],
                                Qte = lotserie == false ? (int)(decimal)reader["LS_QteRestant"] : qte,
                                QteImp = lotserie == false ? qte1 ? 1 : (int)(decimal)reader["LS_QteRestant"] : qte,
                                Designation = reader.IsDBNull(reader.GetOrdinal("AR_Design")) ? "" : (string)reader["AR_Design"],
                            }); 
                        }
                    }
                }
            }

            return articles;
        }

        public static List<Article> SearchSageEmplPrincipal(bool qte1, int deNo, int dpNo, string ctnum, string codeFamille)
        {
            List<Article> articles = new List<Article>();

            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();

                string query = "";
                string queryWhere = "";

                query += $"SELECT F_ARTICLE.AR_ref, AR_Design,AR_CodeBarre,AS_QteSto,F_LOTSERIE.LS_NoSerie,LS_QteRestant " +
                    $"FROM F_ARTICLE " +
                    $"INNER JOIN F_ArtStock on F_ArtStock.AR_Ref = F_ARTICLE.AR_Ref " +
                    $"LEFT JOIN F_LOTSERIE ON F_LOTSERIE.AR_Ref = F_ARTICLE.AR_Ref ";

                if (ctnum != "Tous")
                {
                    query += $"INNER JOIN F_ARTFOURNISS ON F_ARTFOURNISS.AR_Ref = F_ARTICLE.AR_Ref ";
                    queryWhere += $" AND F_ARTFOURNISS.CT_Num = '{ctnum}' " +
                                  $" AND F_ARTFOURNISS.AF_Principal = 1";
                }
                if (codeFamille != "Tous")
                {
                    queryWhere += $" AND F_ARTICLE.FA_CodeFamille = '{codeFamille}' ";
                }

                query += $"WHERE F_ArtStock.DE_No = {deNo} AND F_ArtStock.DP_NoPrincipal = {dpNo} AND AS_QteSto > 0  AND ( LS_LotEpuise = 0 OR LS_LotEpuise IS NULL ) ";

                query += queryWhere;

                query += " ORDER BY  F_ARTICLE.AR_ref";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int qte = (int)(decimal)reader["AS_QteSto"];
                            bool lotserie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie"));
                            articles.Add(new Article
                            {
                                isChecked = false,
                                Ref = reader.IsDBNull(reader.GetOrdinal("AR_Ref")) ? "" : (string)reader["AR_Ref"],
                                CodeBarre = reader.IsDBNull(reader.GetOrdinal("AR_CodeBarre")) ? "" : (string)reader["AR_CodeBarre"],
                                LotSerie = reader.IsDBNull(reader.GetOrdinal("LS_NoSerie")) ? "" : (string)reader["LS_NoSerie"],
                                Qte = lotserie == false ? (int)(decimal)reader["LS_QteRestant"] : qte,
                                QteImp = lotserie == false ? qte1 ? 1 : (int)(decimal)reader["LS_QteRestant"] : qte,
                                Designation = reader.IsDBNull(reader.GetOrdinal("AR_Design")) ? "" : (string)reader["AR_Design"],
                            });
                        }
                    }
                }
            }

            return articles;
        }

        public static List<Article> SearchEasyEmpl(bool qte1, int deNo, int dpNo, string ctnum, string codeFamille)
        {
            List<Article> articles = new List<Article>();

            string catalogSage = Core.Utils.ExtractCatalog(Settings.Default.SageConnection);
            string catalogEasy = Core.Utils.ExtractCatalog(Settings.Default.EasyLogisticConnection);

            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();

                string query = "";
                string queryWhere = "";


                query += $"SELECT F_ARTICLE.AR_ref, AR_Design,AR_CodeBarre,QteSto,StockEmplacement.LotSerie " +
                    $"FROM {catalogSage}..F_ARTICLE " +
                    $"INNER JOIN {catalogEasy}..StockEmplacement ON StockEmplacement.AR_Ref = F_ARTICLE.AR_Ref ";

                if (ctnum != "Tous")
                {
                    query += $"INNER JOIN {catalogSage}..F_ARTFOURNISS ON F_ARTFOURNISS.AR_Ref = F_ARTICLE.AR_Ref ";
                    queryWhere += $" AND F_ARTFOURNISS.CT_Num = '{ctnum}' " +
                                  $" AND F_ARTFOURNISS.AF_Principal = 1 ";
                }

                if (codeFamille != "Tous")
                {
                    queryWhere += $" AND {catalogSage}..F_ARTICLE.FA_CodeFamille = '{codeFamille}' ";
                }

                query += $"WHERE StockEmplacement.DE_No = {deNo} AND StockEmplacement.DP_No = {dpNo} AND QteSto > 0 ";
               
                query += queryWhere;

                query += " ORDER BY  F_ARTICLE.AR_ref";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int qte = (int)(decimal)reader["QteSto"];
                            

                            articles.Add(new Article
                            {
                                isChecked = false,
                                Ref = reader.IsDBNull(reader.GetOrdinal("AR_Ref")) ? "" : (string)reader["AR_Ref"],
                                CodeBarre = reader.IsDBNull(reader.GetOrdinal("AR_CodeBarre")) ? "" : (string)reader["AR_CodeBarre"],
                                LotSerie = reader.IsDBNull(reader.GetOrdinal("LotSerie")) ? "" : (string)reader["LotSerie"],
                                Qte = qte,
                                QteImp = qte1 ? 1 : qte,
                                Designation = reader.IsDBNull(reader.GetOrdinal("AR_Design")) ? "" : (string)reader["AR_Design"],
                            });
                        }
                    }
                }
            }

            return articles;
        }
    }
}
