Event-Server interface
=====

Server
-----

**Server responds to the following REST URLs:**

- .../workflow/workflowname
	- **GET:** Responds with a JSON list of events in the given workflow (eg. _{"workflow": {"eventTitle": "testing", "eventUrl": "22.22.22.22:2222"}}_)
	- **POST:** Adds the event calling the URL to the workflow if the workflow already exists, otherwise creates the workflow and adds the URL of the event to it. The server fetches the event URL from the HTTP request. _POST-ing is only allowed for events, not clients!_
	- **POST:** If the event sends a heartbeat JSON object (eg. _{"heartbeat": {"alive": "true"}}_), then the Server resets the timeout for the given workflow. If the timeout expires, then the workflow is deleted from the Server. 
	- **DELETE:** Removes the given event URL from the list of events in the given workflow. The URL to remove is sent as a JSON object (eg. _{"deadEvent": {"deadUrl": "22.22.22.22:2222"}}_). _Events access this URL when they discover a dead neighbouring event!_

**Server has the following methods:**

- GetWorkflow(string name)
	- This method is called when an Event or a Client accesses /workflow/workflowname. See above. 
- Createworflow(string name, string url)
	- This method is called by the method above if the given wokflow does not exist. 
- RemoveEventFromWorkflow(string workflowId, string url)
	- This method is called by DELETE requests (see above) and removes the given event URL from the given workflow.

Event
-----

**Events have the following methods:**

- NotifyServer(string workflowId)
	- This method is called when the Event is instantiated. It tells the server via a POST request (see above) that it wishes to belong to the given workflow. 
- RemoveEventFromWorkflow(string workflowName, string deadUrl)
	- When an Event discovers that one of its fellow Events has died (through heartbeating), this method is called. It tells the Server that the given Event URl has died by sending the URL as a JSON object (see above).
