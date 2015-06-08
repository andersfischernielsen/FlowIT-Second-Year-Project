# Concurrency Control

For more information ask Adam or Cecilie. 

We've considered both optimistic and pessimistic concurrency and went with pessimistic. 

## Optimistic concurrency
We would have done backwards validation if we used optimistic concurrency. 

An event trying to execute gets a copy of the event it wants to update. The executing event copies this copy, and then have two: The original copy and the updated copy. The executing event send both to the event its trying update. The receiver checks whether the original copy is the same as itself. If it is, it updates to the updated copy. If they are not the same, the receiving event does not update, and the event trying to execute aborts.

This is a very complicated implementation to a problem where we know that only one event will be able to execute even though multiple tries to. 

## Pessimistic concurrency
We Hash all eventId and use as a Id. We lock in order of size, an use a queue if it's not possible to access the event you want to lock. This prevents deadlocks. 

There's a timeout for how long you can be in the queue: 10 seconds. The HttpToolbox holds this timeout and also the while loop asking whether the current event is the next in line. 

The event that is being waited for holds the queue of the events waiting. 




