Public Class HeartbeatAckdPacket
    Inherits GamespyUdpPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        MyBase.New(server, remoteIPEP)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = {&HFE, &HFD, GS_MASTER_CMD_CHALLENGE_ACK}
        ConcatArray(Me.ClientUUID, buf)
        'no idea why they attached these bytes, however the clients wants them -> it gets them
        ConcatArray({0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, buf)
        Return buf
    End Function
End Class