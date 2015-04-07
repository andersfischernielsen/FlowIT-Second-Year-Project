# System Testing

A variety of testing approaches have been used to test the system during development. 
These include unit, integration, system and acceptance testing in varying degrees. Acceptance testing has also been applied after the inital tests were developed.

## Unit Testing
The major components handling data that have been developed by the team (not Microsoft's libraries) have been unit tested to ensure that their functionality was correct. 

### Server
The Controller- and ServerLogic classes have been unit tested. 

#### IServerLogic 
ServerLogic is the implementation of the IServerLogic interface, and has therefore been tested according to the methods specified in this interface. 
Mocking has been used extensively (using the Moq nuGet package) to ensure that the logic was tested in an isolated and controlled environment. 
The logic saves data to an instance of IServerStorage, which is mocked to enable testing. Every method in this interface is mocked, and saves and retrieves data from an in-memory List using callback methods.

#### WorkflowsController
Mocking is used to test the implementation of the WorkflowController class - again to test in a controlled environment. 
An IServerLogic instance is mocked and returns a List of elements when a method is tested. 
Incoming HTTP requests are handled by the controller and therefore methods handling GET, POST, PUT and DELETE requests have been tested using a IServerLogic mock. 
  
### Event

### Client
