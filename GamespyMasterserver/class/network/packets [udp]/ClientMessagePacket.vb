'Message-Forward delivery packet
'JW "LeKeks" 08/2014
Public Class ClientMessagePacket
    Inherits GamespyUdpPacket

    Public Property PeerIPEP As Net.IPEndPoint
    Public Property Payload As Byte()

    Sub New(ByVal server As MSUdpServer, ByVal rIPEP As Net.IPEndPoint)
        MyBase.New(server, rIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Logger.Log("{0} ack'd message", LogLevel.Verbose, Me.RemoteIPEP.ToString)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        'This packet is requested by another client thus we have to fetch the other client's uuid from
        'the database
        Me.ClientUUID = BitConverter.GetBytes(Me.Server.Server.MySQL.FetchClientID(Me.RemoteIPEP))

        'Generate a random message-ID since we're not using it for
        'message-identification it doesn't have to be unique
        Dim msgID(3) As Byte

        With New Random()
            .NextBytes(msgID)
        End With

        'Build the packet
        Dim buffer() As Byte = {}
        ConcatArray(GS_SERVICE_MASTER_PREFIX, buffer)
        ConcatArray({GS_MASTER_CMD_MESSAGE}, buffer)
        ConcatArray(Me.ClientUUID, buffer)
        ConcatArray(msgID, buffer)
        ConcatArray(Me.Payload, buffer)

        Return buffer
    End Function

End Class