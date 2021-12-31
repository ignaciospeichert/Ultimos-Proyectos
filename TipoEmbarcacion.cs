using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador
{
    public class TipoEmbarcacion
    {
        public static IEnumerable<SelectListItem> ObtenerListaTipoEmbarcacion()
        {
            List<SelectListItem> listaTipoEmbarcacion = new List<SelectListItem>();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotCascosTipoEmbarcacionTodosLoad");               
                listaTipoEmbarcacion = MapearLista(dr);              
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }
            return listaTipoEmbarcacion;
        }

        private static List<SelectListItem> MapearLista(DataTableReader dr)
        {
            List<SelectListItem> listaSelect = new List<SelectListItem>();

            while (dr.Read())
            { 
            listaSelect.Add(Extranet.Web.Helpers.Item.MapearSelect(Helpers.Conversion.AsString(dr["cotipoembarcacion"]), Helpers.Conversion.AsString(dr["detipoembarcacion"])));
            }

            return listaSelect;
        }
      
    }
}