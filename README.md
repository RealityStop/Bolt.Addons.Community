# Bolt.Addons.Community
A community-driven project for extending Unity Bolt with custom nodes, types, and helpers.

Development is open to the community.


#### Why Bolt.Addons.Community and not Bolt.Community.Addons?  
While the latter flows off of the tongue more naturally, this project aims to be part of a larger ecosystem of addons and so aims for the former.  If auto-adding custom types from namespaces becomes a supported feature of Bolt, Bolt.Addons.X is preferable.

> [Direct Download (requires matching Bolt version)](https://github.com/RealityStop/Bolt.Addons.Community/releases/)



----------

## Now in V2.0!

 Version 2.0 has arrived!  See the [release notes](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Version-2.0-Released!) for more details on what comes with 2.0 as well why the version number got bumped all the way to 2.0.
 
 
 

## Installing

Import the provided .assetpackage into your project, then and regenerate.  There is no need to add anything to the assembly or type options.  Regenerating is required before the units will be available.

> **Important:** Version 2 is structured significantly different from prior versions.  See the [Upgrade Guide](https://github.com/RealityStop/Bolt.Addons.Community/wiki/Version-2.0-Released!) for more information.


## What's included
There are two .unitypackages delivered currently:

> ### Bolt.Addons.Community.Fundamentals

/Variables
 - **Increment Variable**
 - **Decrement Variable**
 - **Plus Equals**

/Events
 - **On Every X Seconds** -- *(new in 1.6.1)*


/Collections
 - **Random Numbers** 

/Control
	 - **Branch Equal**
	 - **Branch Greater**
	 - **Branch Less**
	 - **Branch Next**
	 - **Branch (Params)** - *(New in 2.0)*
	 - **Gate**
	 - **Edge Trigger**
	 - **Change Detect**
	 - **DoOnce**

/Logic
 - **Latch**
 - **Polarity**
 - **Between**
 - **Logic (Params)** - *(New in 2.0)*

/Math
 - **Math Op** - *(New in 2.0)*
	 /Functions  (Still in testing, feel free to make suggestions, though!)
		 - **Decay**
		 - **Exponential**
		 - **Linear**
		 - **Logarithmic**
		 - **Reverse Linear**
		 - **Sigmoid**

> ### Bolt.Addons.Community.Events
 - **Return**
 - **Return Event**
 - **Trigger Return Event**




## How do I use it?
See the documentation!  Once you've rebuilt your unit options, the new nodes will be available for use.  The repository wiki details each node added.  If we've missed anything, send us a message!


# For Developers:

## How to build
To build the addons for Bolt version {targetversion}.  Add the following dlls from your Unity Project with {targetversion} of Bolt installed:

\Dependencies\BoltBinaries\\{targetversion}
 - Bolt.*.dll  
 - Ludiq.*.dll
(from (*Assets\Ludiq\Assemblies*) )
  
\Dependencies\UnityBinaries
 - UnityEditor.dll
 - UnityEngine.CoreModule.dll
 - UnityEngine.dll
 - UnityEngine.UI.dll
 
 ![](https://i.imgur.com/M7XvCRl.gif)

Open and build the \src\Bolt.Addons.Community.sln solution in Visual Studio 2017, selecting the Release_{targetversion} solution configuration.

The binaries will be copied to \Output\\{targetversion}


## Current contributors (aka who to blame)
 - *Reality.Stop()
 - *JasonJamesLASM
 - Silence
