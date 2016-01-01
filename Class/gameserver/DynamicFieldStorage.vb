'Class for dynamic server-property storage
'Used to store "generic" and game-related server-propertys
'JW "LeKeks" 09/2014
Public Class DynamicFieldStorage

    'Storage for out field collection
    Dim _fieldList As List(Of DataPair)
    Public ReadOnly Property FieldList As List(Of DataPair)
        Get
            Return Me._fieldList
        End Get
    End Property

    Sub New()
        Me._fieldList = New List(Of DataPair)
    End Sub

    'Adds another var/value -pair to the list
    Public Sub PushValue(ByVal varName As String, ByVal value As String)
        Dim dp As New DataPair
        dp.varName = varName
        dp.value = value
        FieldList.Add(dp)
    End Sub

    'Fetches a specific var
    Public Function GetValue(ByVal varName As String) As String
        For Each field As DataPair In Me.FieldList
            If field.varName = varName Then
                Return field.value
            End If
        Next
        Return String.Empty
    End Function

    'Converts the field collection to a string
    Public Function ToParameterString() As String
        'This function must not crash at any time, might freeze the master's main thread
        Try
            Dim str As String = String.Empty
            For Each field As DataPair In Me.FieldList
                str &= EscapeString(field.varName) & "=" & EscapeString(field.value) & ";"
            Next
            Return str
        Catch ex As Exception
            Logger.Log("Parameter parse error: Couldn't build Parameter String", LogLevel.Info)
            Return String.Empty
        End Try
    End Function

    'Parses a string and inserts the specified fields into our field collection
    Public Sub ParseParameterString(ByVal paramStr As String)
        'This function must not crash at any time, might freeze the master's main thread
        Try
            Dim fields() As String = Split(paramStr, ";")
            For Each field As String In fields
                Dim parts() As String = Split(field, "=")
                If parts.Length = 2 Then
                    Me.PushValue(RefactorString(parts(0)), RefactorString(parts(1)))
                End If
            Next
        Catch ex As Exception
            Logger.Log("Parameter parse error: Couldn't parse Parameter String {0}", LogLevel.Info, paramStr)
        End Try
    End Sub

    'Attaches a datatable to the collection
    Public Sub AttachDataTable(ByVal dt As ElementTable)
        For i = 0 To dt.rows - 1
            For j = 0 To dt.header.Length - 1
                Me.PushValue(dt.header(j) & i.ToString(), dt.data(i, j))
            Next
        Next
    End Sub

    'Functions to ensure that the paramstr won't get jammed by ;s or =s
    Private Function EscapeString(ByVal str As String) As String
        str = str.Replace("=", "&~equal")
        str = str.Replace(";", "&~semik")
        Return str
    End Function

    Private Function RefactorString(ByVal str As String) As String
        str = str.Replace("&~equal", "=")
        str = str.Replace("&~semik", ";")
        Return str
    End Function

End Class
