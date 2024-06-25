using ImpressionEtiquetteDepot.Properties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpressionEtiquetteDepot.Model
{
    public class Famille
    {
        public string CodeFamille { get; set; }
        public string IntituleFamille { get; set; }

        public static List<Famille> List()
        {
            List<Famille> familles = new List<Famille>();
            familles.Add(new Famille
            {
                CodeFamille = "Tous",
                IntituleFamille = "Tous"
            });
            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();
                string query = $"SELECT FA_CodeFamille,FA_Intitule FROM F_FAMILLE";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            familles.Add(new Famille
                            {
                                CodeFamille = reader.GetString(0),
                                IntituleFamille = reader.GetString(1)
                            });
                        }
                    }
                }
            }


            return familles;
        }
    }
}
