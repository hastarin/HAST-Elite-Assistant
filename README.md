HAST Elite Assistant
==================================

This will hopefully come to be a windows application to assist commanders in Elite Dangerous with managing navigation and trade data.

The current version should be considered Alpha software and I won't be offering any support.  Use it at your own risk!

![Image of application in use](http://i.imgur.com/603983r.png)

The current version can be used to rapidly find a route between systems.  The route may not be an optimal one, something I hope to improve upon in the future, but for now it's basically instantaneous.  The application window can be pinned on top and it's transparency can be adjusted.  If you play [Elite Dangerous](https://www.elitedangerous.com/) in a full screen window this can be quite handy.

It also speaks a warning via Windows TTS when you enter a system that isn't known in the systems.json file from https://github.com/SteveHodge/ed-systems that it obtains on first startup.  If you hear that you should probably go to [EDStarCoordinator](http://edstarcoordinator.com/) and enter the system.

There is code in there to read data from [EDDN](https://github.com/jamesremuscat/EDDN/wiki) but it's not currently in use.


My TODO list is long, and ever changing.  Some of the many ideas are below:
* Improve the route planner functionality, UI first, then the algorithm behind it.
* Store the EDDN data in the database.
* Provide a report of any systems in the EDDN data that aren't in the navigation data.
* Integrate with the EDStarCoordinator website via its API to find new systems
* Provide a way to add systems/stations/etc
* Provide a way to generate a systems.json file
* Provide a way to sync with another systems.json file
* Provide a way to search the EDDN data for handy trade routes, etc similar to [Slopey's BPC Market Tool](https://forums.frontier.co.uk/showthread.php?t=76081)
