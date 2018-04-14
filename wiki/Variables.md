### Variables

#### What is it?
Variables are a way of storing something, in Monkeyspeak's case it is strings or 
numbers.

#### Break Down
Monkeyspeak Variables start with a `%` sign.  This is configurable to your liking 
via Options found in the MonkeyspeakEngine class.  For all examples, we will use the 
defaults.  Variables can have `_,#,@,$,&,0-9,a-z,A-Z` (no commas).  So a Variable 
called `%__@v$a#r&_` you can create but you might get a headache typing that out all 
the time.

#### Tables
Monkeyspeak Variable Tables are simple Key/Value lists of values.  They can contain 
another Variable, words, or numbers.  You cannot put a Variable Table inside another.
Maybe in the future.  There is only one way to loop through a table and that is by 
using the flow trigger:

`(6:250) for each entry in table %myTable put it into %entry,`

Wherever you can put a variable, you can put a variable table:

```
(5:250) create a table as %myTable.
(5:251) with table %myTable put 123 in it at key {myNumber}.
(5:102) print {%myTable[myNumber]} to the log.
```

#### Object Variables
A new kind of variable called Object Variables that allows you to create variables that are more like objects.
Objects have properties so as a example, lets use a Dog as the object with brown fur.  If the Dog is a
Object then the property fur would be brown.  You would describe that as `Dog.Fur = Brown`.
In Monkeyspeak it is slightly different, `%dog.Fur`.  %dog would be a Object Variable.  You would create one
like so.

```
(5:270) create a object variable %dog
(5:100) set variable %dog.Fur to {Brown}.
```

Easy right?

For the developer, they can add a Object Variable by setting it in the Page.

```csharp
var dog = page.SetVariable(new ObjectVariable("%dog"));
dynamic dogInst = dog.DynamicValue;
dogInst.Fur = "Brown";
// OR
var dog = page.SetVariable(new ObjectVariable("%dog", new DogInfo{Fur="Brown"}));
```

#### Next Up
> :book: [Strings](Strings.md)

#### Last Lesson
> :books: [Triggers](Triggers.md)

> :house_with_garden: [Home](../README.md)