using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Emisor.Riesgo.Inmueble
{
    public class FormularioAP
    {
        public static bool GuardarFormAP(int codigo, FormularioAP formAp)
        {
            bool rta = true;

            if (!Models.CotizadorEmisor.Emisor.Riesgo.Inmueble.FormularioAP.GuardarFormulario(codigo, formAp))               
                {
                    rta = false;
                }          

            return rta;
        }

        private static bool GuardarFormulario(int codigo, FormularioAP formAp)
        {
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            bool rta = false;

            try
            {
                Helpers.SQL.Client.ExecuteScalar(conexion, "", codigo );
                rta = true;
            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Ocurrió un error al intentar actualizar la Declaracion Jurada (Indemnizacion): djadd " + codigo.ToString(), ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return rta;
        }
    }
}