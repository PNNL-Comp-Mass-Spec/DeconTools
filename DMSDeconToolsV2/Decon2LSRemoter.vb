'*********************************************************************************************************
' Written by Dave Clark for the US Department of Energy 
' Pacific Northwest National Laboratory, Richland, WA
' Copyright 2006, Battelle Memorial Institute
' Created 10/20/2006
'
' Last modified 10/26/2006
'*********************************************************************************************************

Imports DMSDeconToolsV2
Imports DeconEngine
Imports System.Runtime.Remoting.Lifetime

Public Class clsDecon2LSRemoter
	Inherits MarshalByRefObject
	Implements IDisposable

	'*********************************************************************************************************
	'Provides remotable wrapper around Decon2LS to protect main program from impact of possible future changes
	'*********************************************************************************************************

#Region "Module variables"
    Dim m_DeconObj As DMSDeconToolsV2.DMSDecon2LSWrapper
	Dim m_ErrMsg As String = ""
	Dim m_StillInUse As Boolean = True
#End Region

#Region "Properties"
	Public Property DataFile() As String
		Get
			Return m_DeconObj.DataFile
		End Get
		Set(ByVal Value As String)
			m_DeconObj.DataFile = Value
		End Set
	End Property

	Public Property DeconFileType() As Decon2LS.Readers.FileType
		Get
			Return m_DeconObj.FileType
		End Get
		Set(ByVal Value As Decon2LS.Readers.FileType)
			m_DeconObj.FileType = Value
		End Set
	End Property

	Public Property OutFile() As String
		Get
			Return m_DeconObj.OutFile
		End Get
		Set(ByVal Value As String)
			m_DeconObj.OutFile = Value
		End Set
	End Property

	Public Property ParamFile() As String
		Get
			Return m_DeconObj.ParameterFile
		End Get
		Set(ByVal Value As String)
			m_DeconObj.ParameterFile = Value
		End Set
	End Property

	Public ReadOnly Property PercentDone() As Integer
		Get
			Return m_DeconObj.PercentDone
		End Get
	End Property

	Public ReadOnly Property DeconState() As DeconState
		Get
			Return m_DeconObj.State
		End Get
	End Property

	Public ReadOnly Property ErrMsg() As String
		Get
			Return m_ErrMsg
		End Get
	End Property

	Public ReadOnly Property CurrentScan() As Integer
		Get
			Return m_DeconObj.CurrentScan
		End Get
	End Property

	Public ReadOnly Property StillInUse() As Boolean
		Get
			Return m_StillInUse
		End Get
	End Property
#End Region

#Region "Methods"
	Public Sub New()

		'Constructor
		System.Console.WriteLine(Now.ToString & ": Instantiating Remote Class")
        m_DeconObj = New DMSDeconToolsV2.DMSDecon2LSWrapper

	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose

		'Allow calling program to make sure the Decon2LS object is released
		System.Console.WriteLine(Now.ToString & ": Disposing Remote Class")
		m_DeconObj = Nothing

	End Sub

	Public Overloads Sub CreateTIC()

		'Assumes the parameters have already been setup prior to calling this method. NOTE: If ResetState has been called, parameters will be null!
		'
		'The Try...Catch loop merely handles any Decon2LS exceptions gracefully and provides a means for the user to find out what happened.
		'	It's assumed that the calling program will be able to detect that the Decon2LS state is "ERROR", and take appropriate action from there

		Try
			m_DeconObj.CreateTic()
		Catch ex As System.Exception
			m_ErrMsg = ex.Message
		End Try

	End Sub

	Public Overloads Sub CreateTIC(ByVal DataFile As String, ByVal FileType As Decon2LS.Readers.FileType, _
	  ByVal OutFile As String, ByVal ParameterFile As String)

		'Overload to allow specifying parameters in method call

        Try
            'Reset the object
            m_DeconObj.ResetState()
        Catch ex As System.Exception
            m_ErrMsg = "Error calling m_DeconObj.ResetState: " & ex.Message
        End Try

        Try
            'Load the parameters
            With m_DeconObj
                .DataFile = DataFile
                .FileType = FileType
                .OutFile = OutFile
                .ParameterFile = ParameterFile
            End With
        Catch ex As System.Exception
            m_ErrMsg = "Error setting the parameters for m_DeconObj: " & ex.Message
        End Try

        'Make the TIC
		Me.CreateTIC()

	End Sub

	Public Overloads Sub DeConvolute()

		'Assumes the parameters have already been setup prior to calling this method. NOTE: If ResetState has been called, parameters will be null!
		'
		'The Try...Catch loop merely handles any Decon2LS exceptions gracefully and provides a means for the user to find out what happened.
		'	It's assumed that the calling program will be able to detect that the Decon2LS state is "ERROR", and take appropriate action from there

		Try
			m_DeconObj.Deconvolute()
		Catch ex As System.Exception
            m_ErrMsg = "Error calling m_DeconObj.Deconvolute: " & ex.Message
		End Try

	End Sub

	Public Overloads Sub DeConvolute(ByVal DataFile As String, ByVal FileType As Decon2LS.Readers.FileType, _
	  ByVal OutFile As String, ByVal ParameterFile As String)

		'Overload to allow specifying parameters in method call

        Try
            'Reset the object
            m_DeconObj.ResetState()
        Catch ex As System.Exception
            m_ErrMsg = "Error calling m_DeconObj.ResetState: " & ex.Message
        End Try

        Try
            'Load the parameters
            With m_DeconObj
                .DataFile = DataFile
                .FileType = FileType
                .OutFile = OutFile
                .ParameterFile = ParameterFile
            End With
        Catch ex As System.Exception
            m_ErrMsg = "Error setting the parameters for m_DeconObj: " & ex.Message
        End Try

        'Deisotope each spectrum
        Me.DeConvolute()

	End Sub

	Public Sub ResetState()

		'Resets the Decon2LS object. Resets internal state to IDLE, stops threaded processes, and sets the parameters to null
        Try
            'Reset the object
            m_DeconObj.ResetState()
        Catch ex As System.Exception
            m_ErrMsg = "Error calling m_DeconObj.ResetState: " & ex.Message
        End Try

	End Sub

	Public Overrides Function InitializeLifetimeService() As Object

		'Sets remote object lifetime

		'Dim lease As ILease = CType(MyBase.InitializeLifetimeService(), ILease)
		'If lease.CurrentState = LeaseState.Initial Then
		'	lease.InitialLeaseTime = TimeSpan.FromMinutes(1)
		'	lease.SponsorshipTimeout = TimeSpan.FromMinutes(2)
		'	lease.RenewOnCallTime = TimeSpan.FromSeconds(2)
		'End If
		'Return lease

		'Attempt to set an infinite object lifetime
		Return Nothing
	End Function

#End Region

End Class
