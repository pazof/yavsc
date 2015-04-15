<%@ Page Title="My estimates" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Estimate>>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% if (((int)ViewData["ResponsibleCount"])>0) { %>
<div>
Les estimations que vous avez faites (<%=ViewData["ResponsibleCount"]%>):<br>
<% 
foreach (Estimate estim in Model) {
	if (string.Compare(estim.Responsible,(string) ViewData["UserName"])==0) { %>

	<%= Html.ActionLink("Titre:"+estim.Title+" Client:"+estim.Client+" Id:"+estim.Id.ToString(),"Estimate",new {Id=estim.Id}) %>
	<br>
	<% }}%>
</div>
<% } %>
	<div>
	Vos estimations <% if (((int)ViewData["ResponsibleCount"])>0) { %>
	en tant que client 
	<% } %> (<%=ViewData["ClientCount"]%>):<br>
	<% foreach (Estimate estim in Model) {
	if (string.Compare(estim.Client,(string)ViewData["UserName"])==0) { %>
	<%= Html.ActionLink("Titre:"+estim.Title+" Responsable:"+estim.Responsible+" Id:"+estim.Id.ToString(),"Estimate",new {Id=estim.Id}) %>
	<br>
	<% }} %>
	</div>
</asp:Content>
