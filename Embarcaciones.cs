using DocumentFormat.OpenXml.Wordprocessing;
using Extranet.Web.Models.Generales.Cotizador;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador
{
    public class Embarcaciones : RiesgoSujeto
    {
        public Embarcaciones()
        {
            this.ListaAños = ObtenerListaAños();                       
            this.Ubicacion = new Ubicacion();
            this.ListaTipoEmbarcacion = Extranet.Web.Models.CotizadorEmisor.Cotizador.TipoEmbarcacion.ObtenerListaTipoEmbarcacion();
            this.ListaEslora = ObtenerListaEslora();
            //this.ListaErrores = new List<String>();                              
        }

        #region Propiedades
        //public short NroItem { get; set; }

        [Display(Name = "Año")]
        public int Año { get; set; }
        public IEnumerable<SelectListItem> ListaAños { get; set; }

        [Display(Name = "Eslora")]
        public decimal Eslora { get; set; }        
        public IEnumerable<SelectListItem> ListaEslora { get; set; }                      

        [Display(Name = "Tipo Embarcacion")]
        public short CodEmbarcacion { get; set; }
        public IEnumerable<SelectListItem> ListaTipoEmbarcacion { get; set; }      

        //[Display(Name = "Cobertura")]
        //public short CodCobertura { get; set; }
        public IEnumerable<SelectListItem> ListaCobertura { get; set; }

        public string ConRoturaDePaloEnRegata { get; set; }
        public int LimiteRC { get; set; }        
        public int PorcAMR { get; set; }
        public int PorcRCPrima { get; set; }

        #endregion Propiedades

        #region Métodos

        public static Tarifa CorregirTarifa(short nroSeccion, short nroProvincia, int codLocalidad, short codigoPostal, int nroProductor, int codAcuerdo)
        {
            Tarifa tarifa = new Tarifa();
            tarifa = Tarifa.ObtenerTarifaFijaPorAcuerdoSeccion(nroSeccion, nroProductor, codAcuerdo);

            if (tarifa.CodTarifa == 0)
            {
                tarifa = Tarifa.ObtenerPorVarios(nroSeccion, nroProvincia, codLocalidad, codigoPostal).FirstOrDefault();
            }

            return tarifa;
        }
      

        public static IEnumerable<SelectListItem> ObtenerListaAños()
        {
            List<SelectListItem> listaAños = new List<SelectListItem>();
            int añoActual = DateTime.Now.Year;

            listaAños = MapearLista(añoActual);     

            return listaAños;
        }

        private static List<SelectListItem> MapearLista(int añoActual)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            for (int i = añoActual; i >= añoActual - 40; i--)
            {
                lista.Add(new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            return lista;
        }

        public static IEnumerable<SelectListItem> ObtenerListaEslora()
        {
            List<SelectListItem> listaEslora = new List<SelectListItem>();
            double esloraMax = 21.00;

            listaEslora = MapearListaEslora(esloraMax);           

            return listaEslora;
        }

        private static List<SelectListItem> MapearListaEslora(double esloraMax)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            for(double i = 3; i <= esloraMax;  i+=0.50)
            {
                lista.Add(new SelectListItem()
                {
                    Text = i.ToString("0.00"),
                    Value = i.ToString("0.00").Replace(',', '.')
                });
            }

            return lista;
        }                  

        //COPIADO DE LA VIEJA WEB

        public static void MapearDatos(ref Embarcaciones embarcacion, DateTime fechaPolizaDesde, int codAcuerdo, short nivelAutorizacion)
        {            
            embarcacion.Ubicacion.Tarifa = Tarifa.ObtenerPorVarios((short)Helpers.Enumeraciones.Seccion.Cascos, embarcacion.Ubicacion.NroProvincia, embarcacion.Ubicacion.CodLocalidad, (short)embarcacion.Ubicacion.CodPostal).FirstOrDefault();                      
            SeleccionarProvincia(ref embarcacion);
            SeleccionarAño(ref embarcacion);             
        }


        private static void SeleccionarAño(ref Embarcaciones embarcacion)
        {
            embarcacion.ListaAños = ObtenerListaAños();

            foreach (SelectListItem itemAño in embarcacion.ListaAños)
            {
                if (itemAño.Value == embarcacion.Año.ToString())
                {
                    itemAño.Selected = true;
                }
            }

            return;
        }      
 

        private static void SeleccionarProvincia(ref Embarcaciones embarcacion)
        {
            string nroProvinciaDefault = embarcacion.Ubicacion.NroProvincia.ToString();
            embarcacion.Ubicacion.ListaProvincia = Models.Generales.Provincia.ObtenerListaPorNroProvincia(0).Select(x => new SelectListItem() { Value = x.NuProvincia.ToString(), Text = x.DeProvincia, Selected = (x.NuProvincia.ToString() == nroProvinciaDefault) });
        }

        private static void SeleccionarLocalidad(ref Embarcaciones embarcacion)
        {
            embarcacion.Ubicacion.ListaProvincia = Models.Generales.Provincia.ObtenerListaPorNroProvincia(0).Select(x => new SelectListItem() { Value = x.NuProvincia.ToString(), Text = x.DeProvincia });

            foreach (SelectListItem itemProvincia in embarcacion.Ubicacion.ListaProvincia)
            {
                if (itemProvincia.Value == embarcacion.Ubicacion.NroProvincia.ToString())
                {
                    itemProvincia.Selected = true;
                }
            }
        }

        public static Extranet.Web.Models.CotizadorEmisor.Cotizador.Embarcaciones EditarModal(short nroProvinciaDefault, int codLocalidad, string localidadDefault, short codPostal)
        {
            Extranet.Web.Models.CotizadorEmisor.Cotizador.Embarcaciones embarcacion = new Extranet.Web.Models.CotizadorEmisor.Cotizador.Embarcaciones();
            MapearUbicacionDefault(ref embarcacion, nroProvinciaDefault, codLocalidad, localidadDefault, codPostal);

            return embarcacion;
        }

        private static void MapearUbicacionDefault(ref Extranet.Web.Models.CotizadorEmisor.Cotizador.Embarcaciones embarcacion, short nroProvinciaDefault, int codLocalidad, string localidadDefault, short codPostal)
        {
            if (nroProvinciaDefault > 0)
            {
                embarcacion.Ubicacion.ListaProvincia = Models.Generales.Provincia.ObtenerListaPorNroProvincia(0).Select(x => new SelectListItem() { Value = x.NuProvincia.ToString(), Text = x.DeProvincia, Selected = (x.NuProvincia.ToString() == nroProvinciaDefault.ToString()) });
                if (codLocalidad > 0)
                {
                    embarcacion.Ubicacion.CodPostal = codPostal;
                    embarcacion.Ubicacion.Localidad = localidadDefault;
                    embarcacion.Ubicacion.CodLocalidad = codLocalidad;
                }
            }
        }

        public static int ObtenerTipoOperacion(short codEmbarcacion)
        {
            switch (codEmbarcacion)
            {
                case 1:
                    return (int)Helpers.Enumeraciones.CotizacionTipoOperacion.CotCascosVelero;
                case 2:
                    return (int)Helpers.Enumeraciones.CotizacionTipoOperacion.CotCascosLancha;
                case 3:
                    return (int)Helpers.Enumeraciones.CotizacionTipoOperacion.CotCascosCruzero;
                default:
                    return 0;
            }
        }

        #endregion Métodos
    }
}