'TCP-Packet Base class
'JW "LeKeks" 04/2014
Public Class GamespyTcpPacket
    Inherits PacketBase

    Public Property client As GamespyClient
    Public Property UseCipher As Boolean = False

    'Gets key for SapphireII
    Friend Function FetchKey(ByVal data() As Byte) As Byte()
        Dim buffer(7) As Byte
        Array.Copy(data, Me.bytesParsed, buffer, 0, buffer.Length)
        bytesParsed += 8
        Return buffer
    End Function

    Sub New(ByVal client As GamespyClient, Optional ByVal data() As Byte = Nothing)
        Me.data = data
        Me.client = client
        Me.bytesParsed = 1 'offset (cmd-id)
    End Sub

End Class