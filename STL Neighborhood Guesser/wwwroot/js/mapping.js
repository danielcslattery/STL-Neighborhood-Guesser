



// Map Settings
var map = L.map('map', {attributionControl: false}).setView([38.63457282385875, -90.24032592773438], 11);

var Esri_WorldTopoMap = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}').addTo(map);

map.setMaxBounds(L.latLngBounds(
	L.latLng(38.77817551784403, -89.99107360839845),
	L.latLng(38.51002726082032, -90.47790527343751)
))

map.options.minZoom = 11;
map.options.maxZoom = 14;


// Function calls and Variable Initialization
let formerGuesses = []
onStart();
map.on('click', onMapClick);


// Fuctions
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
				click: onNeighborhoodClick,
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
/*	await setPromptAndHighlightHints();*/

	highlightHints(neighborhoodGroup);
}


function onNeighborhoodClick(e) {
	formerGuesses.push(e.target.feature.properties.NHD_NAME)
	e.target.setStyle({ fillColor: "red" })
}

function onNeighborhoodHover(e) {
	e.target.setStyle({ fillColor: "blue" })
}

function offNeighborhoodHover(e) {

	// If the neighborhood is one of the hints, return it to yellow.
	if (formerGuesses.some((el) => e.target.feature.properties.NHD_NAME == el)) {
		e.target.setStyle({ fillColor: "red" })
	} else if (hintJson.some((el) => e.target.feature.properties.NHD_NAME == el)) {
		e.target.setStyle({ fillColor: "yellow" })
    } else {
		// Reset layers
		e.target.setStyle({ fillColor: "darkgray" })
	}


}

var popup = L.popup()

async function onMapClick(e) {

	let clickedOn = await getClickedNeighborhood(e.latlng.lng, e.latlng.lat);

	if (clickedOn == "Correct") {
		console.log("Got it right!")
		// Reset formerGuesses
		formerGuesses = [];
/*			setPrompt();*/
		highlightHints(neighborhoodGroup);

	} else {
/*		formerGuesses.push(clickedOn)
		console.log("Added to guesses", formerGuesses)*/
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

	let promptEl = document.getElementById("prompt");
	// The first item in the hintJson is the neighborhood prompt. 
	promptEl.innerHTML = hintJson[0];

}

