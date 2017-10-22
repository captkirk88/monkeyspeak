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
        Dim engine = New Monkeyspeak.MonkeyspeakEngine()
        Console.WriteLine("Running")

        Dim start = Stopwatch.StartNew()
        Dim page As Monkeyspeak.Page = engine.LoadFromFile("testBIG.ms")

        page.LoadAllLibraries()

        page.Execute()
        start.Stop()
        Console.WriteLine("Done! Executed in " & start.Elapsed.ToString())
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub

End Module