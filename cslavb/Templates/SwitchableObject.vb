Imports System.Data.SqlClient

<Serializable()> _
Public Class SwitchableObject
  Inherits BusinessBase(Of SwitchableObject)

#Region " Business Methods "

  ' TODO: add your own fields, properties and methods
  Private mId As Integer

  Public Property Id() As Integer
    <System.Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.NoInlining)> _
    Get
      CanReadProperty(True)
      Return mId
    End Get
    <System.Runtime.CompilerServices.MethodImpl(Runtime.CompilerServices.MethodImplOptions.NoInlining)> _
    Set(ByVal value As Integer)
      CanWriteProperty(True)
      If mId <> value Then
        mId = value
        PropertyHasChanged()
      End If
    End Set
  End Property

  Protected Overrides Function GetIdValue() As Object
    Return mId
  End Function

#End Region

#Region " Validation Rules "

  Protected Overrides Sub AddBusinessRules()

    ' TODO: add validation rules
    'ValidationRules.AddRule(Nothing, "")

  End Sub

#End Region

#Region " Authorization Rules "

  Protected Overrides Sub AddAuthorizationRules()

    ' TODO: add authorization rules
    'AuthorizationRules.AllowWrite("", "")

  End Sub

  Public Shared Function CanAddObject() As Boolean
    Return ApplicationContext.User.IsInRole("")
  End Function

  Public Shared Function CanGetObject() As Boolean
    Return ApplicationContext.User.IsInRole("")
  End Function

  Public Shared Function CanEditObject() As Boolean
    Return ApplicationContext.User.IsInRole("")
  End Function

  Public Shared Function CanDeleteObject() As Boolean
    Return ApplicationContext.User.IsInRole("")
  End Function

#End Region

#Region " Factory Methods "

  Public Shared Function NewSwitchable() As SwitchableObject
    Return DataPortal.Create(Of SwitchableObject)(New RootCriteria())
  End Function

  Friend Shared Function NewSwitchableChild() As SwitchableObject
    Return DataPortal.Create(Of SwitchableObject)(New ChildCriteria())
  End Function

  Public Shared Function GetSwitchableRoot( _
    ByVal id As Integer) As SwitchableObject

    Return DataPortal.Fetch(Of SwitchableObject)(New RootCriteria(id))
  End Function

  Friend Shared Function GetSwitchableChild( _
    ByVal dr As SqlDataReader) As SwitchableObject

    Return New SwitchableObject(dr)
  End Function

  Private Sub New()
    ' require use of factory methods
  End Sub

  Private Sub New(ByVal dr As SqlDataReader)
    Fetch(dr)
  End Sub

#End Region

#Region " Data Access "

  <Serializable()> _
  Private Class RootCriteria
    Private mId As Integer
    Public ReadOnly Property Id() As Integer
      Get
        Return mId
      End Get
    End Property
    Public Sub New(ByVal id As Integer)
      mId = id
    End Sub
    Public Sub New()

    End Sub
  End Class

  <Serializable()> _
  Private Class ChildCriteria

  End Class

  Private Overloads Sub DataPortal_Create(ByVal criteria As RootCriteria)
    DoCreate()
  End Sub

  Private Overloads Sub DataPortal_Create(ByVal criteria As ChildCriteria)
    MarkAsChild()
    DoCreate()
  End Sub

  Private Sub DoCreate()
    ' load default values from database here
  End Sub

  Private Overloads Sub DataPortal_Fetch(ByVal criteria As RootCriteria)
    ' TODO: create data reader to load values
    Using dr As SqlDataReader = Nothing
      DoFetch(dr)
    End Using
  End Sub

  Private Sub Fetch(ByVal dr As SqlDataReader)
    MarkAsChild()
    DoFetch(dr)
  End Sub

  Private Sub DoFetch(ByVal dr As SqlDataReader)
    ' TODO: load values
  End Sub

  Protected Overrides Sub DataPortal_Insert()
    ' insert values
  End Sub

  Protected Overrides Sub DataPortal_Update()
    ' update values
  End Sub

  Protected Overrides Sub DataPortal_DeleteSelf()
    DataPortal_Delete(New RootCriteria(mId))
  End Sub

  Private Overloads Sub DataPortal_Delete(ByVal criteria As RootCriteria)
    ' delete values
  End Sub

  Friend Sub Insert(ByVal parent As Object)
    ' TODO: insert values
    MarkOld()
  End Sub

  Friend Sub Update(ByVal parent As Object)
    ' TODO: update values
    MarkOld()
  End Sub

  Friend Sub DeleteSelf()
    ' TODO: delete values
    MarkNew()
  End Sub

#End Region

End Class
