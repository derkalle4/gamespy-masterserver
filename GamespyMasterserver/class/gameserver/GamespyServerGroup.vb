'Gameserver-Management Class
'JW "LeKeks" 05/2014
Imports System.Reflection
Public Class GamespyServerGroup

    '\\hostname\\numwaiting\\maxwaiting\\numservers\\numplayers
    Dim _id As Int32 = 0
    Public Property Id As Int32
        Set(value As Int32)
            _id = value
            index = _id.ToString()
        End Set
        Get
            Return _id
        End Get
    End Property


    Public Property Hostname As String

    'TODO: fix
    Public Property NumWaiting As String = "0"
    Public Property MaxWaiting As String = "0"
    Public Property NumServers As String = "0"
    Public Property NumPlayers As String = "0"
    Public Property locale As String = ""
    Public Property index As String = "1"


    Public Function GetValue(ByVal varName As String) As String
        Dim propInfo As PropertyInfo = GetType(GamespyServerGroup).GetProperty(varName, BindingFlags.Public + BindingFlags.Instance + BindingFlags.IgnoreCase)
        Dim val As String = String.Empty
        If propInfo Is Nothing Then
            Logger.Log("{0} is not set!", LogLevel.Warning, varName)
            val = "0"
        Else
            val = propInfo.GetValue(Me)
        End If
        Return val
    End Function

End Class
