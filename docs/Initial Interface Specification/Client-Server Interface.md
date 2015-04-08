# Interface between Client and Server
* We don't know what to name this "interface". So far it has been named `IClientStuff`.
* This interface should not be part of any program, but is just an indication of HTTP-methods the Server should provide to the Client.

## REST-service
The service should at minimum provide the following two HTTP methods:

* `GET /` should return a list of the Workflows it contains.  
  The returned information should contain the Ids of the Workflows.
* `GET /WorkflowID/` should return a list of Events which is part of the given Workflow with `Id = WorkflowID`.  
  The Events should contain enough information for the client to contact the Event directly.

We also discussed whether it could be beneficial to support the following method at some point:

* `GET /WorkflowID/EventID/` should return detailed information about a single Event which matches the given WorkflowID and EventID.

## Graph
See [Lucidchart](https://www.lucidchart.com/documents/edit/b38b4c94-fe5b-4454-a906-045781c31c98)
