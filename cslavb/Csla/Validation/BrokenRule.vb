Namespace Validation

  ''' <summary>
  ''' Stores details about a specific broken business rule.
  ''' </summary>
  <Serializable()> _
  Public Class BrokenRule
    Private mRuleName As String
    Private mDescription As String
    Private mProperty As String
    Private mSeverity As RuleSeverity

    Friend Sub New(ByVal rule As IRuleMethod)
      mRuleName = rule.RuleName
      mDescription = rule.RuleArgs.Description
      mProperty = rule.RuleArgs.PropertyName
      mSeverity = rule.RuleArgs.Severity
    End Sub

    ''' <summary>
    ''' Provides access to the name of the broken rule.
    ''' </summary>
    ''' <value>The name of the rule.</value>
    Public ReadOnly Property RuleName() As String
      Get
        Return mRuleName
      End Get
    End Property

    ''' <summary>
    ''' Provides access to the description of the broken rule.
    ''' </summary>
    ''' <value>The description of the rule.</value>
    Public ReadOnly Property Description() As String
      Get
        Return mDescription
      End Get
    End Property

    ''' <summary>
    ''' Provides access to the property affected by the broken rule.
    ''' </summary>
    ''' <value>The property affected by the rule.</value>
    Public ReadOnly Property [Property]() As String
      Get
        Return mProperty
      End Get
    End Property

    ''' <summary>
    ''' Gets the severity of the broken rule.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Severity() As RuleSeverity
      Get
        Return mSeverity
      End Get
    End Property

  End Class

End Namespace
