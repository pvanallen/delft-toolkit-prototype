# Delft AI Toolkit
## Visual Authoring Toolkit for Smart Things

The Delft Toolkit a system for designing smart things. It provides a visual authoring environment that incorporates machine learning and behavior trees to create smart behavior in autonomous devices.

![system diagram](https://i0.wp.com/www.philvanallen.com/wp-content/uploads/2018/01/Pasted_Image_1_16_18__3_50_PM.jpg?resize=640%2C350)

The toolkit is currently in rough prototype form as a part of my research. **It is likely to change significantly as I iteratively develop a technical and design strategy.**

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

![detailed diagram](http://www.philvanallen.com/wp-content/uploads/2017/08/tool-architecture-diagram.002.jpeg)
