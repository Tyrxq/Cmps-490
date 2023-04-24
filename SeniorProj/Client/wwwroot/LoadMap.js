
export function load_map() {
    
    var geojson_data = [
        {
            "type": "Feature",
            "geometry": {
                "type": "Point",
                "coordinates": [30.2003, -92.0763]
            },
            "properties": {
                "name": "Boil water advisory - boil water for one minute before use."
            }
        },
        {
            "type": "Feature",
            "geometry": {
                "type": "Point",
                "coordinates":[30.2701, -92.0371]
            },
            "properties": {
                "name": "Power outage due to down transformer"
            }
        }
    ];
    console.log(geojson_data);
    
    
    
    var map = L.map('map').setView([30.22, -92.01], 12);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    
    
    
    var geojson_layer = L.geoJSON().addTo(map);
    for (var geojson_item of geojson_data) {
        geojson_layer.addData(geojson_item);
        var marker = new L.marker(
            [geojson_item.geometry.coordinates[0],
                geojson_item.geometry.coordinates[1]]
        )
        marker.bindTooltip(geojson_item.properties.name,
            {
                permanent: true,
                className: "my-label",
                offset: [0, 0]
            }
        );
        marker.addTo(map);
    }





    return "";
    
    

}
