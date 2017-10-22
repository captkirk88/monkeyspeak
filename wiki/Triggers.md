### Triggers

#### What is it?
In life, everything has a trigger.  When a ball is thrown, a dog must give chase.  When you 
wake up in the morning, you must have your coffee or tea.  Do you see the pattern?  Triggers 
work in the same way.  You can have Triggers be invoked for any number of reasons, but you 
must create a Handler for those triggers.  I will get into Handlers later.

#### Break Down
- `(` Optional wrapping parenthesis
- `0` The trigger's category.  Triggers are categorized as Causes, Conditions, Effects and Flow.

  - 0 Cause = Nothing happens without a Cause.  It is the first trigger executed in a group of triggers.
  You could look at it as event based.  A Cause is called when something happens.
  - 1 Conditions = These are triggers act as a "if-then-continue".  If the condition succeeds then the 
  rest of the triggers after the condition are executed, if the condition fails then the entire block 
  is exited unless another Condition exists, then that one is evaluated, so on and so forth.
  - 5 Effects = Better known as actions!  These triggers perform the work.  They could do a file write 
  operation, play music, popup a window, etc.  They must always perform work.
  - 6 Flow = No, they have nothing to do with liquid.  These triggers will loop over a block of triggers 
  until the Flow is stopped.  They are slightly more advanced than the others but fear not, there are 
  limitations set so that you will not encounter a infinite loop scenario.

- `:` A seperator between the category and the id.

- `123` A group of numbers that give the trigger a id.  Limited by the max value of Integer. 
  So, a max of 2,147,483,647.

- Id = Any group of numbers that identify that specific trigger.  A Cause can have a category of 
  450 and a Effect can have a id of 450.  As long as you don't have two of the same id in one 
  category or a exception will be thrown.

All of this comes together to form a trigger `(0:123)`.

#### Next Up
> :book: [Variables](Variables.md)

#### Last Lesson
> :house_with_garden: [Home](../README.md)