var tipoDJ = {
    Indemnizacion : 1,
    CoSeguro : 2,
    DañosPrevios : 3,
}

var arrayDJ = [];

function RefComponentesEmisor(ingresoSolicitud) {

    this.IngresoSolicitud = ingresoSolicitud

    this.EmisionSelector = () =>
    {
        return this.IngresoSolicitud.EmisionClass;
    }
}

function Agro() 
{
    this.Riesgos = new Array().concat(riesgosAgro());
    this.Recotiza = false;
    let self = this;

    this.Mapear = () =>
    {
        let agro = new Array();
        
        this.Riesgos.forEach(riesgo => 
        {
            agro.push(riesgo.MapearAgro());
        });
        
        return {Agro: agro, RiesgosJSON:JSON.stringify(agro)};
    }

    this.ConseguirKml = () => {
        return riesgo
    }

    this.ProcesarRiesgo = async codRiesgo =>
    {
        let riesgo = this.ObtenerRiesgo(codRiesgo);
        let fechaSiembra = riesgo.FechaDeSiembra;
        let kml = riesgo.Kml;
        
        if(!this.ExisteKml(codRiesgo, kml) && !this.ExisteFechaSiembraCargada(codRiesgo, fechaSiembra))
        {
            await riesgo.Procesar();            
        }       
    }

    this.Procesar = async () =>
    {        
        let riesgosConDatosIncompletos = this.Riesgos.filter(riesgo => (riesgo.FechaSiembra == undefined || riesgo.Kml == undefined));
        let mensajeRiesgosFaltantes = riesgosConDatosIncompletos.length > 0 ? "Existen riesgos sin datos cargados" : "" ;

        if(riesgosConDatosIncompletos.length != 0)
        {
            riesgosConDatosIncompletos.forEach(riesgo => 
            {   
                if(riesgo.Kml == undefined)
                    riesgo.MostrarErrorKml(riesgo.CodRiesgo);
                if(riesgo.FechaSiembra == undefined)
                    riesgo.MostrarErrorFechaSiembra(riesgo.CodRiesgo);               
            })
    
            throw "Completar y/o corregir los campos indicados. " + mensajeRiesgosFaltantes;
        }
        else {
            this.Riesgos.forEach(riesgo => 
            {   
                riesgo.ValidarHectareasCotizadas();           
            })
        }
        
        if(mensajeRiesgosFaltantes.length != 0)
            throw mensajeRiesgosFaltantes;        
    }

    this.CambiaConRecotizacion = () =>
    {
        return false;
    }

    this.ObtenerRiesgo = codriesgo =>
    {
        return this.Riesgos.filter(riesgo => riesgo.CodRiesgo === codriesgo)[0] //debe ser un solo;
    }

    this.ObtenerDetalle = ()=>
    {
        let riesgo = this.Mapear();
        if(riesgo.Agro.length > 1)
            return `${riesgo.Agro.length} campos asociados`;  
        else
        {
            let CamposDatos = riesgo.Agro.pop();
            return `Nombre del campo: ${CamposDatos.Nombre}, Hectareas: ${CamposDatos.Hectareas}`;
        }
    }   
    
    this.ExisteKmlCargado = (codRiesgo, kml) =>
    {
        const ExisteKml = riesgo => {return riesgo.Kml === kml && kml !== undefined}        
        return this.Riesgos.filter(riesgo => riesgo.CodRiesgo !== codRiesgo).some(ExisteKml);
    }

    this.ExisteFechaSiembraCargada = (codRiesgo, fechaSiembra) =>
    {
        const ExisteFechaSiembra = riesgo => {return riesgo.FechaSiembra === fechaSiembra && fechaSiembra !== undefined}        
        return this.Riesgos.filter(riesgo => riesgo.CodRiesgo !== codRiesgo).some(ExisteFechaSiembra);
    }

    function riesgosAgro()
    {
        let riesgos = new Array();
        let riesgosFilas = $($(".EmisionView .tabla-agro table tbody tr")).toArray();
        
        riesgos = riesgosFilas.map(fila =>  
        {
            return CrearRiesgo(fila)
        });

        function CrearRiesgo(fila) {
            let $fila = $(fila);

            let nroRiesgo = parseInt($fila.attr("data-nrofila"));
            let codCampo = parseInt($fila.attr("data-codcampo"));
            let codRiesgo = parseInt($fila.attr("data-codriesgo"));
            let finCobertura = $fila.attr("data-fincobertura");
            let campos = $fila.find("input").toArray();
            let nombreCampo = $fila.attr("data-nombrecampo");
            let cantQuintal = parseFloat($fila.data("cantquintales"));
            let cantHectareas = parseFloat($fila.data("canthectareas"));
            let sumaAseguradaPorHectarea = parseFloat($fila.data("sumaseguradaporhectarea"));  
            let DDJJ = {
                IndemnizacionDJ: "",
                CoSeguroDJ: "",
                DañosPreviosDJ: ""
            }
            
            return new CampoAgro(codRiesgo, codCampo, nombreCampo, cantQuintal, cantHectareas, sumaAseguradaPorHectarea, nroRiesgo, finCobertura, DDJJ);
        }
        
        return riesgos;
    }

    function CampoAgro(codRiesgo, codCampo, nombreCampo, cantQuintal, cantHectareas, sumaAseguradaPorHectarea, nroRiesgo, finCobertura, DDJJ) 
    {
        let self = this;
        
        this.NroRiesgo = nroRiesgo;
        this.CodCampo = codCampo;
        this.CodRiesgo = codRiesgo;
        this.NombreCampo = nombreCampo;
        this.CantQuintal = cantQuintal;
        this.CantHectareas = cantHectareas;
        this.FinCobertura = finCobertura;
        this.SumaAseguradaPorHectarea = sumaAseguradaPorHectarea;
        this.FechaSiembra;
        this.Kml;
        this.ListaLotes;  
        this.Hectareas;
        this.DeclaracionesJuradas = DDJJ;

        let $modalDJ = ".agroddjjmodal_"+codRiesgo;
        let $modalAdjunto = ".agroimportarmodal_"+codRiesgo; 
        let $modalDibujo = ".agrokmlmodal_"+codRiesgo; 
        let dragAndDrop;

        function AbrirModalAdjunto()
        {
            if(!modalFueCargada($modalAdjunto)) {                      
                var params = {
                    codModal: $(".btimportaragro_"+codRiesgo).data('codmodal'),
                    codRiesgo: codRiesgo
                };
                Ajax.ToDiv('CotizadorEmisor/MostrarModalAgro', params, $($modalAdjunto).find('.modal-body'))
                        .done(function () {                            
                            $($modalAdjunto).modal('show');
                            $($modalAdjunto).addClass('renderizada');                            
                            dragAndDrop = new DragAndDropKml(self.CodRiesgo)                           
                        })                  
            }
            else
            {
                $($modalAdjunto).modal('show');
            }
        }

        function DragAndDropKml(codRiesgo) {
            let extensionesValidas = ["kml", "kmz"];           
            this.DragAndDrop = new DragAndDrop(codRiesgo, extensionesValidas);
      
            this.EsValido = () => {
                return this.DragAndDrop.Archivos.length == 1 ? true : false;    
            }

            this.Data = () => {
                return this.DragAndDrop.Archivos[0].Data;
            }

            this.Reiniciar = () => {
                this.DragAndDrop.BorrarTodos();
            }           
        }

        function AbrirModalDibujo()
        {
            if(!modalFueCargada($modalDibujo)) {              //invocas el metodo del controller                       
                var params = {
                    codModal: $(".btkmlagro_"+codRiesgo).data('codmodal'),
                    codRiesgo: codRiesgo
                };
                Ajax.ToDiv('CotizadorEmisor/MostrarModalAgro', params, $($modalDibujo).find('.modal-body'))
                        .done(function () {                            
                            $($modalDibujo).modal('show');
                            initMap(codRiesgo);
                            $($modalDibujo).addClass('renderizada');
                        })                 
            }
            else
            {
                $($modalDibujo).modal('show');
            }
        }        

        function AbrirModalDJ()
        {
            if(!modalFueCargada($modalDJ)) {
                var params = {
                    codModal: $(".btddjjagro_"+codRiesgo).data('codmodal'),
                    codRiesgo: codRiesgo
                };
                Ajax.ToDiv('CotizadorEmisor/MostrarModalAgro', params, $($modalDJ).find('.modal-body'))
                        .done(function () {
                            $($modalDJ).modal();
                            $($modalDJ).addClass('renderizada');                                                   
                            ajaxCargarProvincia($modalDJ+' .inputProvincia');                            
                        })                   
            }
            else {
                $($modalDJ).modal('show');
            }           
        } 

        function modalFueCargada(modal)
        {     
            return $(modal).hasClass('renderizada');
        }   
        
        var partesDeFecha = self.FinCobertura.split("/");
        var fechaMax = new Date(Number(partesDeFecha[2]), Number(partesDeFecha[1]) - 1, Number(partesDeFecha[0]));
        fechaMax.setMonth(fechaMax.getMonth() - 1);
        $(".fechaDeSiembra_"+codRiesgo).DatepickerEx({
            format:'dd/mm/yyyy', 
            startDate: '-3m', 
            endDate: fechaMax
        });

        $(".fechaDeSiembra_"+codRiesgo).on("change", function(){   
            //var parts = $(this).val().split("/");
            //var d1 = new Date(Number(parts[2]), Number(parts[1]) - 1, Number(parts[0]));
            //self.FechaSiembra = d1.toISOString().split('T')[0];
            self.FechaSiembra = $(this).val();
        }) 

        $($modalDJ).on('change', '.CoseguroChecks input[type="radio"]', function () {          
            if($($modalDJ+' #CheckNoCoSeguro').is(':checked')) {
                $($modalDJ+' .enQueCompaniaCoSeguro').prop('disabled', true);
                $($modalDJ+' .inputPorcentajeCoSeguro').prop('disabled', true);                         
            } else {
                $($modalDJ+' .enQueCompaniaCoSeguro').prop('disabled', false);
                $($modalDJ+' .inputPorcentajeCoSeguro').prop('disabled', false);    
            }
        });  

        $($modalDJ).on('change', '.DañosPreviosChecks input[type="radio"]', function () {  
            if($($modalDJ+' #CheckNoDañosPrevios').is(':checked')) {
                $($modalDJ+' .inputPorcentajeDañosPrevios').prop('disabled', true);                                        
            } else {
                $($modalDJ+' .inputPorcentajeDañosPrevios').prop('disabled', false);                                           
            }             
        });  
        
        $(".btimportaragro_"+codRiesgo).on("click", function () {
            AbrirModalAdjunto();
        })

        $(".btkmlagro_"+codRiesgo).on("click", function () {
            AbrirModalDibujo();
        })

        $(".btddjjagro_"+codRiesgo).on("click", function () {
            AbrirModalDJ();
        })

        $(".btnAceptarArchivo_"+codRiesgo).on("click", function(){   
            if (dragAndDrop.EsValido()) {               
            var params = {}
            params.archivoData = dragAndDrop.Data();
            $.ajax({
                url: 'LeerKml',
                type: 'POST',
                data: params,
                success: function (data) {                   
                    $($modalAdjunto).modal('toggle');
                    self.Kml = data.Campo.Kml.replace(/\s+/g, '');
                    self.ListaLotes = data.Campo.ListaLotes;
                    self.Hectareas = data.Campo.Hectareas;
                    console.log(self);      
                },
                error: function (request, status, error) {
                    Utils.modalError("Archivo inválido");
                    dragAndDrop.Reiniciar();
                    //alert(request.responseText);
                }
            });
            } else {                               
                Utils.modalError("Cuidado" , "Solo se permite adjuntar un solo archivo por Campo");
            }
        }) 

        $(".btnAceptarDJ_"+codRiesgo).on("click", function(){
            MapearDeclaracionesJuradas(codRiesgo);
            arrayDJ = [];
            sessionStorage.removeItem(codRiesgo);            
            $($modalDJ).modal('toggle');
        })       
        
        $($modalDJ).on('click', '.btGuardarIndemnizacion', function () {       
            if((self.ValidarFormulario(tipoDJ.Indemnizacion))) {
                var DJ = ObtenerDatosIndemnizacion();                
                agregarFormularioATabla(DJ, codRiesgo);               
            }
        })       

        $($modalDJ).on('click', '.btGuardarCoSeguro', function () { 
            if (!$('#CheckNoCoSeguro').is(':checked')) {
                if((self.ValidarFormulario(tipoDJ.CoSeguro))) {
                    var DJ = ObtenerDatosCoSeguro();
                    console.log(DJ);
                    agregarFormularioATabla(DJ, codRiesgo);                    
                }
            }            
        })

        $($modalDJ).on('click', '.btGuardarDañosPrevios', function () {  
            if (!$('#CheckNoDañosPrevios').is(':checked')) {        
                if((self.ValidarFormulario(tipoDJ.DañosPrevios))) {
                    var DJ = ObtenerDatosDañosPrevios();
                    console.log(DJ);
                    agregarFormularioATabla(DJ, codRiesgo);
                }
            }
        })        
        
        $(".btnGenerarKml_"+codRiesgo).on("click", function(){       
            var $modal = $('.agrokmlmodal_'+codRiesgo);
            var params = {}
            params.poligonosDibujados = JSON.stringify(poligonosDibujados);           
            $.ajax({
                url: 'GenerarKml',
                type: 'POST',
                data: params,
                success: function (data) {                    
                    $modal.modal('toggle');
                    self.Kml = data.Campo.Kml.replace(/\s+/g, '');
                    self.ListaLotes = data.Campo.ListaLotes;
                    self.Hectareas = data.Campo.Hectareas;
                    console.log(self);                    
                },
                error: function (request, status, error) {

                    alert(request.responseText);
                }
            });           
        });        
        

        this.MapearAgro = () =>
        {
            let agro = {}
            let selectorFila = ".EmisionView .tabla-agro tbody tr[data-codriesgo=" + this.CodRiesgo.toString() + "]";        
            
            agro.NroRiesgo = this.NroRiesgo;
            agro.Codigo = this.CodCampo;
            agro.CodRiesgo = this.CodRiesgo;
            agro.Nombre = this.NombreCampo;
            agro.CantHectareas = this.CantHectareas;
            agro.CantQuintal = this.CantQuintal;
            agro.SumaAseguradaPorHectarea = this.SumaAseguradaPorHectarea;
            agro.FinCobertura = this.FinCobertura;
            agro.FechaSiembra = this.FechaSiembra;
            agro.Kml = this.Kml;
            agro.ListaLotes = this.ListaLotes;
            agro.Hectareas = this.Hectareas;      
            agro.DeclaracionesJuradas = this.DeclaracionesJuradas == undefined ? {} : this.DeclaracionesJuradas ;

            return agro;
        }           

        this.MostrarErrorKml = (codRiesgo) => {
            cargarTooltipMostrar('.errorKml_'+codRiesgo, "Debe cargar/dibujar los lotes del campo", "manual", 'red', "font-size: 1rem;");
        }

        
        this.MostrarErrorFechaSiembra = (codRieso) => {
            cargarTooltipMostrar('.fechaDeSiembra_'+codRiesgo, "Debe cargar la fecha de siembra", "manual", 'red', "font-size: 1rem;");
        }

        this.ValidarHectareasCotizadas = () => {
            if (!Validar(self.CantHectareas, self.Hectareas)) {                
                throw 'La superficie total del riesgo: '+self.NroRiesgo+' dibujada ('+self.Hectareas+' has) no coincide con el total cotizado ('+self.CantHectareas+' has)';           
            }  
        }

        this.ValidarFormulario = (codTipoDJ) =>
        {
            switch (codTipoDJ) {
                case tipoDJ.Indemnizacion:
                    return validarIndemnizacion();
                    break;
                case tipoDJ.CoSeguro:
                    return validarCoSeguro();
                    break;
                case tipoDJ.DañosPrevios:
                    return validarDañosPrevios();
                    break;        
            }
        }

        function validarIndemnizacion() {
            var clase = ObtenerClaseSegunFormulario(tipoDJ.Indemnizacion);
            var inputRazonSocial = clase+' .inputRazonSocial'
            var inputCuit = clase+' .inputCuil'
            var inputMontoPrenda = clase+' .inputMonto'
            var inputProvincia = clase+' .inputProvincia'
            var inputLocalidad = clase+'.inputLocalidad'
            var inputCalle = clase+' .inputCalle'

            if ($(inputRazonSocial).val() !== '' && $(inputCuit).val() !== '' && $(inputMontoPrenda).val() !== '' && $(inputProvincia).val() !== '' && $(inputLocalidad).val() !== '' && $(inputCalle).val() !== '' ) {
                return true;
            } else {
                mostrarTooltipErrores(inputRazonSocial, inputCuit, inputMontoPrenda, inputProvincia, inputLocalidad, inputCalle);    
                return false;
            }
        }

        function agregarFormularioATabla(DJ, codRiesgo) {  
            //var JSONdj = JSON.parse(JSON.stringify(DJ))
            var clase = ObtenerClaseSegunFormulario(DJ.Codigo);           
            var claseCorrespondiente = clase.concat(' .tablaDetalleDJ tbody tr');        
            var nroOrden = $(claseCorrespondiente).length + 1; 
            agregarATablaSegunTipoDJ(DJ, nroOrden)            
            $('.tablaDetalleDJ').css('display', 'block'); 
            arrayDJ.push(DJ)
            sessionStorage.setItem(codRiesgo, JSON.stringify(arrayDJ));
            limpiarFormulario(ObtenerClaseSegunFormulario(DJ.Codigo));
        }

        $($modalDJ).on('click', '.btEliminarDJ', function () {            
            var fila = $(this).closest('tr').data('nrofila');
            Utils.modalQuestion("¡ATENCIÓN!",
            "¿Está seguro que desea eliminar la declaraciones jurada?",
            function () {               
                quitarDJ(fila);                             
            },
            "Si",
            "No"
            );
        }); 

        $($modalDJ).on('change', '.inputProvincia', function () {             
            if ($($modalDJ+' .inputProvincia').val() != null) {
                $($modalDJ+' .inputLocalidad').prop( "disabled", false );
                limpiarCombo($($modalDJ+' .inputLocalidad'));
                ajaxCargarLocalidad($modalDJ+' .inputLocalidad', $modalDJ);             
            }            
        });

        function agregarATablaSegunTipoDJ(DJ, nroOrden) {
            switch (DJ.Codigo) {
                case tipoDJ.Indemnizacion:
                    $('.tablaDetalleDJ tbody').append('<tr data-nrofila='+ nroOrden +' class= nrofila'+nroOrden+'><td class="text-center;">' + nroOrden + '</td><td>' + DJ.TipoDJ + ' </td>' + '</td><td> Razon Social: ' + JSON.stringify(DJ.RazonSocial, null, 2) + ' Cuit: ' + JSON.stringify(DJ.Cuit, null, 2) + 'Monto Prenda: ' + JSON.stringify(DJ.MontoPrenda, null, 2) + ' </td><td class="text-center"><button type="button" class="btn btn-danger btn-xs btEliminarDJ" title="Eliminar"><i class="fa fa-remove"></i></button></td></tr>');
                    break;
                case tipoDJ.CoSeguro:
                    $('.tablaDetalleDJ tbody').append('<tr data-nrofila='+ nroOrden +' class= nrofila'+nroOrden+'><td class="text-center;">' + nroOrden + '</td><td>' + DJ.TipoDJ + ' </td>' + '</td><td> Nombre Compañia: ' + JSON.stringify(DJ.NombreCompañia, null, 2) + ' Porcentaje: ' + JSON.stringify(DJ.PorcentajeAsegurado, null, 2) + ' </td><td class="text-center"><button type="button" class="btn btn-danger btn-xs btEliminarDJ" title="Eliminar"><i class="fa fa-remove"></i></button></td></tr>');
                    break;
                case tipoDJ.DañosPrevios:
                    $('.tablaDetalleDJ tbody').append('<tr data-nrofila='+ nroOrden +' class= nrofila'+nroOrden+'><td class="text-center;">' + nroOrden + '</td><td>' + DJ.TipoDJ + ' </td>' + '</td><td> Porcentaje: '  + JSON.stringify(DJ.PorcentajeDaños, null, 2) + ' </td><td class="text-center"><button type="button" class="btn btn-danger btn-xs btEliminarDJ" title="Eliminar"><i class="fa fa-remove"></i></button></td></tr>');
                    break;
            }
            
        }


        function ObtenerClaseSegunFormulario(codigoDJ) {
            var clase;
            switch (codigoDJ) {
                case tipoDJ.Indemnizacion:
                    clase = "#indemnizacion_"+codRiesgo;
                    break;
                case tipoDJ.CoSeguro:
                    clase = "#coseguro_"+codRiesgo;
                    break;
                case tipoDJ.DañosPrevios:
                    clase = "#dañosprevios_"+codRiesgo;
                    break;                    
            }
            return clase;
        } 
        
        function quitarDJ(fila) {   
            var elemento = $('.tablaDetalleDJ .nrofila'+fila)
            $(elemento).remove();            
            if ($('.tablaDetalleDJ tbody tr').length < 3) {
                $('.tablaDetalleDJ').css('display', 'none');
            }            
        }


        function validarCoSeguro() {     
            var clase = ObtenerClaseSegunFormulario(tipoDJ.CoSeguro);
            var inputCompania = clase+' .enQueCompaniaCoSeguro';
            var inputPorcentajeCoSeguro = clase+' .inputPorcentajeCoSeguro';         

            if ((validarCheckMarcado('.CoseguroChecks')) && $(inputCompania).val() !== '' && $(inputPorcentajeCoSeguro).val() !== '') {
                return true;
            } else {
                mostrarTooltipErrores(inputCompania, inputPorcentajeCoSeguro);    
                return false;
            }
        }

        function validarCheckMarcado(clase) {            
            var UnCheckMarcado = $(clase+' input:checked').length;
            if (UnCheckMarcado == 1) {
                return true;
            } else {
                mostrarTooltipErrores(clase);   
                return false;
            }
        }

        function validarDañosPrevios() {
            var clase = ObtenerClaseSegunFormulario(tipoDJ.DañosPrevios);
            var inputPorcentajeDaños = clase+' .inputPorcentajeDañosPrevios';         

            if ((validarCheckMarcado('.DañosPreviosChecks')) && $(inputPorcentajeDaños).val() !== '' ) {
                return true;
            } else {
                mostrarTooltipErrores(inputPorcentajeDaños);    
                return false;
            }
        }

        function ObtenerDatosIndemnizacion() { 
            var clase = ObtenerClaseSegunFormulario(tipoDJ.Indemnizacion);
            return IndemnizacionDJ = {
                Codigo: tipoDJ.Indemnizacion,
                TipoDJ : 'Indemnizacion',
                RazonSocial: $(clase +' .inputRazonSocial').val(),
                Cuit: $(clase +' .inputCuil').val(),
                MontoPrenda:  $(clase +' .inputMonto').val(),
                Provincia: {
                    NuProvincia: $(clase +' .inputProvincia').find(':selected').val(),
                    DeProvincia: $(clase +' .inputProvincia').find(':selected').text(),
                }, 
                Localidad: {                    
                    Codigo: $(clase +' .inputLocalidad').find(':selected').val(),
                    Descripcion: $(clase+' .inputLocalidad').find(':selected').text(),
                    CodigoPostal: $(clase+' .inputLocalidad').find(':selected').data('cp'),
                    },                
                Calle: $(clase+' .inputCalle').val(),
                NumeroCalle: $(clase+' .inputNroCalle').val(),
                Piso: $(clase+' .inputPiso').val(),
                Depto: $(clase+' .inputDepto').val()
            }
        }

        function ObtenerDatosCoSeguro() {
            var clase = ObtenerClaseSegunFormulario(tipoDJ.CoSeguro);
            return CoSeguroDJ = {
                Codigo: tipoDJ.CoSeguro,
                TipoDJ : "Coseguro",
                NombreCompañia: $(clase +' .enQueCompaniaCoSeguro').val(),
                TieneOtroSeguro: $(clase +' #CheckSiCoSeguro').is(':checked') ? 'S' : 'N',
                PorcentajeAsegurado: $(clase+' .inputPorcentajeCoSeguro').val()             
            }
        }

        function ObtenerDatosDañosPrevios() {
            var clase = ObtenerClaseSegunFormulario(tipoDJ.DañosPrevios);
            return DañosPreviosDJ = {
                Codigo: tipoDJ.DañosPrevios,
                TipoDJ : "DañosPrevios",
                TieneDañosPrevios: $(clase +' #CheckSiDañosPrevios').is(':checked') ? 'S' : 'N',                
                PorcentajeDaños:  $(clase+' .inputPorcentajeDañosPrevios').val()               
            }
        }       

        function mostrarTooltipErrores(...parametros) {
           parametros.forEach(parametro => 
                {   
                    if(($(parametro).val() == undefined) || ($(parametro).val() == '') || ($(parametro).val() == null) ) {
                        cargarTooltipMostrar(parametro, "Este campo es obligatorio", "manual", 'red', "font-size: 1rem;");
                    }                                      
                })           
        }
        
        function ajaxCargarProvincia(selector) {
            $.ajax({
                url: RootPath.concat('/Genericos/ObtenerTodasProvincias'),
                method: 'GET',
                data: null,
                success: function (data) {
                    $.each(data, function (i, opt) {
                        mapearCombo(selector, opt);
                    });                  
                    $(selector).val(null).trigger('change');
                },
                error: function (request, status, error) {
                    Utils.modalError('Error', request.responseText);
                },
            });           
        }

        function ajaxCargarLocalidad(selector, modalDJ) {            
            limpiarCombo($(selector));
            var params = { nroProvincia: $(modalDJ+ ' .inputProvincia').val(), filtro: "", mostrarBarrios: false }           
            $.ajax({
                url: RootPath.concat('/Genericos/ObtenerLocalidadesPorProvincia'),
                method: 'POST',
                data: params,
                beforeSend: function () {
                    mostrarLoaderCombo(selector, true);
                },
                complete: function () {
                    mostrarLoaderCombo(selector, false);
                },
                success: function (data) {
                    $.each(data, function (i, opt) {
                        mapearComboLocalidad(selector, opt);
                    });
                },
                error: function (request, status, error) {
                    Utils.modalError('Error', request.responseText);
                },
            });           
        }

        function mostrarLoaderCombo(selector, flag) {
            if (flag) {
                $(selector).parent().css('display', 'none');
                $(selector).parent().next().css('display', 'block');
            } else {
                $(selector).parent().css('display', 'flex');
                $(selector).parent().next().css('display', 'none');
            }
        }

        function mapearCombo(selector, opt) {
            var option = '<option value="' + opt.Value + '"' +'>' + opt.Text + '</option>';            
            $(selector).append(option).select2({ placeholder: 'Seleccione una Provincia', width: '100%',containerCssClass: 'font-small',
            dropdownCssClass: 'font-small', });           
        }

        function mapearComboLocalidad(selector, opt) {
            var option = '<option value="' + opt.id + '" data-cp="'+opt.cp+'"' + '>' + opt.text + '</option>';
            $(selector).append(option).select2({ placeholder: "Seleccione una Localidad", width: '100%' ,containerCssClass: 'font-small',
            dropdownCssClass: 'font-small', });            
        }

        function limpiarFormulario(selector) {
                    var formulario = $(selector).find('.formularioAgro');

                    formulario
                        .find(':input')
                        .not(':input[type="button"]')
                        .each(function () {
                            var $input = $(this);

                            if ($input.is('input')) {
                                $input.val('');
                            } else if ($input.is('select')) {
                                $input.val(null).trigger('change');
                            }
                    });
        }


        function limpiarCombo(combo) {
            combo.val(null).trigger('change');
        }
        

        function Validar(hectareasCotizadas, hectareasCargadas)
        {
            var porcToleranciaHa = 0.1;
            var haEnRango = false;
            var tolerancia = hectareasCotizadas * porcToleranciaHa;

            var hMin = hectareasCotizadas - tolerancia;
            var hMax = hectareasCotizadas + tolerancia;

            if (hMin < hectareasCargadas && hectareasCargadas < hMax)
            {
                haEnRango = true;
            }

            return haEnRango;
        }

        function MapearDeclaracionesJuradas(codRiesgo) {
            var listaCompletaDJ = ObtenerTablaDJCompleta(codRiesgo);
            console.log(listaCompletaDJ);
            listaCompletaDJ.forEach(lista => {
                if (lista.Codigo == tipoDJ.Indemnizacion) {
                    self.DeclaracionesJuradas.IndemnizacionDJ = lista;
                } 
                else if (lista.Codigo == tipoDJ.CoSeguro) {
                    self.DeclaracionesJuradas.CoSeguroDJ = lista;
                } 
                else if (lista.Codigo == tipoDJ.DañosPrevios) {
                    self.DeclaracionesJuradas.DañosPreviosDJ = lista;
                }
                console.log(self.DeclaracionesJuradas);
            });

        }       

        function ObtenerTablaDJCompleta(codRiesgo) {            
            var listaDDJJ = JSON.parse(sessionStorage.getItem(codRiesgo));            
            //var rows = $('#indemnizacion_'+codRiesgo+' .tablaDetalleDJ tbody tr');
            //listaDDJJ = $.map(rows, function (r) {
            //    return {
            //        Datos: $(r).data("datos")
            //    }
            //});
            return listaDDJJ;
        }

        function cargarTooltipMostrar(elemento, texto, trigger, color, estilo, intervalo) {
            cargarTooltip(elemento, texto, trigger, color, estilo);
            $(elemento).tooltip('show');


            setTimeout(function () {
                $(elemento).tooltip('destroy');
            },
            intervalo != null ? intervalo : 2000)

        }

        function cargarTooltip(elemento, texto, trigger, color, estilo) {

            if (!$(elemento).hasClass('tooltipClassManual'))
            {
                $(elemento).addClass('tooltipClassManual');
            }

            $(elemento).tooltip({
                placement: 'top',
                trigger: trigger,
                container: 'body',
                delay: { show: 0, hide: 500 },
                title: texto,
                template: '<div class="tooltip" style="' + estilo + '" role="tooltip"><div class="tooltip-arrow" style="border-top-color:' + color + ' !important;"></div><div class="tooltip-inner" style="background-color:' + color + '  !important;"></div></div>'
            });
        }
    }
}




