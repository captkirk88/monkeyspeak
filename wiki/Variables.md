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

`(6:250) for each entry in table % put it into %,`

Literally spells it out for you what it does.

#### Next Up
> :book: [Strings](Strings.md)

#### Last Lesson
> :books: [Triggers](Triggers.md)

> :house_with_garden: [Home](../README.md)