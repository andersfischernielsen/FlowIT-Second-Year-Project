# Comments on datamodel
## Server team
We have considered roles, and think it should be the Event's responsibility. 


Intelligent optimization is also best to be handled by the Event.

Event should send heartbeats to each other. If an event dies, another event should notify the server.