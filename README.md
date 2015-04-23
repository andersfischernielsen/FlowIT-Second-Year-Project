Second Year Project
=====

The	RESTful	P2P DCR Graph	Workflow	Engine	Project
-----

The aim of the first part of the project is to:
 1) implement a generic peer-to-peer distributed workflow engine based on Rest
services as described below in more detail.
 2) demonstrate the workflow engine on two example workflows from respectively a hospital in Brazil and a group of GBI students following the Business Process Modelling and IT course.
  
### DCR Graph	Workflow	Processes
Dynamic Condition Response (DCR) Graph workflow process model is a constraint based (declartive) process model developed at IT University of Copenhagen in collaboration with Exformatics A/S. 
Exformatics A/S has developed a graphical SaaS tool for collaborative design of DCRGraphs, available at DCRGraphs.net.

In its most basic form, a DCR Graph workflow process consists of a set of events related by four different kinds of relations: The condition relation, the response relation, the exclusion relation and the inclusion relation. Together, the events and the four different kinds of relations thus form a directed graph, where each edge in the graph is denoting
either a condition, response, exclusion or inclusion relation.

The state of the process is recorded by recording a state of each event. In its most basic form, an event state consists of three Booleans: (executed, included, pending). The ”executed” Boolean records whether the event has been executed at least once. The “included” Boolean records whether the event is currently included in the workflow. The “pending” Boolean records whether a future execution of the event is currently required.

The point is that the events determine what can happen during the workflow process, and the relations constrain the order of events, i.e. when they can happen. That is, by default an event can happen any number of times, but relations may constrain this.

### Example
A course process at ITU could have the events ”register”, ”pass” and ”fail”.

To model that registering for the course is a requirement for passing or failing the exam, we add a condition relation from “register” to both “pass” and “fail”. To model that a student is expected to pass the exam after registering, we add a response relation from “register” to “pass”. 
To model that it is not possible to both pass and fail, we add exclude relations between “pass” and “fail”. Finally, to model that every event can only happen at most once in this workflow, we add for all events an exclude relation from itself to itself.

Formally, the graph may be represented as follows:
    Events = {”register”, ”pass”,”fail”}
    
    State = {
        register -> (executed:false, included:true, pending:false),
        pass     -> (executed:false, included:true, pending:false),
        fail     -> (executed:false, included:true, pending:false), 
    }
    
    Conditions = { (”register,pass”), (”register”, ”fail”) }
    
    Responses  = { (”register”,”pass”) }
    
    Exclusions = { 
        (“register”,”register”) 
        (“pass”,”pass”), 
        (“fail”,”fail”), 
        (“pass”,”fail”), 
        (“fail”,”pass”)
    }

In addition to the information above, each event will normally also be assigned a set of roles used for role based access control (RBAC). 
That is, “register” could be assigned “Student” and “pass” and “fail” will be assigned “Teacher”. 
The access control could of course be refined to assign more fine grained access, e.g. allowing the student to read
(GET) “pass” and “fail” but not to execute (PUT).
