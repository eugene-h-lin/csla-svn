Namespace Validation

  ''' <summary>
  ''' Tracks the business rules broken within a business object.
  ''' </summary>
  <Serializable()> _
  Public Class ValidationRules

    ' list of broken rules for this business object
    Private _brokenRules As BrokenRulesCollection
    ' threshold for short-circuiting to kick in
    Private _processThroughPriority As Integer
    ' reference to current business object
    <NonSerialized()> _
    Private _target As Object
    ' reference to per-instance rules manager for this object
    <NonSerialized()> _
    Private _instanceRules As ValidationRulesManager
    ' reference to per-type rules manager for this object
    <NonSerialized()> _
    Private _typeRules As ValidationRulesManager
    ' reference to the active set of rules for this object
    <NonSerialized()> _
    Private _rulesToCheck As ValidationRulesManager

    Friend Sub New(ByVal businessObject As Object)

      SetTarget(businessObject)

    End Sub

    Friend Sub SetTarget(ByVal businessObject As Object)

      _target = businessObject

    End Sub

    Friend ReadOnly Property Target() As Object
      Get
        Return _target
      End Get
    End Property

    Private ReadOnly Property BrokenRulesList() As BrokenRulesCollection
      Get
        If _brokenRules Is Nothing Then
          _brokenRules = New BrokenRulesCollection
        End If
        Return _brokenRules
      End Get
    End Property

    Private Function GetInstanceRules(ByVal createObject As Boolean) As ValidationRulesManager

      If _instanceRules Is Nothing Then
        If createObject Then
          _instanceRules = New ValidationRulesManager
        End If
      End If
      Return _instanceRules

    End Function

    Private Function GetTypeRules(ByVal createObject As Boolean) As ValidationRulesManager

      If _typeRules Is Nothing Then
        _typeRules = SharedValidationRules.GetManager(_target.GetType, createObject)
      End If
      Return _typeRules

    End Function

    Private ReadOnly Property RulesToCheck() As ValidationRulesManager
      Get
        If _rulesToCheck Is Nothing Then
          Dim instanceRules As ValidationRulesManager = GetInstanceRules(False)
          Dim typeRules As ValidationRulesManager = GetTypeRules(False)
          If instanceRules Is Nothing Then
            If typeRules Is Nothing Then
              _rulesToCheck = Nothing

            Else
              _rulesToCheck = typeRules
            End If

          ElseIf typeRules Is Nothing Then
            _rulesToCheck = instanceRules

          Else
            ' both have values - consolidate into instance rules
            _rulesToCheck = instanceRules
            For Each de As Generic.KeyValuePair(Of String, RulesList) In typeRules.RulesDictionary
              Dim rules As RulesList = _rulesToCheck.GetRulesForProperty(de.Key, True)
              Dim instanceList As List(Of IRuleMethod) = rules.GetList(False)
              instanceList.AddRange(de.Value.GetList(False))
              Dim dependancy As List(Of String) = _
                de.Value.GetDependancyList(False)
              If dependancy IsNot Nothing Then
                rules.GetDependancyList(True).AddRange(dependancy)
              End If
            Next
          End If
        End If
        Return _rulesToCheck
      End Get
    End Property

    ''' <summary>
    ''' Returns an array containing the text descriptions of all
    ''' validation rules associated with this object.
    ''' </summary>
    ''' <returns>String array.</returns>
    ''' <remarks></remarks>
    Public Function GetRuleDescriptions() As String()

      Dim result As New List(Of String)
      Dim rules As ValidationRulesManager = RulesToCheck
      If rules IsNot Nothing Then
        For Each de As Generic.KeyValuePair(Of String, RulesList) In rules.RulesDictionary
          Dim list As List(Of IRuleMethod) = de.Value.GetList(False)
          For i As Integer = 0 To list.Count - 1
            Dim rule As IRuleMethod = list(i)
            result.Add(CObj(rule).ToString)
          Next
        Next
      End If
      Return result.ToArray

    End Function

#Region " Short-Circuiting "

    ''' <summary>
    ''' Gets or sets the priority through which
    ''' CheckRules should process before short-circuiting
    ''' processing on broken rules.
    ''' </summary>
    ''' <value>Defaults to 0.</value>
    ''' <remarks>
    ''' All rules for each property are processed by CheckRules
    ''' though this priority. Rules with lower priorities are
    ''' only processed if no previous rule has been marked as
    ''' broken.
    ''' </remarks>
    Public Property ProcessThroughPriority() As Integer
      Get
        Return _processThroughPriority
      End Get
      Set(ByVal value As Integer)
        _processThroughPriority = value
      End Set
    End Property

#End Region

#Region " Adding Instance Rules "

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    Public Sub AddInstanceRule( _
      ByVal handler As RuleHandler, ByVal propertyName As String)

      GetInstanceRules(True).AddRule(handler, New RuleArgs(propertyName), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddInstanceRule( _
      ByVal handler As RuleHandler, ByVal propertyName As String, _
      ByVal priority As Integer)

      GetInstanceRules(True).AddRule(handler, New RuleArgs(propertyName), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    ''' <typeparam name="T">Type of the business object to be validated.</typeparam>
    Public Sub AddInstanceRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyName As String)

      GetInstanceRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyName), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    ''' <typeparam name="T">Type of the business object to be validated.</typeparam>
    Public Sub AddInstanceRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyName As String, _
      ByVal priority As Integer)

      GetInstanceRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyName), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    Public Sub AddInstanceRule(ByVal handler As RuleHandler, ByVal args As RuleArgs)

      GetInstanceRules(True).AddRule(handler, args, 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddInstanceRule(ByVal handler As RuleHandler, ByVal args As RuleArgs, _
      ByVal priority As Integer)

      GetInstanceRules(True).AddRule(handler, args, priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <typeparam name="T">Type of the target object.</typeparam>
    ''' <typeparam name="R">Type of the arguments parameter.</typeparam>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    Public Sub AddInstanceRule(Of T, R As RuleArgs)( _
      ByVal handler As RuleHandler(Of T, R), ByVal args As R)

      GetInstanceRules(True).AddRule(handler, args, 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <typeparam name="T">Type of the target object.</typeparam>
    ''' <typeparam name="R">Type of the arguments parameter.</typeparam>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddInstanceRule(Of T, R As RuleArgs)( _
      ByVal handler As RuleHandler(Of T, R), ByVal args As R, _
      ByVal priority As Integer)

      GetInstanceRules(True).AddRule(handler, args, priority)

    End Sub

#End Region

#Region " Adding Shared Rules "

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyInfo">
    ''' The PropertyInfo object describing the property.
    ''' </param>
    Public Sub AddRule( _
      ByVal handler As RuleHandler, ByVal propertyInfo As Core.IPropertyInfo)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, New RuleArgs(propertyInfo), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    Public Sub AddRule( _
      ByVal handler As RuleHandler, ByVal propertyName As String)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, New RuleArgs(propertyName), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyInfo">
    ''' The PropertyInfo object describing the property.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule( _
      ByVal handler As RuleHandler, ByVal propertyInfo As Core.IPropertyInfo, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, New RuleArgs(propertyInfo), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule( _
      ByVal handler As RuleHandler, ByVal propertyName As String, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, New RuleArgs(propertyName), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyInfo">
    ''' The PropertyInfo object describing the property.
    ''' </param>
    Public Sub AddRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyInfo As Core.IPropertyInfo)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyInfo), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    Public Sub AddRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyName As String)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyName), 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    Public Sub AddRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal args As RuleArgs)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(Of T, RuleArgs)(handler, args, 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyInfo">
    ''' The PropertyInfo object describing the property.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyInfo As Core.IPropertyInfo, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyInfo), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </para><para>
    ''' The propertyName may be used by the method that implements the rule
    ''' in order to retrieve the value to be validated. If the rule
    ''' implementation is inside the target object then it probably has
    ''' direct access to all data. However, if the rule implementation
    ''' is outside the target object then it will need to use reflection
    ''' or CallByName to dynamically invoke this property to retrieve
    ''' the value to be validated.
    ''' </para>
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="propertyName">
    ''' The property name on the target object where the rule implementation can retrieve
    ''' the value to be validated.
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule(Of T)( _
      ByVal handler As RuleHandler(Of T, RuleArgs), ByVal propertyName As String, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(Of T, RuleArgs)(handler, New RuleArgs(propertyName), priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    Public Sub AddRule(ByVal handler As RuleHandler, ByVal args As RuleArgs)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, args, 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule(ByVal handler As RuleHandler, ByVal args As RuleArgs, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, args, priority)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <typeparam name="T">Type of the target object.</typeparam>
    ''' <typeparam name="R">Type of the arguments parameter.</typeparam>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    Public Sub AddRule(Of T, R As RuleArgs)( _
      ByVal handler As RuleHandler(Of T, R), ByVal args As R)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, args, 0)

    End Sub

    ''' <summary>
    ''' Adds a rule to the list of rules to be enforced.
    ''' </summary>
    ''' <remarks>
    ''' A rule is implemented by a method which conforms to the 
    ''' method signature defined by the RuleHandler delegate.
    ''' </remarks>
    ''' <typeparam name="T">Type of the target object.</typeparam>
    ''' <typeparam name="R">Type of the arguments parameter.</typeparam>
    ''' <param name="handler">The method that implements the rule.</param>
    ''' <param name="args">
    ''' A RuleArgs object specifying the property name and other arguments
    ''' passed to the rule method
    ''' </param>
    ''' <param name="priority">
    ''' The priority of the rule, where lower numbers are processed first.
    ''' </param>
    Public Sub AddRule(Of T, R As RuleArgs)( _
      ByVal handler As RuleHandler(Of T, R), ByVal args As R, ByVal priority As Integer)

      ValidateHandler(handler)
      GetTypeRules(True).AddRule(handler, args, priority)

    End Sub

    Private Function ValidateHandler(ByVal handler As RuleHandler) As Boolean

      Return ValidateHandler(handler.Method)

    End Function

    Private Function ValidateHandler(Of T, R As RuleArgs)(ByVal handler As RuleHandler(Of T, R)) As Boolean

      Return ValidateHandler(handler.Method)

    End Function

    Private Function ValidateHandler(ByVal method As System.Reflection.MethodInfo) As Boolean

      If Not method.IsStatic AndAlso method.DeclaringType.IsInstanceOfType(_target) Then
        Throw New InvalidOperationException( _
          String.Format("{0}: {1}", _
          My.Resources.InvalidRuleMethodException, method.Name))
      End If
      Return True

    End Function
#End Region

#Region " Adding per-type dependencies "

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyInfo">
    ''' PropertyInfo for the property.
    ''' </param>
    ''' <param name="dependentPropertyInfo">
    ''' PropertyInfo for the depandent property.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property.
    ''' </remarks>
    Public Sub AddDependentProperty(ByVal propertyInfo As Core.IPropertyInfo, ByVal dependentPropertyInfo As Core.IPropertyInfo)

      GetTypeRules(True).AddDependentProperty(propertyInfo.Name, dependentPropertyInfo.Name)

    End Sub

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyName">
    ''' The name of the property.
    ''' </param>
    ''' <param name="dependentPropertyName">
    ''' The name of the depandent property.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property.
    ''' </remarks>
    Public Sub AddDependentProperty(ByVal propertyName As String, ByVal dependentPropertyName As String)

      GetTypeRules(True).AddDependentProperty(propertyName, dependentPropertyName)

    End Sub

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyName">
    ''' The name of the property.
    ''' </param>
    ''' <param name="dependantPropertyName">
    ''' The name of the depandent property.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property.
    ''' </remarks>
    <Obsolete("Use AddDependentProperty")> _
    <System.ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)> _
    Public Sub AddDependantProperty(ByVal propertyName As String, ByVal dependantPropertyName As String)

      AddDependentProperty(propertyName, dependantPropertyName)

    End Sub

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyInfo">
    ''' PropertyInfo for the property.
    ''' </param>
    ''' <param name="dependentPropertyInfo">
    ''' PropertyInfo for the depandent property.
    ''' </param>
    ''' <param name="isBidirectional">
    ''' If <see langword="true"/> then a 
    ''' reverse dependancy is also established
    ''' from dependentPropertyName to propertyName.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property. If isBidirectional is 
    ''' <see langword="true"/> then an additional association
    ''' is set up so when rules are checked for
    ''' dependentPropertyName the rules for propertyName
    ''' will also be checked.
    ''' </remarks>
    Public Sub AddDependentProperty(ByVal propertyInfo As Core.IPropertyInfo, ByVal dependentPropertyInfo As Core.IPropertyInfo, _
                                    ByVal isBidirectional As Boolean)

      Dim mgr As ValidationRulesManager = GetTypeRules(True)
      mgr.AddDependentProperty(propertyInfo.Name, dependentPropertyInfo.Name)
      If isBidirectional Then
        mgr.AddDependentProperty(dependentPropertyInfo.Name, propertyInfo.Name)
      End If

    End Sub

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyName">
    ''' The name of the property.
    ''' </param>
    ''' <param name="dependentPropertyName">
    ''' The name of the depandent property.
    ''' </param>
    ''' <param name="isBidirectional">
    ''' If <see langword="true"/> then a 
    ''' reverse dependancy is also established
    ''' from dependentPropertyName to propertyName.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property. If isBidirectional is 
    ''' <see langword="true"/> then an additional association
    ''' is set up so when rules are checked for
    ''' dependentPropertyName the rules for propertyName
    ''' will also be checked.
    ''' </remarks>
    Public Sub AddDependentProperty(ByVal propertyName As String, ByVal dependentPropertyName As String, ByVal isBidirectional As Boolean)

      Dim mgr As ValidationRulesManager = GetTypeRules(True)
      mgr.AddDependentProperty(propertyName, dependentPropertyName)
      If isBidirectional Then
        mgr.AddDependentProperty(dependentPropertyName, propertyName)
      End If

    End Sub

    ''' <summary>
    ''' Adds a property to the list of dependencies for
    ''' the specified property
    ''' </summary>
    ''' <param name="propertyName">
    ''' The name of the property.
    ''' </param>
    ''' <param name="dependantPropertyName">
    ''' The name of the depandent property.
    ''' </param>
    ''' <param name="isBidirectional">
    ''' If <see langword="true"/> then a 
    ''' reverse dependancy is also established
    ''' from dependantPropertyName to propertyName.
    ''' </param>
    ''' <remarks>
    ''' When rules are checked for propertyName, they will
    ''' also be checked for any dependent properties associated
    ''' with that property. If isBidirectional is 
    ''' <see langword="true"/> then an additional association
    ''' is set up so when rules are checked for
    ''' dependantPropertyName the rules for propertyName
    ''' will also be checked.
    ''' </remarks>
    <Obsolete("Use AddDependentProperty")> _
    <System.ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)> _
    Public Sub AddDependantProperty(ByVal propertyName As String, ByVal dependantPropertyName As String, ByVal isBidirectional As Boolean)

      Dim mgr As ValidationRulesManager = GetTypeRules(True)
      mgr.AddDependentProperty(propertyName, dependantPropertyName)
      If isBidirectional Then
        mgr.AddDependentProperty(dependantPropertyName, propertyName)
      End If

    End Sub

