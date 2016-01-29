Module DebugHelper

    Public Const DEBUG_PROTOCOLDUMP_FILE As String = "C:\users\jan\desktop\hexdmp.txt"

    Public Function BuildNiceString(ByVal data() As Byte) As String
        Dim line1 As String = String.Empty
        Dim line2 As String = String.Empty

        For i = 0 To data.Length - 1
            Dim charAsString As String = data(i).ToString
            If charAsString.Length = 1 Then
                line1 &= "00"
            ElseIf charAsString.Length = 2 Then
                line1 &= "0"
            End If
            line1 &= charAsString & ":"

            line2 &= ArrayFunctions.GetString({data(i)}) & "   "
        Next
        Return line1 & vbCrLf & line2
    End Function

    Public Function BuildInlineArray(ByVal data() As Byte) As String
        Dim line1 As String = String.Empty
        Dim line2 As String = String.Empty

        For i = 0 To data.Length - 1
            Dim charAsString As String = data(i).ToString
            line1 &= charAsString & ","
        Next

        line1 = Mid(line1, 1, line1.Length - 1)
        Return "{" & line1 & "}"
    End Function

    Public Sub CheckFlags(ByVal b As Byte)
        If (b And GS_FLAG_CONNECT_NEGOTIATE_FLAG) Then Logger.Log("Flag GS_FLAG_CONNECT_NEGOTIATE_FLAG set", LogLevel.Verbose)
        If (b And GS_FLAG_HAS_FULL_RULES) Then Logger.Log("Flag GS_FLAG_HAS_FULL_RULES set", LogLevel.Verbose)
        If (b And GS_FLAG_HAS_KEYS) Then Logger.Log("Flag GS_FLAG_HAS_KEYS set", LogLevel.Verbose)
        If (b And GS_FLAG_ICMP_IP) Then Logger.Log("Flag GS_FLAG_ICMP_IP set", LogLevel.Verbose)
        If (b And GS_FLAG_NONSTANDARD_PORT) Then Logger.Log("Flag GS_FLAG_NONSTANDARD_PORT set", LogLevel.Verbose)
        If (b And GS_FLAG_NONSTANDARD_PRIVATE_PORT) Then Logger.Log("Flag GS_FLAG_NONSTANDARD_PRIVATE_PORT set", LogLevel.Verbose)
        If (b And GS_FLAG_PRIVATE_IP) Then Logger.Log("Flag GS_FLAG_PRIVATE_IP set", LogLevel.Verbose)
        If (b And GS_FLAG_UNSOLICITED_UDP) Then Logger.Log("Flag GS_FLAG_UNSOLICITED_UDP set", LogLevel.Verbose)
    End Sub

    Public Sub DumpHexData(ByVal data() As Byte)
        Dim hexdmp As String = String.Empty
        hexdmp = BuildNiceString(data) & vbCrLf
        IO.File.AppendAllText(DEBUG_PROTOCOLDUMP_FILE, hexdmp)
    End Sub


End Module
