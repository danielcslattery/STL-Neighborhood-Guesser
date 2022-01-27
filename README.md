# STL Neighborhood Guesser

STL Neighborhhod Guesser was created as a capstone project for the LaunchCode Immersive Code Camp.  The project renders a map of St. Louis with neighborhood boundaries layered on the map.  Users are prompted with a neighborhood to guess the location of by clicking on the map, which has highlights of six possible neighborhoods.  Users are also able to login to track their scores. 

The project uses C#, ASP.NET, and Leaflet.js to render the map.  To maximize the utility of the map, user actions trigger AJAX requests with the async/await pattern to an API.  Consequently, map layers are rendered seamlessly and without reloading.  The backend uses both .NET's MVC pattern and APIs for a dynamic user experience.  

Users are initially given two tool tips.  One letting them know what yellow means on the map, and another telling them where the prompt neighborhood is located.  Both tool tips disappear after the answer is clicked.

![Appearance of project on Login](https://github.com/danielcslattery/STL-Neighborhood-Guesser/blob/a4213ff674c766a04d063b8df4ee5de404523d04/Project%20Images/NeighborhoodGuesserWithInstructions.png)

<br/>

Wrong answers are highlighted in red, and users are able to zoom in until most street names are visible.  Neighborhoods that are not given as hints are left grayed out and do not react to clicks.  

![Appearance of project when zoomed](https://github.com/danielcslattery/STL-Neighborhood-Guesser/blob/a4213ff674c766a04d063b8df4ee5de404523d04/Project%20Images/NeighborhoodGuesserZoomed.PNG)
