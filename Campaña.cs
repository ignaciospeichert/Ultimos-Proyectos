using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Agro
{
    public class Campaña
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public decimal PrecioQuintal { get; set; }


        public static Campaña ObtenerPorCodigo(short codigo)
        {
            Campaña campaña = new Campaña();

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroCampañaPorCodigoLoad", codigo, DateTime.Now);

                if (dr.Read())
                {
                    campaña = MapearUno(dr);

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return campaña;
        }

        private static Campaña MapearUno(DataTableReader dr)
        {
            Campaña campaña = new Campaña();

            campaña.Codigo = Helpers.Conversion.AsShort(dr["cocampaña"]);
            campaña.Descripcion = Helpers.Conversion.AsString(dr["decampaña"]);
            campaña.FechaDesde = Helpers.Conversion.AsDateTime(dr["fedesdecampaña"]);
            campaña.FechaHasta = Helpers.Conversion.AsDateTime(dr["fehastacampaña"]);
            campaña.PrecioQuintal = Helpers.Conversion.AsDecimal(dr["cuprecioquintal"]);

            return campaña;
        }
    }    
}