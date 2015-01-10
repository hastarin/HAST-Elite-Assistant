HAST Elite Assistant
====================

![Image of application in use](http://i.imgur.com/lt6uSUh.png)

My [gamer personality](http://en.wikipedia.org/wiki/Bartle_Test) is firmly in the Explorer category so naturally Elite Dangerous offers some appeal, but I've felt hampered by the in game systems for navigation and exploration.  There are other 3rd party applications out there, but none did quite what I wanted, and so *HAST Elite Assistant* was born.

My day job involves writing applications in WPF and C# so it was a natural choice for this application.  

Latest Release
--------------
The latest release will always be made available [here on Github](https://github.com/hastarin/HAST-Elite-Assistant/releases).

Current Features
----------------
* Auto imports [systems.json](https://github.com/SteveHodge/ed-systems/blob/master/systems.json) from https://github.com/SteveHodge/ed-systems/ on initial startup
* Nearly instantaneous *(though currently non optimal)* route planning
* Topmost and Transparency support to display over the game
* Highlights the next system in the route when you enter a new system
* Copies the next system to the clipboard
* Speaks a warning when you enter a system that's not in the system data.  Add it to [EDStarCoordinator](http://edstarcoordinator.com/) and future versions will import it.

Future plans
------------
The current [list of enhancements](https://github.com/hastarin/HAST.Elite.Dangerous.DataAssistant/issues?q=is%3Aopen+is%3Aissue+label%3Aenhancement) is a long one.

If you do choose to try the app, please feel free to [log any issues](https://github.com/hastarin/HAST.Elite.Dangerous.DataAssistant/issues) you encounter.  If you're lucky I might fix them eventually.

For anyone needing it, there is code in there to read data from [EDDN](https://github.com/jamesremuscat/EDDN/wiki) but it's not currently in use.

Links
-----
Other open source [Elite Dangerous](https://www.elitedangerous.com/) related projects.

* [Elite Copilot](https://www.facebook.com/EliteCopilot) ([Github](https://github.com/w0nk0/Elite-Copilot)) - Python based route planner with voice guidance
* [Regulated Noise](https://forums.frontier.co.uk/showthread.php?t=86908) ([Github](https://github.com/stringandstickytape/RegulatedNoise)) - C# based OCR scanner and trade tool with [EDDN](https://github.com/jamesremuscat/EDDN/wiki) support
* [Quazil's Astro Analytics](https://forums.frontier.co.uk/showthread.php?t=89963) ([Bitbucket](https://bitbucket.org/Quazil/astroanalytics/)) - C# based trade tool with [EDDN](https://github.com/jamesremuscat/EDDN/wiki) support


License/Disclaimer
----------

﻿The MIT License (MIT)

Copyright (c) 2015 Jon Benson

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
