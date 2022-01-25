# STL Neighborhood Guesser

STL Neighborhhod Guesser was created as a capstone project for the LaunchCode Immersive Code Camp.  The project renders a map of St. Louis with neighborhood boundaries layered on the map.  Users are prompted with a neighborhood to guess the location of by clicking on the map, which has highlights of six possible neighborhoods.  Users are also able to login to track their scores. 

The project uses C#, ASP.NET, and Leaflet.js to render the map.  To maximize the utility of the map, user actions trigger AJAX requests with the async/await pattern to an API.  Consequently, map layers are rendered seamlessly and without reloading.  The backend uses both .NET's MVC pattern and APIs for a dynamic user experience.  
