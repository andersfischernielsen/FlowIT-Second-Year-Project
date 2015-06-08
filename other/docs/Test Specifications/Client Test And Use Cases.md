# Client Use Cases

**Intro to "Pay gas-bill":** 

The "Pay gas-bill" workflow works as such: You can "enter gasmeter-info" or "enter person-info" in any order, but you *must* do both before you can "pay bill"

## Scenario 1
1. Karen wants to execute "Pay gas-bill" workflow
2. In a list of available workflows, she selects "Pay gas-bill"
3. From a list of three associated subtasks ('events') - in which a single ("pay bill") is greyed out - she picks one of the two executable ("enter customer info" and "enter gas-meter state"), and executes it.
4.  Karen now followingly executes the other subtask
5.  Karen should be allowed to repeat these two subtasks (she might have entered wrong information)
6. In the list, Karen should now notice that "Pay bill" is no longer greyed out.
7. Karen executes "Pay bill"
8. The three subtaks should not be executable no more (TODO: Discuss: How do we handle this / Do we handle this?)


# Exceptional cases

- If a subtask turns out not to be executable at the moment (determined by the Event), the Client should display Karen a message, explaining why and any potential actions she can take to get it executed. 
- The client should be able to handle (i.e. not crash) the case where an Event has become offline, but the Client hasn't realized this, and issues a requests to this (now) dead Event. 
- 