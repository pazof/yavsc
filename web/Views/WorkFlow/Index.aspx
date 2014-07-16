<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="head" ID="head" runat="server">
	<%= "<title>Index - " + Html.Encode(YavscHelpers.SiteName) + "</title>" %>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ActionLink("blogs","Index","WorkFlow") %>
</div>
     
</asp:Content>

