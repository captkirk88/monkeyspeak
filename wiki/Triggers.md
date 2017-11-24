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
  So, a max of 2,147,483,647.  **You cannot have special number formats here.**

- Id = Any group of numbers that identify that specific trigger.  A Cause can have a category of 
  450 and a Effect can have a id of 450.  As long as you don't have two of the same id in one 
  category or a exception will be thrown.  **You cannot have special number formats here.**

All of this comes together to form a trigger `(0:123)`.

#### Contents of a Trigger

Take the following Triggers as an example:

```
(5:100) set variable % to {...}.
(5:101) set variable % to #.
```
We know that (5:100) declares a trigger but what does the `%` mean?  it wants you to replace `%` 
with a variable such as `%myVar`See [Variables](Variables.md) to find out why.  The {...} 
declares that a [String](Strings.md) should go there.  On the next Trigger (5:101) it wants you 
to replace `%` with a variable and declare a twitter hashtag?  Actually, the `#` sign says the 
Trigger wants you to put a number there.  A number could be 0 to 2,147,483,647 (do not include 
the commas).  Decimals and exponents are supported for numbers.

#### Handlers
Triggers must have some way of interacting with the calling application.  The way Monkeyspeak 
handles that kind of interfacing is through the BaseLibrary class.  See [Libraries](Libraries.md).
The preferred way of adding Trigger Handlers is by the method described on the Libraries page. 
However, if you want to add a Trigger Handler but don't care if it is listable via 
page.GetTriggerDescriptions() then you can add the Trigger Handler by calling 
page.AddTriggerHandler(TriggerCategory, int, TriggerHandler), example below.

```csharp
page.AddTriggerHandler(TriggerCategory.Cause, 0, EnterScript);
```
To remove a TriggerHandler simply call
```csharp
page.RemoveTriggerHandler(TriggerCategory.Cause, 0);
```

#### Next Up
> :book: [Variables](Variables.md)

#### Last Lesson
> :house_with_garden: [Home](../README.md)