'Packet for the "Details" and "Players" section in the serverlist
'(only used for servers with restrictive NAT)
'JW "LeKeks" 09/2014
Public Class ServerinfoRequestPacket
    Inherits GamespyTcpPacket

    Sub New(ByVal client As GamespyClient, ByVal data() As Byte)
        MyBase.New(client, data)
        'Turn on encryption
        Me.UseCipher = True
    End Sub

    Dim gServer As GamespyGameserver = Nothing  'used to store the gameserver
    Dim queryIPEP As Net.IPEndPoint = Nothing   'the gameserver's IPEP

    Public Overrides Sub ManageData()
        'Verify there's enough data to fetch the IPEP
        If Me.data.Length - Me.bytesParsed < 6 Then Return

        'Get the server's public IPEP
        queryIPEP = ArrayFunctions.GetIPEndPointFromByteArray(data, Me.bytesParsed)
        Me.bytesParsed += 6
        Logger.Log("Requested Information about {0}", LogLevel.Verbose, queryIPEP.ToString())

        'Fetch the gameserver from the database
        Me.gServer = Me.client.server.MySQL.FetchServerByIPEP(queryIPEP)
        If Me.gServer Is Nothing Then Return 'TODO: throw some fancy error message

        'Send response
        Me.client.send(Me)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        Dim buf() As Byte = {0, 0, GS_MS_SERVER_CMD_PUSHSERVER, 0} '2 bytes for the len, cmd, 1 for the flags

        'Setting up the bitwise-flags:
        Dim flags As Byte = 0

        If gServer.PortClosed Then
            'restrictive NAT/FW -> use natneg
            Me.ToggleFlag(flags, GS_FLAG_CONNECT_NEGOTIATE_FLAG)

            'check if there's indeed some NAT or just a FW:
            If gServer.IsNatted Then
                Me.ToggleFlag(flags, GS_FLAG_PRIVATE_IP)
                Me.ToggleFlag(flags, GS_FLAG_NONSTANDARD_PRIVATE_PORT)
            End If
        End If

        Me.ToggleFlag(flags, GS_FLAG_ICMP_IP)          'Allow ICMP-Ping
        Me.ToggleFlag(flags, GS_FLAG_NONSTANDARD_PORT) 'Just send the port every time
        Me.ToggleFlag(flags, GS_FLAG_HAS_FULL_RULES)   'We're sending everything we know

        With Me.gServer
            'Attach requested IPEP
            ConcatArray(queryIPEP.Address.GetAddressBytes, buf)
            ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(queryIPEP.Port), buf)

            If gServer.PortClosed Then
                Dim ip0 As Net.IPAddress = Nothing

                'Attaching the public ip for "natneg w/o NAT" (firewall bypass)
                If Not Net.IPAddress.TryParse(.GetValue("localip0"), ip0) Then
                    ip0 = Net.IPAddress.Parse(.PublicIP)
                End If

                'Attach local IPEP
                ConcatArray(ip0.GetAddressBytes, buf)
                ConcatArray(ArrayFunctions.BuildInvertedUInt16Array(UInt16.Parse(.HostPort)), buf)
            End If

            'Attach ICMP IP
            ConcatArray(queryIPEP.Address.GetAddressBytes(), buf)
        End With

        buf(3) = flags

        Me.AttachFullRuleSet(buf)

        'Attach the packet lenght
        Array.Copy(ArrayFunctions.BuildInvertedUInt16Array(buf.Length), buf, 2)

        'Dim s As String = BuildNiceString(buf)
        Return buf
    End Function

    Private Sub AttachFullRuleSet(ByRef buf() As Byte)
        'Get the players on that server
        Dim pt As ElementTable = Me.client.server.MySQL.FetchPlayers(gServer.InternalId, Me.client.server.Config.PlayerTimeout)
        'Push the playertable onto the serverparams
        If Not IsNothing(pt) Then gServer.DynamicStorage.AttachDataTable(pt)

        'Attach every server-property
        For Each field As DataPair In gServer.DynamicStorage.FieldList
            ConcatArray(GetBytes(field.varName), buf, 0)
            ConcatArray(GetBytes(field.value), buf, 0)
        Next
    End Sub

    Private Sub ToggleFlag(ByRef d As Byte, ByVal f As Byte)
        'Just a bitwise or to toggle bits
        d = d Or f
    End Sub

End Class