using ImpressionEtiquetteDepot.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpressionEtiquetteDepot.Core
{
    public static class Print
    {
        public static byte[] ReplaceStringInBytes(byte[] array, string toReplace, string replace)
        {
            //string str = Encoding.Default.GetString(array);
            string str = Encoding.GetEncoding(850).GetString(array);
            while (str.Contains(toReplace))
            {
                int start = Encoding.Default.GetByteCount(str.Substring(0, str.IndexOf(toReplace)));
                int length = Encoding.Default.GetByteCount(toReplace);
                int length2 = Encoding.Default.GetByteCount(replace);

                byte[] array2 = new byte[array.Length - length + length2];
                Array.Copy(array, 0, array2, 0, start);
                Array.Copy(Encoding.Default.GetBytes(replace), 0, array2, start, length2);
                Array.Copy(array, start + length, array2, start + length2, array.Length - start - length);

                array = array2;

                str = str.Substring(0, str.IndexOf(toReplace)) + replace + str.Substring(str.IndexOf(toReplace) + toReplace.Length, str.Length - str.IndexOf(toReplace) - toReplace.Length);
            }

            return array;
        }
        private static string GetCategoryValue(string content, string keyStart, string keyEnd)
        {
            int startIndex = content.IndexOf(keyStart);
            if (startIndex != -1)
            {
                startIndex += keyStart.Length;
                int endIndex = content.IndexOf(keyEnd, startIndex);
                if (endIndex != -1)
                {
                    return content.Substring(startIndex, endIndex - startIndex);
                }
            }
            return null;
        }

        public static void NewPrintArticle(string report, Article article)
        {
            string reportContent = File.ReadAllText(report);

            #region Use Old V.
            string designation = "";
            string codeBarre = article.CodeBarre;
            string enumere = "";
            //string quantite = "";

            DateTime? peremption = null;

            if (string.IsNullOrEmpty(codeBarre))
            {
                codeBarre = null;
            }
            designation = article.Designation;

            byte[] bytes = File.ReadAllBytes(report);

            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION]", designation);
            string designation1 = designation;
            if (designation1.Length > 35)
            {
                designation1 = designation1.Substring(0, 35);
            }
            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION1]", designation1);
            string designation2 = " ";
            if (designation.Length > 35)
            {
                designation2 = designation.Substring(35);
                if (designation2.Length > 35)
                {
                    designation2 = designation2.Substring(0, 35);
                }
            }
            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION2]", designation2);
            bytes = ReplaceStringInBytes(bytes, "[REFERENCE]", article.Ref);
            bytes = ReplaceStringInBytes(bytes, "[CODEBARRE]", codeBarre ?? article.Ref);
            bytes = ReplaceStringInBytes(bytes, "[ENUMERE]", enumere);
            bytes = ReplaceStringInBytes(bytes, "[QUANTITE]", article.Qte.ToString());

            if (peremption?.Year > 1900)
            {
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION_LIBELLE]", "");
                bytes = ReplaceStringInBytes(bytes, "[/PEREMPTION_LIBELLE]", "");
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION]", peremption?.ToShortDateString());
            }
            else
            {
                string libelle = File.ReadAllLines(report).FirstOrDefault(l => l.Contains("[PEREMPTION_LIBELLE]"));
                if (!string.IsNullOrEmpty(libelle))
                {
                    libelle = libelle.Substring(libelle.IndexOf("[PEREMPTION_LIBELLE]"), libelle.IndexOf("[/PEREMPTION_LIBELLE]") - libelle.IndexOf("[PEREMPTION_LIBELLE]") + "/[PEREMPTION_LIBELLE]".Length);
                    bytes = ReplaceStringInBytes(bytes, libelle, "");
                }
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION]", "");
            }
            #endregion

            string categorieTarifairePrix = GetCategoryValue(reportContent, "[CATEGORIETARIFAIRE_PRIX,", "]");
            if (!string.IsNullOrEmpty(categorieTarifairePrix))
            {
                string key = $"[CATEGORIETARIFAIRE_PRIX,{categorieTarifairePrix}]";
                decimal prixVente = 0;
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SageConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT CASE WHEN ISNULL(AC_PrixVen, 0) = 0 THEN AR_PrixVen ELSE AC_PrixVen END AS AC_PrixVen FROM F_ARTICLE"
                        + " LEFT OUTER JOIN (SELECT AR_Ref, AC_PrixVen FROM F_ARTCLIENT INNER JOIN P_CATTARIF ON P_CATTARIF.cbIndice = F_ARTCLIENT.AC_Categorie"
                        + $" WHERE AR_Ref = '{article.Ref}' AND CT_Intitule = '{categorieTarifairePrix}') F_ARTCLIENT ON F_ARTCLIENT.AR_Ref = F_ARTICLE.AR_Ref WHERE F_ARTICLE.AR_Ref = '{article.Ref}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                prixVente = (decimal)reader["AC_PrixVen"];
                            }
                        }
                    }
                }
                bytes = ReplaceStringInBytes(bytes, key, prixVente.ToString("F2"));
            }

            string categorieTarifaireTTTC = GetCategoryValue(reportContent, "[CATEGORIETARIFAIRE_TTC,", "]");
            if (!string.IsNullOrEmpty(categorieTarifaireTTTC))
            {
                string key = $"[CATEGORIETARIFAIRE_TTC,{categorieTarifaireTTTC}]";
                bool TTC = false;
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SageConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT CASE WHEN ISNULL(AC_PrixTTC, 0) = 0 THEN AR_PrixTTC ELSE AC_PrixTTC END AS AC_PrixTTC FROM F_ARTICLE"
                        + " LEFT OUTER JOIN (SELECT AR_Ref, AC_PrixTTC FROM F_ARTCLIENT INNER JOIN P_CATTARIF ON P_CATTARIF.cbIndice = F_ARTCLIENT.AC_Categorie"
                        + $" WHERE AR_Ref = '{article.Ref}' AND CT_Intitule = '{categorieTarifaireTTTC}') F_ARTCLIENT ON F_ARTCLIENT.AR_Ref = F_ARTICLE.AR_Ref WHERE F_ARTICLE.AR_Ref = '{article.Ref}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TTC = (short)reader["AC_PrixTTC"] == 1;
                            }
                        }
                    }
                }
                bytes = ReplaceStringInBytes(bytes, key, TTC ? "TTC" : "HT");
            }

            //string categoryTarifairesp = GetCategoryValue(reportContent, "[CATEGORIETARIFAIRE_SP", "]");

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TarifSp_Calcul))
            {
                // Supposons que TarifSp_Calcul soit une chaîne de caractères
                string tarifSpCalcul = Properties.Settings.Default.TarifSp_Calcul;

                // Découper la chaîne en utilisant le caractère ';'
                string[] tarifCalcule = tarifSpCalcul.Split(';');

                string key = $"[CATEGORIETARIFAIRE_SP]";
                decimal prixVente = 0;
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SageConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT CASE WHEN ISNULL(AC_PrixVen, 0) = 0 THEN AR_PrixVen ELSE AC_PrixVen END AS AC_PrixVen FROM F_ARTICLE"
                        + " LEFT OUTER JOIN (SELECT AR_Ref, AC_PrixVen FROM F_ARTCLIENT INNER JOIN P_CATTARIF ON P_CATTARIF.cbIndice = F_ARTCLIENT.AC_Categorie"
                        + $" WHERE AR_Ref = '{article.Ref}' AND CT_Intitule = '{tarifCalcule[0]}') F_ARTCLIENT ON F_ARTCLIENT.AR_Ref = F_ARTICLE.AR_Ref WHERE F_ARTICLE.AR_Ref = '{article.Ref}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                prixVente = (decimal)reader["AC_PrixVen"];
                            }
                        }
                    }
                }

                if (tarifCalcule.Count() == 2)
                {
                    prixVente = prixVente * decimal.Parse(tarifCalcule[1]);
                }
                else if (tarifCalcule.Count() != 2)
                {
                    Core.Log.WriteLog($"Le TarifSpecial {tarifSpCalcul} n'est pas valide");
                }
                bytes = ReplaceStringInBytes(bytes, key, prixVente.ToString("F2"));
            }

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                for (int i = 0; i < article.QteImp; i++)
                {
                    new RawPrint.Printer().PrintRawStream(Properties.Settings.Default.Imprimante, stream, $"{article.Ref}", false);
                }
            }
        }
        public static void PrintArticle(string report, Article article)
        {
            string designation = "";
            string codeBarre = "";
            string enumere = "";
            //string quantite = "";
            DateTime? peremption = null;

            if (string.IsNullOrEmpty(codeBarre))
            {
                codeBarre = article.CodeBarre;
            }
            designation = article.Designation;

            byte[] bytes = File.ReadAllBytes(report);
            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION]", designation);
            string designation1 = designation;
            if (designation1.Length > 35)
            {
                designation1 = designation1.Substring(0, 35);
            }
            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION1]", designation1);
            string designation2 = " ";
            if (designation.Length > 35)
            {
                designation2 = designation.Substring(35);
                if (designation2.Length > 35)
                {
                    designation2 = designation2.Substring(0, 35);
                }
            }
            bytes = ReplaceStringInBytes(bytes, "[DESIGNATION2]", designation2);
            bytes = ReplaceStringInBytes(bytes, "[REFERENCE]", article.Ref);
            bytes = ReplaceStringInBytes(bytes, "[CODEBARRE]", codeBarre ?? article.Ref);
            bytes = ReplaceStringInBytes(bytes, "[ENUMERE]", enumere);
            bytes = ReplaceStringInBytes(bytes, "[QUANTITE]", article.Qte.ToString());

            if (peremption?.Year > 1900)
            {
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION_LIBELLE]", "");
                bytes = ReplaceStringInBytes(bytes, "[/PEREMPTION_LIBELLE]", "");
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION]", peremption?.ToShortDateString());
            }
            else
            {
                string libelle = File.ReadAllLines(report).FirstOrDefault(l => l.Contains("[PEREMPTION_LIBELLE]"));
                if (!string.IsNullOrEmpty(libelle))
                {
                    libelle = libelle.Substring(libelle.IndexOf("[PEREMPTION_LIBELLE]"), libelle.IndexOf("[/PEREMPTION_LIBELLE]") - libelle.IndexOf("[PEREMPTION_LIBELLE]") + "/[PEREMPTION_LIBELLE]".Length);
                    bytes = ReplaceStringInBytes(bytes, libelle, "");
                }
                bytes = ReplaceStringInBytes(bytes, "[PEREMPTION]", "");
            }
            string categorieTarifairePrix = File.ReadAllLines(report).FirstOrDefault(l => l.Contains("[CATEGORIETARIFAIRE_PRIX,"));
            if (!string.IsNullOrEmpty(categorieTarifairePrix))
            {
                string key = categorieTarifairePrix.Substring(categorieTarifairePrix.IndexOf('['), categorieTarifairePrix.IndexOf(']') - categorieTarifairePrix.IndexOf('[') + 1);
                string categorieTarifaire = key.Replace("[CATEGORIETARIFAIRE_PRIX,", "").Replace("]", "");
                double prixVente = 0;
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SageConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT CASE WHEN ISNULL(AC_PrixVen, 0) = 0 THEN AR_PrixVen ELSE AC_PrixVen END AS AC_PrixVen FROM F_ARTICLE"
                        + " LEFT OUTER JOIN (SELECT AR_Ref, AC_PrixVen FROM F_ARTCLIENT INNER JOIN P_CATTARIF ON P_CATTARIF.cbIndice = F_ARTCLIENT.AC_Categorie"
                        + $" WHERE AR_Ref = '{article.Ref}' AND CT_Intitule = '{categorieTarifaire}') F_ARTCLIENT ON F_ARTCLIENT.AR_Ref = F_ARTICLE.AR_Ref WHERE F_ARTICLE.AR_Ref = '{article.Ref}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                prixVente = (double)(decimal)reader["AC_PrixVen"];
                            }
                        }
                    }
                }
                bytes = ReplaceStringInBytes(bytes, key, prixVente.ToString("F2"));
            }
            string categorieTarifaireTTTC = File.ReadAllLines(report).FirstOrDefault(l => l.Contains("[CATEGORIETARIFAIRE_TTC,"));
            if (!string.IsNullOrEmpty(categorieTarifaireTTTC))
            {
                string key = categorieTarifaireTTTC.Substring(categorieTarifaireTTTC.IndexOf('['), categorieTarifaireTTTC.IndexOf(']') - categorieTarifaireTTTC.IndexOf('[') + 1);
                string categorieTarifaire = key.Replace("[CATEGORIETARIFAIRE_TTC,", "").Replace("]", "");
                bool TTC = false;
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.SageConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("SELECT CASE WHEN ISNULL(AC_PrixTTC, 0) = 0 THEN AR_PrixTTC ELSE AC_PrixTTC END AS AC_PrixTTC FROM F_ARTICLE"
                        + " LEFT OUTER JOIN (SELECT AR_Ref, AC_PrixTTC FROM F_ARTCLIENT INNER JOIN P_CATTARIF ON P_CATTARIF.cbIndice = F_ARTCLIENT.AC_Categorie"
                        + $" WHERE AR_Ref = '{article.Ref}' AND CT_Intitule = '{categorieTarifaire}') F_ARTCLIENT ON F_ARTCLIENT.AR_Ref = F_ARTICLE.AR_Ref WHERE F_ARTICLE.AR_Ref = '{article.Ref}'", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TTC = (short)reader["AC_PrixTTC"] == 1;
                            }
                        }
                    }
                }
                bytes = ReplaceStringInBytes(bytes, key, TTC ? "TTC" : "HT");
            }

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                for (int i = 0; i < article.QteImp; i++)
                {
                    new RawPrint.Printer().PrintRawStream(Properties.Settings.Default.Imprimante, stream, $"{article.Ref}", false);
                }
            }
        }

    }
}
