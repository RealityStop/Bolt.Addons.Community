

# Bolt.Addons.Community
A community-driven project for extending Unity Bolt with custom nodes, types, and helpers.

Development is open to the community.


#### Why Bolt.Addons.Community and not Bolt.Community.Addons?  
While the latter flows off of the tongue more naturally, this project aims to be part of a larger ecosystem of addons and so aims for the former.  If auto-adding custom types from namespaces becomes a supported feature of Bolt, Bolt.Addons.X is preferable.

> [Direct Download (requires matching Bolt version)](https://github.com/RealityStop/Bolt.Addons.Community/releases/)



----------

# Installing

To import the addon, use one of the following methods:


> **Important**: if updating from a pre-3.0 version, please *DELETE* any Bolt.Addons.Community dll files.  By default these were placed in your *Plugins* folder.

### Via Package Manager:

Open the Unity Package Manager, and click the "+" button in the top-left corner :

![](https://imgur.com/v92tiFD.png)

and add the following url:

> https://github.com/RealityStop/Bolt.Addons.Community.git

> **Important**: looking for Unity Visual Scripting (UnityVS) support?  Use https://github.com/RealityStop/Bolt.Addons.Community.git#UVSsupport instead!


(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)

Then, use the Tools menu to Build Unit Options, and they're ready to go!  Once you've rebuilt your unit options, the new nodes will be available for use.



### Manual install:
Alternatively, open Packages/manifest.json and add this line under dependencies:

	"dev.bolt.addons": "https://github.com/RealityStop/Bolt.Addons.Community.git"
	
(for more information, or if errors are encountered, see https://docs.unity3d.com/Manual/upm-ui-giturl.html)
	
Then, use the Tools menu to Build Unit Options, and they're ready to go!  Once you've rebuilt your unit options, the new nodes will be available for use.


# Updating
To update, open Packages/manifest.json and remove the dev.bolt.addons entry under lock at the end of the file.  Once Unity synchronizes the package source, use the Tools menu to Build Unit Options and incorporate any new units.

![](https://imgur.com/siRm7wu.gif)

----------


### What's included
There are two sets of units currently delivered:

> ### Bolt.Addons.Community

/Variables
 - **Increment Variable**
 - **Decrement Variable**
 - **Plus Equals**

/Events
 - **On Every X Seconds**
 - **On Variable Changed**
 - **Manual Event**
 - **Defined Event**
 - **Trigger Defined Event**
 - **Global Defined Event**
 - **Trigger Global Defined Event**

/Documentation
 - **Todo**
 - **Some Value**
 - **Stuff Happens**
 - **Comment**

/Collections
 - **Random Numbers**
 - **CreateMultiArray**		(new in 3.0)
 - **GetArrayItem**		(new in 3.0)
 - **SetArrayItem**		(new in 3.0)
 - **Query**			(new in 3.0)
 - **Random Element**       (new in 3.0)

/Control
 - **Branch (Params)**
 - **Gate**
 - **Edge Trigger**
 - **Change Detect**
 - **DoOnce**

/Logic
 - **Latch**
 - **Polarity**
 - **Between**
 - **Logic (Params)**
 - **Log**

/Utility
 - **Convert**			(new in 3.0)
 - **FlowReroute**		(new in 3.0)
 - **ValueReroute**		(new in 3.0)
 
/Math
 - **Math Op**
 - 
	 /Functions  (Still in testing, feel free to make suggestions, though!)
	 - **Decay**
	 - **Exponential**
	 - **Linear**
	 - **Logarithmic**
	 - **Reverse Linear**
	 - **Sigmoid**



## Current maintainers (aka who to blame)
 - Reality.Stop()
 - JasonJonesLASM
 
 With contributions from:
 - Necka
 - AFoolsDuty
 - Eka
 - Silence
