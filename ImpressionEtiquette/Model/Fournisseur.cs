using ImpressionEtiquetteDepot.Properties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpressionEtiquetteDepot.Model
{
    public class Fournisseur
    {
        public string CtNum { get; set; }
        public string CtIntitule { get; set; }

        public static List<Fournisseur> List()
        {
            List<Fournisseur> fournisseurs = new List<Fournisseur>();
            fournisseurs.Add(new Fournisseur
            {
                CtNum = "Tous",
                CtIntitule = "Tous"
            });
            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();
                string query = $"SELECT CT_Num,CT_Intitule FROM F_COMPTET WHERE CT_Type = 1 ORDER BY CT_Intitule";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fournisseurs.Add(new Fournisseur
                            {
                                CtNum = reader.GetString(0),
                                CtIntitule = reader.GetString(1)
                            });
                        }
                    }
                }
            }


            return fournisseurs;
        }
    }
}
