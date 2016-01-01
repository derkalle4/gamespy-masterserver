'MySQLHandler-Wrapper for Masterserver SQL-actions
'JW "LeKeks" 07/2014
Imports MySql.Data.MySqlClient

Public Class MSMysqlHandler
    Inherits MySQLHandler

    Public Property MasterServerID As Int32

    Private Function BuildServer(ByVal reader As MySqlDataReader) As GamespyGameserver
        Dim server As New GamespyGameserver
        With server
            .InternalId = reader("id")
            .PublicIP = reader("address")
            .PublicPort = reader("port")
            .HostPort = reader("hostport")
            .ServerProtocol = reader("protocol")
            .GameName = reader("type")
            .Hostname = reader("gq_hostname")
            .GameType = reader("gq_gametype")
            .MapName = reader("gq_mapname")
            .MaxPlayers = reader("gq_maxplayers")
            .NumPlayers = reader("gq_numplayers")
            .Password = reader("gq_password")
            .GameVer = reader("gamever")
            .Session = reader("session")
            .PrevSession = reader("prevsession")
            .ServerType = reader("servertype")
            .GameMode = reader("gamemode")
            .ClientID = reader("clientid")
            'If Not IsDBNull(reader("netregion")) Then .SwbRegion = reader("netregion")
            .LastHeartbeat = Me.GetDateTime(Int64.Parse(reader("lastseen")))
            .ChallengeOK = (reader("challengeok").Equals(1) Or Not reader("dynamic").Equals(1))
            .HandshakeOK = (reader("handshakeok").Equals(1) Or Not reader("dynamic").Equals(1))
            If Not IsDBNull(reader("custom")) Then .DynamicStorage.ParseParameterString(reader("custom"))
        End With
        Return server
    End Function
    Public Sub SetHeartBeat(ByVal ipep As Net.IPEndPoint, Optional ByVal challengeOK As Boolean = False, Optional ByVal handshakeOK As Boolean = False)
        Dim sql As String = "update  `" & MYSQL_GAMESERVER_TABLE_NAME & "` set `lastseen` = UNIX_TIMESTAMP()"

        If challengeOK Then sql &= ", `challengeok` = 1"
        If handshakeOK Then sql &= ", `handshakeok` = 1"

        sql &= " where " & _
          "`address` = '" & ipep.Address.ToString & "'" & " and " & _
          "`port` = " & ipep.Port

        Me.NonQuery(sql)
    End Sub
    Public Function ServerActive(ByVal ipep As Net.IPEndPoint, ByVal timeout As Int32) As Boolean
        SyncLock Me.connection
            Dim sql As String = "select * from  `" & MYSQL_GAMESERVER_TABLE_NAME & "` where " & _
                               " (UNIX_TIMESTAMP() - `lastseen`) < " & timeout.ToString & " and " & _
                               "`address` = '" & ipep.Address.ToString & "'" & " and " & _
                               "`port` = " & ipep.Port.ToString

            Using res As MySqlDataReader = Me.DoQuery(sql)
                Return CheckForRows(res)
            End Using
        End SyncLock
    End Function
    Public Function FetchServerKey(ByVal gamename As String) As String
        gamename = Me.CorrectGameType(gamename)
        SyncLock Me.connection
            Dim sql As String = "select `key_key` from  `" & MYSQL_SERVERKEY_TABLE_NAME & "` where " & _
                                "`key_gamename` = '" & EscapeString(gamename) & "'"

            Using res As MySqlDataReader = Me.DoQuery(sql)
                If Not res Is Nothing Then
                    res.Read()
                    If res.HasRows Then
                        Dim key As String = res("key_key")
                        res.Close()
                        Return key
                    End If
                    res.Close()
                End If
                Return String.Empty
            End Using
        End SyncLock
    End Function
    Public Sub RegisterServer(ByVal server As GamespyGameserver)
        Dim type As String = EscapeString(server.GameName)
        type = Me.CorrectGameType(type)

        Dim sql As String = "insert into `" & MYSQL_GAMESERVER_TABLE_NAME & "` set " & _
             "`address` = '" & server.PublicIP & "'" & ", " & _
             "`port` = " & server.PublicPort & ", " & _
             "`hostport` = " & server.HostPort & ", " & _
             "`protocol` = '" & server.ServerProtocol & "', " & _
             "`type` = '" & type & "', " & _
             "`gq_hostname` = '" & EscapeString(server.Hostname) & "', " & _
             "`gq_gametype` = '" & EscapeString(server.GameType) & "', " & _
             "`gq_mapname` = '" & EscapeString(server.MapName) & "', " & _
             "`gq_maxplayers` = " & EscapeString(server.MaxPlayers) & ", " & _
             "`gq_numplayers` = " & EscapeString(server.NumPlayers) & ", " & _
             "`gq_password` = " & EscapeString(server.Password) & ", " & _
             "`gamever` = '" & EscapeString(server.GameVer) & "', " & _
             "`lastseen` = " & GetUnixTimestamp(server.LastHeartbeat) & ", " & _
             "`session` = '" & EscapeString(server.Session) & "', " & _
             "`prevsession` = '" & EscapeString(server.PrevSession) & "', " & _
             "`servertype` = '" & EscapeString(server.ServerType) & "', " & _
             "`gamemode` = '" & EscapeString(server.GameMode) & "', " & _
             "`clientid` = " & server.ClientID.ToString & ", " & _
             "`masterserver` = " & Me.MasterServerID.ToString & ", " & _
             "`custom` = '" & EscapeString(server.DynamicStorage.ToParameterString()) & "', " & _
             "`dynamic` = 1"

        Me.NonQuery(sql)
        Me.InsertServerPlayers(server)
    End Sub
    Public Sub UpdateServer(ByVal server As GamespyGameserver)
        Dim sql As String = "update `" & MYSQL_GAMESERVER_TABLE_NAME & "` set " & _
          "`gq_hostname` = '" & EscapeString(server.Hostname) & "', " & _
          "`gq_gametype` = '" & EscapeString(server.GameType) & "', " & _
          "`gq_mapname` = '" & EscapeString(server.MapName) & "', " & _
          "`gq_maxplayers` = " & EscapeString(server.MaxPlayers) & ", " & _
          "`gq_numplayers` = " & EscapeString(server.NumPlayers) & ", " & _
          "`gq_password` = " & EscapeString(server.Password) & ", " & _
          "`gamever` = '" & EscapeString(server.GameVer) & "', " & _
          "`type` = '" & EscapeString(server.GameName) & "', " & _
          "`session` = '" & EscapeString(server.Session) & "', " & _
          "`prevsession` = '" & EscapeString(server.PrevSession) & "', " & _
          "`servertype` = '" & EscapeString(server.ServerType) & "', " & _
          "`gamemode` = '" & EscapeString(server.GameMode) & "', " & _
          "`dynamic` = 1, " & _
          "`lastseen` = " & GetUnixTimestamp(server.LastHeartbeat) & ", " & _
          "`clientid` = " & server.ClientID.ToString & ", " & _
          "`masterserver` = " & Me.MasterServerID.ToString & ", " & _
          "`custom` = '" & EscapeString(server.DynamicStorage.ToParameterString()) & "'" & _
          " where " & _
          "`address` = '" & server.PublicIP & "'" & " and " & _
          "`port` = " & server.PublicPort
        Me.NonQuery(sql)
        Me.InsertServerPlayers(server)
    End Sub
    Public Function GetServers(ByVal gamename As String, ByVal timeout As Int32, Optional ByVal filter As String = "") As List(Of GamespyGameserver)
        'filter = " and gamever = '1.0' "
        gamename = Me.CorrectGameType(gamename)
        SyncLock Me.connection
            Dim sql As String = "select * from  `" & MYSQL_GAMESERVER_TABLE_NAME & "` where " & _
                                "`type` = '" & EscapeString(gamename) & "' and (UNIX_TIMESTAMP() - `lastseen`) < " & timeout.ToString & filter & " and `gamemode` != 'exiting' order by `gq_numplayers` desc"

            Using res As MySqlDataReader = Me.DoQuery(sql)
                Dim servers As New List(Of GamespyGameserver)
                If Not res Is Nothing Then
                    While res.Read
                        servers.Add(Me.BuildServer(res))
                    End While
                    res.Close()
                End If

                Return servers
            End Using
        End SyncLock
    End Function

    Public Sub InsertServerPlayers(ByVal server As GamespyGameserver)
        Dim players As List(Of GamespyPlayer) = server.GetPlayers()
        server.InternalId = Me.FetchServerID(server)

        'No players or no server -> exit
        If server.InternalId = -1 Or players Is Nothing Then Return

        For Each p As GamespyPlayer In players
            Dim sql As String =
                "select `id` from `players` " & _
                " where `sid` = " & server.InternalId.ToString() & _
                " and `gq_name` = '" & CorrectPlayerName(EscapeString(p.player_)) & "'" & _
                " and (UNIX_TIMESTAMP() - `lastseen`) > " & MYSQL_PLAYERROW_DELAY.ToString() & _
                " limit 1"

            Dim id As Int32 = -1
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                If res.HasRows Then id = res("id")
                res.Close()
            End Using

            If id <> -1 Then
                sql = "update `players` set " & _
                    "`gq_kills` = " & p.kills_.ToString() & ", " & _
                    "`gq_deaths` = " & p.deaths_.ToString() & ", " & _
                    "`gq_score` = " & p.score_.ToString() & ", " & _
                    "`gq_ping` = " & p.ping_.ToString() & ", " & _
                    "`gq_team` = " & p.team_ & ", " & _
                    "`lastseen` = UNIX_TIMESTAMP()" & _
                    " where id=" & id.ToString()
            Else
                'Dim tst As Int32 = Me.GetUnixTimestamp(Now)

                sql = "insert into `players` (`sid`, `gq_name`, `gq_kills`, `gq_deaths`, `gq_score`, `gq_ping`, `gq_team`, `lastseen`) values "
                sql &= String.Format("({0},'{1}',{2},{3},{4},{5},{6},UNIX_TIMESTAMP())",
                                        server.InternalId.ToString(),
                                        CorrectPlayerName(EscapeString(p.player_)),
                                        p.kills_.ToString(),
                                        p.deaths_.ToString(),
                                        p.score_.ToString(),
                                        p.ping_.ToString(),
                                        p.team_.ToString())
            End If
            Me.NonQuery(sql)
        Next
    End Sub

    Public Sub ResetChallenge(ByVal server As Net.IPEndPoint)
        Dim sql As String = "update `gameserver` set  `challengeok` = 0, `handshakeok` = 0 where " & _
                              "`address` = '" & server.Address.ToString & "'" & " and " & _
                              "`port` = " & server.Port.ToString

        Me.NonQuery(sql)
    End Sub
    Public Function ServerExists(ByVal server As Net.IPEndPoint) As Boolean
        SyncLock Me.connection
            Dim sql As String = "select `id` from  `gameserver` where " & _
                                "`address` = '" & server.Address.ToString & "'" & " and " & _
                                "`port` = " & server.Port.ToString
            Using res As MySqlDataReader = Me.DoQuery(sql)
                Return CheckForRows(res)
            End Using
        End SyncLock
    End Function

    Public Function FetchServerID(ByVal server As GamespyGameserver) As Int32
        SyncLock Me.connection
            Dim sql As String =
          "select `id` from `gameserver` where " & _
          "`address` = '" & server.PublicIP & "'" & " and " & _
          "`port` = " & server.PublicPort

            Using res As MySqlDataReader = Me.DoQuery(sql)
                Dim val As Int32 = FetchValue(res, "id")
                If IsNothing(val) Then Return -1
                Return val
            End Using
        End SyncLock
    End Function

    Public Function FetchClientID(ByVal rIPEP As Net.IPEndPoint) As Int32
        SyncLock Me.connection
            Dim sql As String =
          "select `clientid` from `gameserver` where " & _
          "`address` = '" & rIPEP.Address.ToString & "'" & " and " & _
          "`port` = " & rIPEP.Port.ToString
            Using res As MySqlDataReader = Me.DoQuery(sql)
                Return FetchValue(res, "clientid")
            End Using
        End SyncLock
    End Function

    Private Function FetchValue(ByVal reader As MySqlDataReader, ByVal fieldName As String)
        reader.Read()
        If Not reader.HasRows Then
            reader.Close()
            Return Nothing
        End If
        Dim clientId As Object = reader(fieldName)
        reader.Close()
        Return clientId
    End Function

    Public Function FetchMasterserver(ByVal rIPEP As Net.IPEndPoint) As MasterServer
        Dim sql As String = _
            "select `id`, `server_name` from `masterserver` " & _
            "where `server_address` = '" & rIPEP.Address.ToString & "' and " & _
            "`server_port` = " & rIPEP.Port.ToString
        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()
                If res.HasRows Then
                    Dim ms As New MasterServer
                    ms.id = res("id")
                    ms.msName = res("server_name")
                    ms.rIPEP = rIPEP
                    res.Close()
                    Return ms
                Else
                    res.Close()
                    Return Nothing
                End If
            End Using
        End SyncLock
    End Function
   
    Public Function GetManagingMasterserver(ByVal rIPEP As Net.IPEndPoint) As MasterServer
        SyncLock Me.connection
            Dim sql As String =
            "select `masterserver`, `server_name`, `server_address`, `server_port` " & _
            "from `gameserver`, `masterserver` where " & _
            "`address` = '" & rIPEP.Address.ToString & "'" & " and " & _
            "`port` = " & rIPEP.Port.ToString & " and " & _
            "`masterserver` = `masterserver`.`id`"

            Using res As MySqlDataReader = Me.DoQuery(sql)
                res.Read()

                If Not res.HasRows Then
                    res.Close()
                    Return Nothing
                End If

                Dim ms As New MasterServer
                ms.id = res("masterserver")
                ms.msName = res("server_name")
                ms.rIPEP = New Net.IPEndPoint(Net.IPAddress.Parse(res("server_address")), UInt16.Parse(res("server_port")))
                res.Close()
                Return ms
            End Using
        End SyncLock
    End Function

    Public Function FetchServerByIPEP(ByVal rIPEP As Net.IPEndPoint)
        Dim sql As String = _
            "select * from `gameserver` where `address` = '" & rIPEP.Address.ToString() & "' and " & _
            "`port` = " & rIPEP.Port.ToString()

        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                Dim s As GamespyGameserver = Nothing
                If res.HasRows Then
                    res.Read()
                    s = Me.BuildServer(res)
                End If
                res.Close()
                Return s
            End Using
        End SyncLock
    End Function
    Public Function GetMasterServers() As List(Of MasterServer)
        Dim sql As String = _
            "select * from `masterserver`"
        Dim servers As New List(Of MasterServer)
        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                If Not res Is Nothing Then
                    While res.Read
                        Dim ms As New MasterServer
                        ms.id = res("id")
                        ms.msName = res("server_name")
                        ms.rIPEP = New Net.IPEndPoint(Net.IPAddress.Parse(res("server_address")), UInt16.Parse(res("server_port")))
                        servers.Add(ms)
                    End While
                    res.Close()
                End If
            End Using
        End SyncLock
        Return servers
    End Function

    Public Function FetchPlayers(ByVal sid As String, ByVal timeout As Int32) As ElementTable
        Dim sql As String = "select * from `players` where `sid` = " & sid & " and `lastseen` > (UNIX_TIMESTAMP() - " & timeout.ToString() & ")"
        Dim tbl As New ElementTable()
        tbl.header = {"player_", "score_", "deaths_", "ping_", "team_", "kills_"}

        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)

                If res.HasRows Then
                    Dim players As New List(Of String())

                    While res.Read()
                        Dim pArr(5) As String
                        pArr(0) = res("gq_name").ToString()
                        pArr(1) = res("gq_score").ToString()
                        pArr(2) = res("gq_deaths").ToString()
                        pArr(3) = res("gq_ping").ToString()
                        pArr(4) = res("gq_team").ToString()
                        pArr(5) = res("gq_kills").ToString()
                        players.Add(pArr)
                    End While

                    Dim data(players.Count - 1, tbl.header.Length - 1) As String

                    For i = 0 To players.Count - 1
                        For j = 0 To tbl.header.Length - 1
                            data(i, j) = players(i)(j)
                        Next
                    Next
                    tbl.rows = players.Count
                    tbl.data = data
                End If
                res.Close()
            End Using
        End SyncLock

        Return tbl
    End Function

    Public Function ChallengeOK(ByVal rIPEP As Net.IPEndPoint) As Boolean
        Dim sql As String = _
          "select `id` from `gameserver` where `address` = '" & rIPEP.Address.ToString() & "' and " & _
          "`port` = " & rIPEP.Port.ToString() & " and `challengeok` = 1"

        SyncLock Me.connection
            Using res As MySqlDataReader = Me.DoQuery(sql)
                Return Me.CheckForRows(res)
            End Using
        End SyncLock
    End Function


    'Corrects wrong Gametypes
    Private Function CorrectGameType(ByVal type As String) As String
        If type = "swbfrontps2p" Then type = "swbfrontps2"
        If type = "swbfront2ps2p" Then type = "swbfront2ps2"
        Return type
    End Function

    'Prevents buggy Playernames from being stored
    Private Function CorrectPlayerName(ByVal name As String) As String
        If String.IsNullOrEmpty(name) Then
            Return PARAM_NO_PLAYERNAME
        End If
        Return name
    End Function
End Class
