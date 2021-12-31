using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Extranet.Web.Models.CotizadorEmisor.Cotizador.Riesgos.Microseguros
{
    public class PaqueteObjeto : RiesgoSujeto
    {
        public PaqueteObjeto()
        {
            this.ListaCoberturas = new List<SelectListItem>();
            this.ListaSumas = new List<SelectListItem>();
        }

        [Display(Name = "Cobertura")]
        public int NroCobertura { get; set; }
        public List<SelectListItem> ListaCoberturas { get; set; }

        [Display(Name = "Suma")]
        public decimal Suma { get; set; }
        public List<SelectListItem> ListaSumas { get; set; }
        
        public List<PaqueteObjeto> ListaPaqueteObjetos { get; set; }
        
        public static PaqueteObjeto EditarModal(ref PaqueteObjeto obj, int tipoPaquete)
        {
            obj.ListaCoberturas = Generales.Cobertura.ObtenerPorSeccion(Helpers.Conversion.AsInt(Helpers.Enumeraciones.Seccion.CombinadoFamiliar), 0, Helpers.Conversion.AsShort(tipoPaquete)).Select(o => new SelectListItem() { Value = o.Codigo.ToString(), Text = o.Descripcion }).ToList();
            obj = ObtenerListaCoberturaFiltrada(ref obj);
            obj.ListaSumas = new List<SelectListItem>();

            return obj;
        }

        private static PaqueteObjeto ObtenerListaCoberturaFiltrada(ref PaqueteObjeto obj)
        {
            foreach(var cobertura in obj.ListaPaqueteObjetos)
            {
                SelectListItem element = obj.ListaCoberturas.Find(cob => Helpers.Conversion.AsInt(cob.Value) == cobertura.Cobertura.Codigo);
                obj.ListaCoberturas.Remove(element);
            }
            return obj;
        }
    }
}