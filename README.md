# Community Addons
### Unity Visual Scripting Extensions
A community-driven project for extending Unity Visual Scripting with custom nodes, assets, types, and helpers.

Development is open to the community.

## Branches

Unity Visual Scripting is now this master branch. To use Bolt, use the branch `bolt-main`. Be aware, the Bolt branch will no longer be receiving updates.

----------

# Installing

To import the addon, use one of the following methods:


> **Important**: if updating from a pre-3.0 version, please *DELETE* any Bolt.Addons.Community dll files.  By default these were placed in your *Plugins* folder.

### Via Package Manager:

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:

> https://github.com/RealityStop/Bolt.Addons.Community.git

For BOLT use branch `bolt-main` with this url:

> https://github.com/RealityStop/Bolt.Addons.Community.git#bolt-main

(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.



### Manual install:
Alternatively, open Packages/manifest.json and add this line under dependencies:

	"dev.bolt.addons": "https://github.com/RealityStop/Bolt.Addons.Community.git"
	
(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)
	
Then, use the Regenerate Nodes option in Project Settings > Visual Scripting, and they're ready to go!  Once you've rebuilt your node options, the new nodes will be available for use.


# Updating
To update, open Packages/manifest.json and remove the dev.bolt.addons entry under lock at the end of the file.  Once Unity synchronizes the package source, use the Tools menu to Build Unit Options and incorporate any new units.

![](https://imgur.com/siRm7wu.gif)

----------


### What's included
We include a multitude of assets, nodes, and tooling to extend missing language concepts and provide fundamental utilities and extensions for graphs. Here is what you will find:


--------


> ### NODES

Variables
 - **Increment Variable**
 - **Decrement Variable**
 - **Plus Equals**

Events
 - **On Every X Seconds**
 - **On Variable Changed**
 - **Manual Event**
 - **Defined Event**
 - **Trigger Defined Event**
 - **Global Defined Event**
 - **Trigger Global Defined Event**
 - **Return Event**
 - **Event Return**
 - **Trigger Return Event**
 - **Editor Window Events**
 - **Trigger Asset Custom Event**
 - **Reset Graph Listener**

Documentation
 - **Todo**
 - **Some Value**
 - **Stuff Happens**
 - **Comment**

Collections
 - **Random Numbers**
 - **Create Array**
 - **Get Array Item**
 - **Set Array Item**
 - **Query**
 - **Random Element**

Control
 - **Branch (Params)**
 - **Gate**
 - **Edge Trigger**
 - **Change Detect**
 - **Invoke Delegate**
 - **Bind Delegate**
 - **Unbind Delegate**
 - **Create Delegate**

Logic
 - **Latch**
 - **Polarity**
 - **Between**
 - **Logic (Params)**
 - **Log**

Utility
 - **Convert**	
 - **Flow Reroute**
 - **Value Reroute**

Graphs
 - **Machine Is**
 - **Get Machine**
 - **Get Machines**
 - **Get Machine Variable**
 - **Set Machine Variable**
 - **Has Machine Variable**

Time
 - **Yield**
 - **Enumerator**

Editor
 - **Window Is**
 - **Get Window Variable**
 - **Set Window Variable**

Math
 - **Math Op**
 - **Functions**
	 - **Decay**
	 - **Exponential**
	 - **Linear**
	 - **Logarithmic**
	 - **Reverse Linear**
	 - **Sigmoid**


--------

> ### ASSETS

Code Assets are used to generate dependency free C# code.

Code
 - **Class** (Experimental)
 - **Delegate**
 - **Enum**
 - **Interface** (Experimental)
 - **Struct** (Experimental)

Editor
 - **Editor Window**


--------


> ### UTILITIES

 - **Node Selection to Asset or Embed**
 - **Compiler Button**


--------


## Current maintainers (aka who to blame)
 - Reality.Stop()
 - JasonJonesLASM
 
 With contributions from:
 - Necka
 - AFoolsDuty
 - Eka
 - Silence
 - PurerLogic
