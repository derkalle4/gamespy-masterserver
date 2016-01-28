Public Class GamespyServer
    Public Property GSTcpServer As TcpServer
    Public Property GSUdpServer As MSUdpServer

    Public Property MSP2PHandler As P2PServerHandler

    Public Property Clients As List(Of GamespyClient)
    Public Property MySQL As MSMysqlHandler

    Public Property Config As CoreConfig
    Private ConfigMan As ConfigSerializer

#Region "Program"
    Public Sub Run()
        Me.PreInit()
        Me.Execution()
        Me.PostInit()
    End Sub

    Private Sub PreInit()
        Logger.Log("Setting up components...", LogLevel.Info)

        Me.ConfigMan = New ConfigSerializer(GetType(CoreConfig))
        Me.Config = Me.ConfigMan.LoadFromFile(CFG_FILE, CurDir() & CFG_DIR)

        Logger.MinLogLevel = Me.Config.Loglevel
        Logger.LogToFile = Me.Config.LogToFile
        Logger.LogFileName = Me.Config.LogFileName

        Me.MySQL = New MSMysqlHandler()
        With Me.MySQL
            .Hostname = Me.Config.MySQLHostname
            .Port = Me.Config.MySQLPort
            .DbName = Me.Config.MySQLDatabase
            .DbUser = Me.Config.MySQLUsername
            .DbPwd = Me.Config.MySQLPwd
            .MasterServerID = Me.Config.ServerID
        End With

        Me.Clients = New List(Of GamespyClient)
        Me.GSTcpServer = New TcpServer()
        Me.GSUdpServer = New MSUdpServer(Me)

        Me.GSUdpServer.Address = Net.IPAddress.Parse(Me.Config.UDPHeartbeatAddress)
        Me.GSUdpServer.Port = Me.Config.UDPHeartbeatPort
        Me.GSUdpServer.CheckPort = Me.Config.UDPFWCheckPort

        Me.GSTcpServer.Port = Me.Config.TCPQueryPort
        Me.GSTcpServer.Address = Net.IPAddress.Parse(Me.Config.TCPQueryAddress)

        AddHandler Me.GSTcpServer.ClientConnected, AddressOf Me.GSTcpServer_OnClientConnect

        If Me.Config.P2PEnable Then
            Me.MSP2PHandler = New P2PServerHandler(Me)
            With Me.MSP2PHandler
                .Address = Net.IPAddress.Parse(Me.Config.P2PAddress)
                .Port = Me.Config.P2PPort
                .EncKey = ArrayFunctions.GetBytes(Me.Config.P2PKey)
            End With
        End If
    End Sub

    Private Sub Execution()
        Logger.Log("Starting components...", LogLevel.Info)
        Me.MySQL.Connect()
        Me.GSTcpServer.Open()
        Me.GSUdpServer.Open()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Open()

        'Me.GSUdpServer.InjectPacket({3, 35, 187, 35, 81, 108, 111, 99, 97, 108, 105, 112, 48, 0, 49, 57, 50, 46, 49, 54, 56, 46, 49, 55, 56, 46, 51, 53, 0, 108, 111, 99, 97, 108, 105, 112, 49, 0, 49, 57, 50, 46, 49, 54, 56, 46, 53, 54, 46, 49, 0, 108, 111, 99, 97, 108, 105, 112, 50, 0, 49, 57, 50, 46, 49, 54, 56, 46, 49, 49, 46, 49, 0, 108, 111, 99, 97, 108, 105, 112, 51, 0, 49, 54, 57, 46, 50, 53, 52, 46, 49, 51, 50, 46, 50, 51, 52, 0, 108, 111, 99, 97, 108, 112, 111, 114, 116, 0, 51, 54, 54, 48, 0, 110, 97, 116, 110, 101, 103, 0, 49, 0, 103, 97, 109, 101, 110, 97, 109, 101, 0, 115, 119, 98, 102, 114, 111, 110, 116, 50, 112, 99, 0, 112, 117, 98, 108, 105, 99, 105, 112, 0, 53, 57, 56, 57, 49, 49, 49, 54, 56, 0, 112, 117, 98, 108, 105, 99, 112, 111, 114, 116, 0, 51, 54, 54, 48, 0, 104, 111, 115, 116, 110, 97, 109, 101, 0, 98, 108, 97, 50, 0, 103, 97, 109, 101, 118, 101, 114, 0, 49, 46, 48, 0, 104, 111, 115, 116, 112, 111, 114, 116, 0, 51, 54, 54, 48, 0, 109, 97, 112, 110, 97, 109, 101, 0, 99, 111, 114, 49, 99, 95, 99, 111, 110, 0, 103, 97, 109, 101, 116, 121, 112, 101, 0, 100, 117, 101, 108, 0, 110, 117, 109, 112, 108, 97, 121, 101, 114, 115, 0, 48, 0, 110, 117, 109, 116, 101, 97, 109, 115, 0, 50, 0, 109, 97, 120, 112, 108, 97, 121, 101, 114, 115, 0, 49, 52, 0, 112, 97, 115, 115, 119, 111, 114, 100, 0, 49, 0, 103, 97, 109, 101, 109, 111, 100, 101, 0, 111, 112, 101, 110, 112, 108, 97, 121, 105, 110, 103, 0, 116, 101, 97, 109, 112, 108, 97, 121, 0, 49, 0, 102, 114, 97, 103, 108, 105, 109, 105, 116, 0, 50, 48, 0, 116, 105, 109, 101, 108, 105, 109, 105, 116, 0, 53, 48, 48, 48, 0, 115, 101, 115, 115, 105, 111, 110, 0, 45, 49, 54, 54, 50, 52, 53, 49, 51, 52, 52, 0, 112, 114, 101, 118, 115, 101, 115, 115, 105, 111, 110, 0, 48, 0, 115, 119, 98, 114, 101, 103, 105, 111, 110, 0, 51, 0, 116, 101, 97, 109, 100, 97, 109, 97, 103, 101, 0, 49, 0, 97, 117, 116, 111, 97, 105, 109, 0, 49, 0, 104, 101, 114, 111, 101, 115, 0, 48, 0, 104, 101, 114, 111, 117, 110, 108, 111, 99, 107, 0, 51, 0, 104, 101, 114, 111, 117, 110, 108, 111, 99, 107, 118, 97, 108, 0, 49, 48, 0, 104, 101, 114, 111, 116, 101, 97, 109, 0, 51, 0, 104, 101, 114, 111, 112, 108, 97, 121, 101, 114, 0, 50, 0, 104, 101, 114, 111, 114, 101, 115, 112, 97, 119, 110, 0, 57, 48, 0, 104, 101, 114, 111, 114, 101, 115, 112, 97, 119, 110, 118, 97, 108, 0, 57, 48, 0, 97, 117, 116, 111, 116, 101, 97, 109, 0, 48, 0, 110, 117, 109, 97, 105, 0, 49, 54, 0, 116, 101, 97, 109, 49, 114, 101, 105, 110, 102, 111, 114, 99, 101, 109, 101, 110, 116, 115, 0, 49, 53, 48, 0, 116, 101, 97, 109, 50, 114, 101, 105, 110, 102, 111, 114, 99, 101, 109, 101, 110, 116, 115, 0, 49, 53, 48, 0, 115, 101, 114, 118, 101, 114, 116, 121, 112, 101, 0, 50, 0, 109, 105, 110, 112, 108, 97, 121, 101, 114, 115, 0, 49, 0, 97, 105, 100, 105, 102, 102, 105, 99, 117, 108, 116, 121, 0, 51, 0, 115, 104, 111, 119, 112, 108, 97, 121, 101, 114, 110, 97, 109, 101, 115, 0, 49, 0, 105, 110, 118, 105, 110, 99, 105, 98, 105, 108, 105, 116, 121, 116, 105, 109, 101, 0, 48, 0, 0, 0}, New Net.IPEndPoint(New Net.IPAddress(0), 123))

        Logger.Log("Launch OK. Server is up.", LogLevel.Info)
        Logger.Log("Press [Return] to exit", LogLevel.Info)
        Console.ReadLine()
        Logger.Log("Shutting down...", LogLevel.Info)
    End Sub

    Private Sub PostInit()
        'Prevent new clients from being added to the client-list
        RemoveHandler Me.GSTcpServer.ClientConnected, AddressOf Me.GSTcpServer_OnClientConnect

        'Drop all remaining clients before shutting down the main infrastructure
        For Each c As GamespyClient In Me.Clients
            c.Dispose()
        Next

        Me.Clients.Clear()

        Me.GSTcpServer.Close()
        Me.GSUdpServer.Close()
        If Me.Config.P2PEnable Then Me.MSP2PHandler.Close()
        Me.MySQL.Close()

        Me.MSP2PHandler = Nothing
        Me.ConfigMan = Nothing
        Me.Config = Nothing
        Me.GSTcpServer = Nothing
        Me.GSUdpServer = Nothing
        Me.Clients = Nothing
        Me.MySQL = Nothing
        GC.Collect()
        Logger.Log("Server stopped.", LogLevel.Info)
        End
    End Sub
#End Region

#Region "Events"
    Private Sub GSTcpServer_OnClientConnect(ByVal sender As TcpServer, ByVal client As Net.Sockets.TcpClient)
        Me.SetupClient(client)
    End Sub
    Private Sub Client_ConnectionLost(ByVal sender As GamespyClient)
        Me.RemoveClient(sender)
    End Sub
#End Region

#Region "Client/Server Funktionen"
    Private Sub SetupClient(ByVal client As Net.Sockets.TcpClient)
        Dim gsc As New GamespyClient(Me, client)
        Clients.Add(gsc)
        AddHandler gsc.ConnectionLost, AddressOf Me.Client_ConnectionLost
    End Sub
    Private Sub RemoveClient(ByVal gsc As GamespyClient)
        RemoveHandler gsc.ConnectionLost, AddressOf Me.Client_ConnectionLost
        Me.Clients.Remove(gsc)
    End Sub
#End Region
End Class