﻿Imports Hotelie.Application.Bills.Models

Namespace Common.Infrastructure
	Public Interface IBillsListPresenter
		Sub Reload()
		
		Function ReloadAsync() As Task

		Sub OnBillAdded( model As IBillModel )

		Sub OnBillRemoved( id As String )
	End Interface
End Namespace