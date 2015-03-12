# Why did we use ASP.NET WebAPI instead of implementing our own F# webserver?
We chose this to get

* A thoroughly tested webserver implementation
* The webserver will probably be stateful (we should probably use the non-functional features of F#)
* We have worked with the framework before and know how to i.e. limit access.
* We can use F# libraries in the code anyway.

However, as discussed, we will use F# as much as possible in the Second Year Project.
