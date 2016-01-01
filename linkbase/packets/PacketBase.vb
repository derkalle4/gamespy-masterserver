Public Class PacketBase
    Public Property PacketId As Byte
    Public Property data As Byte()

    Public Property RemoteIPEP As Net.IPEndPoint
    Friend bytesParsed As Int32 = 0

    Friend Function FetchString(ByVal buffer() As Byte) As String
        Dim strEnd As Int32 = Array.IndexOf(buffer, CByte(0), bytesParsed)
        Dim buf(strEnd - bytesParsed - 1) As Byte
        Array.Copy(buffer, bytesParsed, buf, 0, buf.Length)
        bytesParsed = strEnd + 1
        Return ArrayFunctions.GetString(buf)
    End Function

    'Function stubs
    Public Overridable Function CompileResponse() As Byte()
        Return {}
    End Function

    Public Overridable Sub ManageData()
        Return
    End Sub
End Class