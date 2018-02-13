# Delft AI Toolkit
## Visual Authoring Toolkit for Smart Things

The Delft Toolkit a system for designing smart things. It incorporates machine learning and behavior trees to create smart behavior, all in a visual authoring environment.

The toolkit is currently in rough prototype form as a part of my research. _It is rapid development, and likely to change significantly as I form a technical and design strategy._

The goal of this project is to develop an authoring system approach that enables designers to easily and iteratively prototype smart things. This approach includes the ability to Wizard-of-Oz AI behaviors and simulate physical hardware in 3D, and then migrate these simulations to working prototypes that use machine learning and real hardware.

* [Overall Project description](http://www.philvanallen.com/portfolio/delft-ai-toolkit/)
* [Process Blog](http://ai-toolkit.tumblr.com)

The system currently has two parts:
* Control System running on a PC
  * Visual Authoring System, Unity3D
  * Communications Server, Node.js
* Robot
  * Raspberry Pi
  * Arduino
  * Motors, sensors, microphone, camera, etc.

Each of these has a codebase, and includes a range of libraries. In particular, the Unity system is based on the [NodeCanvas](http://nodecanvas.paradoxnotion.com) paid asset for Unity3D, and provides the foundation for the visual authoring of behavior trees. My hope is to eventually make this part of the system free in some way.