#End Region

#Region " Checking Rules "

    Private _suppressRuleChecking As Boolean

    ''' <summary>
    ''' Gets or sets a value indicating whether calling
    ''' CheckRules should result in rule
    ''' methods being invoked.
    ''' </summary>
    ''' <value>True to suppress all rule method invocation.</value>
    Public Property SuppressRuleChecking() As Boolean
      Get
        Return _suppressRuleChecking
      End Get
      Set(ByVal value As Boolean)
        _suppressRuleChecking = value
      End Set
    End Property

    ''' <summary>
    ''' Invokes all rule methods associated with
    ''' the specified property and any 
    ''' dependent properties.
    ''' </summary>
    ''' <param name="propertyInfo">
    ''' Property to validate.
    ''' </param>
    Public Function CheckRules(ByVal propertyInfo As Csla.Core.IPropertyInfo) As String()

      CheckRules(propertyInfo.Name)
      Return New String() {propertyInfo.Name}

    End Function

    ''' <summary>
    ''' Invokes all rule methods associated with
    ''' the specified property and any 
    ''' dependent properties.
    ''' </summary>
    ''' <param name="propertyName">The name of the property to validate.</param>
    Public Function CheckRules(ByVal propertyName As String) As String()

      Dim result As New List(Of String)
      result.Add(propertyName)

      If _suppressRuleChecking Then
        Return result.ToArray
      End If

      ' get the rules dictionary
      Dim rules As ValidationRulesManager = RulesToCheck
      If rules IsNot Nothing Then
        ' get the rules list for this property
        Dim rulesList As RulesList = rules.GetRulesForProperty(propertyName, False)
        If rulesList IsNot Nothing Then
          ' get the actual list of rules (sorted by priority)
          Dim list As List(Of IRuleMethod) = rulesList.GetList(True)
          If list IsNot Nothing Then
            CheckRules(list)
          End If
          Dim dependencies As List(Of String) = rulesList.GetDependancyList(False)
          If dependencies IsNot Nothing Then
            For i As Integer = 0 To dependencies.Count - 1
              Dim dependentProperty As String = dependencies(i)
              CheckRules(rules, dependentProperty)
              result.Add(dependentProperty)
            Next
          End If
        End If
      End If
      Return result.ToArray

    End Function

    Private Sub CheckRules(ByVal rules As ValidationRulesManager, ByVal propertyName As String)

      ' get the rules list for this property
      Dim rulesList As RulesList = rules.GetRulesForProperty(propertyName, False)
      If rulesList IsNot Nothing Then
        ' get the actual list of rules (sorted by priority)
        Dim list As List(Of IRuleMethod) = rulesList.GetList(True)
        If list IsNot Nothing Then
          CheckRules(list)
        End If
      End If

    End Sub

    ''' <summary>
    ''' Invokes all rule methods for all properties
    ''' in the object.
    ''' </summary>
    Public Sub CheckRules()

      If _suppressRuleChecking Then
        Return
      End If

      Dim rules As ValidationRulesManager = RulesToCheck
      If rules IsNot Nothing Then
        For Each de As Generic.KeyValuePair(Of String, RulesList) In rules.RulesDictionary
          CheckRules(de.Value.GetList(True))
        Next
      End If

    End Sub

    ''' <summary>
    ''' Given a list
    ''' containing IRuleMethod objects, this
    ''' method executes all those rule methods.
    ''' </summary>
    Private Sub CheckRules(ByVal list As List(Of IRuleMethod))

      Dim previousRuleBroken As Boolean
      Dim shortCircuited As Boolean

      For index As Integer = 0 To list.Count - 1
        Dim rule As IRuleMethod = list(index)
        ' see if short-circuiting should kick in
        If Not shortCircuited AndAlso (previousRuleBroken AndAlso rule.Priority > _processThroughPriority) Then
          shortCircuited = True
        End If

        If shortCircuited Then
          ' we're short-circuited, so just remove
          ' all remaining broken rule entries
          BrokenRulesList.Remove(rule)

        Else
          ' we're not short-circuited, so check rule
          Dim ruleResult As Boolean
          Try
            ruleResult = rule.Invoke(_target)

          Catch ex As Exception
            '' force a broken rule
            'ruleResult = False
            'rule.RuleArgs.Severity = RuleSeverity.Error
            'rule.RuleArgs.Description = _
            '  String.Format(My.Resources.ValidationRuleException & "{{2}}", rule.RuleArgs.PropertyName, rule.RuleName, ex.Message)
            ' throw a more detailed exception
            Throw New ValidationException( _
              String.Format(My.Resources.ValidationRuleException, rule.RuleArgs.PropertyName, rule.RuleName), ex)
          End Try

          If ruleResult Then
            ' the rule is not broken
            BrokenRulesList.Remove(rule)

          Else
            ' the rule is broken
            BrokenRulesList.Add(rule)
            If rule.RuleArgs.Severity = RuleSeverity.Error Then
              previousRuleBroken = True
            End If
          End If
          If rule.RuleArgs.StopProcessing Then
            shortCircuited = True
            ' reset the value for next time
            rule.RuleArgs.StopProcessing = False
          End If
        End If
      Next

    End Sub

#End Region

#Region " Status retrieval "

    ''' <summary>
    ''' Returns a value indicating whether there are any broken rules
    ''' at this time. 
    ''' </summary>
    ''' <returns>A value indicating whether any rules are broken.</returns>
    Friend ReadOnly Property IsValid() As Boolean
      Get
        Return BrokenRulesList.ErrorCount = 0
      End Get
    End Property

    ''' <summary>
    ''' Returns a reference to the readonly collection of broken
    ''' business rules.
    ''' </summary>
    ''' <remarks>
    ''' The reference returned points to the actual collection object.
    ''' This means that as rules are marked broken or unbroken over time,
    ''' the underlying data will change. Because of this, the UI developer
    ''' can bind a display directly to this collection to get a dynamic
    ''' display of the broken rules at all times.
    ''' </remarks>
    ''' <returns>A reference to the collection of broken rules.</returns>
    Public Function GetBrokenRules() As BrokenRulesCollection
      Return BrokenRulesList
    End Function

#End Region

  End Class

End Namespace
