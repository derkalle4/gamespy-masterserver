'Standart UDP-Heartbeat, kein Token
Public Class ServerHeartbeatPacket
    Inherits GamespyUdpPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        MyBase.New(server, remoteIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Me.SetupIDs()
        'If Me.Server.Server.MySQL.ServerExists(Me.RemoteIPEP) Then
        Me.Server.Server.MySQL.SetHeartBeat(Me.RemoteIPEP)
        Logger.Log("Received UDP heartbeat from {0}", LogLevel.Verbose, Me.RemoteIPEP.Address.ToString)
        'End If
    End Sub

End Class