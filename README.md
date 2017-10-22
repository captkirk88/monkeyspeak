![Monkeyspeak Hero](http://s2.quickmeme.com/img/1a/1a25ceb16d9921a1da97468be477f77c9fad22e552896ef49a74c0c1e15a9252.jpg)
# Monkeyspeak

Monkeyspeak aims to give the end-user a very easy to use scripting language.  

### Progress v7.0
- [x] [Triggers](wiki/Triggers.md) (the core construct of Monkeyspeak)
  - [x] Causes `(0:0) when something happens,`
  - [x] Conditions `(1:0) and it really did happen,`
  - [x] Effects `(5:0) do something about it.`
  - [x] Flow `(6:0) while it still is happening,`
- [x] [Variables](wiki/Variables.md) `%myVariable or %myTable[myKey]`
  - [x] Double (with -+ and exponent support)
  - [x] Strings (Unicode)
  - [x] Tables (Dictionary objects with a configurable limit)
- [ ] [Core Library](wiki/Libraries.md)
  - [ ] Sys (triggers that support core Monkeyspeak tasks like setting variables)
  - [ ] Math (very basic math operations + - / *)
  - [ ] StringOperations (very basic string operations)
  - [ ] IO (basic file operations)
  - [ ] Timers (basic timer support with optional delay)
  - [ ] Tables (supports the for each trigger)
  - [ ] Loops (supports while loop and possibly more in the future)

### Basic Explaination
A Trigger may optional be wrapped in parenthesis but must alway begin with a number 
from 0-9 and a colon in the middle to seperate the trigger's category, which is the 
first number, and the trigger's id, which is the last group of numbers.

Triggers are grouped into "blocks".  Blocks start with a Cause (see below) trigger 
and usually end with a Effect (see below) trigger.
##### Trigger Example:

`(` Optional wrapping parenthesis

`0` The trigger's category.  Triggers are categorized as Causes, Conditions, Effects and Flow.

* 0 Cause = Nothing happens without a Cause.  It is the first trigger executed in a group of triggers.
  You could look at it as event based.  A Cause is called when something happens.
* 1 Conditions = These are triggers act as a "if-then-continue".  If the condition succeeds then the 
  rest of the triggers after the condition are executed, if the condition fails then the entire block 
  is exited.
* 5 Effects = Better known as actions!  These triggers perform the work.  They could do a file write 
  operation, play music, popup a window, etc.  They must always perform work.
* 6 Flow = No, they have nothing to do with liquid.  These triggers are for executing a block of 
  triggers with a loop or a iterator.  They are slightly more advanced but fear not, there are 
  limitations set so that you will not encounter a infinite loop scenario.

`:` A seperator between the category and the id.

`123` A group of numbers that give the trigger a id.

* Id = Any group of numbers that identify that specific trigger.  A Cause can have a category of 
  450 and a Effect can have a id of 450.  As long as you don't have two of the same id in one 
  category or a exception will be thrown.

All of this comes together to form a trigger `(0:123)`.

### Basic Usage
For simplicity sake, in this example, we will assume trigger (0:0) has already been given a handler.

Let's say you had a Monkeyspeak script like this:
```stylus
(0:0) when the script is started,
        (5:100) set %hello to {Hello World}.
        (5:101) set %num to 5.1212E+003.
        (5:102) print {num = %num} to the console.
        (5:102) print {%hello} to the console.
```
You would load and execute it using a few methods:
```csharp
using Monkeyspeak;

var engine = new MonkeyspeakEngine();
Page page = engine.LoadFromString(testScript);

page.Error += DebugAllErrors;

page.LoadAllLibraries();

page.Execute(); // optionally provide the trigger Id of 0 to execute (0:0)
```
Output:
```csharp
num = 5121.2
Hello World
```
