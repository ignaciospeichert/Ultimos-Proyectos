using Extranet.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Microseguros
{
    public class Paquete
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public short NroSeccion { get; set; }


        public static Paquete ObtenerPorCodigo(short nroPaquete)
        {
            Paquete paquete = new Paquete();

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_ExtCotPaqPaquetePorCodigoLoad", nroPaquete);
                if(dr.Read())
                {
                    paquete = MapearUno(dr);
                }

            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Ocurrió un error al obtener el paquete.", ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return paquete;
        }

        private static Paquete MapearUno(DataTableReader dr)
        {
            Paquete paquete = new Paquete();

            paquete.Codigo = Conversion.AsInt(dr["copaquete"]);
            paquete.Descripcion = Conversion.AsString(dr["depaquete"]);
            paquete.NroSeccion = Conversion.AsShort(dr["nuseccion"]);

            return paquete;
        }
    }
}