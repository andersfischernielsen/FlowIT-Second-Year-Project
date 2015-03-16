#Interface between the Client and Event

##Graph
Link to Lucidchart: https://www.lucidchart.com/invitations/accept/75244e80-849d-496d-b1c6-6d472c55e29d

##Event
We recommend that the event will be written using a ASP.NET WEB API since it can support all the features and is already a well tested framework.

###Required
Name: GetState
Parameters: 
Returns: HttpResult with a StateDto

Name: PushExecute
Parameters: user-hash
Returns: HttpResult with success value

###Optional
Name: GetEvent
Parameters: 
Returns: HttpResult with a EventDto

##Client
The client should not have an API for the event to access. This will provide a cleaner solution where Events do not know anything about the client.
We recommend that the client wil be written in C# using WPF

##DTOs
We recommend that we use DTOs to send data. To support the interface one must implement 1(optionally 2) DTOs
- StateDTO: includes the state of the event (excluded, executed and pending)
- EventDTO: Includes all information available on the event (state, relations, userslist and so on)

##Notes
The way we think roles will work is that the Server has a list of Users with according user-hash/(username, Password), and role. The Client then logs into the server, gets his User-Hash.
When the Event posts itself to the server, it will say which roles or specific users, can execute it and the Server will then return the list of User-Hashs.
Then when the client pushes an execute, the client sends it's own user-hash and the Event then checks up on that.

