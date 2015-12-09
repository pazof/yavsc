<%@ Page Title="YourEstimates" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Estimate>>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (((int)ViewData["ResponsibleCount"])>0) { %>
<div>
Les estimations que vous avez faites (<%=ViewData["ResponsibleCount"]%>):<br>
<% 
foreach (Estimate estim in Model) {
	if (string.Compare(estim.Responsible,(string) ViewData["UserName"])==0) { %>

	<%= Html.TranslatedActionLink("Titre:"+estim.Title+" Client:"+estim.Client,"Estimate",new{id=estim.Id}) %>
	<br>
	<% }}%>
</div>
<% } %>
<% if (((int)ViewData["ClientCount"])>0) { %>
	<div>
	Vos estimations en tant que client 
	(<%=ViewData["ClientCount"]%>):<br>
	<% foreach (Estimate estim in Model) { %>
		<%= Html.TranslatedActionLink("Titre:"+estim.Title+" Responsable:"+estim.Responsible,"Estimate",new{id=estim.Id}) %>
	<br><% } %>
	</div>
	<% } %> 
</asp:Content>
