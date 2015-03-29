# Client Use Cases

**Intro to "Pay gas-bill":** 

The "Pay gas-bill" workflow works as such: You can "enter gasmeter-info" or "enter person-info" in any order, but you *must* do both before you can "pay bill"

## Scenario 1
- Karen wants to execute "Pay gas-bill" workflow
- In a list of available workflows, she selects "Pay gas-bill"
- From a list of three associated subtasks ('events') - in which a single ("pay bill") is greyed out - she picks one of the two executable ("enter customer info" and "enter gas-meter state"), and executes it.
- Karen now followingly executes the other subtask
- Karen should be allowed to repeat these two subtasks (she might have entered wrong information)
- In the list, Karen should now notice that "Pay bill" is no longer greyed out.
- Karen executes "Pay bill"
- The three subtaks should not be executable no more (TODO: Discuss: How do we handle this / Do we handle this?)

