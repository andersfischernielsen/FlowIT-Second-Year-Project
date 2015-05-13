# System Testing

This section descibes how we have tested the software. 

A variety of testing approaches have been used to test the system during development. 
These include unit-, integration-, system- and acceptance testing in varying degrees. Acceptance testing has also been applied after the inital tests were developed.

## Unit Testing
The major components handling data that have been developed by the team (not Microsoft's libraries) have been unit-tested to ensure that their functionality was correct. 

### Server
The unittests on Server are found in the Server.tests project (in the provided Visual Studio solution)

The WebAPI controller and server logic classes have been unit tested as follows:

#### IServerLogic 
ServerLogic is the implementation of the IServerLogic interface, and has therefore been tested according to the methods specified in this interface. 
Mocking has been used extensively (using the Moq nuGet package) to ensure that the logic was tested in an isolated and controlled environment. 
The logic saves data to an instance of IServerStorage, which is mocked to enable testing. Every method in this interface is mocked, and saves and retrieves data from an in-memory List using callback methods.

#### WorkflowsController
Mocking is used to test the implementation of the WorkflowController class - again to test in a controlled environment. 
An IServerLogic instance is mocked and returns a List of elements when a method is tested. 
Incoming HTTP requests are handled by the WorkflowsController and therefore methods handling GET, POST, PUT and DELETE requests have been tested using a IServerLogic mock. 
  
### Event
The unit-tests concerning Event are located in the Event.tests project (in the provided Visual Studio solution)

The outgoing communicator, WebAPI controller and event logic classes have been unit tested as follows:

#### EventCommunicator
The EventCommunicator should throw certain expections when receiving invalid requests. This has been tested for methods in the IServerFromEvent interface.

#### EventLogic
EventLogic is the implementation of the IEventLogic interface. The methods in this interface have been unit tested by creating an in-memory instance of IEventStorage and testing against this. 
Assertions for expected results (including exceptions) is used to test that methods return the expected results. 

#### EventStateController
The locking functionality of Events has been tested using unit testing and assertions for expected results. 

### Client
THe unittests on Client are located in Client.tests projects (in the provided Visual Studio-solution)

The connection of the Client to the Server has been unit tested as follows:

#### ServerConnection
The ServerConnection inherits from the IServerConnection interface and the methods defined in this interface have been unit tested.
An instance of HTTPClientToolbox is mocked to ensure that the ServerConnection can be tested in an isolated environment. 
Testing that the correct exceptions are thrown on invalid requests and correct data is returned on valid requests is done by assertion.

## Integration Testing
Formal test cases have been specified, but have not been evaluated formally. 
Most functionality has been tested throughout development, but test results have not been written down due to time pressure. Getting the system to "just work" after unit testing was completed was the main focus. 

## System Testing
System testing has not yet been formally completed. 

## Acceptance Testing
Acceptance testing would preferrably be done by the receiver/user of the system using test cases specified either by the client or the developers. 
Acceptance testing has not been done yet.