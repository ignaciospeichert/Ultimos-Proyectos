let hectareasTotal = [];
let poligonosDibujados = [];
let poligonoDibujado = {}
var selectedShape;
var newShape;
var iconoMarcador;

var myStyles =[
  {
      featureType: "poi",
      elementType: "labels",
      stylers: [
            { visibility: "off" }
      ]
  }
];

function initMap(codRiesgo) {
    hectareasTotal = [];
    poligonosDibujados = [];
    var partido = $('.partido_' + codRiesgo).text();
    var provincia = $('.provincia_' + codRiesgo).text();
    actualizarLabelHectareasEsperadas(codRiesgo);
    $('#input_'+codRiesgo).val(partido+', '+provincia+', '+'ARGENTINA')
   
    const map = new google.maps.Map(document.getElementById("map_"+codRiesgo), {
        center: { lat: -35.17465448276313, lng: -64.40614540548573 },
        zoom: 5,     
        center: new google.maps.LatLng(-40, -64),      
        disableDefaultUI:true,
        panControl: true, 
        scaleControl: true,
        zoomControl :true,
        zoomControlOptions: {
            style: google.maps.ZoomControlStyle.LARGE
        },
        mapTypeId: google.maps.MapTypeId.HYBRID,  
        styles: myStyles
    });

    const geocoder = new google.maps.Geocoder();
    const infowindow = new google.maps.InfoWindow();
   
    document.getElementById("submit_"+codRiesgo).addEventListener("click", () => {
        geocodeLatLng(codRiesgo, geocoder, map, infowindow);
    });
    
    ubicarLugarPunto(partido, provincia, map);
    const input = document.getElementById("input_"+codRiesgo);
    const searchBox = new google.maps.places.SearchBox(input);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);
    // Bias the SearchBox results towards current map's viewport.   
    map.addListener("bounds_changed", () => {
        searchBox.setBounds(map.getBounds());
    });
    let markers = [];
    // Listen for the event fired when the user selects a prediction and retrieve
    // more details for that place.
    searchBox.addListener("places_changed", () => {
        const places = searchBox.getPlaces();

        if (places.length == 0) {
            Utils.modalInfo("Formato no valido, por favor reintente");
        }
        // Clear out the old markers.
        markers.forEach((marker) => {
            marker.setMap(null);
        });
        markers = [];
        // For each place, get the icon, name and location.
        const bounds = new google.maps.LatLngBounds();
        places.forEach((place) => {
            if (!place.geometry || !place.geometry.location) {
                console.log("Returned place contains no geometry");
                return;
            }
            const icon = {
                url: place.icon,
                size: new google.maps.Size(71, 71),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(17, 34),
                scaledSize: new google.maps.Size(25, 25),
            };
            // Create a marker for each place.
            markers.push(
              new google.maps.Marker({
                  map,
                  icon,
                  title: place.name,
                  position: place.geometry.location,
              })
            );

            if (place.geometry.viewport) {
                // Only geocodes have viewport.
                bounds.union(place.geometry.viewport);
            } else {
                bounds.extend(place.geometry.location);
            }
           
                for (var i = 0; i < place.address_components.length; i++) {
                    for (var j = 0; j < place.address_components[i].types.length; j++) {
                        if (place.address_components[i].types[j] == "postal_code") {
                            console.log(place.address_components[i].long_name);
                        }
                    }
                }           
           
        });
        map.fitBounds(bounds);       
    });    
    
    const drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: google.maps.drawing.OverlayType.MARKER,
        drawingControl: true,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: [               
              google.maps.drawing.OverlayType.POLYGON,         
            ],    
        },
        markerOptions: {
            editable: true,
            icon: '/largeTDGreenIcons/blank.png'
        },
        polygonOptions: {
            fillColor: "#0F0",
            strokeColor: "#0F0",
        },
        polylineOptions: {
            strokeColor: "#FF273A"
        }
    });
    drawingManager.setMap(map);    

    var coord_listener = google.maps.event.addListener(drawingManager, "polygoncomplete", function (polygon) {        
        drawingManager.setDrawingMode(null);
        var coordinates = polygon.getPath();      
        var MVCArray = coordinates.getArray();          

        var area = google.maps.geometry.spherical.computeArea(coordinates);      
        var hectarea = metrosToHectareas(area).toFixed(2);
        hectareasTotal.push(parseFloat(hectarea));

        var totalHectareas = hectareasTotal.reduce((a, b) => a + b);      
        
        console.log(area);
        console.log(hectarea);
        console.log(hectareasTotal);
        console.log(totalHectareas);        

        var polygonBounds = polygon.getPath();
        var bounds = [];
        for (var i = 0; i < polygonBounds.length; i++) {
            var point = {
                Nrovertice: i+1,
                lat: Number(polygonBounds.getAt(i).lat().toFixed(12)),
                lng: Number(polygonBounds.getAt(i).lng().toFixed(12)),
                alt: 0
            };
            bounds.push(point);
        }
      
        var marcador = new google.maps.Marker({
            position: polygonCenter(polygon),
            map: map

        });
            
        poligonoDibujado.codigo = obtenerUltimoElemento() + 1;
        poligonoDibujado.vertices = bounds;
        poligonoDibujado.nombre = "Lote " +poligonoDibujado.codigo;
        poligonoDibujado.centro = { x: center_x, y: center_y };
        poligonoDibujado.hectareas = hectarea;
        guardarPoligono(poligonoDibujado);
        refrescarHectareas(totalHectareas, codRiesgo);       

        var contentString =
        '<div id="infodiv" style="width: 220px; height: 130px; font-family: Roboto,Arial,sans-serif;">'
                    + '<strong>Lote numero </strong>  #' + obtenerUltimoElemento() + '<br />'
                    + '<br /><a style="cursor: pointer;" onclick="deleteSelectedShape('+poligonoDibujado.codigo+',' +hectarea+',' +codRiesgo+');"><font color="FF0055">Haga click aqui­ para eliminar este lote.</font></a>'
                    + '<br /><br /><strong>Informacion del lote:</strong><br /><br />'
                    + '<strong>&bull; Hectareas: </strong> ' + hectarea + ' Ha<br />' 
                    + '<strong>&bull; Centro: </strong> [' + center_x.toFixed(3) + ', ' + center_y.toFixed(3) + ']<br />'                  		
                    + '<br /><i></i>'
                    + '<br /></div>' 
        var infowindow = new google.maps.InfoWindow({
            content: contentString,
        });        

        marcador.addListener("click", () => {
            $('.gm-style-iw-t').remove();
            infowindow.open({
                anchor: marcador,
                map,
                shouldFocus: false,               
            });
            iconoMarcador = marcador;
            setSelection(polygon);
        });            
    });

    google.maps.event.addListener(drawingManager, 'overlaycomplete', function (e) {
        newShape = e.overlay;
        newShape.type = e.type;
        setSelection(newShape);
    });
}

