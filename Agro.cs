using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Extranet.Web.BostonFrameworkServiceReference;
using Extranet.Web.Models.Generales.Cotizador;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador
{
    public class Agro: RiesgoSujeto
    {        

        public Agro()
        {           
            this.Ubicacion = new Ubicacion();
            this.ListaTipoCultivo = new List<TipoCultivo>();
            this.ListaAdicionales = new List<TipoAdicional>();
            this.ListaErrores = new List<Generales.Error>();
            this.Cultivo = new TipoCultivo();
            this.ListaFranquicias = new List<Franquicia>();              
        }       

        #region Propiedades     

        [Display(Name = "Tipo Cultivo")]      
        public TipoCultivo Cultivo { get; set; }  
        public List<TipoCultivo> ListaTipoCultivo { get; set; }                  
        public decimal Hectareas { get; set; }
        public decimal SumaAseguradaPorHectarea { get; set; }
        public string NombreCampo { get; set; }       
        public List<TipoAdicional> ListaAdicionales { get; set; }
        public List<Franquicia> ListaFranquicias { get; set; }

        #endregion Propiedades


        public static void MapearDatos(ref Agro agro, int codTipoMoneda)
        {
            agro.Ubicacion.Tarifa = Models.Generales.Cotizador.Tarifa.ObtenerPorPartido(agro.Ubicacion.CodPartido);
            agro.ListaTipoCultivo = Models.CotizadorEmisor.Cotizador.TipoCultivo.ObtenerTodos(agro.Ubicacion.CodPartido, codTipoMoneda);
            agro.Cultivo = Models.CotizadorEmisor.Cotizador.TipoCultivo.ObtenerPorCodigo(agro.Cultivo.Codigo, agro.Ubicacion.CodPartido);
            agro.ListaAdicionales = TipoAdicional.ObtenerListaAdicionalesSeleccionados(agro.ListaAdicionales, agro.Cultivo.Codigo, agro.Ubicacion.CodPartido);
            agro.ListaFranquicias = MarcarFranquiciaSeleccionada(agro.Franquicia, Franquicia.ObtenerTodas(agro.Ubicacion.CodPartido, agro.Cultivo.Codigo));
            SeleccionarProvincia(ref agro);
            SeleccionarPartido(ref agro);
        }

        private static List<Franquicia> MarcarFranquiciaSeleccionada(Franquicia franquicia, List<Franquicia> listaFranquicias)
        {
            foreach (var franqui in listaFranquicias)
            {
                if (franqui.Codigo == franquicia.Codigo)
                {
                    franqui.EsDefault = "S";
                }
                else
                {
                    franqui.EsDefault = "N";
                }
            }
            return listaFranquicias;
        }

      

        private static void SeleccionarProvincia(ref Agro agro)
        {
            string nroProvinciaDefault = agro.Ubicacion.NroProvincia.ToString();
            agro.Ubicacion.ListaProvincia = Models.Generales.Provincia.ObtenerListaPorNroProvincia(0).Where(x => x.NuProvincia != 26).Select(x => new SelectListItem() { Value = x.NuProvincia.ToString(), Text = x.DeProvincia, Selected = (x.NuProvincia.ToString() == nroProvinciaDefault) });
        }

        private static void SeleccionarPartido(ref Agro agro)
        {
            agro.Ubicacion.ListaPartido = Models.Generales.Partido.ObtenerPorNuProvincia(0).Select(x => new SelectListItem() { Value = x.Codigo.ToString(), Text = x.Descripcion });

            foreach (SelectListItem itemPartido in agro.Ubicacion.ListaPartido)
            {
                if (itemPartido.Value == agro.Ubicacion.CodPartido.ToString())
                {
                    itemPartido.Selected = true;
                }
            }
        }

        public static Tarifa CorregirTarifa(short nroSeccion, int codPartido, int nroProductor, int codAcuerdo)
        {
            Tarifa tarifa = new Tarifa();
            tarifa = Tarifa.ObtenerTarifaFijaPorAcuerdoSeccion(nroSeccion, nroProductor, codAcuerdo);

            //if (tarifa.CodTarifa == 0)
            //{
            //    tarifa = Tarifa.ObtenerPorVarios(nroSeccion, nroProvincia, codLocalidad, codigoPostal).FirstOrDefault();
            //}

            return tarifa;
        }

        public static Extranet.Web.Models.CotizadorEmisor.Cotizador.Agro EditarModal(short nroProvinciaDefault, int nuTipoMoneda)
        {
            Extranet.Web.Models.CotizadorEmisor.Cotizador.Agro agro = new Extranet.Web.Models.CotizadorEmisor.Cotizador.Agro();        
            MapearUbicacionDefault(ref agro, nroProvinciaDefault);

            return agro;
        }

        private static void MapearUbicacionDefault(ref Extranet.Web.Models.CotizadorEmisor.Cotizador.Agro agro, short nroProvinciaDefault)
        {
            if (nroProvinciaDefault > 0)
            {
                int codPartidoDefault = 0;
                agro.Ubicacion.ListaProvincia = Models.Generales.Provincia.ObtenerListaPorNroProvincia(0).Where(x => x.NuProvincia != 26).Select(x => new SelectListItem() { Value = x.NuProvincia.ToString(), Text = x.DeProvincia, Selected = (x.NuProvincia.ToString() == nroProvinciaDefault.ToString()) });
                if (codPartidoDefault > 0)
                {
                    agro.Ubicacion.CodPartido = codPartidoDefault;
                }
            }
        }

        public static List<Agro> CompletarListaDatos(List<Agro> listaRiesgos)
        {
            List<Agro> listaAgro = new List<Agro>();
            foreach (Agro agro in listaRiesgos)
            {
                listaAgro.Add(CompletarDatos(agro));
            }

            return listaAgro.OrderBy(e => e.NroItem).ToList();
        }

        private static Agro CompletarDatos(Agro agro)
        {
            agro.Cultivo = Models.CotizadorEmisor.Cotizador.TipoCultivo.ObtenerPorCodigo(agro.Cultivo.Codigo, agro.Ubicacion.CodPartido);            
            
            return agro;
        }

        public static decimal ObtenerCantQuintal(int codTipoMoneda, decimal sumaAseguradaPorHectarea)
        {
            decimal cantQuintal = 0;
            if (codTipoMoneda == (short)Helpers.Enumeraciones.TipoMonedaSise.Quintales)
            {
                cantQuintal = sumaAseguradaPorHectarea;
            }
            return cantQuintal;
        }
    }
}
