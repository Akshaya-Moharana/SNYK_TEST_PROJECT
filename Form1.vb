Imports System.Xml.Linq
Imports System.Xml
Imports System.IO
Public Class Form1
    Private _isUserAuthenticated As Boolean = False
    Private _isPrivacyClaimUser As Boolean = False
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim message As String = CallCDSToAuthenticate(TextBox1.Text, TextBox2.Text)
        TextBox4.Text = message

        If _isPrivacyClaimUser = True Then
            TextBox4.Text = TextBox4.Text + " [CAN SEE PRIVATE CLAIMS]"
        End If
    End Sub

    Private Function CallCDSToAuthenticate(ByVal UserName As String, ByVal Password As String) As String
        Dim userNotFound As String = "User (xyz@cdorav00.crawco.local) was not found.".Replace("xyz", UserName)
        Dim badPassowrd As String = "Logon failure: unknown user name or bad password."
        Dim passwordExpiredFlagMessage As String = "Password Expired.  User must change Password."
        Dim passwordExpiredFlagMessage2 As String = "User must change Password."
        Dim constraintViolation As String = "Password could not be changed. A constraint violation occurred."
        Dim accountLocked As String = "Account has been locked out due to excessive failed logon attempts."
        Dim unexpectedError As String = "Unexpected error."
        Dim message As String = String.Empty
        Dim errorID As String = ""
        Try
            message = CDSAuthenticate(UserName, Password)

            '    If message IsNot Nothing Then
            '        'If message <> "authorized" Then
            '        Select Case message.ToUpper.Trim
            '            Case userNotFound.ToUpper.Trim
            '                errorID = "101"
            '            Case badPassowrd.ToUpper.Trim
            '                errorID = "102"
            '            Case passwordExpiredFlagMessage.ToUpper.Trim
            '                errorID = "103"
            '                'AppContext.Session.Item("UserName") = UserName
            '                'AppContext.Session.Item("Password") = Password
            '                'redirect users to  password reset screens
            '                ' AppContext.Site.Redirect_To_ChangePassword()
            '            Case passwordExpiredFlagMessage2.ToUpper.Trim
            '                errorID = "104"
            '                'AppContext.Session.Item("UserName") = UserName
            '                'AppContext.Session.Item("Password") = Password
            '                'redirect users to  password reset screens
            '                'AppContext.Site.Redirect_To_ChangePassword()
            '            Case constraintViolation.ToUpper.Trim
            '                errorID = "105"
            '            Case accountLocked.ToUpper.Trim
            '                errorID = "106"
            '            Case unexpectedError.ToUpper.Trim
            '                errorID = "107"
            '            Case Else
            '                errorID = message
            '        End Select
            '        Dim orgMessage As String = message
            '        message = "Invalid User Name and / or Password"
            '        'LogMessage(UserName, "CallCDS", message, orgMessage, "Call to CDS  web service for authentication.")
            '    ElseIf message Is String.Empty Then
            '        errorID = "107"
            '        Dim orgMessage As String = "CDS web service not responding."
            '        message = "Invalid User Name and / or Password"
            '        'LogMessage(UserName, "CallCDS", message, orgMessage, "Error occured at CDS web service call for authentication.")
            '    Else
            '        message = "AuthenticUser"
            '        'AppContext.Session.Item("UserAuthenticated") = True
            '        'LogMessage(UserName, "CallCDS", message, "User successfully authenticated through CDS call", "Call to CDS  web service for authentication.")
            '    End If
        Catch ex As Exception
            '    If Not ex.InnerException Is Nothing Then
            '        'LogMessage(UserName, "CallCDS", message, ex.Message + "Inner Exception: " + ex.InnerException.Message, "Error occured at CDS web service call  for authentication.")
            '    Else
            '        'LogMessage(UserName, "CallCDS", message, ex.Message, "Error occured at CDS web service call  for authentication.")
            '    End If
        End Try
        Return message
    End Function

    Public Function CDSAuthenticate(ByVal UserName As String, ByVal password As String) As String
        Dim _CheckPasswordFlag As Boolean = False
        Dim returnString As String = String.Empty
        Try
            Dim obj As New WSManager
            Dim cdswObj = obj.getCDSWSInstance
            Dim credential As String = String.Empty
            Dim out As String = String.Empty
            Dim xmlDoc As New XmlDocument
            Dim LDAPDomainAlias As String = "cdorav00.crawco.local"
            Dim xmlOutput As String = String.Empty
            Dim _error As String = String.Empty
            Dim inputxml As String = String.Empty
            Dim inputElement As XElement = <CDSAuthenticateRequest>
                                               <Request type="authenticate"/>
                                               <Credentials account=<%= UserName %> code=<%= password %>/>
                                               <Domains>
                                                   <Domain name=<%= LDAPDomainAlias %>/>
                                               </Domains>
                                           </CDSAuthenticateRequest>

            xmlOutput = cdswObj.Authenticate(inputElement.ToString)
            xmlOutput = xmlOutput.Replace("&amp;", "and")

            Dim _outputXelemet As XElement = XElement.Parse(xmlOutput)
            Dim msg As String = ""
            Dim groupNodeList As XmlNodeList

            TextBox3.Text = xmlOutput

            Try
                msg = _outputXelemet.<Domains>.<Domain>.@msg

                If msg Is Nothing Then
                    Dim isAuth As String = _outputXelemet.<Domains>.<Domain>.@credentials
                    '_outputXelemet.<Domains>.<Domain>.<entity>.<properties>.<property>
                    If isAuth.ToLower.Trim = "authorized" Then
                        _isUserAuthenticated = True
                        _isPrivacyClaimUser = False

                        xmlDoc.LoadXml(xmlOutput.ToUpper)

                        Dim GroupNamePriv As String = "RT_TCRPriv".ToUpper()
                        Dim XmlPrivacyTag As String = "/CDSAuthenticateRequest/Domains/Domain/entity/properties/property[@name='memberof']/propertyvalue".ToUpper
                        groupNodeList = xmlDoc.SelectNodes(XmlPrivacyTag)

                        'Logging to debug defect  12/2/13 HRB
                        'File.AppendAllText(AppContext.Config.LoginErrorFilePath + "\HolliesLog.txt", vbCrLf + "xmlOutput : " + xmlOutput + vbCrLf)

                        For Each groupNode In groupNodeList
                            If groupNode.InnerText.Contains(GroupNamePriv) Then
                                _isPrivacyClaimUser = True
                                'File.AppendAllText(AppContext.Config.LoginErrorFilePath + "\HolliesLog.txt", "..." + vbCrLf)
                                'File.AppendAllText(AppContext.Config.LoginErrorFilePath + "\HolliesLog.txt", "groupNode.InnerText : " + groupNode.InnerText + vbCrLf)
                                Exit For
                            End If
                        Next

                    Else
                        _isUserAuthenticated = False
                    End If
                End If
                returnString = msg
            Catch ex As Exception
                'AppContext.Log.Write(ex, "User.AuthenticateUser: error occured while Calling CDS")
            End Try
        Catch ex As Exception
            'AppContext.Log.Write(ex, "User.AuthenticateUser: error occured while Calling CDS")
        End Try
        Return returnString
    End Function
End Class
