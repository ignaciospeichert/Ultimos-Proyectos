using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Agro
{
    public class Cosecha
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }

        public static Cosecha ObtenerPorCodigo(short codigo)
        {
            Cosecha cosecha = new Cosecha();

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroCosechaPorCodigoLoad", codigo);

                if (dr.Read())
                {
                    cosecha = MapearUno(dr);

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
            return cosecha;
        }

        private static Cosecha MapearUno(DataTableReader dr)
        {
            Cosecha cosecha = new Cosecha();

            cosecha.Codigo = Helpers.Conversion.AsShort(dr["cocosecha"]);
            cosecha.Descripcion = Helpers.Conversion.AsString(dr["decosecha"]);    
              
            return cosecha;
        }
    }
}