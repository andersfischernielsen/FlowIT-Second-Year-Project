Datamodel - Event 
=====
An event has the following data.

State
-----
	Executed : Dictionary<userId : int, executed : bool>
	Included : Dictionary<userId : int, included : bool>
	Pending  : Dictionary<userId : int, pending : bool>

The state is transferred between Events. 
See interfaces for details. 

Rules
-----
	Preconditions : Dictionary<PreconditionURL : string, (IsExecuted : bool, IsExcluded : bool)>
	Response      : string
	Exclusions    : List<toExcludeURL : string>
	Inclusions    : List<toNotifyURL : string>
	
The rules are for internal use in the Event. 

Heartbeating
-----
	AliveNeighbours : Dictionary<neighbourURL : string, alive : bool>
	PingInterval    : int
	Timeout         : int
	
Heartbeating is for internal use in Events. 