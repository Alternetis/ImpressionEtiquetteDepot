using ImpressionEtiquetteDepot.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpressionEtiquetteDepot.Model
{
    public class Emplacement
    {
        public int DpNo { get; set;}
        public string DpCode { get; set;}
        public static List<Emplacement> List( int deNo)
        {
            if (Settings.Default.DataEasyLogistic)
            {
                List<Emplacement> emplacements = new List<Emplacement>() { };
                emplacements.Add(new Emplacement
                {
                    DpCode = "Tous",
                    DpNo = 0
                });
                using (SqlConnection connection = new SqlConnection(Settings.Default.EasyLogisticConnection))
                {
                    connection.Open();
                    string query = $"SELECT DP_No, DP_Code FROM Emplacements WHERE DE_No = {deNo} ORDER BY DP_Code";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                emplacements.Add(new Emplacement
                                {
                                    DpNo = reader.GetInt32(0),
                                    DpCode = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return emplacements;
            }
            else
            {
                List<Emplacement> emplacements = new List<Emplacement>() { };
                emplacements.Add(new Emplacement
                {
                    DpCode = "Tous",
                    DpNo = 0
                });
                using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
                {
                    connection.Open();
                    string query = $"SELECT DP_No, DP_Code FROM F_DEPOTEMPL WHERE DE_No = {deNo} ORDER BY DP_Code";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                emplacements.Add(new Emplacement
                                {
                                    DpNo = reader.GetInt32(0),
                                    DpCode = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return emplacements;
            }
        }
    }

    public class Depot
    {
        public int DeNo {  get; set; }
        public string DeIntitule { get; set; }
        public List<Emplacement> Emplacements { get; set; }

        public static List<Depot> List()
        {
            List<Depot> depots = new List<Depot>() { };

            using (SqlConnection connection = new SqlConnection(Settings.Default.SageConnection))
            {
                connection.Open();
                string query = "SELECT DE_No, DE_Intitule FROM F_DEPOT ORDER BY DE_Intitule";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int deno = reader.GetInt32(0);
                            depots.Add(new Depot
                            {
                                DeNo = deno,
                                DeIntitule = reader.GetString(1),
                                Emplacements = Emplacement.List(deno)
                            });
                        }
                    }
                }
            }

            return depots;
        }
    }
}
