# Monkeyspeak

[![GitHub release](https://img.shields.io/github/release/captkirk88/monkeyspeak.svg)](https://gitHub.com/captkirk88/monkeyspeak/releases/)

Linux/Mono | Windows
------------ | ---------
[![Build Status](https://travis-ci.org/captkirk88/monkeyspeak.svg?branch=master)](https://travis-ci.org/captkirk88/monkeyspeak) | [![Build status](https://ci.appveyor.com/api/projects/status/eh3ang36lymr5ty8/branch/master?svg=true)](https://ci.appveyor.com/project/captkirk88/monkeyspeak/branch/master)

[![Waffle.io](https://badge.waffle.io/captkirk88/monkeyspeak.svg?columns=inbox,In%20Progress&style=plastic)](https://waffle.io/captkirk88/monkeyspeak) 

[![forthebadge](http://forthebadge.com/images/badges/60-percent-of-the-time-works-every-time.svg)](http://forthebadge.com)  [![forthebadge](http://forthebadge.com/images/badges/winter-is-coming.svg)](http://forthebadge.com)

Monkeyspeak aims to give the end-user a very easy to use scripting language.  

### Progress v1.0

```
'+' = Complete
'-' = Incomplete
'!' = In progress
```

```diff
+ [Triggers] (the core construct of Monkeyspeak)
+ Causes `(0:0) when something happens,
+ Conditions `(1:0) and it really did happen,
+ Effects `(5:0) do something about it.
+ Flow `(6:0) while it still is happening,

+ [Strings]
+ Prefix support to remove certain formatting
+ '!' will remove processing variables resulting in returning the variable name
+ '@' will remove human readable format of numbers (i.e comma seperated numbers won't show commas)

+ [Variables] %myVariable or %myTable[myKey]
+ Double (with -+ and exponent support)
+ Strings (Unicode)
+ Tables (Dictionary objects with a configurable limit)
+ Constants (unmodifiable variables)

+ [Core Library]
+ Sys (triggers that support core Monkeyspeak tasks like setting variables)
+ Math (very basic math operations + - / *)
+ StringOperations (very basic string operations)
+ IO (basic file operations)
+ Timers (basic timer support with optional delay)
+ Tables (supports the for each trigger)
+ Loops (supports while loop and possibly more in the future)
```

### Basics
A Trigger may optionally be wrapped in parenthesis but must alway begin with a number 
from 0-9 and a colon in the middle to seperate the trigger's category, which is the 
first number, and the trigger's id, which is the last group of numbers.

Triggers are grouped into "blocks".  Blocks start with a Cause (see below) trigger 
and usually end with a Effect (see below) trigger.
##### Trigger Explanation:

See [Triggers](wiki/Triggers.md#break-down).

#### Basic Usage
For simplicity sake, in this example, we will assume trigger (0:0) has already been given a handler.

Let's say you had a Monkeyspeak script like this:
```
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

page.LoadAllLibraries();

// optionally provide the trigger Id of 0 to execute (0:0), 1 to execute (0:1), etc.
page.Execute();
```
Output:
```
num = 5121.2
Hello World
```

> :bulb: Try to run the above snippet and experiment with it.

#### Advanced Usage

Here is a example of using Flow triggers

```
(0:0) when the script is started,
    (5:250) create a table as %myTable.
    (5:100) set %hello to {hi}
    (5:252) with table %myTable put {%hello} in it at key {myKey1}.
    (5:252) with table %myTable put {%hello} in it at key {myKey2}.
    (5:252) with table %myTable put {%hello} in it at key {myKey3}.
    (5:252) with table %myTable put {%hello} in it at key {myKey4}.
    (5:252) with table %myTable put {%hello} in it at key {myKey5}.
    (5:252) with table %myTable put {%hello} in it at key {myKey6}.
    (5:252) with table %myTable put {%hello} in it at key {myKey7}.
    (6:250) for each entry in table %myTable put it into %entry,
        (5:102) print {%entry} to the console.
        (5:150) take variable %i and add 1 to it.
        (5:102) print {%i} to the console.
    (6:454) after the loop is done,
        (5:102) print {I'm done!} to the console.
        (1:108) and variable %myTable is a table,
            (5:101) set %myTable[myKey1] to 123
            (5:102) print {%myTable[myKey1]} to the console.

(0:0) when the script is started,
    (5:101) set %answer to 0
    (5:101) set %life to 42
    (5:102) print {The answer to LIFE is...} to the console.
    (6:450) while variable %answer is not %life,
        (5:150) take variable %answer and add 1 to it.
        (1:102) and variable %answer equals 21,
            (5:450) exit the current loop.
    (6:454) after the loop is done,
        (5:102) print {We may never know the answer...} to the console.
```

The above script creates a table in the first Trigger block, iterates over 
that table with Flow trigger (6:250) and after it prints "I'm done!" to the 
console.  The last Trigger block attempts to answer that very important 
universal question but fails because we may never know the answer...

To execute the [Advanced Usage](#advanced-usage) example, it is no different 
than [Basic Usage](#basic-usage)'s execution example.

#### Monkeyspeak Editor
Yes there is a editor in there.  See [Editor](wiki/Editor.md)

#### Guides
1) :book: [Triggers](wiki/Triggers.md)
2) :book: [Variables](wiki/Variables.md)
3) :book: [Strings](wiki/Strings.md)
4) :book: [Libraries](wiki/Libraries.md)
5) :book: [Compiler](wiki/Compiler.md)
