



// Map Settings 2
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
/*map.on('click', onMapClick);*/


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

	await highlightHints();

	getScore();

}

async function getScore() {
	const response = await fetch(`https://localhost:5001/neighborhood/score`);
	const scoreJson = await response.json();
	console.log(scoreJson)


	let pointsEl = document.getElementById("points");
	pointsEl.innerHTML = scoreJson.points;

	let attemptsEl = document.getElementById("attempts");
	attemptsEl.innerHTML = scoreJson.attempts;
}


async function onNeighborhoodClick(e) {

	if (hintJson.some(el => el == e.target.feature.properties.NHD_NAME)) {
		formerGuesses.push(e.target.feature.properties.NHD_NAME)
		let clickedOn = await checkCliickedNeighborhoods(e.latlng.lng, e.latlng.lat);
		console.log("Server response to click: ", clickedOn)
		if (clickedOn == "Correct") {
			// Reset formerGuesses
			formerGuesses = [];
			highlightHints(neighborhoodGroup);

		} else {
			e.target.setStyle({ fillColor: "red" })
		}

		getScore();
    }
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

async function checkCliickedNeighborhoods(lon, lat) {
	const response = await fetch(`https://localhost:5001/neighborhood/click?lon=${lon}&lat=${lat}`);
	const responseJson = await response.json();

	return responseJson;
}

async function getHintNeighborhoods() {
	const response = await fetch(`https://localhost:5001/neighborhood/GetHints`);
	const hintJson = await response.json();

	return hintJson;
}

async function highlightHints() {

	// Hint neighborhoods made placed into global scope.
	hintJson = await getHintNeighborhoods();

	neighborhoodGroup.eachLayer(function (layer) {
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

