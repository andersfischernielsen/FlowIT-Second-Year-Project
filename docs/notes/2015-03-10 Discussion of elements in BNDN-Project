## Notes in relation to P2P
* When an Event connects to the central server, it should present its Event- and Workflow ID.
* The central server responds with a list of other Events in the same workflow, including Event-ID and URL/IP-address.
* From here it is up to the Event to tell other Events of its presence.

## Event
* WorkflowID
* EventID (Maybe a name that represents the Event)
* Allowed users/roles
* URL
* Per user state (in database)
    * Executed : bool
    * Included : bool
    * Pending : bool
* 4 lists of edges.

### Functionality
* Should be able to notify server of it's existence.
* HasExecuted
* CanExecute (Called by execute, not sure if it should be available for task)
* IsIncluded
* Execute (Should be able to fail, if it is not possible to execute.)
    * CanExecute?.
    * Update all Events after execution.
* See specification and check if all REST-stuff is covered.

### Who should know which kind of edge
* An Event's list of conditions represents the events which must be executed, before it can execute itself.
* An Event's list of responses represents the events which must be executed at some point after this Event's execution unless the response event is excluded.
* An Event's list of exclusions is the events which must be disabled after execution of this event.
* An Event's list of inclusions is the events which must be enabled after execution of this event.

### Random note
If new edges should be added to a current workflow, the administrator must update each Event.

## Server
* Foreach workflow
    * Foreach event
        * EventID
        * URL
        * (WorkflowID)

(Maybe something heartbeat)

### Functionality
* Return list of workflows.
* Return lists of events for a workflow.
* Accept new events.

## Client
* Will be able to contact Server for list of Events.
* Contacts every Event to see if it is executable.
* The user should only see events which can be executed.
* Client should not be able to change the workflow. Only execute it.

### Functionality
* Get list of executables, possibly order by pending.
* Execute on click. Update list.
* Login-stuff
* Multiple workflows (Select workflow).

## Optionals
* Heartbeart
* Event-locking
* P2P Failure handling
* Intelligent optimizations
    * Event knows who to contact
    * Splitting server up in two servers, one per Workflow and one for all workflows.
    * Something with stopping the client from flooding all events by getting information about changed events when executed.
* GUI for client.
