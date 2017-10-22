#### Break Down
Monkeyspeak has a lot of existing Trigges to play with.  See below for a list.

```
Loops
(5:450) exit the current loop.
(6:450) while variable % is not #,
(6:451) while variable % is #,
(6:452) while variable % is not {...},
(6:453) while variable % is {...},
(6:454) after the loop is done,

Tables
(5:250) create a table as %.
(5:251) with table % put # in it at key {...}.
(5:252) with table % put {...} in it at key {...}.
(5:253) with table % get key {...} put it in into variable %.
(6:250) for each entry in table % put it into %,

Debug
(0:10000) when a debug breakpoint is hit,
(5:10000) create a debug breakpoint here,

IO
(1:200) and the file {...} exists,
(1:201) and the file {...} does not exist,
(1:202) and the file {...} can be read from,
(1:203) and the file {...} can be written to,
(5:200) append {...} to file {...}.
(5:201) read from file {...} and put it into variable %.
(5:202) delete file {...}.
(5:203) create file {...}.

Math
(1:150) and variable % is greater than #,
(1:151) and variable % is greater than or equal to #,
(1:152) and variable % is less than #,
(1:153) and variable % is less than or equal to #,
(5:150) take variable % and add # to it.
(5:151) take variable % and subtract # from it.
(5:152) take variable % and multiply it by #.
(5:153) take variable % and divide it by #.

StringOperations
(5:400) with {...} get word count and set it to variable %.
(5:401) with {...} set it to variable %.
(5:402) with {...} get words starting at # to # and set it to variable %.
(5:403) with {...} get index of {...} and set it to variable %.

Sys
(0:100) when job # is called,
(1:100) and variable % is defined,
(1:101) and variable % is not defined,
(1:102) and variable % equals #,
(1:103) and variable % does not equal #,
(1:104) and variable % equals {...},
(1:105) and variable % does not equal {...},
(1:106) and variable % is constant,
(1:107) and variable % is not constant,
(5:100) set variable % to {...}.
(5:101) set variable % to #.
(5:102) print {...} to the console.
(5:103) get the environment variable named {...} and put it into %, (ex: PATH)
(5:104) create random number and put it into variable %.
(5:107) delete variable %.
(5:110) load library from file {...}. (example Monkeyspeak.dll)
(5:115) call job #.

Timers
(0:300) when timer # goes off,
(1:300) and timer # is running,
(1:301) and timer # is not running,
(5:300) create timer # to go off every # second(s) with a start delay of # second(s).
(5:301) stop timer #.
(5:302) get current timer and put the id into variable %.
(5:303) pause script execution for # seconds.
(5:304) get the current uptime and put it into variable %.
(5:305) get the current hour and put it into variable %.
(5:306) get the current minutes and put it into variable %.
(5:307) get the current seconds and put it into variable %.
(5:308) get the current day of the month and put it into variable %.
(5:309) get the current month and put it into variable %.
(5:310) get the current year and put it into variable %.
```

#### Next Up
> :home: [Home](../README.md)

#### Last Lesson
> :books: [Strings](Strings.md)