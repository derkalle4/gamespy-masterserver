Public Class MessagePacket
    Inherits GamespyTcpPacket

    Private cookie(3) As Byte
    Private payload() As Byte

    Sub New(ByVal client As GamespyClient, Optional ByVal data() As Byte = Nothing)
        MyBase.New(client, data)
    End Sub

    Public Overrides Sub ManageData()
        Dim peerIPEP As Net.IPEndPoint = ArrayFunctions.GetIPEndPointFromByteArray(data, Me.bytesParsed)
        Me.bytesParsed += 6

        Dim message(Me.data.Length - Me.bytesParsed - 1) As Byte
        Me.payload = message
        Array.Copy(Me.data, bytesParsed, message, 0, message.Length)
        Array.Copy(Me.data, Me.data.Length - 4, cookie, 0, cookie.Length)

        If Not Me.client.server.Config.P2PEnable Then
            Logger.Log("Forwarding Message-Request to Peer at {0} ", LogLevel.Verbose, peerIPEP.ToString)
            Dim cmp As New ClientMessagePacket(Me.client.server.GSUdpServer, peerIPEP)
            cmp.PeerIPEP = peerIPEP
            cmp.Payload = message
            Me.client.server.GSUdpServer.send(cmp)
        Else
            'Check if we're the masterserver handling the client
            Dim ms As MasterServer = Me.client.server.MySQL.GetManagingMasterserver(peerIPEP)
            If ms.id = Me.client.server.Config.ServerID Then
                Logger.Log("Forwarding Message-Request to Peer at {0} ", LogLevel.Verbose, peerIPEP.ToString)
                Dim cmp As New ClientMessagePacket(Me.client.server.GSUdpServer, peerIPEP)
                cmp.PeerIPEP = peerIPEP
                cmp.Payload = message
                Me.client.server.GSUdpServer.send(cmp)
            Else
                'Forward the packet to the proper masterserver which will deliver it to the client
                'The packet can't be send by the local instance since the client is "connected" to the other server
                '(Every NAT will block it)
                Logger.Log("MS-Forward to {0} at {1}", LogLevel.Verbose, ms.msName, ms.rIPEP.ToString)
                Dim mfp As New MessageForwardPacket(Me.client.server.MSP2PHandler, ms.rIPEP)
                mfp.FwdIPEP = peerIPEP
                mfp.FwdPayload = message
                Me.client.server.MSP2PHandler.send(mfp)
            End If
        End If
        Me.client.send(Me)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buffer() As Byte = {}
        ConcatArray(GS_SERVICE_NATNEG_PREFIX, buffer)
        ConcatArray(GS_NATNEG_HEADER, buffer)
        ConcatArray(Me.cookie, buffer)
        Return buffer
    End Function

End Class