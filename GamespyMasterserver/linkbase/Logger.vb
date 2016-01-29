'Static Logger
'JW "LeKeks" 05/2014
Public Enum LogLevel
    Verbose = 1
    Info = 2
    Warning = 3
    Exception = 4
End Enum

Public Class Logger
    Public Shared Property LogToFile As Boolean = False
    Public Shared Property LogFileName As String = "/log.txt"
    Public Shared Property MinLogLevel As Byte = 2

    Public Shared Sub Log(ByVal message As String, ByVal level As LogLevel, ParamArray tags() As String)
        If Not level >= MinLogLevel And Not DEBUGMODE_ENABLE Then Return

        For i = 0 To tags.Length - 1
            message = message.Replace("{" & i.ToString() & "}", tags(i))
        Next

        Select Case level
            Case LogLevel.Verbose
                message = "DEBUG | " & message
            Case LogLevel.Warning
                message = "WARN  | " & message
            Case LogLevel.Exception
                message = "EX    | " & message
            Case LogLevel.Info
                message = "INFO  | " & message
        End Select

        message = "[" & Now.ToString() & "] " & message

        Console.WriteLine(message)
        Debug.WriteLine(message)

        If LogToFile = True Then
            My.Computer.FileSystem.WriteAllText(CurDir() & LogFileName, message & vbCrLf, True)
        End If

        If level = LogLevel.Exception Then
            'Console.ReadLine()
            End 'Just exit for now, will savely relaunch
        End If
    End Sub

End Class