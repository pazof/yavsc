<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage<BlogEntry>" MasterPageFile="~/Models/App.master" %>


<asp:Content ContentPlaceHolderID="titleContent" ID="titleContentContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using (Html.BeginForm("RemovePost","Blogs")) { %>

<%= Html.LabelFor(model => model.Title) %> :
<%= Html.TextBox( "Title" ) %>
<%= Html.ValidationMessage("Title", "*") %><br/>

<label for="confirm">supprimer le billet</label>
<%= Html.CheckBox( "confirm" ) %>
<%= Html.ValidationMessage("AgreeToRemove", "*") %>

<%= Html.Hidden("returnUrl") %>
 <input type="submit"/>
 <% } %>

</asp:Content>


