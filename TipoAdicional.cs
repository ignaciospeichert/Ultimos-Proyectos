using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extranet.Web.BostonFrameworkServiceReference;
using Extranet.Web.Models.Generales.Cotizador;
using Extranet.Web.Helpers;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador
{
    public class TipoAdicional
    {
        #region Propiedades

        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public bool EstaSeleccionado { get; set; }
        public decimal Prima { get; set; }
        public bool HabilitadoWeb { get; set; }

        #endregion

        #region Metodos Publicos

        public static List<TipoAdicional> ObtenerTodos()
        {
            List<TipoAdicional> listaAdicionales = new List<TipoAdicional>();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroAdicionalTodosLoad");
                listaAdicionales = MapearLista(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }
            return listaAdicionales;
        }

        public static List<TipoAdicional> ObtenerPorVarios(int codCultivo, int codPartido)
        {
            List<TipoAdicional> listaAdicionales = new List<TipoAdicional>();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroAdicionalPorVariosLoad", codCultivo, codPartido, "S");
                listaAdicionales = MapearLista(dr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }
            return listaAdicionales;
        }

        public static TipoAdicional ObtenerPorCodigo(int codAdicional)
        {
            TipoAdicional  adicional = new TipoAdicional();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroAdicionalPorCodigoLoad", codAdicional);
                while (dr.Read())
                {
                    adicional = MapearUno(dr);
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
            return adicional;
        }

        private static TipoAdicional ObtenerAdicionalPorTipoCobertura(string codTipoCobertura)
        {
            TipoAdicional adicional = new TipoAdicional();
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroAdicionalPorTipoCoberturaLoad", codTipoCobertura);
                while (dr.Read())
                {
                    adicional = MapearUno(dr);
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
            return adicional;
        }

        public static List<TipoAdicional> ObtenerListaAdicionalesSeleccionados(List<TipoAdicional> listaAdicionales, int codCultivo, int codPartido)
        {
            List<TipoAdicional> listaAdicionalesTodos = ObtenerPorVarios(codCultivo, codPartido);
            List<TipoAdicional> listaAdicionalesASeleccionar = listaAdicionales;            
            if (listaAdicionalesASeleccionar != null)
            {
                listaAdicionalesASeleccionar = TipoAdicional.MarcarSeleccionados(listaAdicionalesTodos,listaAdicionalesASeleccionar);
            }
            return listaAdicionalesASeleccionar;
        }       

        public static List<TipoAdicional> ObtenerAdicionales(List<Generales.Cotizador.CoberturaRiesgo> listaCoberturasRiesgo)
        {
            List<TipoAdicional> listaAccesorios = new List<TipoAdicional>();

            foreach (Models.Generales.Cotizador.CoberturaRiesgo cobertura in listaCoberturasRiesgo)
            {
                if (EsAdicional(cobertura.TipoCobertura.Codigo.ToUpper()))
                {
                    TipoAdicional tipoadicional = ObtenerAdicionalPorTipoCobertura(cobertura.TipoCobertura.Codigo);
                    tipoadicional.Prima = cobertura.Prima;
                    listaAccesorios.Add(tipoadicional);
                }
            }            

            return listaAccesorios;
        }

        private static bool EsAdicional(string codTipoCobertura)
        {
            bool existe = false;
            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotAgroAdicionalExistePorTipoCoberturaLoad", codTipoCobertura);
                if (dr.Read())
                {
                    existe = Conversion.AsBool(dr["existe"]);
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

            return existe;
        }

        public static List<TipoAdicional> MarcarSeleccionados(List<TipoAdicional> listaAdicionales, List<TipoAdicional> listaAdicionalesSeleccionados)
        {           
                foreach (TipoAdicional adicional in listaAdicionales)
                {
                    foreach (TipoAdicional adicionalASelecionar in listaAdicionalesSeleccionados)
                    {
                        if (adicional.Codigo == adicionalASelecionar.Codigo)
                        {
                            adicional.EstaSeleccionado = true;                           
                        }
                    }
                }
           
            return listaAdicionales;
        }

        public static List<TipoAdicional> ObtenerListaParaAgroPorCoberturasRiesgo(List<Models.Generales.Cotizador.CoberturaRiesgo> listaCoberturaRiesgo)
        {
            List<TipoAdicional> listaAdicionales = new List<TipoAdicional>();

            foreach (Models.Generales.Cotizador.CoberturaRiesgo cob in listaCoberturaRiesgo)
            {
                if (cob.TipoCobertura.Codigo.ToUpper() != "GRA")
                {
                    TipoAdicional adicional = TipoAdicional.ObtenerAdicionalPorTipoCobertura(cob.TipoCobertura.Codigo);
                    listaAdicionales.Add(adicional);
                }
            }

            return listaAdicionales;
        }

        private static TipoAdicional ObtenerAdicional(short codigoAccesorio)
        {
            TipoAdicional tipoAdicional = TipoAdicional.ObtenerPorCodigo(codigoAccesorio);            
            return tipoAdicional;
        }


        #endregion

        #region Metodos Privados
        private static TipoAdicional MapearUno(DataTableReader dr)
        {
            TipoAdicional adicional = new TipoAdicional();
            adicional.Codigo = Helpers.Conversion.AsShort(dr["coagroadicional"]);
            adicional.Descripcion = Helpers.Conversion.AsString(dr["deagroadicional"]);
            adicional.HabilitadoWeb = Helpers.Conversion.AsBool(dr["flhabilitadoweb"]);

            return adicional;
        }

        private static List<TipoAdicional> MapearLista(DataTableReader dr)
        {
            List<TipoAdicional> listaAdicionales = new List<TipoAdicional>();

            while (dr.Read())
            {
                listaAdicionales.Add(MapearUno(dr));
            }

            return listaAdicionales;
        }

        public static ArrayOfInt PasarAArray(List<TipoAdicional> listaAdicionales)
        {
            ArrayOfInt array = new ArrayOfInt();
            foreach (var adicional in listaAdicionales)
            {
                array.Add(adicional.Codigo);
            }
            return array;
        }
        #endregion
    }
}
