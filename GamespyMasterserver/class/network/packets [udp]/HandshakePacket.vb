'Some generic "ping"-packet
Public Class HandshakePacket
    Inherits GamespyUdpPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        MyBase.New(server, remoteIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Me.SetupIDs()
        Logger.Log("Got response from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)
        Me.Server.Server.MySQL.SetHeartBeat(Me.RemoteIPEP, False, True)
        'Got some data -> updating the timestamp 
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = {}
        ConcatArray(GS_SERVICE_MASTER_PREFIX, buf)
        ConcatArray({GS_MASTER_CMD_HANDSHAKE}, buf)
        ConcatArray(Me.ClientUUID, buf)
        ConcatArray(GetBytes(GS_HANDSHAKE_STRING), buf) 'will just carry some payload
        Return buf
    End Function

End Class