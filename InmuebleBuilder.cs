using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extranet.Web.Models.CotizadorEmisor.Emisor.Riesgo.Inmueble
{
    public class InmuebleBuilder : SolicitudBuilder
    {
        #region Constructor

        public InmuebleBuilder(int codUsuario, int codCotizacion)
        {
            this.CodUsuario = codUsuario;
            this.Solicitud = new SolicitudViewModel(codCotizacion, "Inmueble");
        }

        #endregion

        #region Metodos Publicos

        public override void BuildTomador()
        {
            Solicitud.Tomador = Tomador.TomadorViewModel.Obtener(this.Solicitud.Cotizacion.Tomador);
        }

        public override void BuildRiesgo()
        {
            Solicitud.Riesgo = new RiesgoViewModel();
            Solicitud.Riesgo.SetearTipo(new Generales.Inmueble());
            Solicitud.Riesgo.ObtenerDatosSegunCotizacion(this.Solicitud.Cotizacion);
        }

        public override void BuildMedioPago()
        {
            Solicitud.MedioPago = new MedioPago.MedioPagoViewModel(Solicitud.Tomador.Tomador.NroAsegurado, this.Solicitud.Cotizacion);
        }

        public override void BuildComision()
        {
            Solicitud.Comision = new Comision.ComisionViewModel(this.Solicitud.Cotizacion, this.CodUsuario);
        }

        public override void BuildDetalleEmision()
        {
            Solicitud.DetalleEmision = new DetalleEmisionViewModel(this.Solicitud.Cotizacion);
            Solicitud.DetalleEmision.PrimaSSN += (Solicitud.Comision.ObtenerComisionTotal() * Solicitud.DetalleEmision.Prima / 100);
        }

        #endregion
    }
}