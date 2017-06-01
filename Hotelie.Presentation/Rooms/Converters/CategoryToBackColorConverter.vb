﻿Imports System.Globalization
Imports Hotelie.Presentation.Common

Namespace Rooms.Converters
	Public Class CategoryToBackColorConverter
		Implements IValueConverter

		Dim ReadOnly _colors As List(Of Color)
		Dim _currentColorIndex As Integer

		Dim ReadOnly _recoredCategories As Dictionary(Of String, Color)

		Public Sub New()
			_currentColorIndex = 0
			_colors = New List(Of Color) From {B1, B2, B3, B4, B5, B6, B7, B8, B9, B10}

			_recoredCategories = New Dictionary(Of String, Color)()
		End Sub

		Public Function Convert( value As Object,
		                         targetType As Type,
		                         parameter As Object,
		                         culture As CultureInfo ) As Object Implements IValueConverter.Convert
			Dim id = CType(value, String)

			If Not _recoredCategories.ContainsKey( id )
				_recoredCategories.Add( id, _recoredCategories( _currentColorIndex ) )

				_currentColorIndex += 1
				If _currentColorIndex >= _colors.Count Then _currentColorIndex = 0
			End If

			Return _recoredCategories(id)
		End Function

		Public Function ConvertBack( value As Object,
		                             targetType As Type,
		                             parameter As Object,
		                             culture As CultureInfo ) As Object Implements IValueConverter.ConvertBack
			Throw New NotImplementedException()
		End Function
	End Class
End Namespace
