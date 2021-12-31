using Extranet.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Microseguros
{
    public class PaqueteCoberturaSuma
    {
        public Paquete Paquete { get; set; }
        public Generales.Cobertura Cobertura { get; set; }
        public decimal Suma { get; set; }
        public short NroOrden { get; set; }
        public bool FlDefault { get; set; }
        public DateTime VigenciaDesde { get; set; } 

        public PaqueteCoberturaSuma()
        {
            this.Paquete = new Paquete();
            this.Cobertura = new Generales.Cobertura();
        }

        public static IEnumerable<SelectListItem> ObtenerListaSumasSelect(int nroPaquete, int nroCobertura)
        {
            List<PaqueteCoberturaSuma> lista = ObtenerListaSumasPorPaqueteYCobertura(nroPaquete, nroCobertura);

            List<SelectListItem> listaSelect = MapearListaSelect(lista);

            if (listaSelect.Count > 1)
            {
                listaSelect.Insert(0, Helpers.Item.MapearSelect("0", "<< Todos >>", false));
            }
            
            int indiceSeleccionado = Helpers.Item.ObtenerIndiceSeleccion(listaSelect);

            SelectList objreturn = new SelectList(listaSelect, "Value", "Text", selectedValue: listaSelect[indiceSeleccionado]);
            return objreturn;
        }

        private static List<SelectListItem> MapearListaSelect(List<PaqueteCoberturaSuma> lista)
        {
            List<SelectListItem> selectLista = new List<SelectListItem>();

            foreach (PaqueteCoberturaSuma suma in lista)
            {
                selectLista.Add(Helpers.Item.MapearSelect(suma.Suma.ToString(), "$"+suma.Suma.ToString()+".-", suma.FlDefault));
            }

            return selectLista;
        }

        public static List<PaqueteCoberturaSuma> ObtenerListaSumasPorPaqueteYCobertura(int nroPaquete, int nroCobertura)
        {
            List<PaqueteCoberturaSuma> lista = new List<PaqueteCoberturaSuma>();

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_ExtCotPaqCoberturaSumaPorPaqueteCoberturaLoad", nroPaquete, nroCobertura);
                lista = MapearLista(dr);

            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Ocurrió un error al obtener la lista de sumas aseguradas.", ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return lista;
        }


        public static decimal ObtenerPrima(int tarifa, int nroCobertura, decimal suma)
        {
            decimal prima = 0;

            SqlConnection conexion = Helpers.SQL.Conexion.ObtenerInstancia(Helpers.SQL.Conexion.BaseDeDatosSQL.dtbSeguros);

            try
            {
                DataTableReader dr = Helpers.SQL.Client.ExecuteDataTableReader(conexion, "pa_CotCondComIntegralTarifaDetallePrimaMicrosegurosLoad", tarifa, nroCobertura, suma);
                if (dr.Read())
                {
                    prima = Conversion.AsDecimal(dr["cuprima"]);
                }
            }
            catch (Exception ex)
            {
                throw Helpers.Log.Log.NuevaExcepcion("Ocurrió un error al obtener la prima.", ex.Message);
            }
            finally
            {
                Helpers.SQL.Conexion.Desconectar(conexion);
            }

            return prima;
        }

        private static List<PaqueteCoberturaSuma> MapearLista(IDataReader dr)
        {
            List<PaqueteCoberturaSuma> lista = new List<PaqueteCoberturaSuma>();

            while (dr.Read())
            {
                lista.Add(MapearUno(dr));
            }

            return lista;
        }

        private static PaqueteCoberturaSuma MapearUno(IDataReader dr)
        {
            PaqueteCoberturaSuma suma = new PaqueteCoberturaSuma();

            suma.Paquete = Paquete.ObtenerPorCodigo(Conversion.AsShort(dr["copaquete"]));
            suma.Cobertura = Generales.Cobertura.ObtenerPorCodigo(Conversion.AsInt(dr["cocobertura"]), 0);
            suma.Suma = Conversion.AsInt(dr["cusuma"]);
            suma.NroOrden = Conversion.AsShort(dr["nuorden"]);
            suma.FlDefault = Conversion.AsBool(dr["fldefault"]);
            suma.VigenciaDesde = Conversion.AsDateTime(dr["fevigdesde"]);

            return suma;
        }
    }
}