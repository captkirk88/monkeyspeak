### Libraries

#### Break Down
Monkeyspeak has a lot of existing Triggers to play with.  Basic libraries are already 
provided, such as Sys, Math, Timers, Tables, StringOperations.  If you want to add your own it 
is very very simple.  First, you would create a class, then you would inherit BaseLibrary 
(advanced form is AutoIncrementBaseLibrary that creates the Trigger IDs for you).

```csharp
    public class MyLibrary : BaseLibrary
    {
        public override void Initialize(params object[] args)
        {
            // add trigger handlers here
            Add(TriggerCategory.Cause, 0, EntryPointForScript,
                "(0:0) entering script here,");
            Add(TriggerCategory.Effect, 0, FirstHandlerThatPrints,
                "(5:0) hello!");

            Add(TriggerCategory.Effect, 1, SecondHandlerTakesAString,
                "(5:1) write {...}.");
        }

        private bool EntryPointForScript(TriggerReader reader)
        {
            return true; // return false stops execution of any triggers below the one that called this method
        }

        public bool FirstHandlerThatPrints(TriggerReader reader)
        {
            Console.WriteLine("Hello!");
            return true; // return false stops execution of any triggers below the one that called this method
        }

        public bool SecondHandlerTakesAString(TriggerReader reader)
        {
            Console.WriteLine(reader.ReadString());
            return true; // return false stops execution of any triggers below the one that called this method
        }

        public override void Unload(Page page)
        {
            // this is called by page.Dispose() which is not called automatically
            // remove any unmanaged and disposable resources here or just do a ending action
        }
    }
```
Then add it to your page by calling `page.LoadLibrary(new MyLibrary())`, or if you want to load all libraries call `page.LoadAllLibraries()`.

To remove it call.
```csharp
page.RemoveLibrary<MyLibrary>();
// or the non-generic
page.RemoveLibrary(typeof(MyLibrary));
```


#### Current Default Triggers
```
Loops
(5:450) exit the current loop.
(6:450) while variable % is not #,
(6:451) while variable % is #,
(6:452) while variable % is not {...},
(6:453) while variable % is {...},
(6:454) after the loop is done,

Tables
(1:250) and variable % is a table,
(1:251) and variable % is not a table,
(5:250) create a table as %.
(5:251) with table % put # in it at key {...}.
(5:252) with table % put {...} in it at key {...}.
(5:253) with table % get key {...} put it in into variable %.
(5:254) with table % remove all entries in it.
(6:250) for each entry in table % put it into %,
(6:251) for each key/value pair in table % put them into % and %,

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
(0:100) when job # is called put arguments into table % (optional),
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
(5:102) print {...} to the log.
(5:103) get the environment variable named {...} and put it into %, (ex: PATH)
(5:104) create random number and put it into variable %.
(5:107) delete variable %.
(5:110) load library from file {...}. (example Monkeyspeak.dll)
(5:115) call job # with (add strings, variables, numbers here) arguments.

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
(5:311) start timer #.
(5:312) set the time zone to {...}
(5:313) get univeral time zone and put it into variable %
(5:314) get the available time zones and put it into table %
```

#### Next Up
> :house_with_garden: [Home](../README.md)

#### Last Lesson
> :books: [Strings](Strings.md)