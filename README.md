Second Year Project
=====

Hacks to Fix Before Final Release
-----

### DTO-hack
ExecuteDTO bliver brugt til GetEvents() for et givent workflow. 
Enten skal denne omdøbes til RoleDTO el. lign., eller også skal der oprettes en ny DTO. 

### Admin-reset
Reset flow er lidt hacky. Admin rollen bliver tilføjet til klienten, så bliver admin events hentet (alle events) og derefter bliver rollen fjernet. Så resettes events.

### XML-Parser
The entire parser is kind of hacky and should be excluded from the solution and moved to its own solution. 
