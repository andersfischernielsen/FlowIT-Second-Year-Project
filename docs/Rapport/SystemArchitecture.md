#System Architecture Overview

## Purpose of the system

The basic idea of the system is to enable users (through a Windows-client) to have an overview of and execute the events within a workflow. An "event" is one part / a step of a workflow. 

## System description

To fulfill the requirements of the project, three subsystems are created: a central server ("Server"), a client ("Client) and events ("Event"). The three subsystems are described in the following section.

![Subsystems and the interfaces between them.](Subsystems.png)

###Central Server
The Central server functions as a RESTfull WebAPI, where Events can hook up onto (an existing) Workflow and the Client can retrieve a list of Workflows and Events. The Server subsystem is implemented in C# using the ASP.NET WEP API framework

###Client

The main functionality of the Client subsystem is to provide the users an overview of Workflows and Events on the workflows, and provide the user with a way to send an Execute call to a specific event.
The Client is implemented in C# using the .NET framework WPF for GUI components. The client has a connection submodule which handles all outgoing calls to both the Server and events.

###Event

The Event subsystem is implemented in C# using the ASP.NET WEP API framework, which allows for easy routing and setup of a RESTful WebAPI. An Event also have submodules which controls all outgoing calls to the Server and to the events which (the Event) is related to. 
The implementation of Event allows for multiple Events to be stored at either, the same machine or be distributed across network and multiple machines or a combination of the two. 