'Validates a gameserver-connect
'JW "LeKeks" 07/2014
Public Class ChallengeRequestPacket
    Inherits GamespyUdpPacket

    Public Property GServer As GamespyGameserver

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint, Optional ByVal gserver As GamespyGameserver = Nothing)
        MyBase.New(server, remoteIPEP)

        'Ensure there's a server
        If Not gserver Is Nothing Then
            Me.GServer = gserver
        End If
    End Sub

    Public Overrides Sub ManageData()
        Me.SetupIDs()
        'get the challenge-token
        Dim token As String = ArrayFunctions.GetString(Me.data)
        Logger.Log("Received token '{0}' from {1}", LogLevel.Verbose, token, Me.RemoteIPEP.ToString)

        'Ack the packet
        Dim hap As New HeartbeatAckdPacket(Me.Server, Me.RemoteIPEP)
        hap.SetupIDs(Me)
        Me.Server.send(hap)
        Logger.Log("Ack'd token from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)

        'check if we can directly connect to the server
        Dim hsp As New HandshakePacket(Me.Server, Me.RemoteIPEP)
        hsp.SetupIDs(Me)
        Me.Server.Server.GSUdpServer.sendCheck(hsp)

        Logger.Log("Sending firewall-check-packet to {0}", LogLevel.Verbose, Me.RemoteIPEP.Address.ToString)

        'Also update the server as we ack'd it's last heartbeat
        Me.Server.Server.MySQL.SetHeartBeat(Me.RemoteIPEP, True)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = GS_SERVICE_MASTER_PREFIX

        ConcatArray({GS_MASTER_CMD_CHALLENGE}, buf)
        ConcatArray(Me.ClientUUID, buf)
        ConcatArray(Me.GServer.GetChallengeToken(), buf)
        ConcatArray({&H30, &H30}, buf)

        Dim addr() As Byte = Me.RemoteIPEP.Address.GetAddressBytes()
        Dim port() As Byte = ArrayFunctions.BuildInvertedUInt16Array(Me.RemoteIPEP.Port)

        'The client's public ip is attached
        Dim rIPEPHexDmp As String = String.Empty
        rIPEPHexDmp &= GetHexDump(addr)
        rIPEPHexDmp &= GetHexDump(port)

        ConcatArray(GetBytes(rIPEPHexDmp), buf)
        'terminate the string
        ConcatArray({&H0}, buf)
        Return buf
    End Function

    Private Function GetHexDump(ByVal buf() As Byte) As String
        Dim r As String = String.Empty
        For i = 0 To buf.Length - 1
            Dim rb As String = Conversion.Hex(buf(i))
            If rb.Length = 1 Then rb = "0" & rb
            r &= rb
        Next
        Return r
    End Function

End Class