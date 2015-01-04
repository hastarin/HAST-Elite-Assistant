HAST.Elite.Dangerous.DataAssistant
==================================

This will hopefully come to be a windows application to assist commanders in Elite Dangerous with managing navigation and trade data.


At the moment it simply speaks a warning via Windows TTS when you enter a system that isn't known in the systems.json file from https://github.com/SteveHodge/ed-systems that it obtains on first startup.

I've also got it reading data from [EDDN](https://github.com/jamesremuscat/EDDN/wiki).

My TODO list is a long one:
* Store the EDDN data in the database.
* Provide a report of any systems in the EDDN data that aren't in the navigation data.
* Integrate with the EDStarCoordinator website via its API to find new systems
* Provide a way to add systems/stations/etc
* Provide a way to generate a systems.json file
* Provide a way to sync with another systems.json file
* Implement some form of shortest path navigation aid similar to [Elite Copilot](https://www.facebook.com/EliteCopilot)
* Provide a way to search the EDDN data for handy trade routes, etc similar to [Slopey's BPC Market Tool](https://forums.frontier.co.uk/showthread.php?t=76081)

NOTE: I plan to use SharpDX to handle any Vector mathematics.
