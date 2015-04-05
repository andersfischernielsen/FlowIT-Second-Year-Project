##System Architecture Overview

To forfill the requirements of the project, three subsystems are created: a central server, a client and an event. The three subsystems are described in the following section.

![Subsystems and the interfaces between them.](Subsystems.png)

###Central Server
The Central server functions as a restful web api where Events can hook up on Workflows and the Client can get a lists of Workflows and Events. The subsystem is implemented in C# using the ASP.NET WEP API framework

###Client

The main functionality of the Client subsystem is to provide the users of an overview of Workflows and Events on the workflows, and provide the user with a way to send an Execute call to a specific event.
The Client is implemented in C# using the .NET framework WPF for GUI components. The client has a connection submodule which handles all outgoing calls to both the Server and events.

###Event

The Event subsystem is implemented in C# using the ASP.NET WEP API framework, which allows for easy routing and setup of a restful web-api. An event also has a submodule which controls all outgoing calls to the Server and the events which it is in relation to. 
The system can have an abitrary amount of Events, as long as the hardware it is run on supports it.