function clearSelection() {
    if (selectedShape) {
        //selectedShape.setEditable(false);
        selectedShape = null;
    }
}

function setSelection(shape) {
    clearSelection();
    selectedShape = shape;    
}

function deleteSelectedShape(codigoPoligono, hectarea, codRiesgo) {
    if (selectedShape) {
        $('.gm-style-iw-t').remove();
        iconoMarcador.setMap(null);
        selectedShape.setMap(null);
        hectareasTotal = eliminarHectarea(hectarea);
        var totalHectareas = hectareasTotal.length > 0 ? hectareasTotal.reduce((a, b) => a + b) : 0;
        refrescarHectareas(totalHectareas, codRiesgo);
        eliminarPoligonoDibujado(codigoPoligono)
    }
}

function eliminarPoligonoDibujado(codigoPoligono) {  
    poligonosDibujados = poligonosDibujados.filter(poligono => (poligono.Codigo != codigoPoligono));
}

function eliminarHectarea(hectareaEliminada) {  
    return hectareasTotal.filter(hectarea => (hectarea != hectareaEliminada)); 
}

function geocodeLatLng(codRiesgo, geocoder, map, infowindow) {
    const input = document.getElementById("inputgeo_" + codRiesgo).value;
    const latlngStr = parse_gps(input);
    const latlng = {
        lat: parseFloat(latlngStr[0]),
        lng: parseFloat(latlngStr[1]),
    };

    geocoder
      .geocode({ location: latlng })
      .then((response) => {
          if (response.results[0]) {
              map.setZoom(15);
              map.setCenter(latlng);

              const marker = new google.maps.Marker({
                  position: latlng,
                  map: map,
              });

              infowindow.setContent(response.results[0].formatted_address);
              infowindow.open(map, marker);
          } else {
              Utils.modalInfo("Informacion", "No se encontraron resultados");
          }
      })
      .catch((e) => Utils.modalInfo("Cuidado", "Formato invalido de Geolocalización"));
}

