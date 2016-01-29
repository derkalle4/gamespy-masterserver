'Implementation of gamespy's available service
'JW "LeKeks" 06/2014
Public Class GameAvailiablePacket
    Inherits GamespyUdpPacket

    Sub New(ByVal server As UdpServer, ByVal remoteIPEP As Net.IPEndPoint)
        MyBase.New(server, remoteIPEP)
    End Sub

    Public Overrides Sub ManageData()
        Me.SetupIDs()
        Logger.Log("Got avail.-request from {0}", LogLevel.Verbose, Me.RemoteIPEP.ToString)
        Me.Server.send(Me)
    End Sub

    Public Overrides Function CompileResponse() As Byte()
        'Just gonna tell the client everything is up and ready for that game for now
        Dim buf() As Byte = {&HFE, &HFD, GS_MASTER_CMD_AVAILIABLE, &H0, &H0, &H0, &H0}
        Return buf
    End Function
End Class