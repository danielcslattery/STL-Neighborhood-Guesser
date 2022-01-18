
var map = L.map('map').setView([38.63457282385875, -90.24032592773438], 11);

var Esri_WorldTopoMap = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}', {
	attribution: 'Tiles &copy; Esri &mdash; Esri, DeLorme, NAVTEQ, TomTom, Intermap, iPC, USGS, FAO, NPS, NRCAN, GeoBase, Kadaster NL, Ordnance Survey, Esri Japan, METI, Esri China (Hong Kong), and the GIS User Community'
}).addTo(map);

map.setMaxBounds(L.latLngBounds(
	L.latLng(38.77817551784403, -89.99107360839845),
	L.latLng(38.51002726082032, -90.47790527343751)
))

map.options.minZoom = 11;
map.options.maxZoom = 14;



async function getNeighborhoods() {
	const response = await fetch('https://localhost:5001/neighborhood/all');
	const myJson = await response.json(); //extract JSON from the http response

	let neighborhoodGroup = L.layerGroup();


	L.geoJSON(myJson, {
		onEachFeature: function (feature, layer) {
			layer.addTo(neighborhoodGroup);

			layer.setStyle({
				fillColor: "gray",
				color: "#363636",
				weight: "1"
			})

			layer.on({
				mouseover: onNeighborhoodHover,
				mouseout: offNeighborhoodHover
			})
		}
	})

	neighborhoodGroup.addTo(map);

	return neighborhoodGroup;
}

async function onStart() {

	// neighborhoodGroup layer placed into globalscope
	neighborhoodGroup = await getNeighborhoods();
	await setPrompt();

	highlightHints(neighborhoodGroup);



}

onStart();

async function setPrompt() {
	const response = await fetch(`https://localhost:5001/neighborhood/getPrompt`);
	const prompt = await response.json();

	let promptEl = document.getElementById("prompt");
	promptEl.innerHTML = prompt;

}

function onNeighborhoodHover(e) {
	e.target.setStyle({ fillColor: "blue" })
}

function offNeighborhoodHover(e) {

	// If the neighborhood is one of the hints, return it to yellow. 
	if (hintJson.some((el) => e.target.feature.properties.NHD_NAME == el)) {

		e.target.setStyle({ fillColor: "yellow" })

	} else {
		// Reset layers
		e.target.setStyle({ fillColor: "darkgray" })
	}


}

var popup = L.popup()

	async function onMapClick(e) {
		console.log("Longitude: ", e.latlng.lng, " Latitude: ", e.latlng.lat)

		let clickedOn = await getClickedNeighborhood(e.latlng.lng, e.latlng.lat);

		if (clickedOn == "Correct") {
			console.log("Got it right!")
			setPrompt();
			highlightHints(neighborhoodGroup);
		} else {
			//do nothing for now
		}
	}

async function getClickedNeighborhood(lon, lat) {
	const response = await fetch(`https://localhost:5001/neighborhood/click?lon=${lon}&lat=${lat}`);
	const myJson = await response.json();

	return myJson;
}

async function getHintNeighborhoods() {
	const response = await fetch(`https://localhost:5001/neighborhood/GetHints`);
	const hintJson = await response.json();

	return hintJson;
}

async function highlightHints(feature) {

	// Hint neighborhoods made placed into global scope.
	hintJson = await getHintNeighborhoods();
	console.log(hintJson)
	feature.eachLayer(function (layer) {
		if (hintJson.some((el) => layer.feature.properties.NHD_NAME == el)) {
			layer.setStyle({ fillColor: "yellow" })
		} else {
			// Reset layers
			layer.setStyle({ fillColor: "darkgray" })
        }
    })

}

map.on('click', onMapClick);