function parse_gps(input) {

    if (input.indexOf('N') == -1 && input.indexOf('S') == -1 &&
        input.indexOf('W') == -1 && input.indexOf('E') == -1) {
        return input.split(',');
    }

    var parts = input.split(/[°'"]+/).join(' ').split(/[^\w\S]+/);

    var directions = [];
    var coords = [];
    var dd = 0;
    var pow = 0;

    for (i in parts) {       
        if (isNaN(parts[i])) {

            var _float = parseFloat(parts[i]);

            var direction = parts[i];

            if (!isNaN(_float)) {
                dd += (_float / Math.pow(60, pow++));
                direction = parts[i].replace(_float, '');
            }

            direction = direction[0];

            if (direction == 'S' || direction == 'W')
                dd *= -1;

            directions[directions.length] = direction;

            coords[coords.length] = dd;
            dd = pow = 0;

        } else {

            dd += (parseFloat(parts[i]) / Math.pow(60, pow++));

        }

    }

    if (directions[0] == 'W' || directions[0] == 'E') {
        var tmp = coords[0];
        coords[0] = coords[1];
        coords[1] = tmp;
    }

    return coords;
}

function guardarPoligono(poligonoDibujado) {  
    poligonosDibujados.push({
        Codigo: poligonoDibujado.codigo,
        NroLote: poligonoDibujado.codigo,
        Nombre: poligonoDibujado.nombre,
        Vertices: poligonoDibujado.vertices,
        Centro: poligonoDibujado.centro,
        Hectareas: poligonoDibujado.hectareas
    });
}

function actualizarLabelHectareasEsperadas(codRiesgo) {  
    var hectareasEsperadas = $('.hectareasEsperadas_'+codRiesgo);
    var hectareasCotizadas = $('.hectareasCotizadas_'+codRiesgo).text().replace(',','.');
    hectareasEsperadas.text(hectareasCotizadas);
}

function metrosToHectareas (metros2) {      
    var hectareas = metros2 / 10000
    return hectareas;
}

function obtenerUltimoElemento() {
    if (poligonosDibujados.length > 0) {
        return poligonosDibujados[Object.keys(poligonosDibujados)[Object.keys(poligonosDibujados).length - 1]].Codigo
    } else {
        return poligonosDibujados.length;
    }    
}

function refrescarHectareas(totalHectareas, codRiesgo) {
    document.querySelector('.hectareasDibujadas_' + codRiesgo).innerHTML = totalHectareas.toFixed(2);
} 

function polygonCenter(poly) {
    var lowx,
      highx,
      lowy,
      highy,
      lats = [],
      lngs = [],
      vertices = poly.getPath();

    for (var i = 0; i < vertices.length; i++) {
        lngs.push(vertices.getAt(i).lng());
        lats.push(vertices.getAt(i).lat());
    }

    lats.sort();
    lngs.sort();
    lowx = lats[0];
    highx = lats[vertices.length - 1];
    lowy = lngs[0];
    highy = lngs[vertices.length - 1];
    center_x = lowx + ((highx - lowx) / 2);
    center_y = lowy + ((highy - lowy) / 2);
    return (new google.maps.LatLng(center_x, center_y));
}

function ubicarLugarPunto(partido, provincia, map) {
    var geocoder;
    var rLongitud = 'N/D';
    var rLatitud = 'N/D';
    var geoCoord;
    var address = partido + ',' + provincia + ', Argentina';    
    var StatusTrigger = 0;


    geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'address': address }, function (results, status) {

        if (status == google.maps.GeocoderStatus.OK) {
            rLatitud = results[0].geometry.location.lat();
            rLongitud = results[0].geometry.location.lng();
            StatusTrigger = 1;
        } else {
            rLongitud = 'N/D';
            rLatitud = 'N/D';
        }

        if (StatusTrigger == 1) {
            var center = new google.maps.LatLng(rLatitud, rLongitud);
            map.setCenter(center);
            map.setZoom(14);
            //map.setOptions({ minZoom: 14 });           

            var marker = new google.maps.Marker({
                position: center,
                map: map,
                title: partido,               
            });
           
        }
        else {
            throw('No se ha encontrado la ubicacion.');
        }
    });
}