



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

// Fuctions
async function getNeighborhoods() {
	response = await fetch('https://localhost:5001/neighborhood/all').then(response => response.json());

	let neighborhoodGroup = L.layerGroup();

	L.geoJSON(response, {
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

	addInstructionalToolTips();

	getScore();

}

// Removes instructional tooltips, used when user clicks on the prompted neighborhood
function removeToolTips() {
	map.eachLayer(function (layer) {
		if (layer.options.pane === "tooltipPane") layer.removeFrom(map);
	});
}

function addInstructionalToolTips() {
	let count = 1

	neighborhoodGroup.eachLayer(function (layer) {
		if (layer.feature.properties.NHD_NAME == hintJson[0]){
			layer.bindTooltip("Clicked here on " + hintJson[0] + " to recieve a point if logged in.",
				{ permanent: true }
			)
			layer.openTooltip()
			layer.on('click', removeToolTips)
		}

		if (hintJson.some((el) => layer.feature.properties.NHD_NAME == el) &&
			layer.feature.properties.NHD_NAME != hintJson[0] &&
			count == 1) {
			layer.bindTooltip(`Some neighborhoods, including the neighborhood to guess,
				are highlighted in yellow to give you hints.`,
				{ permanent: true })
			layer.openTooltip()
			count--
		}
	})
}

async function getScore() {
	const response = await fetch(`https://localhost:5001/neighborhood/score`).then(response => response.json());

	if (response.userId) {
		document.getElementById("points").innerHTML = response.points;
		document.getElementById("attempts").innerHTML = response.attempts;
    }

}

// When a neighborhood is clicked, turn it red if its not the answer, move to next neighborhood challenge if correct
// Neighborhoods that are not in the set of hints do not react to clicks.
async function onNeighborhoodClick(e) {

	if (hintJson.some(el => el == e.target.feature.properties.NHD_NAME)) {
		formerGuesses.push(e.target.feature.properties.NHD_NAME)
		let clickedOn = await checkCliickedNeighborhoods(e.latlng.lng, e.latlng.lat);

		if (clickedOn) {
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

async function checkCliickedNeighborhoods(lon, lat) {
	return await fetch(`https://localhost:5001/neighborhood/click?lon=${lon}&lat=${lat}`).then(response => response.json());
}

async function getHintNeighborhoods() {
	return await fetch(`https://localhost:5001/neighborhood/GetHints`).then(response => response.json());
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

	// The first item in the hintJson is the neighborhood prompt. 
	document.getElementById("prompt").innerHTML = hintJson[0];

}

