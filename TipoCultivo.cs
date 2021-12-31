using Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Agro;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador
{
    public class TipoCultivo
    {
        #region Propiedades

        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public string FinVigencia { get; set; }
        public Cosecha Cosecha { get; set; }
        public Campaña Campaña { get; set; }        
        public bool HabilitadoWeb { get; set; }

        #endregion

        #region Metodos Publicos
        public static List<TipoCultivo> ObtenerTodos(int codPartido, int nuMoneda)
        {
            List<TipoCultivo> listaCultivo = new List<TipoCultivo>();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroCultivoPorVariosLoad",codPartido, nuMoneda, DateTime.Now, "S");
                listaCultivo = MapearLista(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }
            return listaCultivo;
        }       

        public static TipoCultivo ObtenerPorCodigo(int codCultivo, int codPartido)
        {
            TipoCultivo cultivo = new TipoCultivo();

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroCultivoPorCodigoLoad", codCultivo, codPartido);

                if (dr.Read())
                {
                    cultivo = MapearUno(dr);

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
            return cultivo;
        }

        public static bool ValidarsumaAsegurada(int codPartido, int codCultivo, int nuMoneda, decimal sumaAsegurada, ref string mensaje)
        {
            bool sumaAseguradaValida = false;
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);
            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroSumaMaxValidaLoad", codPartido, codCultivo, nuMoneda, sumaAsegurada, DateTime.Now);

                if (dr.Read())
                {
                    sumaAseguradaValida = Helpers.Conversion.AsBool(dr["esValido"]);
                    mensaje = Helpers.Conversion.AsString(dr["mensaje"]);
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
            return sumaAseguradaValida;
        }


        #endregion

        #region Metodos Privados

        private static List<TipoCultivo> MapearLista(DataTableReader dr)
        {
            List<TipoCultivo> listaCultivo = new List<TipoCultivo>();

            while (dr.Read())
            {
                listaCultivo.Add(MapearUno(dr));
            }

            return listaCultivo;
        }

        private static TipoCultivo MapearUno(DataTableReader dr)
        {
            TipoCultivo cultivo = new TipoCultivo();

            cultivo.Codigo = Helpers.Conversion.AsShort(dr["cocultivo"]);
            cultivo.Descripcion = Helpers.Conversion.AsString(dr["decultivo"]);
            cultivo.Cosecha = Cosecha.ObtenerPorCodigo(Helpers.Conversion.AsShort(dr["cocosecha"]));
            cultivo.Campaña = Campaña.ObtenerPorCodigo(Helpers.Conversion.AsShort(dr["cocampaña"]));
            cultivo.HabilitadoWeb = Helpers.Conversion.AsBool(dr["flhabilitadoweb"]);
            cultivo.FinVigencia = Helpers.Conversion.AsString(dr["nufinvig"]);

            return cultivo;
        }
        
        #endregion  
    }
}