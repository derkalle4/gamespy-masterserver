Public Class MessageForwardPacket
    Inherits P2PUdpPacket

    Public Property FwdIPEP As Net.IPEndPoint
    Public Property FwdPayload As Byte()

    Sub New(ByRef server As UdpServer, ByVal rIPEP As Net.IPEndPoint)
        MyBase.New(server, rIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Me.bytesParsed = 1
        Dim peerIPEP As Net.IPEndPoint = ArrayFunctions.GetIPEndPointFromByteArray(data, Me.bytesParsed)
        Me.bytesParsed += 6

        Dim payload() As Byte = {}
        Array.Resize(payload, Me.data.Length - Me.bytesParsed)
        Array.Copy(Me.data, bytesParsed, payload, 0, payload.Length)

        Dim cmp As New ClientMessagePacket(Me.GSServer.GSUdpServer, peerIPEP)
        cmp.Payload = payload
        Me.GSServer.GSUdpServer.send(cmp)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = {P2P_CMD_SENDMESSAGE}
        ConcatArray(Me.FwdIPEP.Address.GetAddressBytes, buf)
        ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(FwdIPEP.Port), buf)
        ConcatArray(Me.FwdPayload, buf)
        Return buf
    End Function
End Class
