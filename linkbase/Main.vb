'Gamespy Master Server Emulator
'Written by J.Weigelt "LeKeks" 2014
Module Main
    Private server As GamespyServer

    'Main Program
    '<System.STAThread> _
    Sub Main()
        Console.WriteLine(PRODUCT_NAME)
        server = New GamespyServer()
        server.Run()
    End Sub
End Module