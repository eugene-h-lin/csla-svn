﻿''' <summary>
''' Maintains metadata about a property.
''' </summary>
''' <typeparam name="T">
''' Data type of the property.
''' </typeparam>
Public Class PropertyInfo(Of T)

  Implements Core.IPropertyInfo

  ''' <summary>
  ''' Creates a new instance of this class.
  ''' </summary>
  ''' <param name="name">Name of the property.</param>
  Public Sub New(ByVal name As String)

    Me.New(name, "")

  End Sub

  ''' <summary>
  ''' Creates a new instance of this class.
  ''' </summary>
  ''' <param name="name">Name of the property.</param>
  ''' <param name="friendlyName">
  ''' Friendly display name for the property.
  ''' </param>
  Public Sub New(ByVal name As String, ByVal friendlyName As String)

    mName = name
    mFriendlyName = friendlyName
    If GetType(T).Equals(GetType(String)) Then
      mDefaultValue = DirectCast(DirectCast(String.Empty, Object), T)

    Else
      mDefaultValue = Nothing
    End If

  End Sub

  ''' <summary>
  ''' Creates a new instance of this class.
  ''' </summary>
  ''' <param name="name">Name of the property.</param>
  ''' <param name="defaultValue">
  ''' Default value for the property.
  ''' </param>
  Public Sub New(ByVal name As String, ByVal defaultValue As T)

    mName = name
    mDefaultValue = defaultValue

  End Sub

  ''' <summary>
  ''' Creates a new instance of this class.
  ''' </summary>
  ''' <param name="name">Name of the property.</param>
  ''' <param name="friendlyName">
  ''' Friendly display name for the property.
  ''' </param>
  ''' <param name="defaultValue">
  ''' Default value for the property.
  ''' </param>
  Public Sub New(ByVal name As String, ByVal friendlyName As String, ByVal defaultValue As T)

    mName = name
    mDefaultValue = defaultValue
    mFriendlyName = friendlyName

  End Sub

  Private mName As String
  ''' <summary>
  ''' Gets the property name value.
  ''' </summary>
  Public ReadOnly Property Name() As String Implements Core.IPropertyInfo.Name
    Get
      Return mName
    End Get
  End Property

  Private mType As Type
  ''' <summary>
  ''' Gets the type of the property.
  ''' </summary>
  Public ReadOnly Property Type() As Type Implements Core.IPropertyInfo.Type
    Get
      Return GetType(T)
    End Get
  End Property

  Private mFriendlyName As String
  ''' <summary>
  ''' Gets the friendly display name
  ''' for the property.
  ''' </summary>
  ''' <remarks>
  ''' If no friendly name was provided, the
  ''' property name itself is returned as a
  ''' result.
  ''' </remarks>
  Public ReadOnly Property FriendlyName() As String Implements Core.IPropertyInfo.FriendlyName
    Get
      If Not String.IsNullOrEmpty(mFriendlyName) Then
        Return mFriendlyName

      Else
        Return mName
      End If
    End Get
  End Property

  Private mDefaultValue As T
  ''' <summary>
  ''' Gets the default initial value for the property.
  ''' </summary>
  ''' <remarks>
  ''' This value is used to initialize the property's
  ''' value, and is returned from a property get
  ''' if the user is not authorized to 
  ''' read the property.
  ''' </remarks>
  Public ReadOnly Property DefaultValue() As T
    Get
      Return mDefaultValue
    End Get
  End Property

  Private ReadOnly Property IPropertyInfo_DefaultValue() As Object Implements Core.IPropertyInfo.DefaultValue
    Get
      Return mDefaultValue
    End Get
  End Property

  Private Function NewFieldData() As Core.FieldDataManager.IFieldData Implements Core.IPropertyInfo.NewFieldData
    Return New Core.FieldDataManager.FieldData(Of T)
  End Function

End Class