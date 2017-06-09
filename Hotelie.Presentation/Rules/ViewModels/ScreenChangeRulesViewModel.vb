﻿Imports System.Collections.Specialized
Imports Caliburn.Micro
Imports Hotelie.Application.Leases.Commands
Imports Hotelie.Application.Leases.Factories
Imports Hotelie.Application.Leases.Queries
Imports Hotelie.Application.Parameters.Commands
Imports Hotelie.Application.Parameters.Queries
Imports Hotelie.Application.Rooms.Commands
Imports Hotelie.Application.Rooms.Factories
Imports Hotelie.Application.Rooms.Queries
Imports Hotelie.Application.Services.Authentication
Imports Hotelie.Presentation.Common
Imports Hotelie.Presentation.Common.Controls
Imports Hotelie.Presentation.Rules.Models
Imports Hotelie.Presentation.Start.MainWindow.Models
Imports Hotelie.Presentation.Start.WorkspaceShell.ViewModels
Imports MaterialDesignThemes.Wpf

Namespace Rules.ViewModels
	Public Class ScreenChangeRulesViewModel
		Inherits AppScreenHasSaving
		Implements INeedWindowModals
		Implements IChild(Of WorkspaceShellViewModel)

		' Dependencies
		Private ReadOnly _authentication As IAuthentication
		Private ReadOnly _getParametersQuery As IGetParametersQuery
		Private ReadOnly _getAllRoomCategoriesQuery As IGetAllRoomCategoriesQuery
		Private ReadOnly _getAllCustomerCategoriesQuery As IGetAllCustomerCategoriesQuery
		Private ReadOnly _updateParametersCommand As IUpdateParametersCommand
		Private ReadOnly _updateRoomCategoryCommand As IUpdateRoomCategoryCommand
		Private ReadOnly _updateCustomerCategoryCommand As IUpdateCustomerCategoryCommand
		Private ReadOnly _removeRoomCategoryCommand As IRemoveRoomCategoryCommand
		Private ReadOnly _removeCustomerCategoryCommand As IRemoveCustomerCategoryCommand
		Private ReadOnly _createRoomCategoryFactory As ICreateRoomCategoryCommand
		Private ReadOnly _createCustomerCategoryFactory As ICreateCustomerCategoryFactory

		' Backing fields
		Private _isEdited As Boolean
		Private _originalRoomCapacity As Integer
		Private _originalExtraCoefficient As Double
		Private ReadOnly _originalCustomerCategories As List(Of EditableCustomerCategoryModel)
		Private ReadOnly _originalRoomCategories As List(Of EditableRoomCategoryModel)

		Public Property Parent As WorkspaceShellViewModel Implements IChild(Of WorkspaceShellViewModel).Parent

		Private Property IChild_Parent As Object Implements IChild.Parent
			Get
				Return Parent
			End Get
			Set
				Parent = Value
			End Set
		End Property

		' Bind models
		Public Property Rule As EditableRuleModel

		Public ReadOnly Property Username As String
			Get
				Return _authentication.LoggedAccount?.Username
			End Get
		End Property

		Public Sub New( authentication As IAuthentication,
		                getParametersQuery As IGetParametersQuery,
		                getAllRoomCategoriesQuery As IGetAllRoomCategoriesQuery,
		                getAllCustomerCategoriesQuery As IGetAllCustomerCategoriesQuery,
		                updateParametersCommand As IUpdateParametersCommand,
		                updateRoomCategoryCommand As IUpdateRoomCategoryCommand,
		                updateCustomerCategoryCommand As IUpdateCustomerCategoryCommand,
		                removeRoomCategoryCommand As IRemoveRoomCategoryCommand,
		                removeCustomerCategoryCommand As IRemoveCustomerCategoryCommand,
		                createRoomCategoryFactory As ICreateRoomCategoryCommand,
		                createCustomerCategoryFactory As ICreateCustomerCategoryFactory )
			MyBase.New( ColorZoneMode.Accent )
			_authentication = authentication
			_getParametersQuery = getParametersQuery
			_getAllRoomCategoriesQuery = getAllRoomCategoriesQuery
			_getAllCustomerCategoriesQuery = getAllCustomerCategoriesQuery
			_updateParametersCommand = updateParametersCommand
			_updateRoomCategoryCommand = updateRoomCategoryCommand
			_updateCustomerCategoryCommand = updateCustomerCategoryCommand
			_removeRoomCategoryCommand = removeRoomCategoryCommand
			_removeCustomerCategoryCommand = removeCustomerCategoryCommand
			_createRoomCategoryFactory = createRoomCategoryFactory
			_createCustomerCategoryFactory = createCustomerCategoryFactory

			DisplayName = "Thay đổi quy định"
			Rule = New EditableRuleModel()
			Rule.RoomCapacity = 0
			Rule.ExtraCoefficient = 0

			_originalRoomCategories = New List(Of EditableRoomCategoryModel)()
			_originalCustomerCategories = New List(Of EditableCustomerCategoryModel)()
			_originalRoomCapacity = Rule.RoomCapacity
			_originalExtraCoefficient = Rule.ExtraCoefficient
		End Sub

		Private Sub ResetValues()
			Rule.CustomerCategories.Clear()
			Rule.RoomCategories.Clear()
			Rule.RoomCapacity = 0
			Rule.ExtraCoefficient = 0
			_originalCustomerCategories.Clear()
			_originalRoomCategories.Clear()
			_originalRoomCapacity = Rule.RoomCapacity
			_originalExtraCoefficient = Rule.ExtraCoefficient

			RemoveHandler Rule.RoomCategories.CollectionChanged, AddressOf OnRulesChanged
			RemoveHandler Rule.CustomerCategories.CollectionChanged, AddressOf OnRulesChanged
			_isEdited = False
		End Sub

		' Show
		Public Overrides Sub Show()
			ReloadRules()
		End Sub

		Private Sub ReloadRules()
			Dim model = _getParametersQuery.Execute()
			Rule.ExtraCoefficient = model.ExtraCoefficient * 100 'convert to percentage
			Rule.RoomCapacity = model.RoomCapacity

			_originalExtraCoefficient = Rule.ExtraCoefficient
			_originalRoomCapacity = Rule.RoomCapacity

			Dim customerCategories = _getAllCustomerCategoriesQuery.Execute()
			Rule.CustomerCategories.Clear()
			Rule.CustomerCategories.AddRange(
				customerCategories.Select( Function( c ) New EditableCustomerCategoryModel With {
					                         .Id = c.Id, .Name = c.Name, .Coefficient = c.Coefficient} ) )

			_originalCustomerCategories.Clear()
			_originalCustomerCategories.AddRange( Rule.CustomerCategories )
			AddHandler Rule.CustomerCategories.CollectionChanged, AddressOf OnRulesChanged

			Dim roomCategories = _getAllRoomCategoriesQuery.Execute()
			Rule.RoomCategories.Clear()
			Rule.RoomCategories.AddRange( roomCategories.Select( Function( r ) New EditableRoomCategoryModel With {
				                                                   .Id = r.Id, .Name = r.Name, .UnitPrice = r.UnitPrice} ) )

			_originalRoomCategories.Clear()
			_originalRoomCategories.AddRange( Rule.RoomCategories )
			AddHandler Rule.RoomCategories.CollectionChanged, AddressOf OnRulesChanged
		End Sub

		Public Overrides Async Function ShowAsync() As Task
			Await ReloadRulesAsync()
		End Function

		Private Async Function ReloadRulesAsync() As Task
			Dim model = Await _getParametersQuery.ExecuteAsync()
			Rule.ExtraCoefficient = model.ExtraCoefficient * 100
			Rule.RoomCapacity = model.RoomCapacity

			_originalExtraCoefficient = Rule.ExtraCoefficient
			_originalRoomCapacity = Rule.RoomCapacity

			Dim customerCategories = Await _getAllCustomerCategoriesQuery.ExecuteAsync()
			Rule.CustomerCategories.Clear()
			Rule.CustomerCategories.AddRange(
				customerCategories.Select( Function( c ) New EditableCustomerCategoryModel With {
					                         .Id = c.Id, .Name = c.Name, .Coefficient = c.Coefficient} ) )
			_originalCustomerCategories.Clear()
			_originalCustomerCategories.AddRange( Rule.CustomerCategories )
			AddHandler Rule.CustomerCategories.CollectionChanged, AddressOf OnRulesChanged

			Dim roomCategories = Await _getAllRoomCategoriesQuery.ExecuteAsync()
			Rule.RoomCategories.Clear()
			Rule.RoomCategories.AddRange( roomCategories.Select( Function( r ) New EditableRoomCategoryModel With {
				                                                   .Id = r.Id, .Name = r.Name, .UnitPrice = r.UnitPrice} ) )
			_originalRoomCategories.Clear()
			_originalRoomCategories.AddRange( Rule.RoomCategories )
			AddHandler Rule.RoomCategories.CollectionChanged, AddressOf OnRulesChanged
		End Function

		Private Sub OnRulesChanged( sender As Object,
		                            e As NotifyCollectionChangedEventArgs )
			_isEdited = True
		End Sub

		' Domain actions
		Public Sub AddEmptyRoomCategory()
			Rule.RoomCategories.Add( New EditableRoomCategoryModel )
		End Sub

		Public Sub AddEmptyCustomerCategory()
			Rule.CustomerCategories.Add( New EditableCustomerCategoryModel )
		End Sub

		' Exit
		Public Overrides ReadOnly Property IsEdited As Boolean
			Get
				Return _isEdited OrElse CheckForPendingChanges()
			End Get
		End Property

		Public Function CheckForPendingChanges() As Boolean
			Return (Not Equals( Rule.ExtraCoefficient, _originalExtraCoefficient )) Or
			       (Not Equals( Rule.RoomCapacity, _originalRoomCapacity ))
		End Function

		Public Overrides Function ActualExitAsync() As Task
			ResetValues()
			Return MyBase.ActualExitAsync()
		End Function

		' Save
		Public Overrides Function CanSave() As Task(Of Boolean)
			Return Task.Run( Function() ValidateSaving() )
		End Function

		Private Function ValidateSaving() As Boolean
			If Rule.RoomCapacity <= 0
				ShowStaticBottomNotification( StaticNotificationType.Information,
				                              "Số khách tối đa phải lớn hơn hoặc bằng 1" )
				Return False
			End If

			If Rule.RoomCategories.Count = 0
				ShowStaticBottomNotification( StaticNotificationType.Information,
				                              "Phải có ít nhất một loại phòng" )
				Return False
			End If

			If Rule.CustomerCategories.Count = 0
				ShowStaticBottomNotification( StaticNotificationType.Information,
				                              "Phải có ít nhất một loại khách" )
				Return False
			End If

			Return True
		End Function

		Private Async Function ConfirmDeleteRoomCategories() As Task(Of Boolean)
			Dim dialog = New TwoButtonDialog( "Xóa loại phòng sẽ dẫn đến xóa tất cả các phiếu thuê phòng, hóa đơn liên quan",
			                                  "TIẾP TỤC XÓA",
			                                  "HỦY",
			                                  True,
			                                  False )
			Dim result As String = Await ShowDynamicWindowDialog( dialog )
			If String.Equals( result, $"TIẾP TỤC XÓA" ) Then Return True
			Return False
		End Function

		Private Async Function ConfirmDeleteCustomerCategories() As Task(Of Boolean)
			Dim dialog = New TwoButtonDialog( "Xóa loại khách sẽ dẫn đến xóa tất cả các phiếu thuê phòng, hóa đơn liên quan",
			                                  "TIẾP TỤC XÓA",
			                                  "HỦY",
			                                  True,
			                                  False )
			Dim result As String = Await ShowDynamicWindowDialog( dialog )
			If String.Equals( result, $"TIẾP TỤC XÓA" ) Then Return True
			Return False
		End Function

		Public Overrides Async Function ActualSaveAsync() As Task
			ShowStaticWindowLoadingDialog()

			'try update parameters
			If Not Await UpdateParameters()
				CloseStaticWindowDialog()
				Return
			End If

			'split room categories actions
			Dim roomCategoriesToRemove = New List(Of String)
			Dim roomCategoriesToUpdate = New List(Of EditableRoomCategoryModel)
			Dim roomCategoriesToCreate = New List(Of EditableRoomCategoryModel)

			SpitRoomCategoryActions( roomCategoriesToRemove, roomCategoriesToUpdate, roomCategoriesToCreate )
			Await UpdateRoomCategories( roomCategoriesToUpdate )
			Await CreateRoomCategories( roomCategoriesToCreate )
			If roomCategoriesToRemove.Count > 0
				CloseStaticWindowDialog()
				If Await ConfirmDeleteRoomCategories()
					Await RemoveRoomCategores( roomCategoriesToRemove )
				End If
				ShowStaticWindowLoadingDialog()
			End If

			'split customer categories actions
			Dim customerCategoriesToRemove = New List(Of String)
			Dim customerCategoriesToUpdate = New List(Of EditableCustomerCategoryModel)
			Dim customerCategoriesToCreate = New List(Of EditableCustomerCategoryModel)

			SplitCustomerCategoryActions( customerCategoriesToRemove, customerCategoriesToUpdate, customerCategoriesToCreate )
			Await UpdateCustomerCategories( customerCategoriesToUpdate )
			Await CreateCustomerCategories( customerCategoriesToCreate )
			If customerCategoriesToRemove.Count > 0
				CloseStaticWindowDialog()
				If Await ConfirmDeleteCustomerCategories()
					Await RemoveCustomerCategories( customerCategoriesToRemove )
				End If
				ShowStaticWindowLoadingDialog()
			End If

			'reload workspaces
			If Parent IsNot Nothing
				Await Parent.ReloadAllWorkspaces()
			End If

			'exit
			CloseStaticWindowDialog()
			Await ActualExitAsync()
		End Function

		' Parameters actions
		Private Async Function UpdateParameters() As Task(Of Boolean)
			Dim err = Await _updateParametersCommand.ExecuteAsync( Rule.RoomCapacity, Rule.ExtraCoefficient / 100 )

			If Not String.IsNullOrEmpty( err )
				ShowStaticBottomNotification( StaticNotificationType.Error, err )
				Return False
			End If

			Return True
		End Function

		' Customer category actions
		Private Sub SplitCustomerCategoryActions( customerCategoriesToRemove As List(Of String),
		                                          customerCategoriesToUpdate As List(Of EditableCustomerCategoryModel),
		                                          customerCategoriesToCreate As List(Of EditableCustomerCategoryModel) )
			For Each customerCategory As EditableCustomerCategoryModel In _originalCustomerCategories
				Dim oldCustomerCategory = Rule.CustomerCategories.FirstOrDefault( Function( c ) c.Id = customerCategory.Id )
				If oldCustomerCategory Is Nothing
					customerCategoriesToRemove.Add( customerCategory.Id )
				Else
					customerCategoriesToUpdate.Add( oldCustomerCategory )
				End If
			Next
			For Each customerCategory As EditableCustomerCategoryModel In Rule.CustomerCategories
				If Not _originalCustomerCategories.Any( Function( r ) r.Id = customerCategory.Id )
					customerCategoriesToCreate.Add( customerCategory )
				End If
			Next
		End Sub

		Private Async Function CreateCustomerCategories( list As List(Of EditableCustomerCategoryModel) ) As Task
			For Each model As EditableCustomerCategoryModel In list
				Await _createCustomerCategoryFactory.ExecuteAsync( model.Name, model.Coefficient )
			Next
		End Function

		Private Async Function UpdateCustomerCategories( list As List(Of EditableCustomerCategoryModel) ) As Task
			For Each model As EditableCustomerCategoryModel In list
				Await _updateCustomerCategoryCommand.ExecuteAsync( model.Id, model.Name, model.Coefficient )
			Next
		End Function

		Private Async Function RemoveCustomerCategories( list As List(Of String) ) As Task
			For Each id As String In list
				Await _removeCustomerCategoryCommand.ExecuteAsync( id )
			Next
		End Function

		' Room category actions
		Private Sub SpitRoomCategoryActions( roomCategoriesToRemove As List(Of String),
		                                     roomCategoriesToUpdate As List(Of EditableRoomCategoryModel),
		                                     roomCategoriesToCreate As List(Of EditableRoomCategoryModel) )
			For Each roomCategory As EditableRoomCategoryModel In _originalRoomCategories
				Dim oldRoomCategory = Rule.RoomCategories.FirstOrDefault( Function( r ) r.Id = roomCategory.Id )
				If oldRoomCategory Is Nothing
					roomCategoriesToRemove.Add( roomCategory.Id )
				Else
					roomCategoriesToUpdate.Add( oldRoomCategory )
				End If
			Next
			For Each roomCategory As EditableRoomCategoryModel In Rule.RoomCategories
				If Not _originalRoomCategories.Any( Function( r ) r.Id = roomCategory.Id )
					roomCategoriesToCreate.Add( roomCategory )
				End If
			Next
		End Sub

		Private Async Function CreateRoomCategories( list As List(Of EditableRoomCategoryModel) ) As Task
			For Each model As EditableRoomCategoryModel In list
				Await _createRoomCategoryFactory.ExecuteAsync( model.Name, model.UnitPrice )
			Next
		End Function

		Private Async Function UpdateRoomCategories( list As List(Of EditableRoomCategoryModel) ) As Task
			For Each model As EditableRoomCategoryModel In list
				Await _updateRoomCategoryCommand.ExecuteAsync( model.Id, model.Name, model.UnitPrice )
			Next
		End Function

		Private Async Function RemoveRoomCategores( list As List(Of String) ) As Task
			For Each id As String In list
				Await _removeRoomCategoryCommand.ExecuteAsync( id )
			Next
		End Function
	End Class
End Namespace