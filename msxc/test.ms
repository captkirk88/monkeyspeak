(0:0) when the script is started,
    (5:102) print {Welcome to %MONKEY} to the console.

(0:0) when the script is started,
    (5:102) print {None = %none} to the console.

(0:0) when the script is started,
    (1:666) and false,
        (5:102) print {better not be seen} to the console.

(0:0) when the script is started,
    (1:104) and variable %hello equals {this will be false move on to next condition}
		(5:100) set %hello to {Hello World}.
        (5:101) set %helloNum to 5.
        (5:102) print {hello = %hello helloNum = %helloNum} to the console.

(0:0) when the script is started,
    *Uncommented version
    (1:104) and variable %hello equals {this will be false move on to next condition}
        (5:102) print {the pen is blue!} to the console
    (1:104) and variable %hello equals {Hello World}
        (5:102) print {the pen is red!} to the console
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:100) set %hello to {@%helloNum}
        (5:101) set %helloNum to 5
        (5:102) print {hello = %hello helloNum = %helloNum} to the console
        (5:115) call job 1.
        (5:115) call job 2.

(0:0) when the script is started,
        (5:250) create a table as %myTable.
        (5:252) with table %myTable put {Hello World} in it at key {myKey}.
        (5:102) print {%myTable[myKey]} to the console.
        (6:250) for each entry in table %myTable put it into %entry,
            (5:102) print {%entry} to the console.

(0:0) when the script is started,
		(5:10000) create a debug breakpoint here,
        (5:100) set %testVariable to {Modified!}. -- try to modify constant variable
		(5:102) print {%testVariable} to the console.

(0:10000) when a debug breakpoint is hit,
		(5:102) print {Hit a breakpoint!} to the console.
        (5:105) raise an error. * dirty exit

(0:100) when job 1 is called,
    (5:102) print {job 1 executed} to the console

(0:100) when job 2 is called,
    (5:102) print {job will not execute because infinite loop possibility} to the console
   (5:115) call job 1.