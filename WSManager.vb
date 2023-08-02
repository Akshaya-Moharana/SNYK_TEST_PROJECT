
Public Class WSManager
    Protected _objCDSWS As com.crawco.cdsws.CDSWS
    Protected _objCDSWSAuthHeader As com.crawco.cdsws.AuthHeader
    ' Protected _objPasswordGenerator As PasswordGenerator.AutomaticPassword
    Public Function getCDSWSInstance() As com.crawco.cdsws.CDSWS
        _objCDSWS = New com.crawco.cdsws.CDSWS
        Return _objCDSWS
    End Function
    Public Function getCDSWAuthHeaderInstance() As com.crawco.cdsws.AuthHeader
        _objCDSWSAuthHeader = New com.crawco.cdsws.AuthHeader
        Return _objCDSWSAuthHeader
    End Function

    Public Function getCDSWSSecureInstance() As com.crawco.cdsws.CDSWS
        _objCDSWS = New com.crawco.cdsws.CDSWS
        Dim authheader = getCDSWAuthHeaderInstance()
        authheader.Username = "sa_csstest@cdorav00.crawco.local"
        authheader.Password = "L00kM3Up!"
        authheader.Domain = "craw.us"
        _objCDSWS.AuthHeaderValue = authheader
        Return _objCDSWS
    End Function

    '<add key="CDSWSUserName" value="sa_csstest@cdorav00.crawco.local" />
    '<add key="CDSWSPassword" value="L00kM3Up!" />
    '  <add key="CDSWSDomain" value="craw.us" />
End Class