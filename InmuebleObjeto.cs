using Extranet.Web.Models.CotizadorEmisor.Emisor.Cotizacion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Emisor.Riesgo.Inmueble
{
    public class InmuebleObjeto
    {
        public int Codigo { get; set; }
        public Models.Generales.TipoObjeto TipoObjeto { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string NumeroDeSerie { get; set; }
        public decimal SumaAsegurada { get; set; }
        public Generales.Cobertura Cobertura { get; set; }
        public Models.CotizadorEmisor.Emisor.Riesgo.Inmueble.FormularioAP FormAp { get; set; }

        public static List<InmuebleObjeto> ObtenerDatosInmuebleObjeto(int codRiesgo)
        {           
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            List<InmuebleObjeto> listainmuebleobjeto = new List<InmuebleObjeto>();
            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotCotizacionInmuebleObjetoLoadAll", codRiesgo);
                listainmuebleobjeto = MapearLista(dr);

            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Error al obtener la lista objetos.", ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return listainmuebleobjeto;
        }

        private static List<InmuebleObjeto> MapearLista(DataTableReader dr)
        {
            List<InmuebleObjeto> listaInmueble = new List<InmuebleObjeto>();

            while (dr.Read())
            {
                listaInmueble.Add(MapearUno(dr));
            }

            return listaInmueble;
        }


        private static InmuebleObjeto MapearUno(DataTableReader dr)
        {
            InmuebleObjeto inmuebleobjeto = new InmuebleObjeto();

            inmuebleobjeto.Codigo = Helpers.Conversion.AsInt(dr["coinmuebleobjeto"]);
            inmuebleobjeto.TipoObjeto = Models.Generales.TipoObjeto.ObtenerPorCodigo(Helpers.Conversion.AsInt(dr["cotipoobjeto"]));
            inmuebleobjeto.Marca = Helpers.Conversion.AsString(dr["demarca"]);
            inmuebleobjeto.Modelo = Helpers.Conversion.AsString(dr["demodelo"]);
            inmuebleobjeto.NumeroDeSerie = Helpers.Conversion.AsString(dr["denumeroserie"]);
            inmuebleobjeto.SumaAsegurada = Helpers.Conversion.AsDecimal(dr["cusumaasegurada"]);            
            inmuebleobjeto.Cobertura = Models.Generales.Cobertura.ObtenerPorCodigo(Helpers.Conversion.AsInt(dr["cocoberturaaaplicar"]), 0);

            return inmuebleobjeto;
        }

        public static bool ActualizarInmuebleObjeto(int codRiesgo, int codTipoObjeto, string marca, string modelo)
        {
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            bool rta = false;

            try
            {
                Helpers.SQL.Client.ExecuteScalar(conexion, "pa_CotCotizacionInmuebleObjetoUpd", codRiesgo, codTipoObjeto, marca, modelo);
                rta = true;
            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Ocurrió un error al intentar actualizar el objeto: " + codTipoObjeto.ToString(), ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return rta;
        }

        
    }   
}