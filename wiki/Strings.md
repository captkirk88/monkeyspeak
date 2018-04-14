### Strings

#### What is it?
A word, a phrase, a sentence.  Strings are made up of all of those, the simplest explanation 
is that this entire paragraph is a string.  In Monkeyspeak, Strings are surrounded by a 
`{` and `}` (configurable but we will use the defaults for simplicity).

#### Break Down
Monkeyspeak Strings can contain Variables, the Variable's value is converted unless a @ is 
at the beginning of the string, like so `{@%dontProcessMe}`.

#### Limitations
Strings are optionally limited to a certain amount, the 
default is 1000 characters.  Strings cannot contain inner brackets such as `{ {} }`.  It is a limitation of the 
engine unfortunately.  Strings do not support unicode characters as of v7.0.

#### Prefixes
Certain prefixes can be used at the beginning of a String to prevent certain processing actions.

- **!** - will prevent numbers from being converted into a human readable format.  Such as with commas for 1,000.
- **@** - will prevent variables from being processed.  For example, %myVar has a value of 2 so printing %myVar with a @ will not print 2, it will print %myVar.

Usage would be {@Set %myVar!} or {!1000} or {@!%myVar = 1000}.

#### Next Up
> :book: [Libraries](Libraries.md)

#### Last Lesson
> :books: [Variables](Variables.md)