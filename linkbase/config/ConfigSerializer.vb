'Generic Config Serializer
'JW "LeKeks" 11/2013
Imports System.IO
Imports System.Xml.Serialization
Public Class ConfigSerializer
    Private serializer As XmlSerializer
    Public ReadOnly Property cfgType As Type
        Get
            Return _cfgType
        End Get
    End Property
    Private _cfgType As Type

    Sub New(ByVal cfgType As Type)
        Me._cfgType = cfgType
        serializer = New XmlSerializer(cfgType)
    End Sub

    Protected Overrides Sub Finalize()
        serializer = Nothing
        MyBase.Finalize()
    End Sub

    Public Sub SaveToFile(ByVal fileName As String, ByVal directoryName As String, ByVal config As Object)
        Try
            If Not IO.Directory.Exists(directoryName) Then
                IO.Directory.CreateDirectory(directoryName)
            End If
            fileName = directoryName & fileName
            Dim writer As New StreamWriter(fileName)
            serializer.Serialize(writer, config)
            writer.Close()
        Catch ex As Exception
            Logger.Log("Couldn't write to '{0}'", LogLevel.Exception, fileName)
        End Try
    End Sub

    Public Function LoadFromFile(ByVal fileName As String, ByVal directoryName As String) As Object
        Dim filePath = directoryName & fileName
        Dim config As Object = Nothing
        If IO.File.Exists(filePath) Then
            Try
                Dim reader As New StreamReader(filePath)
                config = serializer.Deserialize(reader)
                reader.Close()
            Catch ex As Exception
                config = Activator.CreateInstance(_cfgType)
                Logger.Log("Couldn't read '{0}'", LogLevel.Exception, fileName)
            End Try
        Else
            config = Activator.CreateInstance(_cfgType)
            saveToFile(fileName, directoryName, config)
        End If
        Return config
    End Function

End Class