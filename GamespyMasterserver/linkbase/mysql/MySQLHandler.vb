'ST MySQL-Wrapper for .NET-connector
'JW "LeKeks" 04/2014
Imports MySql.Data
Imports MySql.Data.MySqlClient
Public Class MySQLHandler
    Public Property Hostname As String
    Public Property Port As Int32
    Public Property DbName As String
    Public Property DbUser As String
    Public Property DbPwd As String

    Friend connection As MySqlConnection

    Public Function Connect() As Boolean
        connection = New MySqlConnection
        Dim connectionString As String = String.Empty
        connectionString = "server=" & _
                            Me.Hostname & ";port=" & _
                            Me.Port.ToString & ";uid = " & _
                            Me.DbUser & ";pwd=" & _
                            Me.DbPwd & ";database=" & _
                            Me.DbName & ";"

        connection.ConnectionString = connectionString

        Logger.Log("Testing MySQL Connection...", LogLevel.Info)

        Try
            connection.Open()
            connection.Close()
            Logger.Log("MySQL OK, connection ready for operation.", LogLevel.Info)
            Return True
        Catch ex As Exception

            Logger.Log("Can't connect to MySQL Server!", LogLevel.Exception)
        End Try
        Return False
    End Function

    Public Function DoQuery(ByVal sql As String) As MySqlDataReader
        Dim query As MySqlCommand = Nothing
        Dim reader As MySqlDataReader = Nothing
        SyncLock Me.connection
            Try
                Logger.Log("Query: " & sql, LogLevel.Verbose)
                If Not Me.connection.State = ConnectionState.Open Then Me.connection.Open()
                query = New MySqlCommand(sql)
                query.Connection = Me.connection
                query.Prepare()

                reader = query.ExecuteReader()

                Return reader
            Catch ex As Exception
                If Not reader Is Nothing Then
                    If reader.IsClosed = False Then reader.Close()
                End If
                Logger.Log("Failed to execute Query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
            End Try
            Return Nothing
        End SyncLock

    End Function

    Public Function NonQuery(ByVal sql As String) As Boolean
        Dim query As MySqlCommand = Nothing
        SyncLock Me.connection
            Try
                Logger.Log("Query: " & sql, LogLevel.Verbose)
                If Not Me.connection.State = ConnectionState.Open Then Me.connection.Open()

                query = New MySqlCommand(sql)
                query.Connection = Me.connection
                query.Prepare()
                query.ExecuteNonQuery()
            Catch ex As Exception
                Logger.Log("Failed to execute Query " & sql & vbCrLf & ex.ToString, LogLevel.Warning)
                Return False
            End Try
        End SyncLock
        Return True
    End Function

    Public Function EscapeString(ByVal sql As String) 'remove the nasty stuff
        Return MySqlHelper.EscapeString(sql)
    End Function

    Public Sub Close()
        If Not Me.connection.State = ConnectionState.Open Then
            Me.connection.Close()
        End If
        connection = Nothing
    End Sub

    Friend Function GetUnixTimestamp(ByVal time As DateTime) As Int64
        Return (DateTime.UtcNow - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
    End Function

    Friend Function GetDateTime(ByVal timestamp As Int64) As DateTime
        Dim dt As New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        dt.AddSeconds(timestamp).ToLocalTime()
        Return dt
    End Function
    Friend Function CheckForRows(ByVal res As MySqlDataReader) As Boolean
        If Not res Is Nothing Then
            res.Read()
            If res.HasRows Then
                res.Close()
                Return True
            End If
            res.Close()
        End If
        Return False
    End Function
End Class