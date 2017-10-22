Imports System.Runtime.CompilerServices

Public Module Module1

    ''' <summary>
    ''' Helper extension method
    ''' </summary>
    ''' <param name="reader"></param>
    ''' <param name="addIfNotExist"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension()>
    Public Function ReadVariableOrNumber(ByVal reader As Monkeyspeak.TriggerReader, Optional addIfNotExist As Boolean = False) As Double
        If reader.PeekVariable Then
            Dim value = reader.ReadVariable(addIfNotExist).Value
            If value = GetType(Double) Then
                Return value
            End If
        ElseIf reader.PeekNumber Then
            Return reader.ReadNumber
        End If
        Return Nothing
    End Function

    Sub Main()
        Dim testScript As String = <a>
(0:0) when the script starts,
	(5:100) set variable %file to {test.txt}.

(0:0) when the script starts,
	(1:200) and the file {%file} exist,
		(5:202) delete file {%file}.
		(5:203) create file {%file}.

(0:0) when the script starts,
	(1:200) and the file {%file} exists,
	(1:203) and the file {%file} can be written to,
		(5:200) append {Hello World from Monkeyspeak!} to file {%file}.
        (5:200) append {End of file... .. .. .} to file {%file}.

(0:0) when the script starts,
	(5:150) take variable %test and add 2 to it.
	(5:102) print {%test} to the console.
    (5:100) set variable %test to {test}.
    (5:102) print {%test} to the console.

(0:1000) test cause with reflection,
	(5:1001) test print {Hello World with Reflection!!}.
</a>.Value

        Dim engine = New Monkeyspeak.MonkeyspeakEngine()
        Console.WriteLine("Running " & vbCrLf & testScript)

        Dim start = Stopwatch.StartNew()
        Dim page As Monkeyspeak.Page = engine.LoadFromString(testScript)

        page.LoadAllLibraries()

        page.Execute()
        page.Execute(1000)
        Console.WriteLine("Done! Executed in " & start.Elapsed.ToString())
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub

End Module

Class TestLibrary

    <Monkeyspeak.TriggerHandler(Monkeyspeak.TriggerCategory.Cause, 1000, "(0:1000) test cause with reflection,")>
    Shared Function TestVBReflectionTriggerHandler(reader As Monkeyspeak.TriggerReader) As Boolean 'IMPORTANT label return with As Boolean
        Return True
    End Function

    <Monkeyspeak.TriggerHandler(Monkeyspeak.TriggerCategory.Effect, 1001, "(5:1001) test print {...}.")>
    Shared Function TestVBReflectionTriggerHandler2(reader As Monkeyspeak.TriggerReader) As Boolean 'IMPORTANT label return with As Boolean
        If reader.PeekString Then
            Console.WriteLine(reader.ReadString())
        End If
        Return True
    End Function

End Class